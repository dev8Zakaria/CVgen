# AI CV Generator

**Live demo:** https://golf-fonts-helen-greater.trycloudflare.com

> Demo note: this URL is served through a temporary Cloudflare Tunnel and can change if the tunnel container is recreated.

AI CV Generator is a full-stack application that helps a user create a professional CV tailored to a specific job offer.

The application lets the user:

- Sign in with Keycloak.
- Complete a structured professional profile.
- Add and analyze job offers with an AI service.
- Generate an ATS-friendly CV adapted to the selected offer.
- Download the generated CV as a PDF.
- Store generated PDFs in MinIO.
- Process long-running CV generation jobs through RabbitMQ.

The project started as a modular monolith and was migrated to a microservices-oriented architecture.

## Current Architecture

The current runnable stack is based on Docker Compose and contains:

- `frontend`: React/Vite web application.
- `bff-gateway`: ASP.NET Core BFF used as the single frontend API entry point.
- `profile-service`: ASP.NET Core service responsible for user profile data.
- `opportunity-service`: ASP.NET Core service responsible for job offers and AI job analysis.
- `cv-service`: ASP.NET Core service responsible for CV generation, PDF rendering, MinIO storage, RabbitMQ jobs, and CV history.
- `ai-service`: Python FastAPI service used for job analysis and CV content generation.
- `postgres`: one PostgreSQL container hosting multiple logical databases.
- `keycloak`: authentication and identity provider.
- `minio`: object storage for generated CV PDFs and files.
- `rabbitmq`: message broker used for async jobs and business events.

## How The Application Works

1. The user opens the React frontend at `http://localhost:5173`.
2. The user signs in through Keycloak.
3. The frontend sends API requests to the BFF Gateway at `http://localhost:5100/api`.
4. The BFF validates the JWT and forwards requests to the internal services.
5. The Profile Service stores the candidate profile in `aicv_profile`.
6. The Opportunity Service stores job offers in `aicv_opportunity` and calls the AI service to analyze descriptions.
7. The CV Service creates CV generation jobs in `aicv_cv`.
8. RabbitMQ receives the CV generation job.
9. The CV worker consumes the job, calls the AI service, renders a PDF, uploads it to MinIO, and updates the job status.
10. The frontend polls the job status and lets the user preview/download the final PDF.

The architecture is hybrid:

- REST/HTTP is used for immediate operations.
- RabbitMQ is used for long-running CV generation and business events.

## Tech Stack

### Frontend

- React 19
- Vite
- React Router
- Axios
- Tailwind CSS
- Vitest
- Testing Library

### Backend

- ASP.NET Core / .NET 10
- Entity Framework Core
- PostgreSQL
- JWT authentication with Keycloak
- Swagger in development
- Health and info endpoints

### AI Service

- Python
- FastAPI
- Gemini/LLM provider support
- Mock/fallback mode for local development

### Infrastructure

- Docker
- Docker Compose
- PostgreSQL 16
- Keycloak
- MinIO
- RabbitMQ Management

## Repository Structure

