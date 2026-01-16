namespace GlacialBytes.Core.BlobStorage.Kernel.Exceptions;

/// <summary>
/// Исключение при операции с хранилищем в режиме только для чтения.
/// </summary>
/// <param name="blobId">Идентификатор BLOB объекта.</param>
public class BlobReadOnlyException(Guid blobId) : StorageOperationException($"Operation is not permitted. Storage is in read-only mode.", blobId)
{
}
