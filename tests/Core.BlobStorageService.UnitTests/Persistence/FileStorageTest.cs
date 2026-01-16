using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GlacialBytes.Core.BlobStorage.Kernel;
using GlacialBytes.Core.BlobStorage.Kernel.Exceptions;
using GlacialBytes.Core.BlobStorage.Persistence;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using FileInfo = GlacialBytes.Core.BlobStorage.Persistence.FileInfo;

namespace Core.BlobStorage.UnitTests.Persistence;

[TestClass]
public class FileStorageTests
{
  private const bool UseSafeDelete = true;
  private const string RecycleBinDirectoryName = "deleted";
  private readonly BlobStorageMode _testMode = BlobStorageMode.ReadAndWrite;

  private Mock<IFileSystem> _fileSystemMock;
  //private Mock<IFileInfo> _fileInfoMock;
  private FileStorage _fileStorage;

  [TestInitialize]
  public void Initialize()
  {
    _fileSystemMock = new Mock<IFileSystem>();
    //_fileInfoMock = new Mock<IFileInfo>();
    _fileStorage = new FileStorage(_fileSystemMock.Object, _testMode, UseSafeDelete);
  }

  #region Test Method Tests

  [TestMethod]
  public void Test_WhenFileSystemIsAvailable_ShouldNotThrow()
  {
    // Arrange
    _fileSystemMock.Setup(fs => fs.IsAvailable()).Returns(true);
    _fileSystemMock.Setup(fs => fs.OpenFileStream(
        It.IsAny<string>(),
        It.IsAny<FileMode>(),
        It.IsAny<FileAccess>(),
        It.IsAny<FileShare>(),
        It.IsAny<int>(),
        It.IsAny<FileOptions>()))
        .Returns(new MemoryStream());

    // Act & Assert
    _fileStorage.Test();
  }

  [TestMethod]
  [ExpectedException(typeof(StorageTestException))]
  public void Test_WhenFileSystemNotAvailable_ShouldThrowStorageTestException()
  {
    // Arrange
    _fileSystemMock.Setup(fs => fs.IsAvailable()).Returns(false);

    // Act
    _fileStorage.Test();
  }

  [TestMethod]
  [ExpectedException(typeof(StorageTestException))]
  public void Test_WhenReadAndWriteModeAndNoWritePermissions_ShouldThrowStorageTestException()
  {
    // Arrange
    var readOnlyStorage = new FileStorage(_fileSystemMock.Object, BlobStorageMode.ReadAndWrite, UseSafeDelete);
    _fileSystemMock.Setup(fs => fs.IsAvailable()).Returns(true);
    _fileSystemMock.Setup(fs => fs.OpenFileStream(
        It.IsAny<string>(),
        It.IsAny<FileMode>(),
        It.IsAny<FileAccess>(),
        It.IsAny<FileShare>(),
        It.IsAny<int>(),
        It.IsAny<FileOptions>()))
        .Throws<UnauthorizedAccessException>();

    // Act
    readOnlyStorage.Test();
  }

  [TestMethod]
  public void Test_WhenReadOnlyMode_ShouldNotCheckWritePermissions()
  {
    // Arrange
    var readOnlyStorage = new FileStorage(_fileSystemMock.Object, BlobStorageMode.ReadOnly, UseSafeDelete);
    _fileSystemMock.Setup(fs => fs.IsAvailable()).Returns(true);

    // Act
    readOnlyStorage.Test();

    // Assert - не должно быть вызова OpenFileStream
    _fileSystemMock.Verify(fs => fs.OpenFileStream(
        It.IsAny<string>(),
        It.IsAny<FileMode>(),
        It.IsAny<FileAccess>(),
        It.IsAny<FileShare>(),
        It.IsAny<int>(),
        It.IsAny<FileOptions>()), Times.Never);
  }

  #endregion

  #region Get Method Tests

  [TestMethod]
  public void Get_WhenBlobExists_ShouldReturnBlobInfo()
  {
    // Arrange
    var blobId = Guid.NewGuid();
    var expectedPath = GetExpectedFilePath(blobId);

    var expectedFileInfo = new FileInfo(Path.GetFileName(expectedPath), 1024, DateTime.UtcNow.AddHours(-1), DateTime.UtcNow);
    _fileSystemMock.Setup(fs => fs.GetFileInfo(expectedPath)).Returns(expectedFileInfo);

    // Act
    var result = _fileStorage.Get(blobId);

    // Assert
    Assert.IsNotNull(result);
    Assert.AreEqual(blobId, result.Id.Value);
    Assert.AreEqual(1024, result.Length);
    Assert.IsFalse(result.IsReadOnly);
  }

