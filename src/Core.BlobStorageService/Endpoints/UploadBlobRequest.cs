namespace GlacialBytes.Core.BlobStorage.Endpoints
{
  public class UploadBlobRequest
  {
    public long Offset { get; set; }
    public long Size { get; set; }
    public IFormFile UploadingFile { get; set; }
  }
}
