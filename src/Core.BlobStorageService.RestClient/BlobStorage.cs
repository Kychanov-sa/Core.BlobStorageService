using Refit;
using GlacialBytes.Core.BlobStorageService.Exceptions;

namespace GlacialBytes.Core.BlobStorageService
{
  /// <summary>
  /// Хранилище BLOB объектов.
  /// </summary>
  /// <param name="storageApi">Интерфейс API сервиса хранения BLOB объектов.</param>
  public class BlobStorage(IBlobStorageApi storageApi)
  {
    /// <summary>
    /// Возвращает описание BLOB объекта.
    /// </summary>
    /// <param name="blobId">Идентификатор BLOB объекта.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Описание BLOB объекта.</returns>
    /// <exception cref="BlobNotFoundException">Объект не найден.</exception>
    /// <exception cref="UnexpectedServiceException">Внутренняя ошибка сервиса.</exception>
    public async Task<BlobDescription> GetBlobDescription(Guid blobId, CancellationToken cancellationToken = default)
    {
      var response = await storageApi.GetBlobDescriptionHeaders(blobId);
      if (response.IsSuccessStatusCode)
      {
        return new BlobDescription()
        {
          Id = blobId,
          Length = response.Content.Headers.ContentLength ?? -1,
          Hash = response.Headers.ETag?.Tag,
          Created = response.Headers.GetBlobCreated(),
          Modified = response.Headers.GetBlobModified(),
        };
      }

      throw await ServiceExceptionFactory.CreateServiceExceptionFromResponseMessage(response, cancellationToken);
    }

    /// <summary>
    /// Возвращает размер BLOB объекта.
    /// </summary>
    /// <param name="blobId">Идентификатор BLOB объекта.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Размер объекта в байтах.</returns>
    /// <exception cref="BlobNotFoundException">Объект не найден.</exception>
    /// <exception cref="UnexpectedServiceException">Внутренняя ошибка сервиса.</exception>
    public async Task<long> GetBlobLength(Guid blobId, CancellationToken cancellationToken = default)
    {
      var blob = await GetBlobDescription(blobId, cancellationToken);
      return blob.Length;
    }

    /// <summary>
    /// Возвращает хэш BLOB объекта.
    /// </summary>
    /// <param name="blobId">Идентификатор BLOB объекта.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Строка хэша.</returns>
    /// <exception cref="BlobNotFoundException">Объект не найден.</exception>
    /// <exception cref="UnexpectedServiceException">Внутренняя ошибка сервиса.</exception>
    public async Task<string?> GetBlobHash(Guid blobId, CancellationToken cancellationToken = default)
    {
      var blob = await GetBlobDescription(blobId, cancellationToken);
      return blob.Hash;
    }

    /// <summary>
    /// Проверить наличие BLOB объекта.
    /// </summary>
    /// <param name="blobId">Идентификатор BLOB объекта.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>true если объект существует, иначе false.</returns>
    /// <exception cref="UnexpectedServiceException">Внутренняя ошибка сервиса.</exception>
    public async Task<bool> IsBlobExists(Guid blobId, CancellationToken cancellationToken = default)
    {
      var response = await storageApi.GetBlobDescriptionHeaders(blobId, cancellationToken);
      if (response.IsSuccessStatusCode)
        return true;
      if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        return false;

      throw await ServiceExceptionFactory.CreateServiceExceptionFromResponseMessage(response, cancellationToken);
    }

    /// <summary>
    /// Копировать бинарные данные.
    /// </summary>
    /// <param name="blobId">Идентификатор целевого BLOB объекта.</param>
    /// <param name="sourceBlobId">Идентификатор исходного BLOB объекта.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <exception cref="BlobNotFoundException">Объект не найден.</exception>
    /// <exception cref="UnexpectedServiceException">Внутренняя ошибка сервиса.</exception>
    /// <exception cref="ServiceContractException">Идентификатор целевого и исходного объекта совпадает.</exception>
    public async Task CopyBlob(Guid blobId, Guid sourceBlobId, CancellationToken cancellationToken = default)
    {
      try
      {
        await storageApi.CopyBlob(blobId, sourceBlobId, cancellationToken);
      }
      catch (ApiException ex)
      {
        throw ServiceExceptionFactory.CreateExceptionFromApiException(ex);
      }
    }

