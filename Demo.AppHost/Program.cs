var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Demo_ApiService>("apiservice")
    .WithHttpsHealthCheck("/health");

builder.Build().Run();
