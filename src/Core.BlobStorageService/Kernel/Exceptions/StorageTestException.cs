namespace GlacialBytes.Core.BlobStorageService.Kernel.Exceptions;

/// <summary>
/// Исключение при проверке хранилища.
/// </summary>
/// <param name="message">Текст сообщения.</param>
public class StorageTestException(string message) : Exception(message)
{
}
