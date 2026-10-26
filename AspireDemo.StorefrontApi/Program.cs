using System.Diagnostics;
using AspireDemo.StorefrontApi.Observability;
using Microsoft.Extensions.Caching.Distributed;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddSource(StorefrontTelemetry.SourceName))
    .WithMetrics(metrics => metrics.AddMeter(StorefrontTelemetry.MeterName));

builder.Services.AddHttpClient("catalog", client =>
{
    client.BaseAddress = new Uri("https+http://catalog-api");
});

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("cache")
        ?? throw new InvalidOperationException("Missing Redis connection string: cache");
});

var app = builder.Build();
app.MapDefaultEndpoints();

app.MapGet("/api/storefront/products", async (
    IHttpClientFactory factory,
    ILogger<Program> logger,
    CancellationToken ct) =>
{
    using var activity = StorefrontTelemetry.ActivitySource.StartActivity("storefront.load-products");
    activity?.SetTag("catalog.operation", "list-products");

    var client = factory.CreateClient("catalog");
    var stopwatch = Stopwatch.StartNew();
    try
    {
        var json = await client.GetStringAsync("/api/products", ct);
        StorefrontTelemetry.CatalogDuration.Record(stopwatch.Elapsed.TotalMilliseconds);
        logger.LogInformation("Catalog products fetched in {ElapsedMs} ms", stopwatch.Elapsed.TotalMilliseconds);
        return Results.Content(json, "application/json");
    }
    catch (HttpRequestException ex)
    {
        activity?.SetStatus(ActivityStatusCode.Error, "Catalog API request failed");
        logger.LogError(ex, "Catalog API request failed");
        return Results.Problem(title: "Catalog service unavailable", statusCode: 503);
    }
});

app.MapGet("/api/storefront/products/cached", async (
    IDistributedCache cache,
    IHttpClientFactory factory,
    ILogger<Program> logger,
    CancellationToken ct) =>
{
    using var activity = StorefrontTelemetry.ActivitySource.StartActivity("storefront.get-products");
    const string key = "storefront:products";

    var cached = await cache.GetStringAsync(key, ct);
    if (cached is not null)
    {
        StorefrontTelemetry.CacheHits.Add(1);
        activity?.SetTag("cache.hit", true);
        logger.LogInformation("Product catalog served from cache");
        return Results.Content(cached, "application/json");
    }

    StorefrontTelemetry.CacheMisses.Add(1);
    activity?.SetTag("cache.hit", false);
    logger.LogInformation("Product catalog cache miss");

    var client = factory.CreateClient("catalog");
    var stopwatch = Stopwatch.StartNew();
    try
    {
        var json = await client.GetStringAsync("/api/products", ct);
        StorefrontTelemetry.CatalogDuration.Record(stopwatch.Elapsed.TotalMilliseconds);

        await cache.SetStringAsync(key, json,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30)
            }, ct);

        return Results.Content(json, "application/json");
    }
    catch (HttpRequestException ex)
    {
        activity?.SetStatus(ActivityStatusCode.Error, "Catalog API request failed");
        logger.LogError(ex, "Catalog API request failed");
        return Results.Problem(title: "Catalog service unavailable", statusCode: 503);
    }
});

app.Run();
