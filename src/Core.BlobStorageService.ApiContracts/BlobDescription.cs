namespace GlacialBytes.Core.BlobStorageService;

/// <summary>
/// Описание BLOB объекта.
/// </summary>
public class BlobDescription
{
  /// <summary>
  /// Идентификатор.
  /// </summary>
  public Guid Id { get; set; }

  /// <summary>
  /// Длина данных объекта.
  /// </summary>
  public long Length { get; set; }

  /// <summary>
  /// Дата создания.
  /// </summary>
  public DateTime? Created { get; set; }

  /// <summary>
  /// Дата изменения.
  /// </summary>
  public DateTime? Modified { get; set; }

  /// <summary>
  /// Хэш данных объекта.
  /// </summary>
  public string? Hash { get; set; }

  /// <summary>
  /// Признак объекта, доступного только для чтения.
  /// </summary>
  public bool IsReadOnly { get; set; }
}
