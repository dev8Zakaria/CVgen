# AI CV Microservices Workspace

This folder contains the first migration bootstrap for the .NET services that will replace the modular monolith gradually.

## Services

| Service | Local URL | Responsibility |
| --- | --- | --- |
| `bff-gateway` | `http://localhost:5100` | Frontend-facing API composition and routing to downstream services. |
| `profile-service` | `http://localhost:5101` | User profile, education, experience, skills, languages, and projects. |
| `opportunity-service` | `http://localhost:5102` | Job opportunity data, requirements, keywords, and analysis inputs. |
| `cv-service` | `http://localhost:5103` | CV generation orchestration, versions, templates, and document metadata. |

Each service currently exposes:

- `GET /health`
- `GET /info`
- Swagger UI in `Development`
- Basic CORS configuration
- Optional Keycloak JWT validation when `Keycloak:Authority` is configured
- A service-specific xUnit test project
- A Dockerfile

## Local Commands

Run all service tests:

```powershell
dotnet test services\AiCv.Microservices.slnx
```

Run one service locally:

```powershell
dotnet run --project services\bff-gateway\src\AiCv.BffGateway\AiCv.BffGateway.csproj
```

Run the current platform stack with the new service containers:

```powershell
docker compose -f docker-compose.yml -f docker-compose.microservices.yml up --build
```

The current monolith is intentionally still present. The new services are bootstrap targets where the existing modules can be extracted progressively.
