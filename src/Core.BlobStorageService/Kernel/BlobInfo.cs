using StronglyTypedIds;

namespace GlacialBytes.Core.BlobStorageService.Kernel;

/// <summary>
/// Идентификатор BLOB объекта.
/// </summary>
[StronglyTypedId]
public readonly partial struct BlobId { };

/// <summary>
/// Информация о BLOB объекте.
/// </summary>
/// <param name="Id">Идентификатор.</param>
/// <param name="Length">Длина данных объекта.</param>
/// <param name="Created">Дата создания.</param>
/// <param name="Modified">Дата изменения.</param>
/// <param name="Hash">Хэш данных объекта.</param>
/// <param name="IsReadOnly">Признак объекта, доступного только для чтения.</param>
public record BlobInfo(BlobId Id, long Length, DateTime Created, DateTime Modified, string Hash, bool IsReadOnly);
