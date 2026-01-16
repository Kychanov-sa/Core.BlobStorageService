using System.ComponentModel.DataAnnotations;

namespace GlacialBytes.Core.BlobStorage.Options;

/// <summary>
/// Настройки хранилища.
/// </summary>
public class BlobStorageSettings
{
  /// <summary>
  /// Идентификатор хранилища.
  /// </summary>
  [Required]
  public required string StorageId { get; set; }

  /// <summary>
  /// Директория для хранения.
  /// </summary>
  [Required]
  public required string StorageDirectory { get; set; }

  /// <summary>
  /// Режим работы хранилища.
  /// </summary>
  /// <remarks>Значение по умолчанию Persistent.</remarks>
  public BlobStorageMode StorageMode { get; set; } = BlobStorageMode.Persistent;

  /// <summary>
  /// Включение усечения пустых директорий.
  /// </summary>
  public bool EnableDirectoriesTruncation { get; set; } = false;

  /// <summary>
  /// Период запуска усечения.
  /// </summary>
  public TimeSpan TruncationPeriod { get; set; } = TimeSpan.FromDays(1);

  /// <summary>
  /// Включение удаления в корзину.
  /// </summary>
  public bool EnableDeleteToRecycleBin { get; set; } = false;

  /// <summary>
  /// Период очистки корзины.
  /// </summary>
  public TimeSpan RecyclingPeriod { get; set; } = TimeSpan.FromDays(1);

  /// <summary>
  /// Количество разделов в хранилище.
  /// </summary>
  [Range(128, 1024)]
  public int PartitionsCount { get; set; } = 1024;

  /// <summary>
  /// Время удержания BLOB'ов при временном хранении.
  /// </summary>
  public TimeSpan? BlobRetention { get; set; } = TimeSpan.FromDays(3);

  /// <summary>
  /// Максимальный размер BLOB'ов в байтах.
  /// </summary>
  [Range(1024, int.MaxValue)]
  public int BlobMaxSizeBytes { get; set; } = 4 * 1024 * 1024;
}
