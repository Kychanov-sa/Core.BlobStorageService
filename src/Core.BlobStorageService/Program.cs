using FluentValidation;
using GlacialBytes.Core.BlobStorage.Endpoints;
using GlacialBytes.Core.BlobStorage.Kernel;
using GlacialBytes.Core.BlobStorage.Options;
using GlacialBytes.Core.BlobStorage.Persistence;
using GlacialBytes.Core.BlobStorage.Services;
using Microsoft.Extensions.Options;

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

    // Валидаторы
    builder.Services.AddScoped<IValidator<BlobStorageSettings>, BlobStorageSettingsValidator>();

    // Опции
    builder.Services
      .AddOptions<BlobStorageSettings>()
      .BindConfiguration("Storage")
      .ValidateDataAnnotations()
      .ValidateFluentValidation()
      .ValidateOnStart();

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
    builder.Services.AddSingleton<IFileSystem, LocalFileSystem>((sp) =>
    {
      var options = sp.GetRequiredService<IOptions<BlobStorageSettings>>();
      return new LocalFileSystem(options.Value.StorageDirectory);
    });
    builder.Services.AddSingleton<IBlobStorage, FileStorage>((sp) =>
    {
      var options = sp.GetRequiredService<IOptions<BlobStorageSettings>>();
      var fileSystem = sp.GetRequiredService<IFileSystem>();
      var mode = options.Value.StorageMode switch
      {
        Options.BlobStorageMode.Temporary => Kernel.BlobStorageMode.ReadAndWrite,
        Options.BlobStorageMode.Persistent => Kernel.BlobStorageMode.ReadAndWrite,
        Options.BlobStorageMode.Archival => Kernel.BlobStorageMode.ReadAndAppendOnly,
        _ => Kernel.BlobStorageMode.ReadOnly,
      };
      return new FileStorage(fileSystem, mode, options.Value.EnableDeleteToRecycleBin);
    });
    builder.Services.AddSingleton<IBlobStorageService, BlobStorageService>();

    // Подключаем фоновые задачи

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
}
