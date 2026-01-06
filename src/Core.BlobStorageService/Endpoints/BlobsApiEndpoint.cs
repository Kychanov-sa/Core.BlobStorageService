//using AutoMapper;
//using GlacialBytes.Core.EventStorageService.ApiService.Domain.Exceptions;
//using GlacialBytes.Core.EventStorageService.ApiService.Services;
//using GlacialBytes.Core.EventStorageService.ApiShared;
using AutoMapper;
using GlacialBytes.Core.BlobStorageService.Domain;
using GlacialBytes.Core.BlobStorageService.Exceptions;
using GlacialBytes.Core.BlobStorageService.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
//using Entities = GlacialBytes.Core.EventStorageService.ApiService.Domain.Entities;
//using ValueObjects = GlacialBytes.Core.EventStorageService.ApiService.Domain.ValueObjects;

namespace GlacialBytes.Core.BlobStorageService.Endpoints;

/// <summary>
/// Конечная точка API для работы с BLOB объектами.
/// </summary>
[ApiVersion("1.0")]
public static class BlobsApiEndpoint
{
  /// <summary>Автосопоставитель моделей.</summary>
  private static readonly Mapper _mapper;

  /// <summary>
  /// Статический конструктор.
  /// </summary>
  static BlobsApiEndpoint()
  {
    var mapperConfiguration = new MapperConfiguration(cfg =>
    {
      cfg.CreateMap<BlobEntity, BlobDescription>();
    });
    _mapper = new Mapper(mapperConfiguration);
  }

  /// <summary>
  /// Добавляет методы конечной точки API BLOB объектов.
  /// </summary>
  /// <param name="app">Настраиваемое веб-приложение.</param>
  public static void MapBlobsApiEndpoint(this WebApplication app)
  {
    // Запрос метаинформации
    app.MapMethods("/blobs/{blobId}", [HttpMethod.Head.Method], GetBlobMeta)
      .Produces(StatusCodes.Status204NoContent)
      .Produces(StatusCodes.Status404NotFound)
      .WithName("GetBlobMeta");

    // Копирование
    app.MapPost("/blobs/{blobId}/copy", CopyBlob)
      .Produces<BlobDescription>(StatusCodes.Status201Created)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status404NotFound)
      .Produces(StatusCodes.Status409Conflict)
      .WithName("CopyBlob");

    // Удаление
    app.MapDelete("/blobs/{blobId}", DeleteBlob)
      .Produces(StatusCodes.Status204NoContent)
      .Produces(StatusCodes.Status400BadRequest)
      .WithName("DeleteBlob");

    // Восстановление
    app.MapPost("/blobs/{blobId}/restore", RestoreBlob)
      .Produces<BlobDescription>(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status404NotFound)
      .WithName("RestoreBlob");

    // Запись
    app.MapPut("/blobs/{blobId}", WriteBlobChunk)
      .Produces<BlobDescription>(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status400BadRequest)
      .WithName("WriteBlob");