  [TestMethod]
  public void Get_WhenBlobNotExists_ShouldReturnNull()
  {
    // Arrange
    var blobId = Guid.NewGuid();
    var expectedPath = GetExpectedFilePath(blobId);

    _fileSystemMock.Setup(fs => fs.GetFileInfo(expectedPath)).Returns((FileInfo?)null);

    // Act
    var result = _fileStorage.Get(blobId);

    // Assert
    Assert.IsNull(result);
  }

  [TestMethod]
  public void Get_WhenReadOnlyMode_ShouldReturnReadOnlyBlobInfo()
  {
    // Arrange
    var readOnlyStorage = new FileStorage(_fileSystemMock.Object, BlobStorageMode.ReadOnly, UseSafeDelete);
    var blobId = Guid.NewGuid();
    var expectedPath = GetExpectedFilePath(blobId);

    var expectedFileInfo = new FileInfo(Path.GetFileName(expectedPath), 1024, DateTime.UtcNow.AddHours(-1), DateTime.UtcNow);    
    _fileSystemMock.Setup(fs => fs.GetFileInfo(expectedPath)).Returns(expectedFileInfo);

    // Act
    var result = readOnlyStorage.Get(blobId);

    // Assert
    Assert.IsNotNull(result);
    Assert.IsTrue(result.IsReadOnly);
  }

  #endregion

  #region Copy Method Tests

  [TestMethod]
  public void Copy_WhenSourceExistsAndDestinationNotExists_ShouldCopySuccessfully()
  {
    // Arrange
    var sourceBlobId = Guid.NewGuid();
    var destBlobId = Guid.NewGuid();
    var sourcePath = GetExpectedFilePath(sourceBlobId);
    var destPath = GetExpectedFilePath(destBlobId);

    _fileSystemMock.Setup(fs => fs.IsFileExist(sourcePath)).Returns(true);
    _fileSystemMock.Setup(fs => fs.IsFileExist(destPath)).Returns(false);
    _fileSystemMock.Setup(fs => fs.OpenFileStream(
        sourcePath,
        It.Is<FileStreamOptions>(o => o.Mode == FileMode.Open)))
        .Returns(new MemoryStream(new byte[1024]));
    _fileSystemMock.Setup(fs => fs.OpenFileStream(
        destPath,
        It.Is<FileStreamOptions>(o => o.Mode == FileMode.CreateNew)))
        .Returns(new MemoryStream());

    var destFileInfo = new FileInfo(Path.GetFileName(destPath), 1024, DateTime.UtcNow, DateTime.UtcNow);
    _fileSystemMock.Setup(fs => fs.GetFileInfo(destPath)).Returns(destFileInfo);

    // Act
    var result = _fileStorage.Copy(sourceBlobId, destBlobId, CancellationToken.None);

    // Assert
    Assert.IsNotNull(result);
    Assert.AreEqual(destBlobId, result.Id.Value);
    _fileSystemMock.Verify(fs => fs.CreateDirectory(It.IsAny<string>()), Times.AtLeastOnce);
    _fileSystemMock.Verify(fs => fs.OpenFileStream(It.IsAny<string>(), It.Is<FileStreamOptions>(options => options.Mode == FileMode.Open)), Times.Once);
    _fileSystemMock.Verify(fs => fs.OpenFileStream(It.IsAny<string>(), It.Is<FileStreamOptions>(options => options.Mode == FileMode.CreateNew)), Times.Once);
  }

