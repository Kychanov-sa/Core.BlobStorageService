using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace GlacialBytes.Core.BlobStorage.Client;

/// <summary>
/// Инжектор зависимостей.
/// </summary>
public static class DependencyInjection
{
  /// <summary>
  /// Добавляет клиента сервиса хранения BLOB объектов.
  /// </summary>
  /// <param name="services">Коллекция сервисов.</param>
  /// <param name="serviceConnectionString">Строка подключения к сервису.</param>
  /// <returns>Построитель HTTP клиентов.</returns>
  public static IHttpClientBuilder AddBlobStorageClient(this IServiceCollection services, string serviceConnectionString)
  {
    return services.AddRefitClient<IBlobStorageApi>()
      .ConfigureHttpClient(c => c.BaseAddress = new Uri(serviceConnectionString));
  }
}
