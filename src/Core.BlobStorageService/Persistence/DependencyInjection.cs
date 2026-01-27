using FluentValidation;
using GlacialBytes.Core.BlobStorage.Kernel;
using GlacialBytes.Core.BlobStorage.Options;

namespace GlacialBytes.Core.BlobStorage.Persistence;

/// <summary>
/// Инжектор зависимостей.
/// </summary>
public static class DependencyInjection
{
  /// <summary>
  /// Добавляет сервисы файлового хранилизща.
  /// </summary>
  /// <param name="services">Коллекция сервисов.</param>
  /// <param name="configuration">Конфигурация.</param>
  /// <returns>Дополненная коллекция сервисов.</returns>
  public static IServiceCollection AddFileStorage(this IServiceCollection services, IConfiguration configuration)
  {
    // валидаторы
    services.AddScoped<IValidator<FileStorageSettings>, FileStorageSettingsValidator>();

    // опции
    services.AddOptions<FileStorageSettings>()
      .Bind(configuration)
      .ValidateDataAnnotations()
      .ValidateFluentValidation()
      .ValidateOnStart();

    // сервисы
    services.AddSingleton<IBlobStorageFactory, FileStorageFactory>();
    return services;
  }
}