  [TestMethod]
  public void Copy_WhenSourceExistsAndDestinationExists_ShouldCopySuccessfully()
  {
    // Arrange
    var sourceBlobId = Guid.NewGuid();
    var destBlobId = Guid.NewGuid();
    var sourcePath = GetExpectedFilePath(sourceBlobId);
    var destPath = GetExpectedFilePath(destBlobId);

    _fileSystemMock.Setup(fs => fs.IsFileExist(sourcePath)).Returns(true);
    _fileSystemMock.Setup(fs => fs.IsFileExist(destPath)).Returns(true);
    _fileSystemMock.Setup(fs => fs.OpenFileStream(
        sourcePath,
        It.Is<FileStreamOptions>(o => o.Mode == FileMode.Open)))
        .Returns(new MemoryStream(new byte[1024]));
    _fileSystemMock.Setup(fs => fs.OpenFileStream(
        destPath,
        It.Is<FileStreamOptions>(o => o.Mode == FileMode.Truncate)))
        .Returns(new MemoryStream());

    var destFileInfo = new FileInfo(Path.GetFileName(destPath), 1024, DateTime.UtcNow, DateTime.UtcNow);
    _fileSystemMock.Setup(fs => fs.GetFileInfo(destPath)).Returns(destFileInfo);

    // Act
    var result = _fileStorage.Copy(sourceBlobId, destBlobId, CancellationToken.None);

    // Assert
    Assert.IsNotNull(result);
    Assert.AreEqual(destBlobId, result.Id.Value);
    _fileSystemMock.Verify(fs => fs.CreateDirectory(It.IsAny<string>()), Times.AtLeastOnce);
    _fileSystemMock.Verify(fs => fs.OpenFileStream(It.IsAny<string>(), It.Is<FileStreamOptions>(options => options.Mode == FileMode.Open)), Times.Once);
    _fileSystemMock.Verify(fs => fs.OpenFileStream(It.IsAny<string>(), It.Is<FileStreamOptions>(options => options.Mode == FileMode.Truncate)), Times.Once);
  }

  [TestMethod]
  [ExpectedException(typeof(BlobNotExistsException))]
  public void Copy_WhenSourceNotExists_ShouldThrowBlobNotExistsException()
  {
    // Arrange
    var sourceBlobId = Guid.NewGuid();
    var destBlobId = Guid.NewGuid();
    var sourcePath = GetExpectedFilePath(sourceBlobId);

    _fileSystemMock.Setup(fs => fs.IsFileExist(sourcePath)).Returns(false);

    // Act
    _fileStorage.Copy(sourceBlobId, destBlobId, CancellationToken.None);
  }

  [TestMethod]
  [ExpectedException(typeof(BlobReadOnlyException))]
  public void Copy_WhenReadOnlyModeAndDestinationExists_ShouldThrowBlobReadOnlyException()
  {
    // Arrange
    var readOnlyStorage = new FileStorage(_fileSystemMock.Object, BlobStorageMode.ReadOnly, UseSafeDelete);
    var sourceBlobId = Guid.NewGuid();
    var destBlobId = Guid.NewGuid();
    var sourcePath = GetExpectedFilePath(sourceBlobId);
    var destPath = GetExpectedFilePath(destBlobId);

    _fileSystemMock.Setup(fs => fs.IsFileExist(sourcePath)).Returns(true);
    _fileSystemMock.Setup(fs => fs.IsFileExist(destPath)).Returns(true);

    // Act
    readOnlyStorage.Copy(sourceBlobId, destBlobId, CancellationToken.None);
  }

  [TestMethod]
  [ExpectedException(typeof(OperationFailedException))]
  public void Copy_WhenDestinationFileNotFoundAfterCopy_ShouldThrowOperationFailedException()
  {
    // Arrange
    var sourceBlobId = Guid.NewGuid();
    var destBlobId = Guid.NewGuid();
    var sourcePath = GetExpectedFilePath(sourceBlobId);
    var destPath = GetExpectedFilePath(destBlobId);

    _fileSystemMock.Setup(fs => fs.IsFileExist(sourcePath)).Returns(true);
    _fileSystemMock.Setup(fs => fs.IsFileExist(destPath)).Returns(false);
    _fileSystemMock.Setup(fs => fs.OpenFileStream(
        It.IsAny<string>(),
        It.IsAny<FileStreamOptions>()))
        .Returns(new MemoryStream());
    _fileSystemMock.Setup(fs => fs.GetFileInfo(destPath)).Returns((FileInfo?)null);

    // Act
    _fileStorage.Copy(sourceBlobId, destBlobId, CancellationToken.None);
  }

  #endregion

  #region Delete Method Tests

  [TestMethod]
  public void Delete_WhenFileExistsAndSafeDeleteEnabled_ShouldMoveToRecycleBin()
  {
    // Arrange
    var blobId = Guid.NewGuid();
    var filePath = GetExpectedFilePath(blobId);
    var recycleBinPath = Path.Combine(RecycleBinDirectoryName, $"{blobId}.blob");

    _fileSystemMock.Setup(fs => fs.IsFileExist(filePath)).Returns(true);
    _fileSystemMock.Setup(fs => fs.IsDirectoryExist(RecycleBinDirectoryName)).Returns(true);

    // Act
    _fileStorage.Delete(blobId, CancellationToken.None);

    // Assert
    _fileSystemMock.Verify(fs => fs.MoveFile(filePath, recycleBinPath), Times.Once);
  }

