# ProjectManagerApi

API REST para la gestión de **proyectos** y **tareas**, construida con **.NET 10 (`net10.0`)**, **Clean Architecture**, **Entity Framework Core 10**, **PostgreSQL**, **Swagger (OpenAPI)** y pruebas unitarias con **xUnit**, **Moq** y **FluentAssertions**.

---

## Arquitectura del Proyecto

La solución sigue **Clean Architecture** respetando estrictamente la regla de dependencia hacia el centro (`Domain`):

```text
ProjectManagerApi.API             ──► Application, Infrastructure
ProjectManagerApi.Infrastructure  ──► Application
ProjectManagerApi.Application     ──► Domain
ProjectManagerApi.Domain          ──► (Sin dependencias externas)
```

### Estructura de la solución

- **`src/ProjectManagerApi.Domain`**: Entidades (`Proyecto`, `Tarea`), enumeraciones (`EstadoTarea`, `PrioridadTarea`), excepciones de dominio (`NotFoundException`, `BusinessRuleException`) e interfaces de repositorio (`IProyectoRepository`, `ITareaRepository`).
- **`src/ProjectManagerApi.Application`**: DTOs con validación por `DataAnnotations`, mapeadores (`ProyectoMapper`, `TareaMapper`), contratos (`IProyectoService`, `ITareaService`) y servicios con las reglas de negocio.
- **`src/ProjectManagerApi.Infrastructure`**: `AppDbContext`, configuraciones Fluent API (`ProyectoConfiguration`, `TareaConfiguration`), repositorios EF Core con `.AsNoTracking()` en lecturas y migraciones de PostgreSQL.
- **`src/ProjectManagerApi.API`**: Controladores REST (`ProyectosController`, `TareasController`), middleware global de excepciones (`ExceptionMiddleware`), configuración de Swagger, serialización de enums como texto (`JsonStringEnumConverter`) e inyección de dependencias.
- **`tests/`**:
  - `ProjectManagerApi.Domain.UnitTests`
  - `ProjectManagerApi.Application.UnitTests`
  - `ProjectManagerApi.Infrastructure.UnitTests`
  - `ProjectManagerApi.API.UnitTests`

---

## Reglas de Negocio Implementadas

1. **Cálculo de porcentaje de avance (`Proyecto.CalcularPorcentajeAvance`)**:
   - Retorna `0` si el proyecto no tiene tareas.
   - Calcula `Math.Round((double)completadas / total * 100, 2)` en caso contrario.
2. **Validación de fechas de proyecto**:
   - Si `FechaFin` se especifica y es menor a `FechaInicio`, se lanza `BusinessRuleException` (`409 Conflict`).
3. **Protección al eliminar proyectos**:
   - No permite eliminar un proyecto si existen tareas no completadas (`Pendiente` o `EnProgreso`), lanzando `BusinessRuleException` (`409 Conflict`). La FK en base de datos también usa `DeleteBehavior.Restrict`.
4. **Integridad de tareas**:
   - Al crear o actualizar una tarea, se valida la existencia del `ProyectoId` (`404 Not Found` si no existe).
5. **Consultas especializadas**:
   - Filtro dinámico de tareas por `estado` y/o `prioridad`.
   - Consulta de tareas vencidas (`FechaLimite < DateTime.UtcNow` y `Estado != Completada`).
6. **Manejo estandarizado de errores (`ExceptionMiddleware`)**:
   - `NotFoundException` → `404 Not Found`
   - `BusinessRuleException` → `409 Conflict`
   - `Exception` no controlada → `500 Internal Server Error`
   - Formato JSON: `{ "status": 404, "title": "...", "detail": "..." }`.

---

## Endpoints Disponibles

### Proyectos (`/api/proyectos`)

| Método | Ruta | Descripción | Códigos HTTP |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/proyectos` | Lista todos los proyectos con totales y porcentaje de avance | `200` |
| `GET` | `/api/proyectos/{id}` | Obtiene un proyecto por ID | `200`, `404` |
| `POST` | `/api/proyectos` | Crea un nuevo proyecto | `201`, `400`, `409` |
| `PUT` | `/api/proyectos/{id}` | Actualiza un proyecto existente | `200`, `400`, `404`, `409` |
| `DELETE` | `/api/proyectos/{id}` | Elimina un proyecto sin tareas pendientes | `204`, `404`, `409` |
| `GET` | `/api/proyectos/{id}/tareas` | Lista todas las tareas de un proyecto | `200`, `404` |

### Tareas (`/api/tareas`)

| Método | Ruta | Descripción | Códigos HTTP |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/tareas` | Lista tareas con query params opcionales `estado` y `prioridad` | `200` |
| `GET` | `/api/tareas/vencidas` | Obtiene tareas vencidas no completadas | `200` |
| `GET` | `/api/tareas/{id}` | Obtiene una tarea por ID | `200`, `404` |
| `POST` | `/api/tareas` | Crea una nueva tarea | `201`, `400`, `404` |
| `PUT` | `/api/tareas/{id}` | Actualiza completamente una tarea | `200`, `400`, `404` |
| `PATCH` | `/api/tareas/{id}/estado` | Cambia únicamente el estado de una tarea | `200`, `400`, `404` |
| `DELETE` | `/api/tareas/{id}` | Elimina una tarea | `204`, `404` |

