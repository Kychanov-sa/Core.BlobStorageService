using Microsoft.Extensions.Options;

namespace GlacialBytes.Core.BlobStorageService.Options;

/// <summary>
/// Расширение построителя опций.
/// </summary>
public static class OptionsBuilderExtensions
{
  /// <summary>
  /// Добавляет валидацию опций через FluentValidate.
  /// </summary>
  /// <typeparam name="TOptions">Тип опций.</typeparam>
  /// <param name="builder">Построитель.</param>
  /// <returns>Построитель опций.</returns>
  public static OptionsBuilder<TOptions> ValidateFluentValidation<TOptions>(
      this OptionsBuilder<TOptions> builder)
      where TOptions : class
  {
    builder.Services.AddSingleton<IValidateOptions<TOptions>>(
        serviceProvider => new FluentValidateOptions<TOptions>(
            serviceProvider,
            builder.Name));

    return builder;
  }
}
