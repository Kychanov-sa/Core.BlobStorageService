
using GlacialBytes.Core.BlobStorageService.Options;
using Microsoft.AspNetCore.Http.Features;
using System.Diagnostics;

namespace GlacialBytes.Core.BlobStorageService;

public class Program
{
  public static void Main(string[] args)
  {
    var builder = WebApplication.CreateBuilder(args);

    // Configuration
    builder.Services
      .AddOptions<BlobStorageOptions>()
      .BindConfiguration("Storage")
      .ValidateDataAnnotations()
      .ValidateOnStart();

    // Add services to the container.
    builder.AddServiceDefaults();
    builder.Services.AddProblemDetails(options =>
    {
      options.CustomizeProblemDetails = context =>
      {
        context.ProblemDetails.Instance = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
        context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);

        Activity? activity = context.HttpContext.Features.Get<IHttpActivityFeature>()?.Activity;
        context.ProblemDetails.Extensions.TryAdd("traceId", activity?.Id);

        if (builder.Environment.IsDevelopment())
        {
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

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
      app.UseSwagger();
      app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseAuthorization();

    var summaries = new[]
    {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

    app.MapGet("/weatherforecast", (HttpContext httpContext) =>
    {
      var forecast = Enumerable.Range(1, 5).Select(index =>
              new WeatherForecast
            {
              Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
              TemperatureC = Random.Shared.Next(-20, 55),
              Summary = summaries[Random.Shared.Next(summaries.Length)]
            })
              .ToArray();
      return forecast;
    })
    .WithName("GetWeatherForecast")
    .WithOpenApi();

    app.Run();
  }
}
