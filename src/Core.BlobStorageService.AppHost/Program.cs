var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Core_BlobStorage>("Core-BlobStorage");

builder.Build().Run();
