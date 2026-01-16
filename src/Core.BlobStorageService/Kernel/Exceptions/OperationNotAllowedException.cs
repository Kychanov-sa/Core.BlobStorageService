namespace GlacialBytes.Core.BlobStorage.Kernel.Exceptions;

/// <summary>
/// Исключение при операции с хранилищем.
/// </summary>
/// <param name="blobId">Идентификатор BLOB объекта.</param>
public class OperationNotAllowedException(Guid blobId) : StorageOperationException("Operation is not allowed. Storage is not in appropriate mode.", blobId)
{
}
