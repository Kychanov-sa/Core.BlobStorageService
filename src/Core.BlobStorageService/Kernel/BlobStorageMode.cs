namespace GlacialBytes.Core.BlobStorage.Kernel;

/// <summary>
/// Режим работы хранилища.
/// </summary>
public enum BlobStorageMode
{
  /// <summary>
  /// Доступно чтение и запись.
  /// </summary>
  ReadAndWrite = 0,

  /// <summary>
  /// Доступно чтение и добавление новых данных.
  /// </summary>
  ReadAndAppendOnly,

  /// <summary>
  /// Доступно только чтение.
  /// </summary>
  ReadOnly,
}
