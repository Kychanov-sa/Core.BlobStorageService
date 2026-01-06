using GlacialBytes.Core.BlobStorageService.Domain;
using GlacialBytes.Core.BlobStorageService.Exceptions;
using GlacialBytes.Core.BlobStorageService.Services.Results;

namespace GlacialBytes.Core.BlobStorageService.Services;

/// <summary>
/// Сервис хранения BLOB объектов.
/// </summary>
public interface IBlobStorageService
{
  /// <summary>
  /// Возвращает BLOB объект.
  /// </summary>
  /// <param name="blobId">Идентификатор BLOB объекта.</param>
  /// <param name="cancellationToken">Токен отмены.</param>
  /// <returns>Описание BLOB объекта, либо null, если объект не найден.</returns>
  Task<BlobEntity?> GetBlob(Guid blobId, CancellationToken cancellationToken);

  /// <summary>
  /// Копировать бинарные данные.
  /// </summary>
  /// <param name="sourceBlobId">Идентификатор исходного BLOB объекта.</param>
  /// <param name="destBlobId">Идентификатор целевого BLOB объекта.</param>
  /// <param name="cancellationToken">Токен отмены.</param>
  /// <exception cref="BlobNotFoundException">Объект не найден.</exception>
  /// <exception cref="ServiceContractException">Идентификатор целевого и исходного объекта совпадает.</exception>
  Task<CreateBlobResult> CopyBlob(Guid sourceBlobId, Guid destBlobId, CancellationToken cancellationToken);

  /// <summary>
  /// Удалить бинарные данные.
  /// </summary>
  /// <param name="blobId">Идентификатор BLOB объекта.</param>
  /// <param name="cancellationToken">Токен отмены.</param>
  /// <exception cref="BlobNotFoundException">Объект не найден.</exception>
  Task DeleteBlob(Guid blobId, CancellationToken cancellationToken);

  /// <summary>
  /// Отменить удаление бинарных данных.
  /// </summary>
  /// <param name="blobId">Идентификатор BLOB объекта.</param>
  /// <param name="cancellationToken">Токен отмены.</param>
  /// <exception cref="BlobNotFoundException">Объект не найден.</exception>
  /// <exception cref="ServiceContractException">Восстанавливаемый объект не числится среди удалённых.</exception>
  Task<CreateBlobResult> RestoreBlob(Guid blobId, CancellationToken cancellationToken);

  /// <summary>
  /// Записать чанк данных BLOB объекта.
  /// </summary>
  /// <param name="blobId">Идентификатор BLOB объекта.</param>
  /// <param name="offset">Начальная позиция для записи данных, либо -1, если необходимо дописать данные в конец.</param>
  /// <param name="size">Размер чанка записываемых данных, либо -1, если необходимо записать все данные.</param>
  /// <param name="dataStream">Данные для записи.</param>
  /// <param name="cancellationToken">Токен отмены.</param>
  /// <returns>Описание созданного объекта.</returns>
  Task<BlobEntity> WriteBlobChunk(Guid blobId, long offset, int size, Stream dataStream, CancellationToken cancellationToken);

  /// <summary>
  /// Считывает чанк данных BLOB объекта.
  /// </summary>
  /// <param name="blobId">Идентификатор BLOB объекта.</param>
  /// <param name="offset">Начальная позиция для чтения данных.</param>
  /// <param name="size">Размер чанка читаемых данных, либо -1, если необходимо прочитать все данные.</param>
  /// <param name="cancellationToken">Токен отмены.</param>
  /// <exception cref="BlobNotFoundException">Объект не найден.</exception>
  /// <returns>Поток с данными.</returns>
  Task<Stream> ReadBlobChunk(Guid blobId, long offset, int size, CancellationToken cancellationToken);
}
