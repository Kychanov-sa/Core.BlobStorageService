namespace GlacialBytes.Core.BlobStorage.Kernel
{
  /// <summary>
  /// Фабрика хранилищ BLOB объектов.
  /// </summary>
  public interface IBlobStorageFactory
  {
    /// <summary>
    /// Создаёт хранилище.
    /// </summary>
    /// <param name="mode">Режим работы хранилища.</param>
    /// <param name="useSafeDelete">Признак использования безопасного удаления.</param>
    /// <returns>Хранилище BLOB объектов.</returns>
    IBlobStorage CreateStorage(BlobStorageMode mode, bool useSafeDelete);
  }
}
