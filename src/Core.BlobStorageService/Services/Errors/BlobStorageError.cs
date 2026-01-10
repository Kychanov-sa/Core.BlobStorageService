namespace GlacialBytes.Core.BlobStorageService.Services.Errors;

/// <summary>
/// Описание ошибки при работе с хранилищем BLOB объектов.
/// </summary>
/// <param name="Code">Код ошибки.</param>
/// <param name="Description">Описание ошибки.</param>
public record BlobStorageError(string Code, string? Description);