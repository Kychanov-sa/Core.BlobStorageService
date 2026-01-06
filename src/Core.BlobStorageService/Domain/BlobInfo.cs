namespace GlacialBytes.Core.BlobStorageService.Domain;

/// <summary>
/// Информация о BLOB объекте.
/// </summary>
/// <param name="Id">Идентификатор.</param>
/// <param name="Length">Длина данных объекта.</param>
/// <param name="Created">Дата создания.</param>
/// <param name="Modified">Дата изменения.</param>
/// <param name="Md5Hash">Хэш данных объекта.</param>
/// <param name="IsReadOnly">Признак объекта, доступного только для чтения.</param>
public record BlobInfo(Guid Id, long Length, DateTime Created, DateTime Modified, string Md5Hash, bool IsReadOnly);
