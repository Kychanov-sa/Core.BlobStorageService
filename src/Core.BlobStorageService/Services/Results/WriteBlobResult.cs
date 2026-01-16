using GlacialBytes.Core.BlobStorage.Services.Errors;

namespace GlacialBytes.Core.BlobStorage.Services.Results;

/// <summary>
/// Результат записи BLOB объекта.
/// </summary>
public class WriteBlobResult : BlobStorageResult
{
  /// <summary>
  /// Дата изменения.
  /// </summary>
  public DateTime? Modified { get; init; }

  /// <summary>
  /// Хэш объекта.
  /// </summary>
  public string? Hash { get; init; }

  /// <summary>
  /// Возвращает результат с успешным выполнением операции.
  /// </summary>
  /// <param name="modified">Дата изменения.</param>
  /// <param name="hash">Хэш объекта.</param>
  public static WriteBlobResult Success(DateTime modified, string? hash) => new() { Succeeded = true, Modified = modified, Hash = hash, };

  /// <summary>
  /// Создает результат с ошибками.
  /// </summary>
  /// <param name="errors">Ошибки.</param>
  /// <returns>Результат.</returns>
  public static new WriteBlobResult Failed(params BlobStorageError[] errors)
  {
    var result = new WriteBlobResult { Succeeded = false };
    if (errors != null)
      result.Errors.AddRange(errors);
    return result;
  } 
}
