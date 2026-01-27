using GlacialBytes.Core.BlobStorage.Kernel;
using GlacialBytes.Core.BlobStorage.Services;
using Microsoft.Extensions.Options;
using Quartz;

namespace GlacialBytes.Core.BlobStorage.Jobs;

/// <summary>
/// Выполняет удаление пустых директорий.
/// </summary>
[DisallowConcurrentExecution]
public class TruncateEmptyDirectoriesJob : BaseStorageMaintenanceJob
{
  /// <summary>
  /// Конструктор.
  /// </summary>
  /// <param name="storageFactory">Фабрика хранилищ.</param>
  /// <param name="options">Опции хранилища.</param>
  /// <param name="logger">Логгер.</param>
  public TruncateEmptyDirectoriesJob(IBlobStorageFactory storageFactory, IOptions<BlobStorageServiceSettings> options, ILogger<DeleteExpiredBlobsJob> logger)
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
    return _settings.EnableEmptyDirectoriesTruncation;
  }

  /// <summary>
  /// <see cref="BaseStorageMaintenanceJob.Execute(CancellationToken)"/>
  /// </summary>
  protected override Task Execute(CancellationToken cancellationToken)
  {
    _storage.Truncate(cancellationToken);
    _logger.LogInformation("Empty directories truncated.");
    return Task.CompletedTask;
  }

  #endregion
}
