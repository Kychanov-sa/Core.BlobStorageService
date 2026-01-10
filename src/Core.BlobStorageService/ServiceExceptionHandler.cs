using GlacialBytes.Core.BlobStorageService.Kernel.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace GlacialBytes.Core.BlobStorageService;

/// <summary>
/// Обработчик исключений сервиса.
/// </summary>
public class ServiceExceptionHandler(ILogger<ServiceExceptionHandler> logger) : IExceptionHandler
{
  /// <summary>
  /// Сообщение о неожиданной ошибке.
  /// </summary>
  private const string UnexpectedError = "An unexpected error has occurred.";

  /// <summary>
  /// Сообщение о необработанном исключении.
  /// </summary>
  private const string UnhandledExceptionMsg = "An unhandled exception has occurred while executing the request.";

  /// <summary>
  /// Логгер для исключений.
  /// </summary>
  private readonly ILogger<ServiceExceptionHandler> _logger = logger;

  #region IExceptionHandler

  /// <summary>
  /// <see cref="IExceptionHandler.TryHandleAsync(HttpContext, Exception, CancellationToken)"/>
  /// </summary>
  public async ValueTask<bool> TryHandleAsync(
    HttpContext context,
    Exception exception,
    CancellationToken cancellationToken)
  {
    // Логируем исключение
    LogException(exception);

    // Формируем ответ в виде ProblemDetails
    var problemDetails = CreateProblemDetails(exception);
    if (problemDetails.Status is not null)
      context.Response.StatusCode = (int)problemDetails.Status;
    await context.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
    return true;
  }

  #endregion

  /// <summary>
  /// Создаёт объект ProblemDetails по исключению.
  /// </summary>
  /// <param name="exception">Исключение сервиса.</param>
  /// <returns>Описение проблемы.</returns>
  private static ProblemDetails CreateProblemDetails(Exception exception)
  {
    int statusCode = exception switch
    {
      BlobNotExistsException => StatusCodes.Status404NotFound,
      BlobReadOnlyException => StatusCodes.Status400BadRequest,
      OperationNotAllowedException => StatusCodes.Status400BadRequest,
      OperationFailedException => StatusCodes.Status500InternalServerError,
      _ => StatusCodes.Status500InternalServerError,
    };

    var reasonPhrase = ReasonPhrases.GetReasonPhrase(statusCode);
    if (String.IsNullOrEmpty(reasonPhrase))
    {
      reasonPhrase = UnexpectedError;
    }

    var problemDetails = new ProblemDetails
    {
      Type = exception.GetType().Name,
      Status = statusCode,
      Title = reasonPhrase,
      Detail = exception.Message,
    };

    return problemDetails;
  }

  /// <summary>
  /// Логирует исключение.
  /// </summary>
  /// <param name="exception">Исключение.</param>
  private void LogException(Exception exception)
  {
    if (exception is AggregateException aggregateException)
    {
      foreach (var aggregatedException in aggregateException.InnerExceptions)
        LogException(aggregatedException);
    }
    else if (exception.InnerException != null)
    {
      LogException(exception.InnerException);
    }
    else if (exception is WebApplicationException { IsCritical: true })
    {
      _logger.LogCritical(exception, exception.Message);
      Environment.Exit(1);
    }
    else if (exception is StorageOperationException)
    {
      _logger.LogError(exception, exception.Message);
    }
    else
    {
      _logger.LogError(exception, UnhandledExceptionMsg);
    }
  }
}
