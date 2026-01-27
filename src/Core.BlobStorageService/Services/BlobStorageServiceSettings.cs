using System.ComponentModel.DataAnnotations;

namespace GlacialBytes.Core.BlobStorage.Services;

/// <summary>
/// Настройки хранилища.
/// </summary>
public class BlobStorageServiceSettings
{
  /// <summary>
  /// Идентификатор хранилища.
  /// </summary>
  [Required]
  public required string StorageId { get; set; }

  /// <summary>
  /// Режим работы хранилища.
  /// </summary>
  /// <remarks>Значение по умолчанию Persistent.</remarks>
  public BlobStorageMode StorageMode { get; set; } = BlobStorageMode.Persistent;

  /// <summary>
  /// Включение удаления пустых директорий.
  /// </summary>
  public bool EnableEmptyDirectoriesTruncation { get; set; } = false;

  /// <summary>
  /// Cron-выражение для планирования времени запуска удаления пустых директорий.
  /// </summary>
  /// <remarks>Значение по умолчанию: 1 раз в день в 22:00.</remarks>
  public string? TruncationSchedule { get; set; } = "0 22 * * *";

  /// <summary>
  /// Включение удаления в корзину.
  /// </summary>
  public bool EnableDeleteToRecycleBin { get; set; } = false;

  /// <summary>
  /// Cron-выражение для планирования времени запуска очистки корзины.
  /// </summary>
  /// <remarks>Значение по умолчанию: 1 раз в день в 23:00.</remarks>
  public string? RecyclingSchedule { get; set; } = "0 23 * * *";

  /// <summary>
  /// Включение удаление BLOB объектов с истёкшим сроком.
  /// </summary>
  public bool EnableDeletionOfExpiredBlobs { get; set; } = false;

  /// <summary>
  /// Cron-выражение для планирования времени запуска удаления BLOB объектов с истёкшим сроком.
  /// </summary>
  /// <remarks>Значение по умолчанию: 1 раз в день в 21:00.</remarks>
  public string? DeleteExpiredBlobsSchedule { get; set; } = "0 21 * * *";

  /// <summary>
  /// Время удержания BLOB'ов при временном хранении.
  /// </summary>
  public TimeSpan BlobRetention { get; set; } = TimeSpan.FromDays(3);
}
