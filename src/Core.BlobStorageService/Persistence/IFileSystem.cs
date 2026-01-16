namespace GlacialBytes.Core.BlobStorage.Persistence;

/// <summary>
/// Интерфейс файловой системы.
/// </summary>
public interface IFileSystem
{
  /// <summary>
  /// Проверяет доступность файловой системы.
  /// </summary>
  bool IsAvailable();

  /// <summary>
  /// Возвращает информацию по локальному файлу.
  /// </summary>
  /// <param name="path">Путь к файлу.</param>
  /// <returns>Информация по файлу.</returns>
  FileInfo? GetFileInfo(string path);

  /// <summary>
  /// Возвращает время последней записи в файл.
  /// </summary>
  /// <param name="path">Путь к файлу.</param>
  /// <returns>Время в UTC.</returns>
  DateTime GetFileLastWriteTimeUtc(string path);

  //
  // Summary:
  //     Determines whether the given path refers to an existing directory on disk.
  //
  // Parameters:
  //   path:
  //     The path to test.
  //
  // Returns:
  //     true if path refers to an existing directory; false if the directory does not
  //     exist or an error occurs when trying to determine if the specified directory
  //     exists.
  bool IsDirectoryExist(string path);

  string[] SearchDirectories(string path);

  string[] SearchFiles(string path, string searchPattern, SearchOption searchOption);

  void DeleteDirectory(string path, bool recursive);

  DirectoryInfo CreateDirectory(string path);

  bool IsDirectoryEmpty(string path);

  //
  // Summary:
  //     Determines whether the specified file exists.
  //
  // Parameters:
  //   path:
  //     The file to check.
  //
  // Returns:
  //     true if the caller has the required permissions and path contains the name of
  //     an existing file; otherwise, false. This method also returns false if path is
  //     null, an invalid path, or a zero-length string. If the caller does not have sufficient
  //     permissions to read the specified file, no exception is thrown and the method
  //     returns false regardless of the existence of path.
  bool IsFileExist(string path);

  void DeleteFile(string path);

  void MoveFile(string sourceFileName, string destFileName);

  void CopyFile(string sourceFileName, string destFileName);

  //
  // Summary:
  //     Initializes a new instance of the System.IO.FileStream class with the specified
  //     path, creation mode, read/write and sharing permission, the access other FileStreams
  //     can have to the same file, the buffer size, and additional file options.
  //
  // Parameters:
  //   path:
  //     A relative or absolute path for the file that the current FileStream object will
  //     encapsulate.
  //
  //   mode:
  //     One of the enumeration values that determines how to open or create the file.
  //
  //
  //   access:
  //     A bitwise combination of the enumeration values that determines how the file
  //     can be accessed by the FileStream object. This also determines the values returned
  //     by the System.IO.FileStream.CanRead and System.IO.FileStream.CanWrite properties
  //     of the FileStream object. System.IO.FileStream.CanSeek is true if path specifies
  //     a disk file.
  //
  //   share:
  //     A bitwise combination of the enumeration values that determines how the file
  //     will be shared by processes.
  //
  //   bufferSize:
  //     A positive System.Int32 value greater than 0 indicating the buffer size. The
  //     default buffer size is 4096.
  //
  //   options:
  //     A bitwise combination of the enumeration values that specifies additional file
  //     options.
  //
  // Exceptions:
  //   T:System.ArgumentNullException:
  //     path is null.
  //
  //   T:System.ArgumentException:
  //     .NET Framework and .NET Core versions older than 2.1: path is an empty string
  //     (""), contains only white space, or contains one or more invalid characters.
  //     -or- path refers to a non-file device, such as "con:", "com1:", "lpt1:", etc.
  //     in an NTFS environment.
  //
  //   T:System.NotSupportedException:
  //     path refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in a
  //     non-NTFS environment.
  //
  //   T:System.ArgumentOutOfRangeException:
  //     bufferSize is negative or zero. -or- mode, access, or share contain an invalid
  //     value.
  //
  //   T:System.IO.FileNotFoundException:
  //     The file cannot be found, such as when mode is FileMode.Truncate or FileMode.Open,
  //     and the file specified by path does not exist. The file must already exist in
  //     these modes.
  //
  //   T:System.IO.IOException:
  //     An I/O error, such as specifying FileMode.CreateNew when the file specified by
  //     path already exists, occurred. -or- The stream has been closed.
  //
  //   T:System.Security.SecurityException:
  //     The caller does not have the required permission.
  //
  //   T:System.IO.DirectoryNotFoundException:
  //     The specified path is invalid, such as being on an unmapped drive.
  //
  //   T:System.UnauthorizedAccessException:
  //     The access requested is not permitted by the operating system for the specified
  //     path, such as when access is Write or ReadWrite and the file or directory is
  //     set for read-only access. -or- System.IO.FileOptions.Encrypted is specified for
  //     options, but file encryption is not supported on the current platform.
  //
  //   T:System.IO.PathTooLongException:
  //     The specified path, file name, or both exceed the system-defined maximum length.
  Stream OpenFileStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, FileOptions options);

  //
  // Summary:
  //     Initializes a new instance of the System.IO.FileStream class with the specified
  //     path, creation mode, read/write and sharing permission, buffer size, additional
  //     file options, preallocation size, and the access other FileStreams can have to
  //     the same file.
  //
  // Parameters:
  //   path:
  //     A relative or absolute path for the file that the current System.IO.FileStream
  //     instance will encapsulate.
  //
  //   options:
  //     An object that describes optional System.IO.FileStream parameters to use.
  //
  // Exceptions:
  //   T:System.ArgumentNullException:
  //     path or options is null.
  //
  //   T:System.ArgumentException:
  //     path is an empty string, contains only white space, or contains one or more invalid
  //     characters. -or- path refers to a non-file device, such as CON:, COM1:, or LPT1:,
  //     in an NTFS environment.
  //
  //   T:System.NotSupportedException:
  //     path refers to a non-file device, such as CON:, COM1:, LPT1:, etc. in a non-NTFS
  //     environment.
  //
  //   T:System.IO.FileNotFoundException:
  //     The file cannot be found, such as when System.IO.FileStreamOptions.Mode is FileMode.Truncate
  //     or FileMode.Open, and the file specified by path does not exist. The file must
  //     already exist in these modes.
  //
  //   T:System.IO.IOException:
  //     An I/O error, such as specifying FileMode.CreateNew when the file specified by
  //     path already exists, occurred. -or- The stream has been closed. -or- The disk
  //     was full (when System.IO.FileStreamOptions.PreallocationSize was provided and
  //     path was pointing to a regular file). -or- The file was too large (when System.IO.FileStreamOptions.PreallocationSize
  //     was provided and path was pointing to a regular file).
  //
  //   T:System.Security.SecurityException:
  //     The caller does not have the required permission.
  //
  //   T:System.IO.DirectoryNotFoundException:
  //     The specified path is invalid, such as being on an unmapped drive.
  //
  //   T:System.UnauthorizedAccessException:
  //     The System.IO.FileStreamOptions.Access requested is not permitted by the operating
  //     system for the specified path, such as when System.IO.FileStreamOptions.Access
  //     is System.IO.FileAccess.Write or System.IO.FileAccess.ReadWrite and the file
  //     or directory is set for read-only access. -or- System.IO.FileOptions.Encrypted
  //     is specified for System.IO.FileStreamOptions.Options , but file encryption is
  //     not supported on the current platform.
  //
  //   T:System.IO.PathTooLongException:
  //     The specified path, file name, or both exceed the system-defined maximum length.
  Stream OpenFileStream(string path, FileStreamOptions options);
}
