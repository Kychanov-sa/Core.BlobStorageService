using GlacialBytes.Core.BlobStorageService.Kernel;
using GlacialBytes.Core.BlobStorageService.Kernel.Exceptions;
using GlacialBytes.Core.BlobStorageService.Options;
using GlacialBytes.Core.BlobStorageService.Services.Errors;
using GlacialBytes.Core.BlobStorageService.Services.Results;
using Microsoft.Extensions.Options;

namespace GlacialBytes.Core.BlobStorageService.Services;

/// <summary>
/// Сервис хранения BLOB объектов.
/// </summary>
/// <param name="storage">Хранилище.</param>
/// <param name="options">Опции хранилища.</param>
internal class BlobStorageService(IBlobStorage storage, IOptions<BlobStorageSettings> options) : IBlobStorageService
{
  #region IBlobStorageService

  /// <summary>
  /// <see cref="IBlobStorageService.IsArchiveStorage"/>
  /// </summary>
  public bool IsArchiveStorage { get; } = options.Value.StorageMode == Options.BlobStorageMode.Archival;

  /// <summary>
  /// <see cref="IBlobStorageService.CopyBlob(BlobId, BlobId, CancellationToken)"/>
  /// </summary>
  public async Task<CreateBlobResult> CopyBlob(BlobId sourceBlobId, BlobId destBlobId, CancellationToken cancellationToken)
  {
    if (sourceBlobId == destBlobId)
      return CreateBlobResult.Failed(new BlobStorageError(BlobStorageErrorCodes.InvalidOperation, "Source BLOB id cannot be equal to destination BLOB id."));

    return await Task.Run(() =>
    {
      try
      {
        var blob = storage.Copy(sourceBlobId.Value, destBlobId.Value, cancellationToken);
        return CreateBlobResult.Success(blob.Created, blob.Hash);
      }
      catch (OperationNotAllowedException ex)
      {
        return CreateBlobResult.Failed(new BlobStorageError(BlobStorageErrorCodes.InvalidOperation, ex.Message));
      }
      catch (BlobNotExistsException ex)
      {
        return CreateBlobResult.Failed(new BlobStorageError(BlobStorageErrorCodes.NotFound, ex.Message));
      }
    }, cancellationToken);
  }

  /// <summary>
  /// <see cref="IBlobStorageService.DeleteBlob(BlobId, CancellationToken)"/>
  /// </summary>
  public async Task DeleteBlob(BlobId blobId, CancellationToken cancellationToken)
  {
    await Task.Run(() =>
    {
      try
      {
        storage.Delete(blobId.Value, cancellationToken);
      }
      catch (BlobNotExistsException)
      {
        // ignore
      }
    }, cancellationToken);
  }

  /// <summary>
  /// <see cref="IBlobStorageService.FindBlob(BlobId)"/>
  /// </summary>
  public BlobInfo? FindBlob(BlobId blobId)
  {
    return storage.Get(blobId.Value);
  }

  /// <summary>
  /// <see cref="IBlobStorageService.ReadBlobChunk(BlobId, long, long, CancellationToken)"/>
  /// </summary>
  public async Task<ReadBlobResult> ReadBlobChunk(BlobId blobId, long offset, long size, CancellationToken cancellationToken)
  {
    try
    {
      var blobStream = await storage.ReadAsync(blobId.Value, offset, size, cancellationToken);
      return ReadBlobResult.Success(blobStream);
    }
    catch (BlobNotExistsException ex)
    {
      return ReadBlobResult.Failed(new BlobStorageError(BlobStorageErrorCodes.NotFound, ex.Message));
    }
  }

  /// <summary>
  /// <see cref="IBlobStorageService.RestoreBlob(BlobId, CancellationToken)"/>
  /// </summary>
  public async Task<CreateBlobResult> RestoreBlob(BlobId blobId, CancellationToken cancellationToken)
  {
    return await Task.Run(() =>
    {
      try
      {
        var blob = storage.Restore(blobId.Value, cancellationToken);
        return CreateBlobResult.Success(blob.Created, blob.Hash);
      }
      catch (OperationNotAllowedException ex)
      {
        return CreateBlobResult.Failed(new BlobStorageError(BlobStorageErrorCodes.InvalidOperation, ex.Message));
      }
      catch (BlobNotExistsException ex)
      {
        return CreateBlobResult.Failed(new BlobStorageError(BlobStorageErrorCodes.NotFound, ex.Message));
      }
    }, cancellationToken);
  }

  /// <summary>
  /// <see cref="IBlobStorageService.WriteBlobChunk(BlobId, long, long, Stream, CancellationToken)"/>
  /// </summary>
  public async Task<WriteBlobResult> WriteBlobChunk(BlobId blobId, long offset, long size, Stream dataStream, CancellationToken cancellationToken)
  {
    try
    {
      var blob = await storage.WriteAsync(blobId.Value, offset, size, dataStream, cancellationToken);
      return WriteBlobResult.Success(blob.Modified, blob.Hash);
    }
    catch (OperationNotAllowedException ex)
    {
      return WriteBlobResult.Failed(new BlobStorageError(BlobStorageErrorCodes.InvalidOperation, ex.Message));
    }
  }
  #endregion
}
