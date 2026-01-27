using LocalFileInfo = System.IO.FileInfo;

namespace GlacialBytes.Core.BlobStorage.Persistence;

/// <summary>
/// Локальная файловая система.
/// </summary>
/// <param name="rootPath">Корневой путь.</param>
internal class LocalFileSystem(string rootPath) : IFileSystem
{
  #region IFileSystem

  /// <summary>
  /// <see cref="IFileSystem.IsAvailable"/>
  /// </summary>
  public bool IsAvailable()
  {
    return Directory.Exists(rootPath);
  }

  /// <summary>
  /// <see cref="IFileSystem.GetFileInfo(string)"/>
  /// </summary>
  public FileInfo? GetFileInfo(string path)
  {
    string filePath = Path.Combine(rootPath, path);
    var fileInfo = new LocalFileInfo(filePath);
    if (!fileInfo.Exists)
      return null;

    return new FileInfo(fileInfo.Name, fileInfo.Length, fileInfo.CreationTimeUtc, fileInfo.LastWriteTimeUtc);
  }

  /// <summary>
  /// <see cref="IFileSystem.GetFileLastWriteTimeUtc(string)"/>
  /// </summary>
  public DateTime GetFileLastWriteTimeUtc(string path)
  {
    string filePath = Path.Combine(rootPath, path);
    return File.GetLastWriteTimeUtc(filePath);
  }

  /// <summary>
  /// <see cref="IFileSystem.CreateDirectory(string)"/>
  /// </summary>
  public DirectoryInfo CreateDirectory(string path)
  {
    string directoryPath = Path.Combine(rootPath, path);
    return Directory.CreateDirectory(directoryPath);
  }

  /// <summary>
  /// <see cref="IFileSystem.DeleteDirectory(string, bool)"/>
  /// </summary>
  public void DeleteDirectory(string path, bool recursive)
  {
    string directoryPath = Path.Combine(rootPath, path);
    Directory.Delete(directoryPath, recursive);
  }

  /// <summary>
  /// <see cref="IFileSystem.DeleteFile(string)"/>
  /// </summary>
  public void DeleteFile(string path)
  {
    string filePath = Path.Combine(rootPath, path);
    File.Delete(filePath);
  }

  /// <summary>
  /// <see cref="IFileSystem.IsDirectoryEmpty(string)"/>
  /// </summary>
  public bool IsDirectoryEmpty(string path)
  {
    string directoryPath = Path.Combine(rootPath, path);
    return !Directory.EnumerateFileSystemEntries(directoryPath).Any();
  }

  /// <summary>
  /// <see cref="IFileSystem.SearchDirectories(string)"/>
  /// </summary>
  public string[] SearchDirectories(string path)
  {
    string directoryPath = Path.Combine(rootPath, path);
    return Directory.GetDirectories(directoryPath)
      .Select(p => Path.GetRelativePath(rootPath, p))
      .ToArray();
  }

  /// <summary>
  /// <see cref="IFileSystem.SearchFiles(string, string, SearchOption)"/>
  /// </summary>
  public string[] SearchFiles(string path, string searchPattern, SearchOption searchOption)
  {
    string directoryPath = Path.Combine(rootPath, path);
    return Directory.GetFiles(directoryPath, searchPattern, searchOption)
      .Select(p => Path.GetRelativePath(rootPath, p))
      .ToArray();
  }

  /// <summary>
  /// <see cref="IFileSystem.IsDirectoryExist(string)"/>
  /// </summary>
  public bool IsDirectoryExist(string path)
  {
    string directoryPath = Path.Combine(rootPath, path);
    return Directory.Exists(directoryPath);
  }

  /// <summary>
  /// <see cref="IFileSystem.IsFileExist(string)"/>
  /// </summary>
  public bool IsFileExist(string path)
  {
    string filePath = Path.Combine(rootPath, path);
    return File.Exists(filePath);
  }

  /// <summary>
  /// <see cref="IFileSystem.MoveFile(string, string)"/>
  /// </summary>
  public void MoveFile(string sourceFileName, string destFileName)
  {
    string sourceFilePath = Path.Combine(rootPath, sourceFileName);
    string destFilePath = Path.Combine(rootPath, destFileName);
    File.Move(sourceFilePath, destFilePath);
  }

  /// <summary>
  /// <see cref="IFileSystem.CopyFile(string, string)"/>
  /// </summary>
  public void CopyFile(string sourceFileName, string destFileName)
  {
    string sourceFilePath = Path.Combine(rootPath, sourceFileName);
    string destFilePath = Path.Combine(rootPath, destFileName);
    File.Copy(sourceFilePath, destFilePath);
  }

  /// <summary>
  /// <see cref="IFileSystem.OpenFileStream(string, FileMode, FileAccess, FileShare, int, FileOptions)"/>
  /// </summary>
  public Stream OpenFileStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, FileOptions options)
  {
    string filePath = Path.Combine(rootPath, path);
    return new FileStream(filePath, mode, access, share, bufferSize, options);
  }

  /// <summary>
  /// <see cref="IFileSystem.OpenFileStream(string, FileStreamOptions)"/>
  /// </summary>
  public Stream OpenFileStream(string path, FileStreamOptions options)
  {
    string filePath = Path.Combine(rootPath, path);
    return new FileStream(filePath, options);
  }

  #endregion
}
