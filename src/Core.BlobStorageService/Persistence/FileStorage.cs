using GlacialBytes.Core.BlobStorageService.Domain;
using GlacialBytes.Core.BlobStorageService.Domain.Exceptions;
using System.IO;

namespace GlacialBytes.Core.BlobStorageService.Persistence;

/// <summary>
/// Файловое хранилище.
/// </summary>
internal class FileStorage(string rootPath, bool isReadOnly, bool useSafeDelete)
  : IBlobStorage
{
  /// <summary>
  /// Путь к папке с удалёнными файлами.
  /// </summary>
  private readonly string _basketPath = Path.Combine(rootPath, "deleted");

  #region IBlobStorage

  /// <summary>
  /// <see cref="IBlobStorage.Test"/>
  /// </summary>
  public void Test()
  {
    if (!Directory.Exists(rootPath))
      throw new StorageTestException("Storage root directory is not exists.");

    // Проверяем наличие прав на запись
    if (!isReadOnly)
    {
      try
      {
        string testFileName = Path.Combine(rootPath, Path.GetRandomFileName());
        using var testFileStream = new FileStream(testFileName, FileMode.Create, FileAccess.ReadWrite, FileShare.Write, 1, FileOptions.DeleteOnClose);
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
    return GetBlobByFileName(blobFileName, blobId);
  }  

  /// <summary>
  /// <see cref="IBlobStorage.Copy(Guid, Guid)"/>
  /// </summary>
  public BlobInfo? Copy(Guid sourceBlobId, Guid destBlobId)
  {
    string destBlobFileName = GetFilePath(destBlobId);
    bool destBlobExists = File.Exists(destBlobFileName);

    // Если файл целевого блоба уже существует, а хранилище работает в режиме read-only,
    // то мы не можем заменить его содержимое копированием.
    if (destBlobExists)
      CheckStorageWritePermission(destBlobId);

    // Если исходный блоб не существует, то мы не можем выполнить копирование
    string sourceBlobFileName = GetFilePath(sourceBlobId);
    if (!File.Exists(sourceBlobFileName))
      throw new BlobNotExistsException(sourceBlobId);

    var readingOptions = new FileStreamOptions
    {
      Mode = FileMode.Open,
    };
    using FileStream sourceStream = new(sourceBlobFileName, readingOptions);

    var writingOptions = new FileStreamOptions
    {
      Mode = destBlobExists ? FileMode.CreateNew : FileMode.Truncate,
      Access = FileAccess.Write,
      Options = FileOptions.WriteThrough,
      BufferSize = 0,
      PreallocationSize = sourceStream.Length,
    };

    // Убедимся, что папка для целевого файла блоба создана
    EnsureDirectoryExists(Path.GetDirectoryName(destBlobFileName)!);

    // Копируем данные в целевой файл блоба
    using FileStream destinationStream = new(destBlobFileName, writingOptions);
    sourceStream.CopyTo(destinationStream);

    return GetBlobByFileName(destBlobFileName, destBlobId);
  }

  /// <summary>
  /// <see cref="IBlobStorage.Delete(Guid)"/>
  /// </summary>
  public void Delete(Guid blobId)
  {
    CheckStorageWritePermission(blobId);

    string deletingBlobFileName = GetFilePath(blobId);
    if (File.Exists(deletingBlobFileName))
      throw new BlobNotExistsException(blobId);

    if (useSafeDelete)
    {
      EnsureDirectoryExists(_basketPath);
      File.Move(deletingBlobFileName, Path.Combine(_basketPath, $"{blobId}.blob"));
    }
    else
    {
      ForceFileDelete(deletingBlobFileName);
    }
  }

  /// <summary>
  /// <see cref="IBlobStorage.Restore(Guid)"/>
  /// </summary>
  public BlobInfo? Restore(Guid blobId)
  {
    CheckStorageWritePermission(blobId);

    // Если отключено безопасное удаление, то мы не можем восстановить файл
    if (useSafeDelete)
      throw new InvalidOperationException("Storage is not supporting restoring operations.");

    string restoringBlobFileName = Path.Combine(_basketPath, $"{blobId}.blob");
    if (!File.Exists(restoringBlobFileName))
      throw new BlobNotExistsException(blobId);

    string restoredBlobFileName = GetFilePath(blobId);

    // Убедимся, что папка для восстанавливаемого файла блоба создана
    EnsureDirectoryExists(Path.GetDirectoryName(restoredBlobFileName)!);

    // Перенесём файл из корзины в папку хранения
    File.Move(restoringBlobFileName, restoredBlobFileName);
    return GetBlobByFileName(restoredBlobFileName, blobId);
  }

  /// <summary>
  /// Записывает данные в BLOB объект.
  /// </summary>
  /// <param name="blobId">Идентификатор BLOB объекта.</param>
  /// <param name="offset">Смещение для записи данных.</param>
  /// <param name="size">Размер записываемых данных.</param>
  /// <param name="dataStream">Поток данных.</param>
  /// <returns>Информация о записанном BLOB объекте.</returns>  
  /// <exception cref="BlobReadOnlyException">Записываемый объект доступен только для чтения.</exception>
  Task<BlobInfo> WriteAsync(Guid blobId, long offset, int size, Stream dataStream)
  {

  }

  /// <summary>
  /// Читает данные BLOB объекта.
  /// </summary>
  /// <param name="blobId">Идентификатор BLOB объекта.</param>
  /// <param name="offset">Смещение для чтения данных.</param>
  /// <param name="size">Размер читаемых данных.</param>
  /// <returns>Поток для чтения данных.</returns>
  /// <exception cref="BlobNotExistsException">Читаемый объект не найден.</exception>
  Task<Stream> ReadAsync(Guid blobId, long offset, int size)
  {

  }

  /// <summary>
  /// <see cref="IBlobStorage.DeleteExpiredBlobs(DateTime)" />
  /// </summary>
  IEnumerable<Guid> DeleteExpiredBlobs(DateTime expirationDate)
  {
    if (Directory.Exists(rootPath))
    {
      foreach (var blobFileName in Directory.GetFiles(rootPath, "*.blob", SearchOption.AllDirectories))
      {
        var blobModified = File.GetLastWriteTimeUtc(blobFileName);
        if (blobModified > expirationDate)
        {
          if (Guid.TryParse(Path.GetFileNameWithoutExtension(blobFileName), out var blobId))
          {
            ForceFileDelete(blobFileName);
            yield return blobId;
          }
        }
      }
    }
  }

  /// <summary>
  /// <see cref="IBlobStorage.EmptyRecycleBin" />
  /// </summary>
  public IEnumerable<Guid> EmptyRecycleBin()
  {
    if (useSafeDelete)
    {
      if (Directory.Exists(_basketPath))
      {
        foreach (var deletingFileName in Directory.EnumerateFiles(_basketPath))
        {
          if (Guid.TryParse(Path.GetFileNameWithoutExtension(deletingFileName), out var blobId))
          {
            ForceFileDelete(deletingFileName);
            yield return blobId;
          }
        }
      }
    }
  }

  /// <summary>
  /// <see cref="IBlobStorage.Truncate()"/>
  /// </summary>
  public void Truncate()
  {
    DeleteEmptySubDirectories(rootPath);
  }

  #endregion

  private static void ForceFileDelete(string fileName, bool retryOnFailure = true)
  {
    try
    {
      File.Delete(fileName);
    }
    catch (DirectoryNotFoundException)
    {
      // ignore
    }
    catch (UnauthorizedAccessException)
    {
      if (!retryOnFailure)
        return;

      var currentAttributes = File.GetAttributes(fileName);
      var newAttributes = currentAttributes & ~FileAttributes.ReadOnly;
      if (currentAttributes != newAttributes)
        File.SetAttributes(fileName, newAttributes);

      ForceFileDelete(fileName, false);
    }
    catch (IOException)
    {
      if (!retryOnFailure)
        return;

      Thread.Sleep(DeleteFileNextAttemptDelay);
      File.Delete(fileName);
    }
  }

  /// <summary>
  /// Проверяет наличие прав на запись в хранилище.
  /// </summary>
  /// <param name="blobId">Идентификатор BLOB объекта.</param>
  /// <exception cref="BlobReadOnlyException">Хранилище находится в режиме только для чтения.</exception>
  private void CheckStorageWritePermission(Guid blobId)
  {
    if (isReadOnly)
      throw new BlobReadOnlyException(blobId);
  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="blobFileName"></param>
  /// <param name="blobId"></param>
  /// <returns></returns>
  private BlobInfo? GetBlobByFileName(string blobFileName, Guid blobId)
  {
    var blobFileInfo = new FileInfo(blobFileName);
    if (!blobFileInfo.Exists)
      return null;

    return new BlobInfo(blobId, blobFileInfo.Length, blobFileInfo.CreationTimeUtc, blobFileInfo.LastWriteTimeUtc, String.Empty, isReadOnly);
  }

  /// <summary>
  /// Возвращает имя файла по идентификатору.
  /// </summary>
  /// <param name="blobId">Идентификатор BLOB'а.</param>
  /// <returns>Путь к файлу с данными.</returns>
  private string GetFilePath(Guid blobId)
  {
    // Распределяем файлы по подпапкам с целью оптимизации файловых операций.
    // В одной подпапке будут хранится все файлы, имя которых, конвертированное в int64, дает один и тот же остаток от деления на 10000.
    var result = Math.DivRem(BitConverter.ToInt64(blobId.ToByteArray(), 0) % 100000000, 10000, out var remainder);
    return Path.Combine(rootPath, remainder.ToString(), result.ToString(), $"{blobId}.blob");
  }

  /// <summary>
  /// Убеждается, что директория по указанному пути существует.
  /// </summary>
  /// <param name="directoryPath">Путь к директории.</param>
  /// <remarks>Если директория по указанному пути не существует, она будет создана.</remarks>
  private static void EnsureDirectoryExists(string directoryPath)
  {
    if (!Directory.Exists(directoryPath))
    {
      Directory.CreateDirectory(directoryPath);
    }
  }

  /// <summary>
  /// Удаляет пустые поддиректории.
  /// </summary>
  /// <param name="directoryPath">Родительская папка.</param>
  private static void DeleteEmptySubDirectories(string directoryPath)
  {
    foreach (string subDirectoryPath in Directory.GetDirectories(directoryPath))
    {
      DeleteEmptySubDirectories(subDirectoryPath);
      if (!Directory.EnumerateFileSystemEntries(subDirectoryPath).Any())
        Directory.Delete(subDirectoryPath, false);
    }
  }

  ///// <summary>
  ///// Получить список пустых подпапок для указанной папки.
  ///// </summary>
  ///// <param name="directoryToSearch">Полный путь к папке для поиска пустых папок.</param>
  ///// <returns>Список пустых подпапок.</returns>
  ///// <remarks>
  ///// Подпапка считается пустой, если не содержит файлов, в том числе в своих подпапках на любом уровне вложенности.
  ///// </remarks>
  //public static IEnumerable<string> GetEmptySubdirectories(string directoryToSearch)
  //{
  //  var directoryToSearchInfo = new DirectoryInfo(directoryToSearch);
  //  if (!directoryToSearchInfo.Exists)
  //    return Enumerable.Empty<string>();

  //  var subdirectories = directoryToSearchInfo.GetDirectories("*", SearchOption.TopDirectoryOnly);

  //  return subdirectories
  //    .Where(dir => !dir.GetFiles("*", SearchOption.AllDirectories).Any())
  //    .Select(dir => dir.FullName)
  //    .ToList();
  //}
}