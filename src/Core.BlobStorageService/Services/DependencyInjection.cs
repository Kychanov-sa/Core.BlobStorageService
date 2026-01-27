using FluentValidation;
using GlacialBytes.Core.BlobStorage.Jobs;
using GlacialBytes.Core.BlobStorage.Options;
using Quartz;

namespace GlacialBytes.Core.BlobStorage.Services;

/// <summary>
/// Инжектор зависимостей.
/// </summary>
public static class DependencyInjection
{
  /// <summary>
  /// Добавляет сервисы хранилища BLOB объектов.
  /// </summary>
  /// <param name="services">Коллекция сервисов.</param>
  /// <param name="configuration">Конфигурация.</param>
  /// <returns>Дополненная коллекция сервисов.</returns>
  public static IServiceCollection AddBlobStorageServices(this IServiceCollection services, IConfiguration configuration)
  {
    // валидаторы
    services.AddScoped<IValidator<BlobStorageServiceSettings>, BlobStorageServiceSettingsValidator>();

    // опции
    services.AddOptions<BlobStorageServiceSettings>()
      .Bind(configuration)
      .ValidateDataAnnotations()
      .ValidateFluentValidation()
      .ValidateOnStart();

    // сервисы
    services.AddSingleton<IBlobStorageService, BlobStorageService>();
    return services;
  }

  /// <summary>
  /// Конфигурирует запуск фоновых заданий хранилища BLOB объектов.
  /// </summary>
  /// <param name="serviceCollectionConfigurator">Конфигуратор коллекции сервисов Quartz.</param>
  /// <param name="configuration">Конфигурация.</param>
  /// <returns>Конфигуратор коллекции c новыми заданиями и триггерами.</returns>
  public static IServiceCollectionQuartzConfigurator AddBlobStorageJobs(this IServiceCollectionQuartzConfigurator serviceCollectionConfigurator, IConfiguration configuration)
  {
    string recyclingSchedule = configuration.GetValue<string>(nameof(BlobStorageServiceSettings.RecyclingSchedule))!;
    string truncationSchedule = configuration.GetValue<string>(nameof(BlobStorageServiceSettings.TruncationSchedule))!;
    string deleteExpiredBlobsSchedule = configuration.GetValue<string>(nameof(BlobStorageServiceSettings.DeleteExpiredBlobsSchedule))!;

    AddBlobStorageMaintenanceJob<EmptyRecycleBinJob>(serviceCollectionConfigurator, "EmptyRecycleBin", recyclingSchedule);
    AddBlobStorageMaintenanceJob<DeleteExpiredBlobsJob>(serviceCollectionConfigurator, "DeleteExpiredBlobs", deleteExpiredBlobsSchedule);
    AddBlobStorageMaintenanceJob<TruncateEmptyDirectoriesJob>(serviceCollectionConfigurator, "TruncateEmptyDirectories", truncationSchedule);
    
    return serviceCollectionConfigurator;
  }

  /// <summary>
  /// Добавляет задание в конфигурацию Quartz.
  /// </summary>
  /// <typeparam name="TJob">Тип задания.</typeparam>
  /// <param name="configurator">Конфигуратор.</param>
  /// <param name="jobName">Имя задания.</param>
  /// <param name="schedulerCronExpression">Cron-выражение планировщика.</param>
  private static void AddBlobStorageMaintenanceJob<TJob>(IServiceCollectionQuartzConfigurator configurator, string jobName, string schedulerCronExpression) where TJob : IJob
  {
    var jobKey = new JobKey(jobName, "BlobStorageMaintenance");
    configurator.AddJob<TJob>(opts => opts.WithIdentity(jobKey));
    configurator.AddTrigger(options => options
        .ForJob(jobKey)
        .WithIdentity($"{jobName}-trigger")
        .WithCronSchedule(schedulerCronExpression));
  }
}
