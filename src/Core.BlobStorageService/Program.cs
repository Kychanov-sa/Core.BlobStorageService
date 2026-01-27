using GlacialBytes.Core.BlobStorage.Endpoints;
using GlacialBytes.Core.BlobStorage.Persistence;
using GlacialBytes.Core.BlobStorage.Services;
using Quartz;
using Quartz.AspNetCore;

namespace GlacialBytes.Core.BlobStorage;

/// <summary>
/// Основной класс приложения.
/// </summary>
public class Program
{
  /// <summary>
  /// Точка входа приложения.
  /// </summary>
  /// <param name="args">Аргументы командной строки.</param>
  public static void Main(string[] args)
  {
    var builder = WebApplication.CreateBuilder(args);

    // Добавление сервисов в контейнер
    builder.AddServiceDefaults();
    builder.Services.AddProblemDetails(options =>
    {
      options.CustomizeProblemDetails = context =>
      {
        context.ProblemDetails.Instance = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
        context.ProblemDetails.Extensions.TryAdd("trace-id", context.HttpContext.TraceIdentifier);

        //Activity ? activity = context.HttpContext.Features.Get<IHttpActivityFeature>()?.Activity;
        //context.ProblemDetails.Extensions.TryAdd("traceId", activity?.Id);

        if (builder.Environment.IsDevelopment())
        {
          context.ProblemDetails.Extensions.TryAdd("stack-trace", context.Exception?.StackTrace);
          context.ProblemDetails.Extensions.TryAdd("data", context.Exception?.Data);
        }
      };
    });
    builder.Services.AddExceptionHandler<ServiceExceptionHandler>();

    // Подключаем сервисы хранилища
    AddStorageServices(builder.Services, builder.Configuration);

    // Подключаем фоновые задачи
    AddBackgroundJobs(builder.Services, builder.Configuration);

    // Безопасность
    //builder.Services.AddAuthorization();

    // API
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    app.MapDefaultEndpoints();

    if (app.Environment.IsDevelopment())
    {
      app.UseDeveloperExceptionPage();
      app.UseSwagger();
      app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    //app.UseAuthorization();

    app.MapBlobsApiEndpoint();
    app.Run();
  }

  /// <summary>
  /// Добавляет в коллекцию сервисы хранилища.
  /// </summary>
  /// <param name="services">Коллекция сервисов.</param>
  /// <param name="configuration">Конфигурация приложения.</param>
  /// <returns>Дополненная коллекция сервисов.</returns>
  public static IServiceCollection AddStorageServices(IServiceCollection services, IConfiguration configuration)
  {
    services.AddFileStorage(configuration.GetSection("FileStorage"));
    services.AddBlobStorageServices(configuration.GetSection("StorageService"));

    return services;
  }

  /// <summary>
  /// Добавляет в коллекцию фоновые задания.
  /// </summary>
  /// <param name="services">Коллекция сервисов.</param>
  /// <returns>Дополненная коллекция сервисов.</returns>
  public static IServiceCollection AddBackgroundJobs(IServiceCollection services, IConfiguration configuration)
  {
    services.AddQuartz(configurator =>
    {
      configurator.SchedulerName = "StorageBackgroundOperations";
      configurator.AddBlobStorageJobs(configuration.GetSection("StorageService"));
    });

    services.AddQuartzServer(options =>
    {
      options.WaitForJobsToComplete = true;
      options.AwaitApplicationStarted = true;
    });

    return services;
  }
}
