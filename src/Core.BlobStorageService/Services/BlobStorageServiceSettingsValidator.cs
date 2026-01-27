using FluentValidation;

namespace GlacialBytes.Core.BlobStorage.Services;

/// <summary>
/// Валидатор настроек сервиса хранения BLOB объектов.
/// </summary>
public class BlobStorageServiceSettingsValidator : AbstractValidator<BlobStorageServiceSettings>
{
  /// <summary>
  /// Конструктор.
  /// </summary>
  public BlobStorageServiceSettingsValidator(IHostEnvironment env)
  {
    RuleFor(x => x.StorageId).NotEmpty();

    RuleFor(x => x.TruncationSchedule).NotEmpty().When(options => options.EnableEmptyDirectoriesTruncation);
    RuleFor(x => x.DeleteExpiredBlobsSchedule).NotEmpty().When(options => options.EnableDeletionOfExpiredBlobs);
    RuleFor(x => x.RecyclingSchedule).NotEmpty().When(options => options.EnableDeleteToRecycleBin);
  }
}
