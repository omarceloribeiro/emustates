var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.Emustates_Site_ApiService>("apiservice");

builder.AddProject<Projects.Emustates_Site_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
