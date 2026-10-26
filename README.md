# AspireDemo — módulo 2: catálogo y PostgreSQL

Ejemplo público de **Aspire AppHost + ASP.NET Core Minimal API + PostgreSQL + EF Core + ServiceDefaults**, en C# y .NET 10.

La rama `main` contiene únicamente el punto de partida. Todos los ejemplos del curso se desarrollan en `develop`.

## Requisitos

- .NET 10 SDK
- Aspire CLI compatible con el AppHost
- Docker Desktop o un runtime de contenedores compatible, iniciado
- Visual Studio 2026 (recomendado) o terminal con `dotnet` y Aspire CLI

## Ejecución

```powershell
git clone -b develop https://github.com/Dark762/AspireDemo.git
cd AspireDemo
dotnet restore AspireDemo.slnx
aspire run
```

También puedes abrir `AspireDemo.slnx` en Visual Studio 2026, establecer `AspireDemo.AppHost` como proyecto de inicio y pulsar **F5**.

En el dashboard de Aspire, localiza `catalog-api` y abre su endpoint HTTP.

### API

```http
POST /api/products
Content-Type: application/json

{"name":"Mechanical Keyboard","price":129.99}
```

```http
GET /api/products
```

También se expone `GET /health`.

## Arquitectura

- **AppHost:** declara el contenedor PostgreSQL, la base `catalogdb` y el proyecto API.
- **WithReference(catalogDb):** suministra al servicio la configuración de conexión.
- **WaitFor(catalogDb):** coordina el inicio de la API con la disponibilidad del recurso.
- **ServiceDefaults:** centraliza observabilidad, health checks, descubrimiento de servicios y resiliencia HTTP.
- **CatalogApi:** utiliza EF Core/Npgsql sin conocer el host ni las credenciales de PostgreSQL.
- **WithDataVolume():** mantiene datos en un volumen local de Docker.

> **Importante:** la inicialización de esquema con `EnsureCreatedAsync()` se hace únicamente en desarrollo para facilitar la demo. No usar como sustituto de migraciones EF Core en producción. La configuración es didáctica, no una receta de despliegue productivo.

## Ejercicios

1. Elimina `WithReference(catalogDb)` y observa el error de configuración.
2. Mantén `WithReference` y elimina `WaitFor`; observa las diferencias de readiness.
3. Cambia el nombre `catalogdb` en AppHost sin cambiar `GetConnectionString`; investiga por qué falla.
4. Detén y vuelve a arrancar para comprobar la persistencia del volumen.

## Notas

Las versiones de paquetes se fijan para mantener un punto de partida reproducible. Antes de ampliar este ejemplo, revisa la compatibilidad con la versión del SDK de Aspire disponible en tu entorno.
