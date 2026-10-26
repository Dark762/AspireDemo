var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres").WithDataVolume();
var catalogDb = postgres.AddDatabase("catalogdb");
var cache = builder.AddRedis("cache");

var catalogApi = builder.AddProject<Projects.AspireDemo_CatalogApi>("catalog-api")
    .WithReference(catalogDb)
    .WaitFor(catalogDb)
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.AspireDemo_StorefrontApi>("storefront-api")
    .WithReference(catalogApi)
    .WithReference(cache)
    .WaitFor(catalogApi)
    .WaitFor(cache)
    .WithHttpHealthCheck("/health")
    .WithExternalHttpEndpoints();

builder.Build().Run();