### Ejemplos de Payloads JSON

**Crear Proyecto (`POST /api/proyectos`):**
```json
{
  "nombre": "Migración Plataforma Cloud",
  "descripcion": "Migración de servicios core a contenedores .NET 10",
  "fechaInicio": "2026-11-01T00:00:00Z",
  "fechaFin": "2026-12-15T00:00:00Z"
}
```

**Crear Tarea (`POST /api/tareas`):**
```json
{
  "titulo": "Configurar pipeline CI/CD",
  "descripcion": "Definir build y pruebas automáticas en GitHub Actions",
  "estado": "Pendiente",
  "prioridad": "Alta",
  "fechaLimite": "2026-11-30T23:59:59Z",
  "proyectoId": 1
}
```

**Cambiar Estado (`PATCH /api/tareas/1/estado`):**
```json
{
  "estado": "Completada"
}
```

Valores permitidos para enums:
- `EstadoTarea`: `Pendiente`, `EnProgreso`, `Completada`
- `PrioridadTarea`: `Baja`, `Media`, `Alta`

---

## Requisitos y Puesta en Marcha

### Opción A: Ejecutar todo el stack con Docker (Recomendado)

La raíz del proyecto incluye [`Dockerfile`](Dockerfile) y [`docker-compose.yml`](docker-compose.yml) para levantar **PostgreSQL 17 (`project_manager_db`)** y **`ProjectManagerApi`** en contenedores con migración automática al iniciar (`Database__AutoMigrate=true`).

> **Nota sobre puertos y Docker Desktop en Linux:** El contenedor de PostgreSQL se expone en el puerto **`5433`** del host (`5433:5432`) para evitar conflictos si ya tienes un PostgreSQL local en `5432`. Si tu CLI de Docker apunta al contexto `desktop-linux` sin Docker Desktop iniciado, antepón `DOCKER_CONTEXT=default`.

```bash
# Construir y levantar PostgreSQL + API
DOCKER_CONTEXT=default docker compose up -d --build

# Verificar estado de los contenedores
DOCKER_CONTEXT=default docker compose ps
```

- **Swagger UI:** `http://localhost:8080/swagger`
- **OpenAPI JSON:** `http://localhost:8080/swagger/v1/swagger.json`
- **API Base URL:** `http://localhost:8080/api`
- **PostgreSQL desde el host:** `Host=localhost;Port=5433;Database=project_manager_db;Username=postgres;Password=postgres`

Para detener los contenedores:

```bash
DOCKER_CONTEXT=default docker compose down
```

### Opción B: Ejecutar localmente con .NET SDK

#### Prerrequisitos
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- PostgreSQL 15+ (local o en contenedor Docker)

#### 1. Levantar solo PostgreSQL en Docker (si no usas PostgreSQL local)

```bash
docker run --name projectmanager-pg \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_DB=project_manager_db \
  -p 5432:5432 \
  -d postgres:17-alpine
```

La cadena de conexión por defecto en `src/ProjectManagerApi.API/appsettings.json` es:

```text
Host=localhost;Port=5432;Database=project_manager_db;Username=postgres;Password=postgres
```

#### 2. Restaurar herramientas y aplicar migraciones

```bash
dotnet tool restore
dotnet ef database update \
  --project src/ProjectManagerApi.Infrastructure \
  --startup-project src/ProjectManagerApi.API
```

#### 3. Ejecutar la API y abrir Swagger

```bash
dotnet run --project src/ProjectManagerApi.API
```

En entorno `Development`, la documentación interactiva de Swagger UI estará disponible en:
- `https://localhost:7218/swagger` o `http://localhost:5121/swagger` (según `launchSettings.json`).

---

## Pruebas Unitarias

Para compilar y ejecutar toda la suite de pruebas unitarias:

```bash
dotnet build ProjectManagerApi.sln
dotnet test ProjectManagerApi.sln --no-build --verbosity normal
```
