using AspireDemo.CatalogApi.Data;
using AspireDemo.CatalogApi.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("catalogdb")
    ?? throw new InvalidOperationException("Missing connection string: catalogdb");

builder.Services.AddDbContext<CatalogDbContext>(options => options.UseNpgsql(connectionString));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // Learning-only initialization: use explicit EF Core migrations in real projects.
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.MapDefaultEndpoints();

app.MapGet("/api/products", async (CatalogDbContext db, CancellationToken ct) =>
    Results.Ok(await db.Products.AsNoTracking().OrderBy(p => p.Name).ToListAsync(ct)));

app.MapPost("/api/products", async (CreateProductRequest request, CatalogDbContext db, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(request.Name) || request.Price < 0)
        return Results.BadRequest("Invalid product data.");

    var product = new Product { Id = Guid.NewGuid(), Name = request.Name, Price = request.Price };
    db.Products.Add(product);
    await db.SaveChangesAsync(ct);

    return Results.Created($"/api/products/{product.Id}", product);
});

app.Run();

public sealed record CreateProductRequest(string Name, decimal Price);
