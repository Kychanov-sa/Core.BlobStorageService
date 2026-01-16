namespace GlacialBytes.Core.BlobStorage.Persistence;

/// <summary>
/// Информация по локальному файлу.
/// </summary>
/// <param name="Name">Имя файла.</param>
/// <param name="Length">Размер файла.</param>
/// <param name="Created">Дата создания.</param>
/// <param name="Modified">Дата изменения.</param>
public record FileInfo(string Name, long Length, DateTime Created, DateTime Modified);
