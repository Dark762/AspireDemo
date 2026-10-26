# Módulo 3 — AppHost avanzado y comunicación entre servicios

## Arquitectura

```text
Storefront API --> Catalog API --> PostgreSQL
      |
      +---------> Redis
```

AppHost define los recursos y sus relaciones. `WithReference` suministra configuración y `WaitFor` espera disponibilidad de las dependencias. La llamada HTTP la implementa Storefront mediante `IHttpClientFactory` y el descubrimiento de servicios registrado en ServiceDefaults.

## Ejecución

Requisitos: .NET 10 SDK, Aspire CLI y Docker iniciado.

```powershell
git clone -b develop https://github.com/Dark762/AspireDemo.git
cd AspireDemo
dotnet restore AspireDemo.slnx
aspire run
```

Abre el endpoint de `storefront-api` desde el Dashboard.

1. `GET /api/storefront/products`: siempre llama a Catalog API.
2. `GET /api/storefront/products/cached`: consulta Redis; en miss llama a Catalog API y guarda el JSON durante 30 segundos.
3. `POST /api/products` en Catalog API: crea un producto.
4. Repite la lectura cacheada: hasta la expiración puede mostrar una versión anterior.

## Conceptos

- `IResource`: recurso en el modelo distribuido.
- `IResourceBuilder<T>`: API fluida de configuración.
- `WithReference(catalogApi)`: aporta endpoints lógicos a Storefront.
- `WithReference(cache)`: aporta la conexión de Redis.
- `WaitFor`: coordina readiness en el arranque; no garantiza disponibilidad permanente.
- `WithExternalHttpEndpoints`: permite acceso externo a Storefront.
- `https+http://catalog-api`: nombre lógico resuelto por Service Discovery.

## Ejercicios

- Elimina `WithReference(catalogApi)` y observa el fallo de resolución.
- Elimina `WaitFor(catalogApi)` y compara el arranque.
- Cambia el TTL de Redis a cinco segundos.
- Agrega una estrategia de invalidación de caché al modificar productos.

**Nota:** demo didáctica. Falta manejo de fallos de caché, invalidación, seguridad y pruebas automatizadas. No se ha validado la compilación en un entorno .NET con Docker.
