using GlacialBytes.Core.BlobStorage.Services.Errors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace GlacialBytes.Core.BlobStorage.Endpoints;

/// <summary>
/// Конвертер ошибок сервиса.
/// </summary>
public static class ServiceErrorConverter
{
  /// <summary>
  /// Сообщение о неожиданной ошибке.
  /// </summary>
  private const string UnexpectedError = "An unexpected error has occurred.";

  /// <summary>
  /// Конвертирует ошибку хранилиза в объект ProblemDetails.
  /// </summary>
  /// <param name="error">Ошибка хранилища.</param>
  /// <returns>Описение проблемы.</returns>
  public static ProblemDetails ToProblemDetails(this BlobStorageError error)
  {
    int statusCode = error.Code switch
    {
      BlobStorageErrorCodes.InvalidOperation => StatusCodes.Status400BadRequest,
      BlobStorageErrorCodes.NotFound => StatusCodes.Status404NotFound,
      _ => StatusCodes.Status500InternalServerError,
    };

    var reasonPhrase = ReasonPhrases.GetReasonPhrase(statusCode);
    if (string.IsNullOrEmpty(reasonPhrase))
    {
      reasonPhrase = UnexpectedError;
    }
    
    var problemDetails = new ProblemDetails
    {
      Status = statusCode,
      Title = reasonPhrase,
      Detail = error.Description,
      Extensions =
      {
        ["ErrorCode"] = error.Code,
      },
    };

    return problemDetails;
  }
}
