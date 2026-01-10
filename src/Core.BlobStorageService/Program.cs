
using FluentValidation;
using GlacialBytes.Core.BlobStorageService.Endpoints;
using GlacialBytes.Core.BlobStorageService.Options;
using Microsoft.AspNetCore.Http.Features;
using System.Diagnostics;

namespace GlacialBytes.Core.BlobStorageService;

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

    builder.Services.AddAuthorization();

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
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
    app.UseAuthorization();

    app.MapBlobsApiEndpoint();

    //app.MapGet("/weatherforecast", (HttpContext httpContext) =>
    //{
    //  var forecast = Enumerable.Range(1, 5).Select(index =>
    //          new WeatherForecast
    //        {
    //          Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
    //          TemperatureC = Random.Shared.Next(-20, 55),
    //          Summary = summaries[Random.Shared.Next(summaries.Length)]
    //        })
    //          .ToArray();
    //  return forecast;
    //})
    //.WithName("GetWeatherForecast")
    //.WithOpenApi();

    app.Run();
  }
}
