using FluentValidation;

namespace GlacialBytes.Core.BlobStorage.Options;

/// <summary>
/// Валидатор настроек хранилища.
/// </summary>
public class BlobStorageSettingsValidator : AbstractValidator<BlobStorageSettings>
{
  /// <summary>
  /// Конструктор.
  /// </summary>
  public BlobStorageSettingsValidator(IHostEnvironment env)
  {
    RuleFor(x => x.StorageId).NotEmpty();

    RuleFor(x => x.StorageDirectory)
      .NotEmpty()
      .Must(storagePath =>
      {
        return IsValidPath(storagePath, env.IsDevelopment()) && Directory.Exists(storagePath);
      })
      .WithMessage($"{nameof(BlobStorageSettings.StorageDirectory)} must be a valid local directory path and the directory should exists.");
  }

  /// <summary>
  /// Проверяет является ли указанный путь корректным.
  /// </summary>
  /// <param name="path">Проверямый путь.</param>
  /// <param name="allowRelativePaths">Признак допустимости относительных путей.</param>
  /// <returns>true - если путь корректен, иначе false.</returns>
  private bool IsValidPath(string path, bool allowRelativePaths = false)
  {
    bool isValid = true;

    try
    {
      string fullPath = Path.GetFullPath(path);
      if (allowRelativePaths)
      {
        isValid = Path.IsPathRooted(path);
      }
      else
      {
        string? root = Path.GetPathRoot(path);
        isValid = String.IsNullOrEmpty(root?.Trim(['\\', '/'])) == false;
      }
    }
    catch (Exception)
    {
      isValid = false;
    }

    return isValid;
  }
}
