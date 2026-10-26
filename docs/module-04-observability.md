# Módulo 4 — Observabilidad con OpenTelemetry

## Objetivos

- Entender logs, traces, spans y métricas
- Seguir una petición Storefront -> Catalog mediante TraceId
- Registrar spans personalizados con ActivitySource
- Registrar métricas personalizadas con Meter
- Correlacionar errores HTTP con logs estructurados

## Instrumentación

`AspireDemo.StorefrontApi/Observability/StorefrontTelemetry.cs` define la fuente `AspireDemo.Storefront` y el medidor del mismo nombre.

`Program.cs` registra ambos en OpenTelemetry, cuenta hits/misses de Redis, mide llamadas HTTP a Catalog y registra logs estructurados. `ServiceDefaults` exporta logs, trazas y métricas al receptor OTLP proporcionado por Aspire.

## Ejecución

```powershell
git checkout develop
dotnet restore AspireDemo.slnx
aspire run
```

1. Abre el Dashboard indicado por Aspire CLI.
2. Abre el endpoint de Storefront.
3. Ejecuta `GET /api/storefront/products/cached` dos veces.
4. En **Traces**, busca `storefront.get-products` y compara un cache miss con un cache hit.
5. En **Structured Logs**, busca `Product catalog cache miss` y `Product catalog served from cache`.
6. En **Metrics**, busca `storefront.cache.hits`, `storefront.cache.misses` y `storefront.catalog.duration`.

La primera llamada debe registrar un miss, y la segunda normalmente un hit si no ha expirado la entrada. Los spans automáticos de HTTP permiten correlacionar Storefront y Catalog.

## Consideraciones profesionales

- Evita tags de alta cardinalidad y datos personales.
- No registres contraseñas, tokens, connection strings ni cuerpos completos.
- El dashboard de desarrollo no sustituye un backend de telemetría de producción.
- Las métricas son agregadas; los traces describen operaciones concretas.
- Los errores de Redis todavía no tienen fallback en esta demo.
- El proyecto no ha sido compilado ni probado con Docker desde el entorno de edición.