    // Чтение
    app.MapGet("/blobs/{blobId}", ReadBlobChunk)
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
  private static async Task<IResult> GetBlobMeta(
        [SwaggerParameter("Идентификатор BLOB объекта.")] Guid blobId,
        HttpContext context,
        IBlobStorageService blobStorageService,
        CancellationToken cancellationToken)
  {
    var blob = await blobStorageService.GetBlob(blobId, cancellationToken);
    if (blob == null)
      return Results.Problem($"Blob with id {blobId} is not found.", null, (int)HttpStatusCode.NotFound, "Blob is not found");

    if (blob.Created != null)
      context.Response.Headers.Append(BlobStorageHttpHeaders.BlobCreated, blob.Created?.ToString("O"));
    if (blob.Modified != null)
      context.Response.Headers.Append(BlobStorageHttpHeaders.BlobModified, blob.Modified?.ToString("O"));
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
    var result = await blobStorageService.RestoreBlob(blobId, cancellationToken);
    if (!result.Succeeded)
    {
      return Results.Problem(result.Errors.ToProblemDetails());
    }

    return Results.Created(new BlobDescription()
    {
       Created = result.Created,
       Modified = result.Created,
       Hash = result.Hash,
        Id = blobId,
         
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
  /// <response code="404">BLOB не найден.</response>
  /// <response code="400">Ошибка удаления.</response>
  [SwaggerOperation("Удаляет бинарные данные.")]
  private static async Task<IResult> DeleteBlob(
        [SwaggerParameter("Идентификатор BLOB объекта.")] Guid blobId,
        IBlobStorageService blobStorageService,
        CancellationToken cancellationToken)
  {
    await blobStorageService.DeleteBlob(blobId, cancellationToken);
    return Results.NoContent();
  }

  /// <summary>
  /// Отменяет удаление бинарных данных.
  /// </summary>
  /// <param name="blobId">Идентификатор BLOB объекта.</param>
  /// <param name="blobStorageService">Сервис хранения.</param>
  /// <param name="cancellationToken">Токен отмены.</param>
  /// <returns>NotFound, если BLOB не найден и метаданные объекта, если успешно восстановлен.</returns>
  /// <response code="200">Метаинформация восстановленного объекта.</response>
  /// <response code="404">BLOB не найден.</response>
  /// <response code="400">Ошибка восстановления.</response>
  [SwaggerOperation("Отменяет удаление бинарных данных.")]
  private static async Task<IResult> RestoreBlob(
        [SwaggerParameter("Идентификатор BLOB объекта.")] Guid blobId,
        IBlobStorageService blobStorageService,
        CancellationToken cancellationToken)
  {
    try
    {
      await blobStorageService.RestoreBlob(blobId, cancellationToken);
      return Results.Ok(_mapper.Map<BlobDescription>(blob));
    }
    catch (ServiceContractException ex)
    {
      return Results.Problem(ex.ToProblemDetails());
    }
  }

  /// <summary>
  /// Записывает чанк данных BLOB объекта.
  /// </summary>
  /// <param name="blobId">Идентификатор BLOB объекта.</param>
  /// <param name="offset">Начальная позиция для записи данных.</param>
  /// <param name="size">Размер чанка записываемых данных.</param>
  /// <param name="blobStorageService">Сервис хранения.</param>
  /// <param name="cancellationToken">Токен отмены.</param>
  /// <returns>BadRequest, если будет ошибка запиис, метаинформация объекта, если успешно записан.</returns>
  /// <response code="200">Метаинформация записанного объекта.</response>
  /// <response code="400">Ошибка записи.</response>
  [SwaggerOperation("Записывает чанк данных BLOB объекта.")]
  private static async Task<IResult> WriteBlobChunk(
        [SwaggerParameter("Идентификатор BLOB объекта.")] Guid blobId,
        [SwaggerParameter("смещение для записи BLOB объекта.")][FromQuery] long? offset,
        [SwaggerParameter("Размер для записи BLOB объекта.")][FromQuery] int? size,
        [SwaggerParameter("Данные BLOB объекта.")][FromBody] Stream dataStream,
        IBlobStorageService blobStorageService,
        CancellationToken cancellationToken)
  {
    var result = await blobStorageService.WriteBlobChunk(blobId, offset ?? 0, size ?? -1, dataStream, cancellationToken);
  }

  /// <summary>
  /// Читает чанк данных BLOB объекта.
  /// </summary>
  /// <param name="blobId">Идентификатор BLOB объекта.</param>
  /// <param name="offset">Начальная позиция для чтения данных.</param>
  /// <param name="size">Размер чанка читаемых данных.</param>
  /// <param name="blobStorageService">Сервис хранения.</param>
  /// <param name="cancellationToken">Токен отмены.</param>
  /// <returns>NotFound, если BLOB не найден и данные объекта, если успешно прочитаны.</returns>
  /// <response code="200">Данные BLOB объекта.</response>
  /// <response code="404">BLOB не найден.</response>
  /// <response code="400">Ошибка чтения.</response>
  [SwaggerOperation("Читает чанк данных BLOB объекта.")]
  private static async Task<IResult> ReadBlobChunk(
        [SwaggerParameter("Идентификатор BLOB объекта.")] Guid blobId,
        [SwaggerParameter("Смещение для чтения BLOB объекта.")][FromQuery] long? offset,
        [SwaggerParameter("Размер чанка читаемых данных BLOB объекта.")][FromQuery] int? size,
        IBlobStorageService blobStorageService,
        CancellationToken cancellationToken)
  {
    var result = await blobStorageService.ReadBlobChunk(blobId, offset ?? 0, size ?? -1, cancellationToken);
  }
}
