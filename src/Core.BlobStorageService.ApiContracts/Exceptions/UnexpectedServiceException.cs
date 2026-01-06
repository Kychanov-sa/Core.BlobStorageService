namespace GlacialBytes.Core.BlobStorageService.Exceptions;

/// <summary>
/// Неожиданная ошибка сервиса.
/// </summary>
/// <param name="title">Заголовок исключения.</param>
/// <param name="details">Детальная информация.</param>
public class UnexpectedServiceException(string title, string? details = null) : Exception(title)
{
  /// <summary>
  /// Детальная информация.
  /// </summary>
  public string? Details { get; } = details;
}
