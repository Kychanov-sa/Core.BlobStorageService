//using AutoMapper;
using GlacialBytes.Core.BlobStorage.Kernel;
using GlacialBytes.Core.BlobStorage.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace GlacialBytes.Core.BlobStorage.Endpoints;

/// <summary>
/// Конечная точка API для работы с BLOB объектами.
/// </summary>
[ApiVersion("1.0")]
public static class BlobsApiEndpoint
{
  /// <summary>
  /// Добавляет методы конечной точки API BLOB объектов.
  /// </summary>
  /// <param name="app">Настраиваемое веб-приложение.</param>
  public static void MapBlobsApiEndpoint(this WebApplication app)
  {
    // Запрос метаинформации
    app.MapMethods("/blobs/{blobId:guid}", [HttpMethod.Head.Method], GetBlobMeta)
      .Produces(StatusCodes.Status204NoContent)
      .Produces(StatusCodes.Status404NotFound)
      .WithName("GetBlobMeta");

    // Копирование
    app.MapPost("/blobs/{blobId:guid}/copy", CopyBlob)
      .Produces<BlobDescription>(StatusCodes.Status201Created)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status404NotFound)
      .Produces(StatusCodes.Status409Conflict)
      .WithName("CopyBlob");

    // Удаление
    app.MapDelete("/blobs/{blobId:guid}", DeleteBlob)
      .Produces(StatusCodes.Status204NoContent)
      .Produces(StatusCodes.Status400BadRequest)
      .WithName("DeleteBlob");

