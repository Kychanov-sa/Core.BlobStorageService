using GlacialBytes.Core.BlobStorage.Kernel;
using GlacialBytes.Core.BlobStorage.Kernel.Exceptions;
using Polly;
using Polly.Retry;

namespace GlacialBytes.Core.BlobStorage.Persistence;

/// <summary>
/// Файловое хранилище.
/// </summary>
/// <param name="fileSystem">Файловая система.</param>
/// <param name="mode">Режим работы хранилища.</param>
/// <param name="useSafeDelete">Признак использования безопасного удаления.</param>
internal class FileStorage(IFileSystem fileSystem, BlobStorageMode mode, bool useSafeDelete)
  : IBlobStorage
{
  /// <summary>
  /// Максимальное количество объектов в одной директории.
  /// </summary>
  private const int MaxEntriesInOneDirectory = 10_000;

  /// <summary>
  /// Таймаут файловой операции в секундах.
  /// </summary>
  private const double FileOperationTimeoutSeconds = 60.0;

  /// <summary>
  /// Количество попыток выполнения файловой операции.
  /// </summary>
  private const int FileOperationMaxRetryAttempts = 10;

  /// <summary>
  /// Задержка между попытками выполнения файловой операции в секундах.
  /// </summary>
  private const double FileOperationRetryDelaySeconds = 4.0;

  /// <summary>
  /// Имя папки с удалёнными файлами.
  /// </summary>
  private const string RecycleBinDirectoryName = "deleted";

  /// <summary>
  /// Пайплайн надёжного выполнения файловых операций.
  /// </summary>
  private static readonly ResiliencePipeline _unresilientFileOperationPipeline;

  /// <summary>
  /// Пайплайн выполнения долгих файловых операций.
  /// </summary>
  private static readonly ResiliencePipeline _longFileOperationPipeline;

  /// <summary>
  /// Статический конструктор.
  /// </summary>
  static FileStorage()
  {
    _unresilientFileOperationPipeline = new ResiliencePipelineBuilder()
      .AddRetry(new RetryStrategyOptions()
      {
        MaxRetryAttempts = FileOperationMaxRetryAttempts,
        Delay = TimeSpan.FromSeconds(FileOperationRetryDelaySeconds),
        ShouldHandle = args => args.Outcome switch
        {
          { Exception: UnauthorizedAccessException } => PredicateResult.True(),
          { Exception: IOException } => PredicateResult.True(),
          _ => PredicateResult.False()
        },
      })
      .Build();

    _longFileOperationPipeline = new ResiliencePipelineBuilder()
      .AddTimeout(TimeSpan.FromSeconds(FileOperationTimeoutSeconds))
      .Build();
  }

  #region IBlobStorage

  /// <summary>
  /// <see cref="IBlobStorage.Test"/>
  /// </summary>
  public void Test()
  {
    if (!fileSystem.IsAvailable())
      throw new StorageTestException("Storage root directory is not exists.");

    // Проверяем наличие прав на запись
    if (mode != BlobStorageMode.ReadOnly)
    {
      try
      {
        string testFileName = Path.GetRandomFileName();
        using var testFileStream = fileSystem.OpenFileStream(testFileName, FileMode.Create, FileAccess.ReadWrite, FileShare.Write, 1, FileOptions.DeleteOnClose);
      }
      catch (UnauthorizedAccessException)
      {
        throw new StorageTestException($"Storage root directory has not permissions to create files.");
      }
    }
  }

  /// <summary>
  /// <see cref="IBlobStorage.Get(Guid)"/>
  /// </summary>
  public BlobInfo? Get(Guid blobId)
  {
    string blobFileName = GetFilePath(blobId);
    return GetBlobInfoByFileName(blobFileName, blobId);
  }

  /// <summary>
  /// <see cref="IBlobStorage.Copy(Guid, Guid, CancellationToken)"/>
  /// </summary>
  public BlobInfo Copy(Guid sourceBlobId, Guid destBlobId, CancellationToken cancellationToken)
  {    
    string destBlobFileName = GetFilePath(destBlobId);
    bool destBlobExists = fileSystem.IsFileExist(destBlobFileName);

    // Если файл целевого блоба уже существует, а хранилище работает в режиме read-only,
    // то мы не можем заменить его содержимое копированием.
    if (destBlobExists)
      CheckStorageAppendPermission(destBlobId);

    // Если исходный блоб не существует, то мы не можем выполнить копирование
    string sourceBlobFileName = GetFilePath(sourceBlobId);
    if (!fileSystem.IsFileExist(sourceBlobFileName))
      throw new BlobNotExistsException(sourceBlobId);

    var readingOptions = new FileStreamOptions
    {
      Mode = FileMode.Open,
    };
    using var sourceStream = fileSystem.OpenFileStream(sourceBlobFileName, readingOptions);

    var writingOptions = new FileStreamOptions
    {
      Mode = destBlobExists ? FileMode.Truncate : FileMode.CreateNew,
      Access = FileAccess.Write,
      Options = FileOptions.WriteThrough,
      BufferSize = 0,
      PreallocationSize = sourceStream.Length,
    };

    // Убедимся, что папка для целевого файла блоба создана
    EnsureDirectoryExists(Path.GetDirectoryName(destBlobFileName)!);

    // Копируем данные в целевой файл блоба
    _longFileOperationPipeline.Execute((ct) =>
    {
      using var destinationStream = fileSystem.OpenFileStream(destBlobFileName, writingOptions);
      sourceStream.CopyTo(destinationStream);
    }, cancellationToken);

    var destBlob = GetBlobInfoByFileName(destBlobFileName, destBlobId);
    return destBlob ?? throw new OperationFailedException("Destination BLOB is not found after was copied.", destBlobId);
  }

  /// <summary>
  /// <see cref="IBlobStorage.Delete(Guid, CancellationToken)"/>
  /// </summary>
  public void Delete(Guid blobId, CancellationToken cancellationToken)
  {
    CheckStorageWritePermission(blobId);

    string deletingBlobFileName = GetFilePath(blobId);
    if (!fileSystem.IsFileExist(deletingBlobFileName))
      throw new BlobNotExistsException(blobId);

    if (useSafeDelete)
    {
      EnsureDirectoryExists(RecycleBinDirectoryName);
      ForceMoveFile(deletingBlobFileName, Path.Combine(RecycleBinDirectoryName, $"{blobId}.blob"), cancellationToken);
    }
    else
    {
      ForceDeleteFile(deletingBlobFileName, cancellationToken);
    }
  }

  /// <summary>
  /// <see cref="IBlobStorage.Restore(Guid, CancellationToken)"/>
  /// </summary>
  public BlobInfo Restore(Guid blobId, CancellationToken cancellationToken)
  {
    CheckStorageWritePermission(blobId);

    // Если отключено безопасное удаление, то мы не можем восстановить файл
    if (!useSafeDelete)
      throw new OperationNotAllowedException(blobId);

    string restoringBlobFileName = Path.Combine(RecycleBinDirectoryName, $"{blobId}.blob");
    if (!fileSystem.IsFileExist(restoringBlobFileName))
      throw new BlobNotExistsException(blobId);

    string restoredBlobFileName = GetFilePath(blobId);

    // Убедимся, что папка для восстанавливаемого файла блоба создана
    EnsureDirectoryExists(Path.GetDirectoryName(restoredBlobFileName)!);

    // Перенесём файл из корзины в папку хранения
    ForceMoveFile(restoringBlobFileName, restoredBlobFileName, cancellationToken);
    var blob = GetBlobInfoByFileName(restoredBlobFileName, blobId);
    return blob ?? throw new OperationFailedException("BLOB is not found after was restored.", blobId);
  }

  /// <summary>
  /// <see cref="IBlobStorage.WriteAsync(Guid, long, long, Stream, CancellationToken)"/>>
  /// </summary>
  public async Task<BlobInfo> WriteAsync(Guid blobId, long offset, long size, Stream dataStream, CancellationToken cancellationToken)
  {
    CheckStorageWritePermission(blobId);

    string blobFileName = GetFilePath(blobId);
    bool blobFileExists = fileSystem.IsFileExist(blobFileName);

    if (!blobFileExists && offset > 0)
      throw new OperationNotAllowedException(blobId);

    // Убедимся, что папка для восстанавливаемого файла блоба создана
    EnsureDirectoryExists(Path.GetDirectoryName(blobFileName)!);

    var writingOptions = new FileStreamOptions
    {
      Mode = blobFileExists ? FileMode.Truncate : FileMode.CreateNew,
      Access = FileAccess.Write,
      Options = FileOptions.WriteThrough,
      BufferSize = 0,
      PreallocationSize = blobFileExists ? 0 : dataStream.Length,
    };

    using var blobFileStream = fileSystem.OpenFileStream(blobFileName, writingOptions);
    await dataStream.CopyToAsync(blobFileStream, cancellationToken);

    var blob = GetBlobInfoByFileName(blobFileName, blobId);
    return blob ?? throw new OperationFailedException("BLOB is not found after was written.", blobId);
  }

  /// <summary>
  /// <see cref="IBlobStorage.ReadAsync(Guid, long, long, CancellationToken)"/>
  /// </summary>
  public async Task<Stream> ReadAsync(Guid blobId, long offset, long size, CancellationToken cancellationToken)
  {
    string blobFileName = GetFilePath(blobId);
    if (!fileSystem.IsFileExist(blobFileName))
      throw new BlobNotExistsException(blobId);

    var readingOptions = new FileStreamOptions
    {
      Mode = FileMode.Open,
    };

    if (offset == 0 && size < 0)
    {
      // Будем читать весь файл
      return fileSystem.OpenFileStream(blobFileName, readingOptions);
    }
    else
    {
      using var blobStream = fileSystem.OpenFileStream(blobFileName, readingOptions);
      blobStream.Position = offset;

      // если размер читаемых данных не указан, то читаем до конца.
      if (size < 0)
        size = blobStream.Length - offset;

      var buffer = new byte[size];
      var resultSize = await blobStream.ReadAsync(buffer.AsMemory(0, (int)size), cancellationToken);
      return new MemoryStream(buffer, 0, resultSize, false, true);
    }
  }

  /// <summary>
  /// <see cref="IBlobStorage.DeleteExpiredBlobs(DateTime, CancellationToken)" />
  /// </summary>
  public IEnumerable<Guid> DeleteExpiredBlobs(DateTime expirationDate, CancellationToken cancellationToken)
  {
    if (fileSystem.IsDirectoryExist("./"))
    {
      foreach (var blobFileName in fileSystem.SearchFiles("./", "*.blob", SearchOption.AllDirectories))
      {
        if (cancellationToken.IsCancellationRequested)
          break;

        var blobModified = fileSystem.GetFileLastWriteTimeUtc(blobFileName);
        if (blobModified < expirationDate)
        {
          if (Guid.TryParse(Path.GetFileNameWithoutExtension(blobFileName), out var blobId))
          {
            ForceDeleteFile(blobFileName, cancellationToken);
            yield return blobId;
          }
        }
      }
    }
  }

  /// <summary>
  /// <see cref="IBlobStorage.EmptyRecycleBin(CancellationToken)" />
  /// </summary>
  public IEnumerable<Guid> EmptyRecycleBin(CancellationToken cancellationToken)
  {
    if (useSafeDelete)
    {
      if (fileSystem.IsDirectoryExist(RecycleBinDirectoryName))
      {
        foreach (var deletingFileName in fileSystem.SearchFiles(RecycleBinDirectoryName, "*.blob", SearchOption.TopDirectoryOnly))
        {
          if (cancellationToken.IsCancellationRequested)
            break;
          if (Guid.TryParse(Path.GetFileNameWithoutExtension(deletingFileName), out var blobId))
          {
            ForceDeleteFile(deletingFileName, cancellationToken);
            yield return blobId;
          }
        }
      }
    }
  }

  /// <summary>
  /// <see cref="IBlobStorage.Truncate(CancellationToken)"/>
  /// </summary>
  public void Truncate(CancellationToken cancellationToken)
  {
    DeleteEmptySubDirectories("./");
  }

  #endregion

  /// <summary>
  /// Переносит файл.
  /// </summary>
  /// <param name="sourceFileName">Имя переносимого файла.</param>
  /// <param name="destFileName">Имя файла в точке назначения.</param>
  /// <param name="cancellationToken">Токен отмены.</param>
  private void ForceMoveFile(string sourceFileName, string destFileName, CancellationToken cancellationToken)
  {
    _unresilientFileOperationPipeline.Execute((ctx) =>
    {
      fileSystem.MoveFile(sourceFileName, destFileName);
    }, cancellationToken);
  }

  /// <summary>
  /// Удаляет файл.
  /// </summary>
  /// <param name="fileName">Имя удаляемого файла.</param>
  /// <param name="cancellationToken">Токен отмены.</param>
  private void ForceDeleteFile(string fileName, CancellationToken cancellationToken)
  {
    try
    {
      _unresilientFileOperationPipeline.Execute((ctx) =>
      {
        fileSystem.DeleteFile(fileName);
      }, cancellationToken);
    }
    catch (DirectoryNotFoundException)
    {
      // ignore
    }
    catch (FileNotFoundException)
    {
      // ignore
    }
  }

  /// <summary>
  /// Проверяет наличие прав на добавление в хранилище.
  /// </summary>
  /// <param name="blobId">Идентификатор BLOB объекта.</param>
  /// <exception cref="BlobReadOnlyException">Хранилище находится в режиме только для чтения.</exception>
  private void CheckStorageAppendPermission(Guid blobId)
  {
    if (mode == BlobStorageMode.ReadOnly)
      throw new BlobReadOnlyException(blobId);
  }

  /// <summary>
  /// Проверяет наличие прав на запись в хранилище.
  /// </summary>
  /// <param name="blobId">Идентификатор BLOB объекта.</param>
  /// <exception cref="BlobReadOnlyException">Хранилище находится в режиме только для чтения.</exception>
  private void CheckStorageWritePermission(Guid blobId)
  {
    if (mode != BlobStorageMode.ReadAndWrite)
      throw new BlobReadOnlyException(blobId);
  }

  /// <summary>
  /// Возвращает информацию по BLOB объекту
  /// </summary>
  /// <param name="blobFileName">Имя файла BLOB'а.</param>
  /// <param name="blobId">Идентификатор BLOB'а.</param>
  /// <returns>Информация по BLOB объекту.</returns>
  private BlobInfo? GetBlobInfoByFileName(string blobFileName, Guid blobId)
  {
    var blobFileInfo = fileSystem.GetFileInfo(blobFileName);
    if (blobFileInfo == null)
      return null;

    return new BlobInfo(new BlobId(blobId), blobFileInfo.Length, blobFileInfo.Created, blobFileInfo.Modified, String.Empty, mode != BlobStorageMode.ReadAndWrite );
  }

  /// <summary>
  /// Возвращает имя файла по идентификатору.
  /// </summary>
  /// <param name="blobId">Идентификатор BLOB'а.</param>
  /// <returns>Путь к файлу с данными.</returns>
  private static string GetFilePath(Guid blobId)
  {
    // Распределяем файлы по подпапкам с целью оптимизации файловых операций.
    // Хорошая практика располагать в одной папке не более 10к файлов.
    // Кроме того, чтобы не создавать очень много подпапок в одной папке, мы разделим каталог на 2 уровня.
    long first64BitsOfId = BitConverter.ToInt64(blobId.ToByteArray(), 0);
    var result = Math.DivRem(
      first64BitsOfId % (MaxEntriesInOneDirectory * MaxEntriesInOneDirectory),
      MaxEntriesInOneDirectory,
      out var remainder);
    return Path.Combine(remainder.ToString(), result.ToString(), $"{blobId}.blob");
  }

  /// <summary>
  /// Убеждается, что директория по указанному пути существует.
  /// </summary>
  /// <param name="directoryPath">Путь к директории.</param>
  /// <remarks>Если директория по указанному пути не существует, она будет создана.</remarks>
  private void EnsureDirectoryExists(string directoryPath)
  {
    if (!fileSystem.IsDirectoryExist(directoryPath))
    {
      fileSystem.CreateDirectory(directoryPath);
    }
  }

  /// <summary>
  /// Удаляет пустые поддиректории.
  /// </summary>
  /// <param name="directoryPath">Родительская папка.</param>
  private void DeleteEmptySubDirectories(string directoryPath)
  {
    foreach (string subDirectoryPath in fileSystem.SearchDirectories(directoryPath))
    {
      if (Path.GetDirectoryName(subDirectoryPath) == RecycleBinDirectoryName)
        continue;

      DeleteEmptySubDirectories(subDirectoryPath);
      if (fileSystem.IsDirectoryEmpty(subDirectoryPath))
        fileSystem.DeleteDirectory(subDirectoryPath, false);
    }
  }
}