using GlacialBytes.Core.BlobStorage.Services.Errors;

namespace GlacialBytes.Core.BlobStorage.Services.Results;

/// <summary>
/// Результат операции в хранилище BLOB объектов.
/// </summary>
public class BlobStorageResult
{
  /// <summary>
  /// Список ошибок.
  /// </summary>
  public List<BlobStorageError> Errors = [];

  /// <summary>
  /// Успешость операции.
  /// </summary>
  public bool Succeeded { get; init; }

  /// <summary>
  /// Возвращает результат с успешным выполнением операции.
  /// </summary>
  public static BlobStorageResult Success() => new() { Succeeded = true };

  /// <summary>
  /// Создает результат с ошибками.
  /// </summary>
  /// <param name="errors">Ошибки.</param>
  /// <returns>Результат.</returns>
  public static BlobStorageResult Failed(params BlobStorageError[] errors)
  {
    var result = new BlobStorageResult { Succeeded = false };
    if (errors != null)
      result.Errors.AddRange(errors);
    return result;
  }
}
