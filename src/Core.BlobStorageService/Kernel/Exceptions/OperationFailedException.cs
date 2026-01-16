namespace GlacialBytes.Core.BlobStorage.Kernel.Exceptions;

/// <summary>
/// Исключение при ошибке выполнения операции.
/// </summary>
/// <param name="message">Сообщение об ошибке.</param>
/// <param name="blobId">Идентификатор BLOB объекта.</param>
public class OperationFailedException(string message, Guid blobId) : StorageOperationException(message, blobId)
{
}
