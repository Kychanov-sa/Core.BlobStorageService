namespace GlacialBytes.Core.BlobStorageService.Kernel.Exceptions;

/// <summary>
/// Исключение при операции с несуществующим BLOB объектом.
/// </summary>
/// <param name="blobId">Идентификатор BLOB объекта.</param>
public class BlobNotExistsException(Guid blobId) : StorageOperationException($"Operation is not possible. Blob is not exists.", blobId)
{
}
