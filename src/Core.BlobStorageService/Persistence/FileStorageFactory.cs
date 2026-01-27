using GlacialBytes.Core.BlobStorage.Kernel;
using Microsoft.Extensions.Options;

namespace GlacialBytes.Core.BlobStorage.Persistence;

/// <summary>
/// Фабрика хранилищ на основе файловой системы.
/// </summary>
public class FileStorageFactory(IOptions<FileStorageSettings> options) : IBlobStorageFactory
{
  #region IBlobStorageFactory

  /// <summary>
  /// <see cref="IBlobStorageFactory.CreateStorage(BlobStorageMode, bool)"/>
  /// </summary>
  public IBlobStorage CreateStorage(BlobStorageMode mode, bool useSafeDelete)
  {
    var fileSystem = new LocalFileSystem(options.Value.StorageDirectory);
    return new FileStorage(fileSystem, mode, useSafeDelete);
  }

  #endregion
}
