using GlacialBytes.Core.BlobStorage.Kernel.Exceptions;

namespace GlacialBytes.Core.BlobStorage.Kernel;

/// <summary>
/// Интерфейс хранилища.
/// </summary>
public interface IBlobStorage
{
  /// <summary>
  /// Тестирует хранилище.
  /// </summary>
  /// <exception cref="StorageTestException">Ошибка при проверке хранилища.</exception>
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
  /// <param name="cancellationToken">Токен отмены.</param>
  /// <returns>Данные скопированного BLOB объекта, либо null, если исходный не найден.</returns>
  /// <exception cref="BlobNotExistsException">Исходный BLOB не существует.</exception>
  /// <exception cref="OperationFailedException">Целевой BLOB не был найден после выполнения копирования.</exception>
  /// <exception cref="OperationNotAllowedException">Выполнение операции не допустимо.</exception>
  BlobInfo Copy(Guid sourceBlobId, Guid destBlobId, CancellationToken cancellationToken);

  /// <summary>
  /// Удаляет BLOB объект.
  /// </summary>
  /// <param name="blobId">Идентификатор BLOB объекта.</param>
  /// <param name="cancellationToken">Токен отмены.</param>
  /// <exception cref="BlobNotExistsException">Удаляемый BLOB не существует.</exception>
  /// <exception cref="OperationNotAllowedException">Выполнение операции не допустимо.</exception>
  void Delete(Guid blobId, CancellationToken cancellationToken);

  /// <summary>
  /// Восстанавливает удалённый BLOB объект.
  /// </summary>
  /// <param name="blobId">Идентификатор BLOB объекта.</param>
  /// <param name="cancellationToken">Токен отмены.</param>
  /// <returns>Данные восстановленного BLOB объекта, либо null, если он не найден.</returns>
  /// <exception cref="OperationFailedException">Хранилище не поддерживает восстановление, либо BLOB не был обнаружен после восстановления.</exception>
  /// <exception cref="BlobNotExistsException">Восстанавливаемый BLOB не существует.</exception>
  /// <exception cref="OperationNotAllowedException">Выполнение операции не допустимо.</exception>
  BlobInfo Restore(Guid blobId, CancellationToken cancellationToken);

  /// <summary>
  /// Записывает данные в BLOB объект.
  /// </summary>
  /// <param name="blobId">Идентификатор BLOB объекта.</param>
  /// <param name="offset">Смещение для записи данных.</param>
  /// <param name="size">Размер записываемых данных.</param>
  /// <param name="dataStream">Поток данных.</param>
  /// <param name="cancellationToken">Токен отмены.</param>
  /// <returns>Информация о записанном BLOB объекте.</returns>  
  /// <exception cref="OperationNotAllowedException">Выполнение операции не допустимо.</exception>
  Task<BlobInfo> WriteAsync(Guid blobId, long offset, long size, Stream dataStream, CancellationToken cancellationToken);

  /// <summary>
  /// Читает данные BLOB объекта.
  /// </summary>
  /// <param name="blobId">Идентификатор BLOB объекта.</param>
  /// <param name="offset">Смещение для чтения данных.</param>
  /// <param name="size">Размер читаемых данных.</param>
  /// <param name="cancellationToken">Токен отмены.</param>
  /// <returns>Поток для чтения данных.</returns>
  /// <exception cref="BlobNotExistsException">Читаемый объект не найден.</exception>
  Task<Stream> ReadAsync(Guid blobId, long offset, long size, CancellationToken cancellationToken);

  /// <summary>
  /// Удаляет все BLOB объекты с истёкшим сроком действия.
  /// </summary>
  /// <param name="expirationDate">Срок действительности BLOB объектов.</param>
  /// <param name="cancellationToken">Токен отмены.</param>
  /// <returns>Идентификаторы удаленных объектов.</returns>
  /// <exception cref="OperationNotAllowedException">Выполнение операции не допустимо.</exception>
  IEnumerable<Guid> DeleteExpiredBlobs(DateTime expirationDate, CancellationToken cancellationToken);

  /// <summary>
  /// Очищает удалённые BLOB объекты.
  /// </summary>
  /// <param name="cancellationToken">Токен отмены.</param>
  /// <returns>Идентификаторы удалённых объектов.</returns>
  IEnumerable<Guid> EmptyRecycleBin(CancellationToken cancellationToken);

  /// <summary>
  /// Удаляет пустые подпапки в хранилище.
  /// </summary>
  /// <param name="cancellationToken">Токен отмены.</param>
  void Truncate(CancellationToken cancellationToken);
}
