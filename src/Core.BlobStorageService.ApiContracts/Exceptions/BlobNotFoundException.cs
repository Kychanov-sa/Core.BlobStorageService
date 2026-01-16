namespace GlacialBytes.Core.BlobStorage.Exceptions;

/// <summary>
/// BLOB объект не найден.
/// </summary>
/// <param name="title">Заголовок исключения.</param>
/// <param name="details">Детальная информация.</param>
public class BlobNotFoundException(string title, string? details = null) : ServiceContractException(title, details)
{
}
