using GlacialBytes.Core.BlobStorage.Kernel;
using GlacialBytes.Core.BlobStorage.Services;
using Microsoft.Extensions.Options;
using Quartz;

namespace GlacialBytes.Core.BlobStorage.Jobs;

/// <summary>
/// Базовый класс задания обслуживания хранилища.
/// </summary>
/// <param name="storageFactory">Фабрика хранилищ.</param>
/// <param name="options">Опции хранилища.</param>
/// <param name="logger">Логгер.</param>
public abstract class BaseStorageMaintenanceJob(IBlobStorageFactory storageFactory, IOptions<BlobStorageServiceSettings> options, ILogger<DeleteExpiredBlobsJob> logger) : IJob
{
  /// <summary>
  /// Хранилище.
  /// </summary>
  protected readonly IBlobStorage _storage = CreateStorage(storageFactory, options.Value);

  /// <summary>
  /// Логгер.
  /// </summary>
  protected readonly ILogger _logger = logger;

  /// <summary>
  /// Настройки.
  /// </summary>
  protected readonly BlobStorageServiceSettings _settings = options.Value;

  #region IJob

  /// <summary>
  /// <see cref="IJob.Execute(IJobExecutionContext)"/>
  /// </summary>
  public Task Execute(IJobExecutionContext context)
  {
    string jobName = context.JobDetail.Key.Name;
    if (CanExecute())
    {
      using var scope = _logger.BeginScope("{jobName} interation (refire count: {refireCount})", jobName, context.RefireCount);
      try
      {
        return Execute(context.CancellationToken);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "{jobName} failed.", jobName);
        return Task.FromException(ex);
      }
    }
    return Task.CompletedTask;
  }

  #endregion

  /// <summary>
  /// Возвращает признак возможности выполнения задания.
  /// </summary>
  /// <returns></returns>
  protected abstract bool CanExecute();

  /// <summary>
  /// Выполняет задание.
  /// </summary>
  protected abstract Task Execute(CancellationToken cancellationToken);

  /// <summary>
  /// Создаёт хранилище для BLOB объектов.
  /// </summary>
  /// <param name="storageFactory">Фабрика хранилищ.</param>
  /// <param name="options">Опции.</param>
  /// <returns>Хранилище BLOB объектов.</returns>
  private static IBlobStorage CreateStorage(IBlobStorageFactory storageFactory, BlobStorageServiceSettings options)
  {
    var mode = options.StorageMode switch
    {
      Services.BlobStorageMode.Temporary => Kernel.BlobStorageMode.ReadAndWrite,
      Services.BlobStorageMode.Persistent => Kernel.BlobStorageMode.ReadAndWrite,
      Services.BlobStorageMode.Archival => Kernel.BlobStorageMode.ReadAndAppendOnly,
      _ => Kernel.BlobStorageMode.ReadOnly,
    };
    return storageFactory.CreateStorage(mode, options.EnableDeleteToRecycleBin);
  }
}
