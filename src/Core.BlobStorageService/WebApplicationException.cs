namespace GlacialBytes.Core.BlobStorage;

/// <summary>
/// Исключение веб-приложения.
/// </summary>
/// <param name="message">Сообщение об ошибке.</param>
/// <param name="isCritical">Признак критичности.</param>
/// <param name="innerException">Внутреннее исключение.</param>
public class WebApplicationException(string message, bool isCritical, Exception? innerException) : Exception(message, innerException)
{
  /// <summary>
  /// Признак критичности.
  /// </summary>
  public bool IsCritical { get; } = isCritical;
}
