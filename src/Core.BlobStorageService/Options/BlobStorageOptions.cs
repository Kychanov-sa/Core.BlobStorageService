using System.ComponentModel.DataAnnotations;

namespace GlacialBytes.Core.BlobStorageService.Options;

/// <summary>
/// Опции хранилища.
/// </summary>
public class BlobStorageOptions
{
  /// <summary>
  /// Имя папки хранилища.
  /// </summary>
  [Required]
  public required string StorageDirectoryName { get; set; }

  /// <summary>
  /// Режим работы хранилища.
  /// </summary>
  /// <remarks>Значение по умолчанию Persistent.</remarks>
  public BlobStorageMode StorageMode { get; set; } = BlobStorageMode.Persistent;
}