  [TestMethod]
  public void Delete_WhenFileExistsAndSafeDeleteDisabled_ShouldDeletePermanently()
  {
    // Arrange
    var storageWithoutSafeDelete = new FileStorage(_fileSystemMock.Object, _testMode, false);
    var blobId = Guid.NewGuid();
    var filePath = GetExpectedFilePath(blobId);

    _fileSystemMock.Setup(fs => fs.IsFileExist(filePath)).Returns(true);

    // Act
    storageWithoutSafeDelete.Delete(blobId, CancellationToken.None);

    // Assert
    _fileSystemMock.Verify(fs => fs.DeleteFile(filePath), Times.Once);
  }

  [TestMethod]
  [ExpectedException(typeof(BlobNotExistsException))]
  public void Delete_WhenFileNotExists_ShouldThrowBlobNotExistsException()
  {
    // Arrange
    var blobId = Guid.NewGuid();
    var filePath = GetExpectedFilePath(blobId);

    _fileSystemMock.Setup(fs => fs.IsFileExist(filePath)).Returns(false);

    // Act
    _fileStorage.Delete(blobId, CancellationToken.None);
  }

  [TestMethod]
  [ExpectedException(typeof(BlobReadOnlyException))]
  public void Delete_WhenReadOnlyMode_ShouldThrowBlobReadOnlyException()
  {
    // Arrange
    var readOnlyStorage = new FileStorage(_fileSystemMock.Object, BlobStorageMode.ReadOnly, UseSafeDelete);
    var blobId = Guid.NewGuid();
    var filePath = GetExpectedFilePath(blobId);

    _fileSystemMock.Setup(fs => fs.IsFileExist(filePath)).Returns(true);

    // Act
    readOnlyStorage.Delete(blobId, CancellationToken.None);
  }

  #endregion

  #region Restore Method Tests

  [TestMethod]
  public void Restore_WhenFileInRecycleBinExists_ShouldRestoreSuccessfully()
  {
    // Arrange
    var blobId = Guid.NewGuid();
    var recycleBinPath = Path.Combine(RecycleBinDirectoryName, $"{blobId}.blob");
    var restoredPath = GetExpectedFilePath(blobId);
    var restoredFileInfo = new FileInfo(Path.GetFileName(restoredPath), 1024, DateTime.UtcNow, DateTime.UtcNow);

    _fileSystemMock.Setup(fs => fs.IsFileExist(recycleBinPath)).Returns(true);
    _fileSystemMock.Setup(fs => fs.GetFileInfo(restoredPath)).Returns(restoredFileInfo);

    // Act
    var result = _fileStorage.Restore(blobId, CancellationToken.None);

    // Assert
    Assert.IsNotNull(result);
    Assert.AreEqual(blobId, result.Id.Value);
    _fileSystemMock.Verify(fs => fs.MoveFile(recycleBinPath, restoredPath), Times.Once);
  }

  [TestMethod]
  [ExpectedException(typeof(OperationNotAllowedException))]
  public void Restore_WhenSafeDeleteDisabled_ShouldThrowOperationNotAllowedException()
  {
    // Arrange
    var storageWithoutSafeDelete = new FileStorage(_fileSystemMock.Object, _testMode, false);
    var blobId = Guid.NewGuid();

    // Act
    storageWithoutSafeDelete.Restore(blobId, CancellationToken.None);
  }

  [TestMethod]
  [ExpectedException(typeof(BlobNotExistsException))]
  public void Restore_WhenFileNotInRecycleBin_ShouldThrowBlobNotExistsException()
  {
    // Arrange
    var blobId = Guid.NewGuid();
    var recycleBinPath = Path.Combine(RecycleBinDirectoryName, $"{blobId}.blob");

    _fileSystemMock.Setup(fs => fs.IsFileExist(recycleBinPath)).Returns(false);

    // Act
    _fileStorage.Restore(blobId, CancellationToken.None);
  }