    /// <summary>
    /// Удалить бинарные данные.
    /// </summary>
    /// <param name="blobId">Идентификатор BLOB объекта.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <exception cref="UnexpectedServiceException">Внутренняя ошибка сервиса.</exception>
    public async Task DeleteBlob(Guid blobId, CancellationToken cancellationToken = default)
    {
      try
      {
        await storageApi.DeleteBlob(blobId, cancellationToken);
      }
      catch (ApiException ex)
      {
        throw ServiceExceptionFactory.CreateExceptionFromApiException(ex);
      }
    }

    /// <summary>
    /// Отменить удаление бинарных данных.
    /// </summary>
    /// <param name="blobId">Идентификатор BLOB объекта.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <exception cref="BlobNotFoundException">Объект не найден.</exception>
    /// <exception cref="UnexpectedServiceException">Внутренняя ошибка сервиса.</exception>
    /// <exception cref="ServiceContractException">Восстанавливаемый объект не числится среди удалённых.</exception>
    public async Task RestoreBlob(Guid blobId, CancellationToken cancellationToken = default)
    {
      try
      {
        await storageApi.RestoreBlob(blobId, cancellationToken);
      }
      catch (ApiException ex)
      {
        throw ServiceExceptionFactory.CreateExceptionFromApiException(ex);
      }
    }

    /// <summary>
    /// Записать данные BLOB объекта.
    /// </summary>
    /// <param name="blobId">Идентификатор BLOB объекта.</param>
    /// <param name="dataStream">Данные для записи.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Описание созданного объекта.</returns>
    /// <exception cref="UnexpectedServiceException">Внутренняя ошибка сервиса.</exception>
    public async Task<BlobDescription> WriteBlob(Guid blobId, [Body] Stream dataStream, CancellationToken cancellationToken = default)
    {
      return await WriteBlobChunk(blobId, 0, -1, dataStream, cancellationToken);
    }


    /// <summary>l
    /// Дописать данные BLOB объекта.
    /// </summary>
    /// <param name="blobId">Идентификатор BLOB объекта.</param>
    /// <param name="dataStream">Данные для записи.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат записи.</returns>l
    /// <exception cref="BlobNotFoundException">Объект не найден.</exception>
    /// <exception cref="UnexpectedServiceException">Внутренняя ошибка сервиса.</exception>
    public async Task<BlobDescription> AppendBlob(Guid blobId, [Body] Stream dataStream, CancellationToken cancellationToken = default)
    {
      return await WriteBlobChunk(blobId, -1, -1, dataStream, cancellationToken);
    }

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
    public async Task<BlobDescription> WriteBlobChunk(Guid blobId, [Query] long offset, [Query] int size, [Body] Stream dataStream, CancellationToken cancellationToken = default)
    {
      try
      {
        return await storageApi.WriteBlobChunk(blobId, offset, size, dataStream, cancellationToken);
      }
      catch (ApiException ex)
      {
        throw ServiceExceptionFactory.CreateExceptionFromApiException(ex);
      }
    }

    /// <summary>
    /// Считывает данные BLOB объекта.
    /// </summary>
    /// <param name="blobId">Идентификатор BLOB объекта.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Поток с данными.</returns>
    /// <exception cref="BlobNotFoundException">Объект не найден.</exception>
    /// <exception cref="UnexpectedServiceException">Внутренняя ошибка сервиса.</exception>
    public async Task<Stream> ReadBlob(Guid blobId, CancellationToken cancellationToken = default)
    {
      return await ReadBlobChunk(blobId, 0, -1, cancellationToken);
    }

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
    public async Task<Stream> ReadBlobChunk(Guid blobId, [Query] long offset, [Query] int size, CancellationToken cancellationToken = default)
    {
      try
      {
        return await storageApi.ReadBlobChunk(blobId, offset, size, cancellationToken);
      }
      catch (ApiException ex)
      {
        throw ServiceExceptionFactory.CreateExceptionFromApiException(ex);
      }
    }
  }
}
