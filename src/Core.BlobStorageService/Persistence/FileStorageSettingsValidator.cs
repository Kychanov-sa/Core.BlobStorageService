using FluentValidation;

namespace GlacialBytes.Core.BlobStorage.Persistence;

/// <summary>
/// Валидатор настроек файлового хранилища.
/// </summary>
public class FileStorageSettingsValidator : AbstractValidator<FileStorageSettings>
{
  /// <summary>
  /// Конструктор.
  /// </summary>
  public FileStorageSettingsValidator(IHostEnvironment env)
  {
    RuleFor(x => x.StorageDirectory)
      .NotEmpty()
      .Must(storagePath =>
      {
        return IsValidPath(storagePath, env.IsDevelopment()) && Directory.Exists(storagePath);
      })
      .WithMessage($"{nameof(FileStorageSettings.StorageDirectory)} must be a valid local directory path and the directory should exists.");
  }

  /// <summary>
  /// Проверяет является ли указанный путь корректным.
  /// </summary>
  /// <param name="path">Проверямый путь.</param>
  /// <param name="allowRelativePaths">Признак допустимости относительных путей.</param>
  /// <returns>true - если путь корректен, иначе false.</returns>
  private static bool IsValidPath(string path, bool allowRelativePaths = false)
  {
    bool isValid;
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