```text
.
|-- frontend/                  # React web app
|-- backend/                   # Original modular monolith backend
|-- ai-service/                # Python FastAPI AI service
|-- services/
|   |-- bff-gateway/           # Backend-for-Frontend gateway
|   |-- profile-service/       # Profile microservice
|   |-- opportunity-service/   # Opportunity/job-offer microservice
|   `-- cv-service/            # CV generation microservice + worker
|-- infra/
|   |-- keycloak/              # Keycloak realm import
|   `-- postgres/              # PostgreSQL database initialization
|-- docs/                      # Project documentation and reports
|-- docker-compose.final.yml   # Final microservices stack
|-- docker-compose.deploy.yml  # EC2/demo deployment stack
`-- .env.example               # Local environment template
```

## Prerequisites

Install:

- Docker Desktop
- Docker Compose
- Node.js 20+ if you want to run the frontend outside Docker
- .NET 10 SDK if you want to run or test the .NET services outside Docker
- Python 3.11+ if you want to run the AI service outside Docker

## Run Locally

The recommended local setup is Docker Compose. It starts the frontend, BFF gateway, microservices, PostgreSQL, Keycloak, MinIO, RabbitMQ, and the AI service.

### 1. Create the environment file

From the project root:

```powershell
Copy-Item .env.example .env
```

On Linux/macOS:

```bash
cp .env.example .env
```

Then open `.env` and adjust values only if needed.

Important local defaults:

```env
FRONTEND_PORT=5173
KEYCLOAK_PORT=8080
BFF_GATEWAY_PORT=5100
VITE_API_BASE_URL=http://localhost:5100/api
VITE_KEYCLOAK_URL=http://localhost:8080
POSTGRES_USER=aicv_user
POSTGRES_PASSWORD=aicv_password
KEYCLOAK_ADMIN=admin
KEYCLOAK_ADMIN_PASSWORD=admin
MINIO_USER=minioadmin
MINIO_PASSWORD=minioadmin
RABBITMQ_USER=guest
RABBITMQ_PASSWORD=guest
```

AI configuration:

- Set `GEMINI_API_KEY` if you want to use the real Gemini provider.
- Keep `AI_FALLBACK_TO_MOCK=true` if you want the app to keep working without a valid AI key.
- Set `AI_FALLBACK_TO_MOCK=false` when you want to prove that real AI calls are being used.

Relevant AI variables:

```env
AI_PROVIDER=gemini
AI_FALLBACK_TO_MOCK=true
GEMINI_API_KEY=
```

### 2. Start the full stack

Run:

```powershell
docker compose -f docker-compose.final.yml up -d --build
```

The first build can take several minutes.

### 3. Verify containers

```powershell
docker compose -f docker-compose.final.yml ps
```

Expected services:

- `aicv-frontend`
- `aicv-bff-gateway`
- `aicv-profile-service`
- `aicv-opportunity-service`
- `aicv-cv-service`
- `aicv-ai-service`
- `aicv-postgres`
- `aicv-keycloak`
- `aicv-minio`
- `aicv-rabbitmq`

### 4. Verify health

```powershell
Invoke-WebRequest -Uri http://localhost:5100/health -UseBasicParsing
Invoke-WebRequest -Uri http://localhost:5100/health/downstream -UseBasicParsing
```

The downstream health endpoint should return all services as healthy:

```json
{
  "status": "healthy",
  "services": {
    "profile": "healthy",
    "opportunity": "healthy",
    "cv": "healthy",
    "ai": "healthy"
  }
}
```

### 5. Open the app

Use:

```text
http://localhost:5173
```

Keycloak admin console:

```text
http://localhost:8080
```

Default admin credentials:

```text
admin / admin
```

### 6. Stop the stack

Stop containers without deleting data:

```powershell
docker compose -f docker-compose.final.yml down
```

Reset everything, including PostgreSQL, Keycloak, MinIO, and RabbitMQ volumes:

```powershell
docker compose -f docker-compose.final.yml down -v
```

Use `down -v` only when you want to reset PostgreSQL, Keycloak, MinIO, and RabbitMQ data.

## Local URLs

| Component | URL | Default credentials |
|---|---|---|
| Frontend | http://localhost:5173 | Keycloak user account |
| BFF Gateway | http://localhost:5100 | JWT protected for `/api/*` |
| BFF health | http://localhost:5100/health | Public |
| Downstream health | http://localhost:5100/health/downstream | Public |
| Keycloak | http://localhost:8080 | `admin` / `admin` |
| MinIO Console | http://localhost:9001 | `minioadmin` / `minioadmin` |
| RabbitMQ Management | http://localhost:15672 | `guest` / `guest` |
| PostgreSQL | `localhost:5432` | `aicv_user` / `aicv_password` |

Internal services are not exposed directly to the browser. The frontend should go through the BFF Gateway.

## PostgreSQL Databases

The final stack uses one PostgreSQL container with multiple logical databases:

- `aicv_profile`
- `aicv_opportunity`
- `aicv_cv`

They are created by:

```text
infra/postgres/init-microservices-databases.sql
```

Each service owns its database:

- Profile Service -> `aicv_profile`
- Opportunity Service -> `aicv_opportunity`
- CV Service -> `aicv_cv`

## RabbitMQ Usage

RabbitMQ is used for async CV generation and business events.

Main exchange:

```text
aicv.events
```

Main queues:

```text
cv.generation.requests
aicv.audit
```

Typical CV generation flow:

1. The frontend calls `POST /api/cv/generate`.
2. The BFF forwards the request to the CV Service.
3. The CV Service creates a `CvGenerationJob`.
4. The CV Service publishes `cv.generate.requested` to RabbitMQ.
5. `CvGenerationWorker` consumes the job.
6. The worker generates the CV, renders a PDF, uploads it to MinIO, and marks the job as completed or failed.
7. The frontend polls `GET /api/cv/generation-jobs/{jobId}`.

## MinIO Usage

Generated CV PDFs are stored in MinIO.

Default bucket:

```text
generated-cvs
```

The CV Service stores:

- PDF object key
- Bucket name
- PDF size
- CV content JSON in PostgreSQL

## Main API Routes Through The BFF

The frontend uses:

```text
http://localhost:5100/api
```

Important routes:

| Route | Purpose |
|---|---|
| `GET /api/profile/me` | Load or create the current profile |
| `PUT /api/profile/me` | Update the profile |
| `GET /api/opportunity` | List job offers |
| `POST /api/opportunity` | Create a job offer |
| `POST /api/opportunity/{id}/analyze` | Analyze a job offer |
| `POST /api/cv/generate` | Generate a CV or queue a CV job |
| `GET /api/cv/generation-jobs/{jobId}` | Poll CV generation job status |
| `GET /api/cvs` | List generated CVs |
| `GET /api/cvs/{id}` | Get generated CV details |
| `GET /api/cvs/{id}/download` | Download generated CV PDF |

## Run Tests

### Frontend

```powershell
cd frontend
npm install
npm run test
npm run build
```

### Microservices

```powershell
dotnet test services\AiCv.Microservices.slnx
```

### Original modular monolith

```powershell
dotnet test backend\AiCv.Backend.sln
```

## Development Notes

- The original backend in `backend/` represents the modular monolith phase.
- The current delivery stack is `docker-compose.final.yml`.
- The frontend is configured to call the BFF Gateway, not the internal services directly.
- The AI service can run with a real Gemini key or fallback/mock behavior.
- RabbitMQ is actively used for CV generation jobs.
- MinIO should contain generated PDFs after successful CV generation.

## Useful Debug Commands

Show running containers:

```powershell
docker compose -f docker-compose.final.yml ps
```

Read CV service logs:

```powershell
docker logs aicv-cv-service
```

Read AI service logs:

```powershell
docker logs aicv-ai-service
```

Read RabbitMQ logs:

```powershell
docker logs aicv-rabbitmq
```

Rebuild a clean stack:

```powershell
docker compose -f docker-compose.final.yml down -v
docker compose -f docker-compose.final.yml up --build
```

## Project Status

This version is considered deliverable for the current sprint.

Implemented:

- Frontend user flow
- Keycloak authentication
- BFF Gateway
- Profile Service
- Opportunity Service
- AI job analysis
- CV Generation Service
- ATS-friendly PDF generation
- MinIO PDF storage
- RabbitMQ CV jobs and events
- Docker Compose final stack
- Automated tests for frontend and services

Known future improvements:

- RabbitMQ retry strategy
- Dead-letter queue
- Notification/email consumer
- Centralized observability
- More CV templates
- ATS score and CV quality feedback
