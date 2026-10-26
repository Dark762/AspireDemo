var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume();

var catalogDb = postgres.AddDatabase("catalogdb");

builder.AddProject<Projects.AspireDemo_CatalogApi>("catalog-api")
    .WithReference(catalogDb)
    .WaitFor(catalogDb)
    .WithHttpHealthCheck("/health");

builder.Build().Run();