    // Восстановление
    app.MapPost("/blobs/{blobId:guid}/restore", RestoreBlob)
      .Produces<BlobDescription>(StatusCodes.Status201Created)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status404NotFound)
      .WithName("RestoreBlob");

    // Запись
    app.MapPut("/blobs/{blobId:guid}", WriteBlobChunk)
      .Produces<BlobDescription>(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status400BadRequest)
      .WithName("WriteBlob")
      .DisableAntiforgery();

    // Чтение
    app.MapGet("/blobs/{blobId:guid}", ReadBlobChunk)
      .Produces<BlobDescription>(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status404NotFound)
      .WithName("ReadBlob");
  }

  /// <summary>
  /// Возвращает метаинформацию по BLOB объекту.
  /// </summary>
  /// <param name="blobId">Идентификатор BLOB объекта.</param>
  /// <param name="context">Контекст запроса.</param>
  /// <param name="blobStorageService">Сервис хранения.</param>
  /// <param name="cancellationToken">Токен отмены.</param>
  /// <returns>NotFound, если BLOB не найден и NoContent, если доступен.</returns>
  /// <response code="204">Успешно.</response>
  /// <response code="404">BLOB не найден.</response>
  [SwaggerOperation("Возвращает метаинформацию по BLOB объекту.")]
  private static IResult GetBlobMeta(
        [SwaggerParameter("Идентификатор BLOB объекта.")] Guid blobId,
        HttpContext context,
        IBlobStorageService blobStorageService,
        CancellationToken cancellationToken)
  {
    var blob = blobStorageService.FindBlob(new BlobId(blobId));
    if (blob == null)
      return Results.Problem($"Blob with id {blobId} is not found.", null, (int)HttpStatusCode.NotFound, "Blob is not found");

    context.Response.Headers.Append(BlobStorageHttpHeaders.BlobCreated, blob.Created.ToString("O"));
    context.Response.Headers.Append(BlobStorageHttpHeaders.BlobModified, blob.Modified.ToString("O"));
    if (!String.IsNullOrEmpty(blob.Hash))
      context.Response.Headers.ETag = blob.Hash;
    return Results.NoContent();
  }

  /// <summary>
  /// Копировать бинарные данные.
  /// </summary>
  /// <param name="blobId">Идентификатор целевого BLOB объекта.</param>
  /// <param name="sourceBlobId">Идентификатор исходного BLOB объекта.</param>
  /// <param name="blobStorageService">Сервис хранения.</param>
  /// <param name="cancellationToken">Токен отмены.</param>
  /// <returns>NotFound, если BLOB не найден и NoContent, если доступен.</returns>
  /// <response code="201">Метаинформация созданного объекта.</response>
  /// <response code="404">BLOB не найден.</response>
  /// <response code="409">Идентификатор целевого и исходного объекта совпадает.</response>
  [SwaggerOperation("Создаёт BLOB объект копированием бинарных данных из другого.")]
  private static async Task<IResult> CopyBlob(
        [SwaggerParameter("Идентификатор BLOB объекта.")] Guid blobId,
        [SwaggerParameter("Идентификатор исходного BLOB объекта.")][FromQuery] Guid sourceBlobId,
        IBlobStorageService blobStorageService,
        CancellationToken cancellationToken)
  {
    var result = await blobStorageService.CopyBlob(new BlobId(sourceBlobId), new BlobId(blobId), cancellationToken);
    if (!result.Succeeded)
    {
      return Results.Problem(result.Errors.First().ToProblemDetails());
    }

    return Results.Created(
      $"/blobs/{blobId}",
      new BlobDescription()
      {
        Created = result.Created,
        Modified = result.Created,
        Hash = result.Hash,
        Id = blobId,
        IsReadOnly = blobStorageService.IsArchiveStorage,
      });
  }

  /// <summary>
  /// Удаляет бинарные данные.
  /// </summary>
  /// <param name="blobId">Идентификатор BLOB объекта.</param>
  /// <param name="blobStorageService">Сервис хранения.</param>
  /// <param name="cancellationToken">Токен отмены.</param>
  /// <returns>NotFound, если BLOB не найден и NoContent, если успешно удалён.</returns>
  /// <response code="204">Объект удалён.</response>
  /// <response code="400">Ошибка удаления.</response>
  [SwaggerOperation("Удаляет бинарные данные.")]
  private static async Task<IResult> DeleteBlob(
        [SwaggerParameter("Идентификатор BLOB объекта.")] Guid blobId,
        IBlobStorageService blobStorageService,
        CancellationToken cancellationToken)
  {
    await blobStorageService.DeleteBlob(new BlobId(blobId), cancellationToken);
    return Results.NoContent();
  }

  /// <summary>
  /// Отменяет удаление бинарных данных.
  /// </summary>
  /// <param name="blobId">Идентификатор BLOB объекта.</param>
  /// <param name="blobStorageService">Сервис хранения.</param>
  /// <param name="cancellationToken">Токен отмены.</param>
  /// <returns>NotFound, если BLOB не найден и метаданные объекта, если успешно восстановлен.</returns>
  /// <response code="201">Метаинформация восстановленного объекта.</response>
  /// <response code="404">BLOB не найден.</response>
  /// <response code="400">Ошибка восстановления.</response>
  [SwaggerOperation("Отменяет удаление бинарных данных.")]
  private static async Task<IResult> RestoreBlob(
        [SwaggerParameter("Идентификатор BLOB объекта.")] Guid blobId,
        IBlobStorageService blobStorageService,
        CancellationToken cancellationToken)
  {

    var result = await blobStorageService.RestoreBlob(new BlobId(blobId), cancellationToken);
    if (!result.Succeeded)
    {
      return Results.Problem(result.Errors.First().ToProblemDetails());
    }

    return Results.Created(
       $"/blobs/{blobId}",
     new BlobDescription()
     {
       Created = result.Created,
       Hash = result.Hash,
       Id = blobId,
       IsReadOnly = blobStorageService.IsArchiveStorage,
     });
  }

  ///// <summary>
  ///// Записывает чанк данных BLOB объекта.
  ///// </summary>
  ///// <param name="blobId">Идентификатор BLOB объекта.</param>
  ///// <param name="offset">Начальная позиция для записи данных.</param>
  ///// <param name="size">Размер чанка записываемых данных.</param>
  ///// <param name="dataStream">Записываемые данные.</param>
  ///// <param name="blobStorageService">Сервис хранения.</param>
  ///// <param name="cancellationToken">Токен отмены.</param>
  ///// <returns>BadRequest, если будет ошибка запиис, метаинформация объекта, если успешно записан.</returns>
  ///// <response code="200">Метаинформация записанного объекта.</response>
  ///// <response code="400">Ошибка записи.</response>
  //[SwaggerOperation("Записывает чанк данных BLOB объекта.")]
  ////[Consumes("multipart/form-data")]
  //private static async Task<IResult> WriteBlobChunk(
  //      [SwaggerParameter("Идентификатор BLOB объекта.")] [FromRoute] Guid blobId,
  //      //[SwaggerParameter("смещение для записи BLOB объекта.")][FromForm] long? offset,
  //      //[SwaggerParameter("Размер для записи BLOB объекта.")][FromForm] int? size,
  //      [SwaggerParameter("Данные BLOB объекта.")][FromBody] IFormFile uploadingFile,
  //      //[SwaggerParameter("Загружаемые данные BLOB объекта.")][FromBody] UploadBlobRequest uploadBlob,
  //      IBlobStorageService blobStorageService,
  //      CancellationToken cancellationToken)
  //{
  //  //using var dataStream = uploadBlob.UploadingFile.OpenReadStream();
  //  using var dataStream = uploadingFile.OpenReadStream();
  //  //var result = await blobStorageService.WriteBlobChunk(new BlobId(blobId), offset ?? 0, size ?? -1, dataStream, cancellationToken);
  //  var result = await blobStorageService.WriteBlobChunk(new BlobId(blobId), 0, 1, dataStream, cancellationToken);
  //  if (!result.Succeeded)
  //  {
  //    return Results.Problem(result.Errors.First().ToProblemDetails());
  //  }

  //  return Results.Ok(
  //    new BlobDescription()
  //    {
  //      Modified = result.Modified,
  //      Hash = result.Hash,
  //      Id = blobId,
  //      IsReadOnly = blobStorageService.IsArchiveStorage,
  //    });
  //}

  /// <summary>
  /// Записывает чанк данных BLOB объекта.
  /// </summary>
  /// <param name="blobId">Идентификатор BLOB объекта.</param>
  /// <param name="offset">Начальная позиция для записи данных.</param>
  /// <param name="size">Размер чанка записываемых данных.</param>
  /// <param name="uploadingFile">Загружаемый файл данных.</param>
  /// <param name="blobStorageService">Сервис хранения.</param>
  /// <param name="cancellationToken">Токен отмены.</param>
  /// <returns>BadRequest, если будет ошибка запиис, метаинформация объекта, если успешно записан.</returns>
  /// <response code="200">Метаинформация записанного объекта.</response>
  /// <response code="400">Ошибка записи.</response>
  [SwaggerOperation("Записывает чанк данных BLOB объекта.")]
  private static async Task<IResult> WriteBlobChunk(
        [SwaggerParameter("Идентификатор BLOB объекта.")] Guid blobId,
        [SwaggerParameter("смещение для записи BLOB объекта.")][FromForm] long? offset,
        [SwaggerParameter("Размер для записи BLOB объекта.")][FromForm] long? size,
        [SwaggerParameter("Данные BLOB объекта.")] IFormFile uploadingFile,
        IBlobStorageService blobStorageService,
        CancellationToken cancellationToken)
  {
    using var dataStream = uploadingFile.OpenReadStream();
    var result = await blobStorageService.WriteBlobChunk(new BlobId(blobId), offset ?? 0, size ?? -1, dataStream, cancellationToken);
    if (!result.Succeeded)
    {
      return Results.Problem(result.Errors.First().ToProblemDetails());
    }

    return Results.Ok(
      new BlobDescription()
      {
        Modified = result.Modified,
        Hash = result.Hash,
        Id = blobId,
        IsReadOnly = blobStorageService.IsArchiveStorage,
      });
  }

  /// <summary>
  /// Читает чанк данных BLOB объекта.
  /// </summary>
  /// <param name="blobId">Идентификатор BLOB объекта.</param>
  /// <param name="offset">Начальная позиция для чтения данных.</param>
  /// <param name="size">Размер чанка читаемых данных.</param>
  /// <param name="fileName">Имя для читаемого файла.</param>
  /// <param name="blobStorageService">Сервис хранения.</param>
  /// <param name="cancellationToken">Токен отмены.</param>
  /// <returns>NotFound, если BLOB не найден и данные объекта, если успешно прочитаны.</returns>
  /// <response code="200">Данные BLOB объекта.</response>
  /// <response code="404">BLOB не найден.</response>
  /// <response code="400">Ошибка чтения.</response>
  [SwaggerOperation("Читает чанк данных BLOB объекта.")]
  [SwaggerResponse(200, "Данные BLOB объекта.", typeof(Stream))]
  [SwaggerResponse(404, "BLOB не найден.", typeof(ProblemDetails))]
  [SwaggerResponse(400, "Ошибка чтения.", typeof(ProblemDetails))]
  private static async Task<IResult> ReadBlobChunk(
        [SwaggerParameter("Идентификатор BLOB объекта.")] Guid blobId,
        [SwaggerParameter("Смещение для чтения BLOB объекта.")][FromQuery] long? offset,
        [SwaggerParameter("Размер чанка читаемых данных BLOB объекта.")][FromQuery] long? size,
        [SwaggerParameter("Имя для читаемого файла.")][FromQuery] string? fileName,
        IBlobStorageService blobStorageService,
        CancellationToken cancellationToken)
  {
    var result = await blobStorageService.ReadBlobChunk(new BlobId(blobId), offset ?? 0, size ?? -1, cancellationToken);
    if (!result.Succeeded)
    {
      return Results.Problem(result.Errors.First().ToProblemDetails());
    }

    return Results.Stream(result.DataStream!, fileDownloadName: fileName);
  }
}
