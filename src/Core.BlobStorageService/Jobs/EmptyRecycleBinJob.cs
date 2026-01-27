using GlacialBytes.Core.BlobStorage.Kernel;
using GlacialBytes.Core.BlobStorage.Services;
using Microsoft.Extensions.Options;
using Quartz;

namespace GlacialBytes.Core.BlobStorage.Jobs;

/// <summary>
/// Выполняет очистку корзины.
/// </summary>
[DisallowConcurrentExecution]
public class EmptyRecycleBinJob : BaseStorageMaintenanceJob
{
  /// <summary>
  /// Конструктор.
  /// </summary>
  /// <param name="storageFactory">Фабрика хранилищ.</param>
  /// <param name="options">Опции хранилища.</param>
  /// <param name="logger">Логгер.</param>
  public EmptyRecycleBinJob(IBlobStorageFactory storageFactory, IOptions<BlobStorageServiceSettings> options, ILogger<DeleteExpiredBlobsJob> logger)
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
    return _settings.EnableDeleteToRecycleBin;
  }

  /// <summary>
  /// <see cref="BaseStorageMaintenanceJob.Execute(CancellationToken)"/>
  /// </summary>
  protected override Task Execute(CancellationToken cancellationToken)
  {
    var deletedBlobIds = _storage.EmptyRecycleBin(cancellationToken);
    if (deletedBlobIds.Any())
    {
      _logger.LogInformation("{deletedCount} blobs deleted from recycle bin.", deletedBlobIds.Count());
    }
    else
    {
      _logger.LogInformation("Recycle bin is empty.");
    }
    return Task.CompletedTask;
  }

  #endregion
}
