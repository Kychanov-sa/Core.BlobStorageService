namespace GlacialBytes.Core.BlobStorage.Kernel.Exceptions;

/// <summary>
/// Исключение при операции с хранилищем.
/// </summary>
/// <param name="message">Текст сообщения.</param>
/// <param name="blobId">Идентификатор BLOB объекта.</param>
public class StorageOperationException(string message, Guid blobId) : Exception(message)
{
  /// <summary>
  /// Идентификатор BLOB объекта.
  /// </summary>
  public Guid BlobId { get; } = blobId;
}
