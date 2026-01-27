using GlacialBytes.Core.BlobStorage.Kernel;
using GlacialBytes.Core.BlobStorage.Services;
using Microsoft.Extensions.Options;
using Quartz;

namespace GlacialBytes.Core.BlobStorage.Jobs;

/// <summary>
/// Выполняет удаление BLOB объектов с истёкшим сроком.
/// </summary>
[DisallowConcurrentExecution]
public class DeleteExpiredBlobsJob: BaseStorageMaintenanceJob
{
  /// <summary>
  /// Конструктор.
  /// </summary>
  /// <param name="storageFactory">Фабрика хранилищ.</param>
  /// <param name="options">Опции хранилища.</param>
  /// <param name="logger">Логгер.</param>
  public DeleteExpiredBlobsJob(IBlobStorageFactory storageFactory, IOptions<BlobStorageServiceSettings> options, ILogger<DeleteExpiredBlobsJob> logger)
    : base(storageFactory, options, logger)
  {
  }

  #region BaseStorageMaintenanceJob

  /// <summary>
  /// <see cref="BaseStorageMaintenanceJob.CanExecute"/>
  /// </summary>
  /// <returns></returns>
  protected override bool CanExecute()
  {
    return _settings.EnableDeletionOfExpiredBlobs;
  }

  /// <summary>
  /// <see cref="BaseStorageMaintenanceJob.Execute(CancellationToken)"/>
  /// </summary>
  protected override Task Execute(CancellationToken cancellationToken)
  {
    var deletedBlobIds = _storage.DeleteExpiredBlobs(DateTime.UtcNow - _settings.BlobRetention, cancellationToken);
    if (deletedBlobIds.Any())
    {
      _logger.LogInformation("{deletedCount} expired blobs deleted.", deletedBlobIds.Count());
    }
    else
    {
      _logger.LogInformation("No expired blobs found.");
    }
    return Task.CompletedTask;
  }

  #endregion
}
