namespace GlacialBytes.Core.BlobStorage.Exceptions;

/// <summary>
/// Исключение нарушения контракта сервиса.
/// </summary>
/// <param name="title">Заголовок исключения.</param>
/// <param name="details">Детальная информация.</param>
public class ServiceContractException(string title, string? details = null) : Exception(title)
{
  /// <summary>
  /// Детальная информация.
  /// </summary>
  public string? Details { get; } = details;
}
