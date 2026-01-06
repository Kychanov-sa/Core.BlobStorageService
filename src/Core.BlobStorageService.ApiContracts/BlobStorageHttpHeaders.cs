using System.Net.Http.Headers;

namespace GlacialBytes.Core.BlobStorageService;

/// <summary>
/// HTTP заголовки хранилища BLOB объектов.
/// </summary>
public static class BlobStorageHttpHeaders
{
  /// <summary>
  /// Дата создания BLOB объекта.
  /// </summary>
  public const string BlobCreated = "X-Core-Created";

  /// <summary>
  /// Дата изменения BLOB объекта.
  /// </summary>
  public const string BlobModified = "X-Core-Modified";

  /// <summary>
  /// Возвращает дату создания BLOB объекта из заголовков HTTP ответа.
  /// </summary>
  /// <param name="headers">Заголовки HTTP ответа.</param>
  /// <returns>Дата создания BLOB объекта или null, если соответствующий заголовок не обнаружен в ответе.</returns>
  internal static DateTime? GetBlobCreated(this HttpResponseHeaders headers)
  {
    string? blobCreatedHeader = headers.GetValues(BlobCreated).FirstOrDefault();
    if (DateTime.TryParse(blobCreatedHeader, out var blobCreated))
      return blobCreated;
    return null;
  }

  /// <summary>
  /// Возвращает дату изменения BLOB объекта из заголовков HTTP ответа.
  /// </summary>
  /// <param name="headers">Заголовки HTTP ответа.</param>
  /// <returns>Дата изменения BLOB объекта или null, если соответствующий заголовок не обнаружен в ответе.</returns>
  internal static DateTime? GetBlobModified(this HttpResponseHeaders headers)
  {
    string? blobModifiedHeader = headers.GetValues(BlobModified).FirstOrDefault();
    if (DateTime.TryParse(blobModifiedHeader, out var blobModified))
      return blobModified;
    return null;
  }
}