  [TestMethod]
  [ExpectedException(typeof(OperationFailedException))]
  public void Restore_WhenFileNotFoundAfterRestore_ShouldThrowOperationFailedException()
  {
    // Arrange
    var blobId = Guid.NewGuid();
    var recycleBinPath = Path.Combine(RecycleBinDirectoryName, $"{blobId}.blob");
    var restoredPath = GetExpectedFilePath(blobId);

    _fileSystemMock.Setup(fs => fs.IsFileExist(recycleBinPath)).Returns(true);
    _fileSystemMock.Setup(fs => fs.GetFileInfo(restoredPath)).Returns((FileInfo?)null);

    // Act
    _fileStorage.Restore(blobId, CancellationToken.None);
  }

  #endregion

  #region WriteAsync Method Tests

  [TestMethod]
  public async Task WriteAsync_WhenFileNotExistsAndOffsetIsZero_ShouldCreateFile()
  {
    // Arrange
    var blobId = Guid.NewGuid();
    var filePath = GetExpectedFilePath(blobId);
    var data = new byte[] { 1, 2, 3, 4 };
    using var dataStream = new MemoryStream(data);
    var writtenFileInfo = new FileInfo(Path.GetFileName(filePath), data.Length, DateTime.UtcNow, DateTime.UtcNow);

    _fileSystemMock.Setup(fs => fs.IsFileExist(filePath)).Returns(false);
    _fileSystemMock.Setup(fs => fs.GetFileInfo(filePath)).Returns(writtenFileInfo);

    var capturedOptions = new List<FileStreamOptions>();
    _fileSystemMock.Setup(fs => fs.OpenFileStream(
        filePath,
        It.IsAny<FileStreamOptions>()))
        .Callback<string, FileStreamOptions>((path, options) => capturedOptions.Add(options))
        .Returns(new MemoryStream());

    // Act
    var result = await _fileStorage.WriteAsync(blobId, 0, data.Length, dataStream, CancellationToken.None);

    // Assert
    Assert.IsNotNull(result);
    Assert.AreEqual(data.Length, result.Length);
    Assert.AreEqual(FileMode.CreateNew, capturedOptions[0].Mode);
  }

  [TestMethod]
  public async Task WriteAsync_WhenFileExists_ShouldOpenInTruncateMode()
  {
    // Arrange
    var blobId = Guid.NewGuid();
    var filePath = GetExpectedFilePath(blobId);
    var data = new byte[] { 1, 2, 3, 4 };
    using var dataStream = new MemoryStream(data);
    var writtenFileInfo = new FileInfo(Path.GetFileName(filePath), data.Length, DateTime.UtcNow, DateTime.UtcNow);

    _fileSystemMock.Setup(fs => fs.IsFileExist(filePath)).Returns(true);
    _fileSystemMock.Setup(fs => fs.GetFileInfo(filePath)).Returns(writtenFileInfo);

    var capturedOptions = new List<FileStreamOptions>();
    _fileSystemMock.Setup(fs => fs.OpenFileStream(
        filePath,
        It.IsAny<FileStreamOptions>()))
        .Callback<string, FileStreamOptions>((path, options) => capturedOptions.Add(options))
        .Returns(new MemoryStream());

    // Act
    await _fileStorage.WriteAsync(blobId, 0, data.Length, dataStream, CancellationToken.None);

    // Assert
    Assert.AreEqual(FileMode.Truncate, capturedOptions[0].Mode);
  }

  [TestMethod]
  [ExpectedException(typeof(OperationNotAllowedException))]
  public async Task WriteAsync_WhenFileNotExistsAndOffsetGreaterThanZero_ShouldThrowOperationNotAllowedException()
  {
    // Arrange
    var blobId = Guid.NewGuid();
    var filePath = GetExpectedFilePath(blobId);
    var data = new byte[] { 1, 2, 3, 4 };
    using var dataStream = new MemoryStream(data);

    _fileSystemMock.Setup(fs => fs.IsFileExist(filePath)).Returns(false);

    // Act
    await _fileStorage.WriteAsync(blobId, 10, data.Length, dataStream, CancellationToken.None);
  }

  [TestMethod]
  [ExpectedException(typeof(BlobReadOnlyException))]
  public async Task WriteAsync_WhenReadOnlyMode_ShouldThrowBlobReadOnlyException()
  {
    // Arrange
    var readOnlyStorage = new FileStorage(_fileSystemMock.Object, BlobStorageMode.ReadOnly, UseSafeDelete);
    var blobId = Guid.NewGuid();
    var data = new byte[] { 1, 2, 3, 4 };
    using var dataStream = new MemoryStream(data);

    // Act
    await readOnlyStorage.WriteAsync(blobId, 0, data.Length, dataStream, CancellationToken.None);
  }

