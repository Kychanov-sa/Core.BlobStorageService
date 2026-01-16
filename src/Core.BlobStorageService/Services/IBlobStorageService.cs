using GlacialBytes.Core.BlobStorage.Kernel;
using GlacialBytes.Core.BlobStorage.Exceptions;
using GlacialBytes.Core.BlobStorage.Services.Results;

namespace GlacialBytes.Core.BlobStorage.Services;

/// <summary>
/// Сервис хранения BLOB объектов.
/// </summary>
public interface IBlobStorageService
{
  /// <summary>
  /// Признак архивного хранилища.
  /// </summary>
  bool IsArchiveStorage { get; }

  /// <summary>
  /// Возвращает BLOB объект.
  /// </summary>
  /// <param name="blobId">Идентификатор BLOB объекта.</param>
  /// <returns>Описание BLOB объекта, либо null, если объект не найден.</returns>
  BlobInfo? FindBlob(BlobId blobId);

  /// <summary>
  /// Копировать бинарные данные.
  /// </summary>
  /// <param name="sourceBlobId">Идентификатор исходного BLOB объекта.</param>
  /// <param name="destBlobId">Идентификатор целевого BLOB объекта.</param>
  /// <param name="cancellationToken">Токен отмены.</param>
  /// <exception cref="BlobNotFoundException">Объект не найден.</exception>
  /// <exception cref="ServiceContractException">Идентификатор целевого и исходного объекта совпадает.</exception>
  /// <returns>Результат копирования объекта.</returns>
  Task<CreateBlobResult> CopyBlob(BlobId sourceBlobId, BlobId destBlobId, CancellationToken cancellationToken);

  /// <summary>
  /// Удалить бинарные данные.
  /// </summary>
  /// <param name="blobId">Идентификатор BLOB объекта.</param>
  /// <param name="cancellationToken">Токен отмены.</param>
  /// <exception cref="BlobNotFoundException">Объект не найден.</exception>
  Task DeleteBlob(BlobId blobId, CancellationToken cancellationToken);

  /// <summary>
  /// Отменить удаление бинарных данных.
  /// </summary>
  /// <param name="blobId">Идентификатор BLOB объекта.</param>
  /// <param name="cancellationToken">Токен отмены.</param>
  /// <exception cref="BlobNotFoundException">Объект не найден.</exception>
  /// <exception cref="ServiceContractException">Восстанавливаемый объект не числится среди удалённых.</exception>
  /// <returns>Результат восстановления объекта.</returns>
  Task<CreateBlobResult> RestoreBlob(BlobId blobId, CancellationToken cancellationToken);

  /// <summary>
  /// Записать чанк данных BLOB объекта.
  /// </summary>
  /// <param name="blobId">Идентификатор BLOB объекта.</param>
  /// <param name="offset">Начальная позиция для записи данных, либо -1, если необходимо дописать данные в конец.</param>
  /// <param name="size">Размер чанка записываемых данных, либо -1, если необходимо записать все данные.</param>
  /// <param name="dataStream">Данные для записи.</param>
  /// <param name="cancellationToken">Токен отмены.</param>
  /// <returns>Результат записи объекта.</returns>
  Task<WriteBlobResult> WriteBlobChunk(BlobId blobId, long offset, long size, Stream dataStream, CancellationToken cancellationToken);

  /// <summary>
  /// Считывает чанк данных BLOB объекта.
  /// </summary>
  /// <param name="blobId">Идентификатор BLOB объекта.</param>
  /// <param name="offset">Начальная позиция для чтения данных.</param>
  /// <param name="size">Размер чанка читаемых данных, либо -1, если необходимо прочитать все данные.</param>
  /// <param name="cancellationToken">Токен отмены.</param>
  /// <exception cref="BlobNotFoundException">Объект не найден.</exception>
  /// <returns>Результат чтения данных.</returns>
  Task<ReadBlobResult> ReadBlobChunk(BlobId blobId, long offset, long size, CancellationToken cancellationToken);
}
