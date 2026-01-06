using GlacialBytes.Core.BlobStorageService.Domain.Exceptions;

namespace GlacialBytes.Core.BlobStorageService.Domain;

/// <summary>
/// Интерфейс хранилища.
/// </summary>
public interface IBlobStorage
{
  /// <summary>
  /// Тестирует хранилище.
  /// </summary>
  void Test();

  /// <summary>
  /// Возвращает BLOB объект из хранилища.
  /// </summary>
  /// <param name="blobId">Идентификатор BLOB объекта.</param>
  /// <returns>Информация о BLOB объекте, либо null, если он не найден.</returns>
  BlobInfo? Get(Guid blobId);

  /// <summary>
  /// Копирует BLOB объект.
  /// </summary>
  /// <param name="sourceBlobId">Идентификатор исходного BLOB объекта.</param>
  /// <param name="destBlobId">Идентификатор целевого BLOB объекта.</param>
  /// <returns>Данные скопированного BLOB объекта, либо null, если исходный не найден.</returns>
  BlobInfo? Copy(Guid sourceBlobId, Guid destBlobId);

  /// <summary>
  /// Удаляет BLOB объект.
  /// </summary>
  /// <param name="blobId">Идентификатор BLOB объекта.</param>
  void Delete(Guid blobId);

  /// <summary>
  /// Восстанавливает удалённый BLOB объект.
  /// </summary>
  /// <param name="blobId">Идентификатор BLOB объекта.</param>
  /// <returns>Данные восстановленного BLOB объекта, либо null, если он не найден.</returns>
  BlobInfo? Restore(Guid blobId);

  /// <summary>
  /// Записывает данные в BLOB объект.
  /// </summary>
  /// <param name="blobId">Идентификатор BLOB объекта.</param>
  /// <param name="offset">Смещение для записи данных.</param>
  /// <param name="size">Размер записываемых данных.</param>
  /// <param name="dataStream">Поток данных.</param>
  /// <returns>Информация о записанном BLOB объекте.</returns>  
  /// <exception cref="BlobReadOnlyException">Записываемый объект доступен только для чтения.</exception>
  Task<BlobInfo> WriteAsync(Guid blobId, long offset, int size, Stream dataStream);

  /// <summary>
  /// Читает данные BLOB объекта.
  /// </summary>
  /// <param name="blobId">Идентификатор BLOB объекта.</param>
  /// <param name="offset">Смещение для чтения данных.</param>
  /// <param name="size">Размер читаемых данных.</param>
  /// <returns>Поток для чтения данных.</returns>
  /// <exception cref="BlobNotExistsException">Читаемый объект не найден.</exception>
  Task<Stream> ReadAsync(Guid blobId, long offset, int size);

  /// <summary>
  /// Удаляет все BLOB объекты с истёкшим сроком действия.
  /// </summary>
  /// <param name="expirationDate">Срок действительности BLOB объектов.</param>
  /// <returns>Идентификаторы удаленных объектов.</returns>
  IEnumerable<Guid> DeleteExpiredBlobs(DateTime expirationDate);

  /// <summary>
  /// Очищает удалённые BLOB объекты.
  /// </summary>
  /// <returns>Идентификаторы удалённых объектов.</returns>
  IEnumerable<Guid> EmptyRecycleBin();

  /// <summary>
  /// Удаляет пустые подпапки в хранилище.
  /// </summary>
  void Truncate();
}
