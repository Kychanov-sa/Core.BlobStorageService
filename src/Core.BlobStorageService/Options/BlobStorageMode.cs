namespace GlacialBytes.Core.BlobStorage.Options;

/// <summary>
/// Режим работы хранилища.
/// </summary>
public enum BlobStorageMode
{
  /// <summary>
  /// Временное хранение.
  /// </summary>
  Temporary = 0,

  /// <summary>
  /// Постоянное хранение.
  /// </summary>
  Persistent = 1,

  /// <summary>
  /// Архивное хранение.
  /// </summary>
  Archival = 2,
}
