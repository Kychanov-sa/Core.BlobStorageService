namespace GlacialBytes.Core.BlobStorage.Services.Errors;

/// <summary>
/// Коды ошибок хранилища.
/// </summary>
public class BlobStorageErrorCodes
{
  /// <summary>
  /// Операция некорректна.
  /// </summary>
  public const string InvalidOperation = "ERR_INVALID_OPERATION";

  /// <summary>
  /// Объект не найден.
  /// </summary>
  public const string NotFound = "ERR_BLOB_NOT_FOUND";
}
