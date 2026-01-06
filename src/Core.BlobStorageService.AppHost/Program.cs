var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Core_BlobStorageService>("Core-BlobStorageService");

builder.Build().Run();
