using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace AspireDemo.StorefrontApi.Observability;

public static class StorefrontTelemetry
{
    public const string SourceName = "AspireDemo.Storefront";
    public const string MeterName = "AspireDemo.Storefront";

    public static readonly ActivitySource ActivitySource = new(SourceName);
    private static readonly Meter Meter = new(MeterName);

    public static readonly Counter<long> CacheHits =
        Meter.CreateCounter<long>("storefront.cache.hits", unit: "{hit}");
    public static readonly Counter<long> CacheMisses =
        Meter.CreateCounter<long>("storefront.cache.misses", unit: "{miss}");
    public static readonly Histogram<double> CatalogDuration =
        Meter.CreateHistogram<double>("storefront.catalog.duration", unit: "ms");
}
