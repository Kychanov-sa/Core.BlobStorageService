namespace GlacialBytes.Core.BlobStorageService.Options;

/// <summary>
/// Валидатор опции хранилища.
/// </summary>
public class BlobStorageOptionsValidator
{
  /// <summary>
  /// Имя папки хранилища.
  /// </summary>
  public string StorageDirectoryName { get; set; }

  /// <summary>
  /// Режим работы хранилища.
  /// </summary>
  public BlobStorageMode StorageMode { get; set; }
}