  #endregion

  #region ReadAsync Method Tests

  [TestMethod]
  public async Task ReadAsync_WhenFileExistsAndReadFullFile_ShouldReturnStream()
  {
    // Arrange
    var blobId = Guid.NewGuid();
    var filePath = GetExpectedFilePath(blobId);
    var fileData = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
    using var fileStream = new MemoryStream(fileData);

    _fileSystemMock.Setup(fs => fs.IsFileExist(filePath)).Returns(true);
    _fileSystemMock.Setup(fs => fs.OpenFileStream(
        filePath,
        It.Is<FileStreamOptions>(o => o.Mode == FileMode.Open)))
        .Returns(fileStream);

    // Act
    var result = await _fileStorage.ReadAsync(blobId, 0, -1, CancellationToken.None);

    // Assert
    Assert.AreEqual(fileStream, result);
  }

  [TestMethod]
  public async Task ReadAsync_WhenFileExistsAndReadPartialFile_ShouldReturnMemoryStream()
  {
    // Arrange
    var blobId = Guid.NewGuid();
    var filePath = GetExpectedFilePath(blobId);
    var fileData = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
    using var fileStream = new MemoryStream(fileData);

    _fileSystemMock.Setup(fs => fs.IsFileExist(filePath)).Returns(true);
    _fileSystemMock.Setup(fs => fs.OpenFileStream(
        filePath,
        It.Is<FileStreamOptions>(o => o.Mode == FileMode.Open)))
        .Returns(fileStream);

    // Act
    var result = await _fileStorage.ReadAsync(blobId, 2, 4, CancellationToken.None);

    // Assert
    Assert.IsInstanceOfType(result, typeof(MemoryStream));
    var memoryStream = (MemoryStream)result;
    Assert.AreEqual(4, memoryStream.Length);
  }

  [TestMethod]
  [ExpectedException(typeof(BlobNotExistsException))]
  public async Task ReadAsync_WhenFileNotExists_ShouldThrowBlobNotExistsException()
  {
    // Arrange
    var blobId = Guid.NewGuid();
    var filePath = GetExpectedFilePath(blobId);

    _fileSystemMock.Setup(fs => fs.IsFileExist(filePath)).Returns(false);

    // Act
    await _fileStorage.ReadAsync(blobId, 0, 10, CancellationToken.None);
  }

  #endregion

  #region DeleteExpiredBlobs Method Tests

  [TestMethod]
  public void DeleteExpiredBlobs_WhenFilesExist_ShouldDeleteExpiredOnes()
  {
    // Arrange
    var expirationDate = DateTime.UtcNow.AddDays(-1);
    var expiredBlobId = Guid.NewGuid();
    var expiredFileName = GetExpectedFilePath(expiredBlobId);
    var expiredModifiedDate = DateTime.UtcNow.AddDays(-2);

    _fileSystemMock.Setup(fs => fs.IsDirectoryExist("./")).Returns(true);
    _fileSystemMock.Setup(fs => fs.SearchFiles("./", "*.blob", SearchOption.AllDirectories))
        .Returns(new[] { expiredFileName });
    _fileSystemMock.Setup(fs => fs.GetFileLastWriteTimeUtc(expiredFileName)).Returns(expiredModifiedDate);

    // Act
    var deletedBlobs = new List<Guid>();
    foreach (var blobId in _fileStorage.DeleteExpiredBlobs(expirationDate, CancellationToken.None))
    {
      deletedBlobs.Add(blobId);
    }

    // Assert
    Assert.AreEqual(1, deletedBlobs.Count);
    Assert.AreEqual(expiredBlobId, deletedBlobs[0]);
    _fileSystemMock.Verify(fs => fs.DeleteFile(expiredFileName), Times.Once);
  }

