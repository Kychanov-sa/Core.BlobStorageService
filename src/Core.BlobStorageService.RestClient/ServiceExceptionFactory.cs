using GlacialBytes.Core.BlobStorageService.Exceptions;
using Refit;
using System.Text.Json;
using System.Net.Http.Json;
using System.Net;

namespace GlacialBytes.Core.BlobStorageService;

/// <summary>
/// Фабрика исключений сервиса.
/// </summary>
internal static class ServiceExceptionFactory
{
  /// <summary>
  /// Создаёт исключение сервиса из сообщения ответа.
  /// </summary>
  /// <param name="httpResponseMessage">Сообщение HTTP ответа.</param>
  /// <param name="cancellationToken">Токен отмены.</param>
  /// <returns>Исключение сервиса.</returns>
  public static async Task<Exception> CreateServiceExceptionFromResponseMessage(HttpResponseMessage httpResponseMessage, CancellationToken cancellationToken = default)
  {
    if (!httpResponseMessage.IsSuccessStatusCode)
    {
      var problemDetails = await httpResponseMessage.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken);
      return CreateException(problemDetails, httpResponseMessage.StatusCode);
    }
    throw new InvalidOperationException();
  }

  /// <summary>
  /// Создаёт исключение сервиса из исключения API.
  /// </summary>
  /// <param name="exception">Исключение API.</param>
  /// <returns>Исключение сервиса.</returns>
  public static Exception CreateExceptionFromApiException(ApiException exception)
  {
    if (exception.HasContent)
    {
      var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(exception.Content);
      return CreateException(problemDetails, exception.StatusCode);
    }
    return new UnexpectedServiceException("Unexpected internal error occured.");
  }

  /// <summary>
  /// Создаёт исключение сервиса из ProblemDetails.
  /// </summary>
  /// <param name="problemDetails">Описание проблемы.</param>
  /// <param name="statusCode">Код статуса операции.</param>
  /// <returns>Исключение сервиса.</returns>
  public static Exception CreateException(ProblemDetails? problemDetails, HttpStatusCode statusCode)
  {
    if (statusCode == HttpStatusCode.NotFound)
        return new BlobNotFoundException(problemDetails?.Title ?? "Blob is not found.", problemDetails?.Detail);
      else if (statusCode == HttpStatusCode.BadRequest)
      return new ServiceContractException(problemDetails?.Title ?? "Service contract error.", problemDetails?.Detail);
    else if (statusCode == HttpStatusCode.Unauthorized)
      return new UnauthorizedAccessException(problemDetails?.Title ?? "Unauthorized access.");
    else
      return new UnexpectedServiceException(problemDetails?.Title ?? "Unexpected internal error occured.", problemDetails?.Detail);
  }
}
