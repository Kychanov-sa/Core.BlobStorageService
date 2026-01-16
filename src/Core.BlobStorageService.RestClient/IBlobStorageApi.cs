using Refit;
using GlacialBytes.Core.BlobStorage.Exceptions;

namespace GlacialBytes.Core.BlobStorage.Client;

/// <summary>
/// Интерфейс API хранилища BLOB объектов.
/// </summary>
[Headers("Authorization: Bearer")]
public interface IBlobStorageApi
{
  /// <summary>
  /// Возвращает заголовки с описанием BLOB объекта.
  /// </summary>
  /// <param name="blobId">Идентификатор BLOB объекта.</param>
  /// <param name="cancellationToken">Токен отмены.</param>
  /// <returns>Сообщение ответа с заголовками.</returns>
  [Head("/blobs/{id}")]
  Task<HttpResponseMessage> GetBlobDescriptionHeaders([AliasAs("id")] Guid blobId, CancellationToken cancellationToken = default);

  /// <summary>
  /// Копировать бинарные данные.
  /// </summary>
  /// <param name="blobId">Идентификатор целевого BLOB объекта.</param>
  /// <param name="sourceBlobId">Идентификатор исходного BLOB объекта.</param>
  /// <param name="cancellationToken">Токен отмены.</param>
  /// <returns>Описание скопированного объекта.</returns>
  /// <exception cref="BlobNotFoundException">Объект не найден.</exception>
  /// <exception cref="UnexpectedServiceException">Внутренняя ошибка сервиса.</exception>
  /// <exception cref="ServiceContractException">Идентификатор целевого и исходного объекта совпадает.</exception>
  [Post("/blobs/{id}/copy")]
  Task<BlobDescription> CopyBlob([AliasAs("id")] Guid blobId, [Query] Guid sourceBlobId, CancellationToken cancellationToken = default);

  /// <summary>
  /// Удалить бинарные данные.
  /// </summary>
  /// <param name="blobId">Идентификатор BLOB объекта.</param>
  /// <param name="cancellationToken">Токен отмены.</param>
  /// <exception cref="UnexpectedServiceException">Внутренняя ошибка сервиса.</exception>
  [Delete("/blobs/{id}")]
  Task DeleteBlob([AliasAs("id")] Guid blobId, CancellationToken cancellationToken = default);

  /// <summary>
  /// Отменить удаление бинарных данных.
  /// </summary>
  /// <param name="blobId">Идентификатор BLOB объекта.</param>
  /// <param name="cancellationToken">Токен отмены.</param>
  /// <returns>Описание восстановленного объекта.</returns>
  /// <exception cref="BlobNotFoundException">Объект не найден.</exception>
  /// <exception cref="UnexpectedServiceException">Внутренняя ошибка сервиса.</exception>
  /// <exception cref="ServiceContractException">Восстанавливаемый объект не числится среди удалённых.</exception>
  [Post("/blobs/{id}/restore")]
  Task<BlobDescription> RestoreBlob([AliasAs("id")] Guid blobId, CancellationToken cancellationToken = default);

  /// <summary>
  /// Записать чанк данных BLOB объекта.
  /// </summary>
  /// <param name="blobId">Идентификатор BLOB объекта.</param>
  /// <param name="offset">Начальная позиция для записи данных.</param>
  /// <param name="size">Размер чанка записываемых данных.</param>
  /// <param name="dataStream">Данные для записи.</param>
  /// <param name="cancellationToken">Токен отмены.</param>
  /// <returns>Описание созданного объекта.</returns>
  /// <exception cref="UnexpectedServiceException">Внутренняя ошибка сервиса.</exception>
  [Multipart]
  [Put("/blobs/{id}")]
  Task<BlobDescription> WriteBlobChunk([AliasAs("id")] Guid blobId, [Query] long offset, [Query] int size, [Body] Stream dataStream, CancellationToken cancellationToken = default);

  /// <summary>
  /// Считывает чанк данных BLOB объекта.
  /// </summary>
  /// <param name="blobId">Идентификатор BLOB объекта.</param>
  /// <param name="offset">Начальная позиция для чтения данных.</param>
  /// <param name="size">Размер чанка читаемых данных.</param>
  /// <param name="cancellationToken">Токен отмены.</param>
  /// <exception cref="BlobNotFoundException">Объект не найден.</exception>
  /// <exception cref="UnexpectedServiceException">Внутренняя ошибка сервиса.</exception>
  /// <returns>Поток с данными.</returns>
  [Get("/blobs/{id}")]
  Task<Stream> ReadBlobChunk([AliasAs("id")] Guid blobId, [Query] long offset, [Query] int size, CancellationToken cancellationToken = default);
}
