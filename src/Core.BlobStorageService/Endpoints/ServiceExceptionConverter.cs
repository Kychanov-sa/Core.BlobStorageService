using Microsoft.AspNetCore.Mvc;

namespace GlacialBytes.Core.BlobStorageService.Endpoints
{
  internal static class ServiceExceptionConverter
  {
    public static ProblemDetails ToProblemDetails(this Exception exception)
    {
    }
  }
}
