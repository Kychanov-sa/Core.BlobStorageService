using GlacialBytes.Core.BlobStorageService.Services.Errors;

namespace GlacialBytes.Core.BlobStorageService.Services.Results;

/// <summary>
/// Результат чтения BLOB объекта.
/// </summary>
public class ReadBlobResult : BlobStorageResult
{
  /// <summary>
  /// Поток читаемых данных.
  /// </summary>
  public Stream? DataStream { get; init; }

  /// <summary>
  /// Возвращает результат с успешным выполнением операции.
  /// </summary>
  /// <param name="dataStream"> Поток читаемых данных.</param>
  public static ReadBlobResult Success(Stream dataStream) => new() { Succeeded = true, DataStream = dataStream, };

  /// <summary>
  /// Создает результат с ошибками.
  /// </summary>
  /// <param name="errors">Ошибки.</param>
  /// <returns>Результат.</returns>
  public static new ReadBlobResult Failed(params BlobStorageError[] errors)
  {
    var result = new ReadBlobResult { Succeeded = false };
    if (errors != null)
      result.Errors.AddRange(errors);
    return result;
  } 
}
