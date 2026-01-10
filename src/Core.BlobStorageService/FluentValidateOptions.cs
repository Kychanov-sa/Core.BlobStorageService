using FluentValidation;
using Microsoft.Extensions.Options;

namespace GlacialBytes.Core.BlobStorageService;

/// <summary>
/// Валидация опций через FluentValidation.
/// </summary>
/// <param name="serviceProvider"> Провайдер сервисов.</param>
/// <param name="optionsName">Наименования класса опций.</param>
/// <typeparam name="TOptions">Тип опций.</typeparam>
public class FluentValidateOptions<TOptions>(IServiceProvider serviceProvider, string? optionsName)
    : IValidateOptions<TOptions>
    where TOptions : class
{
  #region IValidateOptions<TOptions>

  /// <summary>
  /// <see cref="IValidateOptions{TOptions}.Validate(string?, TOptions)"/>
  /// </summary>
  public ValidateOptionsResult Validate(string? name, TOptions options)
  {
    if (optionsName is not null && optionsName != name)
    {
      return ValidateOptionsResult.Skip;
    }

    ArgumentNullException.ThrowIfNull(options);

    using var scope = serviceProvider.CreateScope();

    var validator = scope.ServiceProvider.GetRequiredService<IValidator<TOptions>>();

    var result = validator.Validate(options);
    if (result.IsValid)
    {
      return ValidateOptionsResult.Success;
    }

    var type = options.GetType().Name;
    var errors = new List<string>();

    foreach (var failure in result.Errors)
    {
      errors.Add($"Validation failed for {type}.{failure.PropertyName} " +
                 $"with the error: {failure.ErrorMessage}");
    }

    return ValidateOptionsResult.Fail(errors);
  }

  #endregion
}
