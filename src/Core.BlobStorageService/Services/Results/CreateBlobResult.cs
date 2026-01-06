using GlacialBytes.Core.BlobStorageService.Services.Errors;

namespace GlacialBytes.Core.BlobStorageService.Services.Results;

/// <summary>
/// Результат создания BLOB объекта.
/// </summary>
public class CreateBlobResult : BlobStorageResult
{
  /// <summary>
  /// Дата создания.
  /// </summary>
  public DateTime? Created { get; init; }

  /// <summary>
  /// Хэш объекта.
  /// </summary>
  public string? Hash { get; init; }

  /// <summary>
  /// Возвращает результат с успешным выполнением операции.
  /// </summary>
  public static CreateBlobResult Success(DateTime created, string? hash) => new() { Succeeded = true, Created = created, Hash = hash, };

  /// <summary>
  /// Создает результат с ошибками.
  /// </summary>
  /// <param name="errors">Ошибки.</param>
  /// <returns>Результат.</returns>
  public static new CreateBlobResult Failed(params BlobStorageError[] errors)
  {
    var result = new CreateBlobResult { Succeeded = false };
    if (errors != null)
      result.Errors.AddRange(errors);
    return result;
  }
}
