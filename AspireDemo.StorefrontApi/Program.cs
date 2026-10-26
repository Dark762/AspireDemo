using Microsoft.Extensions.Caching.Distributed;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

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

app.MapGet("/api/storefront/products", async (IHttpClientFactory factory, CancellationToken ct) =>
{
    var client = factory.CreateClient("catalog");
    using var response = await client.GetAsync("/api/products", ct);
    response.EnsureSuccessStatusCode();
    var json = await response.Content.ReadAsStringAsync(ct);
    return Results.Content(json, "application/json");
});

app.MapGet("/api/storefront/products/cached", async (
    IDistributedCache cache,
    IHttpClientFactory factory,
    CancellationToken ct) =>
{
    const string key = "storefront:products";
    var cached = await cache.GetStringAsync(key, ct);
    if (cached is not null)
        return Results.Content(cached, "application/json");

    var client = factory.CreateClient("catalog");
    var json = await client.GetStringAsync("/api/products", ct);
    await cache.SetStringAsync(key, json,
        new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30)
        }, ct);
    return Results.Content(json, "application/json");
});

app.Run();