  [TestMethod]
  public void DeleteExpiredBlobs_WhenCancellationTokenRequested_ShouldStopProcessing()
  {
    // Arrange
    var expirationDate = DateTime.UtcNow.AddDays(-1);
    var cancellationTokenSource = new CancellationTokenSource();

    _fileSystemMock.Setup(fs => fs.IsDirectoryExist("./")).Returns(true);
    _fileSystemMock.Setup(fs => fs.SearchFiles("./", "*.blob", SearchOption.AllDirectories))
        .Returns(new[] { "file1.blob", "file2.blob" });

    // Act
    cancellationTokenSource.Cancel();
    var deletedBlobs = _fileStorage.DeleteExpiredBlobs(expirationDate, cancellationTokenSource.Token);
    var result = new List<Guid>(deletedBlobs);

    // Assert
    Assert.AreEqual(0, result.Count);
  }

  #endregion

  #region EmptyRecycleBin Method Tests

  [TestMethod]
  public void EmptyRecycleBin_WhenSafeDeleteEnabled_ShouldDeleteAllFiles()
  {
    // Arrange
    var blobId1 = Guid.NewGuid();
    var blobId2 = Guid.NewGuid();
    var file1 = Path.Combine(RecycleBinDirectoryName, $"{blobId1}.blob");
    var file2 = Path.Combine(RecycleBinDirectoryName, $"{blobId2}.blob");

    _fileSystemMock.Setup(fs => fs.IsDirectoryExist(RecycleBinDirectoryName)).Returns(true);
    _fileSystemMock.Setup(fs => fs.SearchFiles(RecycleBinDirectoryName, "*.blob", SearchOption.TopDirectoryOnly))
        .Returns(new[] { file1, file2 });

    // Act
    var deletedBlobs = new List<Guid>();
    foreach (var blobId in _fileStorage.EmptyRecycleBin(CancellationToken.None))
    {
      deletedBlobs.Add(blobId);
    }

    // Assert
    Assert.AreEqual(2, deletedBlobs.Count);
    _fileSystemMock.Verify(fs => fs.DeleteFile(file1), Times.Once);
    _fileSystemMock.Verify(fs => fs.DeleteFile(file2), Times.Once);
  }

  [TestMethod]
  public void EmptyRecycleBin_WhenSafeDeleteDisabled_ShouldReturnEmpty()
  {
    // Arrange
    var storageWithoutSafeDelete = new FileStorage(_fileSystemMock.Object, _testMode, false);

    // Act
    var deletedBlobs = storageWithoutSafeDelete.EmptyRecycleBin(CancellationToken.None);
    var result = new List<Guid>(deletedBlobs);

    // Assert
    Assert.AreEqual(0, result.Count);
  }

  #endregion

  #region Truncate Method Tests

  [TestMethod]
  public void Truncate_WhenEmptyDirectoriesExist_ShouldDeleteThem()
  {
    // Arrange
    var emptyDir1 = "empty1";
    var emptyDir2 = "empty2";

    _fileSystemMock.Setup(fs => fs.SearchDirectories("./"))
        .Returns(new[] { emptyDir1, emptyDir2 });
    _fileSystemMock.Setup(fs => fs.IsDirectoryEmpty(emptyDir1)).Returns(true);
    _fileSystemMock.Setup(fs => fs.IsDirectoryEmpty(emptyDir2)).Returns(true);

    // Act
    _fileStorage.Truncate(CancellationToken.None);

    // Assert
    _fileSystemMock.Verify(fs => fs.DeleteDirectory(emptyDir1, false), Times.Once);
    _fileSystemMock.Verify(fs => fs.DeleteDirectory(emptyDir2, false), Times.Once);
  }

  [TestMethod]
  public void Truncate_WhenDirectoriesNotEmpty_ShouldNotDeleteThem()
  {
    // Arrange
    var nonEmptyDir = "nonempty";

    _fileSystemMock.Setup(fs => fs.SearchDirectories("./"))
        .Returns(new[] { nonEmptyDir });
    _fileSystemMock.Setup(fs => fs.IsDirectoryEmpty(nonEmptyDir)).Returns(false);

    // Act
    _fileStorage.Truncate(CancellationToken.None);

    // Assert
    _fileSystemMock.Verify(fs => fs.DeleteDirectory(nonEmptyDir, false), Times.Never);
  }

  #endregion

  #region Helper Methods

  private string GetExpectedFilePath(Guid blobId)
  {
    // Реализация из FileStorage.GetFilePath
    long first64BitsOfId = BitConverter.ToInt64(blobId.ToByteArray(), 0);
    var result = Math.DivRem(
        first64BitsOfId % (10000 * 10000),
        10000,
        out var remainder);
    return Path.Combine(remainder.ToString(), result.ToString(), $"{blobId}.blob");
  }

  #endregion
}
