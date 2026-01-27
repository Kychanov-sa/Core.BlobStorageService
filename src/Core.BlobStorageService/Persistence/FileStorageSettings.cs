using System.ComponentModel.DataAnnotations;

namespace GlacialBytes.Core.BlobStorage.Persistence;

/// <summary>
/// Настройки файлового хранилища.
/// </summary>
public class FileStorageSettings
{
  /// <summary>
  /// Директория для хранения.
  /// </summary>
  [Required]
  public required string StorageDirectory { get; set; }

  /// <summary>
  /// Количество разделов в хранилище.
  /// </summary>
  [Range(128, 1024)]
  public int PartitionsCount { get; set; } = 1024;

  /// <summary>
  /// Максимальный размер BLOB'ов в байтах.
  /// </summary>
  [Range(1024, int.MaxValue)]
  public int BlobMaxSizeBytes { get; set; } = 4 * 1024 * 1024;
}
