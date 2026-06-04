# AI CV Generator - Conversation Context Handoff

This file summarizes the important context from the current Codex conversation so future sessions can continue quickly without rediscovering the project state.

## User Communication Preference

- The user prefers simple, direct explanations.
- Avoid overly technical language unless needed.
- Be factual and pragmatic.
- When explaining architecture, use simple analogies and concrete examples.
- The user is working toward a professor/demo-ready project, so focus on practical verification and visible behavior.

## Current Project Goal

The project started as a modular monolith and is being migrated to microservices.

The target is to get the app around 90% complete in the microservices version before final polishing. RabbitMQ is now moving from a symbolic/basic integration toward a real async implementation.

## Architecture Decisions Already Made

- Use microservices instead of continuing the modular monolith.
- Use one PostgreSQL container with multiple databases, not multiple schemas.
- Use a BFF gateway, not a generic gateway.
- Use RabbitMQ for async work, especially CV generation.
- Use MinIO for generated CV PDF storage.
- Use Keycloak for authentication.
- Deployment target is Docker Compose on Proxmox VMs, no Kubernetes.
- Planned VM split:
  - VM 1: frontend, BFF, .NET services, Python AI service containers.
  - VM 2: infrastructure containers such as PostgreSQL, MinIO, Keycloak, RabbitMQ.

## Target Services

- `bff-gateway`: ASP.NET Core BFF gateway for frontend-shaped API access.
- `profile-service`: ASP.NET Core service for user profile data.
- `opportunity-service`: ASP.NET Core service for job offers and analysis data.
- `cv-service`: ASP.NET Core service for CV generation, generated CV metadata, PDF generation, MinIO storage, and RabbitMQ worker.
- `ai-service`: Python FastAPI service used by backend services for AI analysis/generation.
- `keycloak`: auth provider, not custom auth service.
- `rabbitmq`: async messaging.
- `postgres`: one container, multiple databases.
- `minio`: object storage for generated PDF files.

## Team Sprint Roles

- Zakaria: full stack, currently owns CV generation service.
- Assaad: backend and CI, owns BFF gateway.
- Senku: backend and local deployment, owns opportunity service.
- Youssef: backend and frontend QA, owns profile service.
- Zentari: deployment and backend QA.

## BFF Gateway Context

The BFF is an entire ASP.NET Core project, not just config.

Purpose:

- Single frontend entry point.
- Handles frontend-specific API shape.
- Can aggregate data later, for example dashboard summary.
- Routes authenticated frontend requests to downstream services.

Current BFF work:

- BFF compiles.
- `/health` exists.
- `/health/downstream` checks profile, opportunity, CV, and AI services.
- BFF has manual proxy endpoints instead of YARP.
- `ProxyEndpoints.cs` was added.
- `ExceptionMiddleware.cs` was added.
- BFF downstream health currently returns healthy for profile, opportunity, cv, and ai.

Known remaining BFF ideas:

- Dashboard aggregation is not fully complete.
- Future work could expose a single dashboard summary endpoint instead of the frontend collecting everything.

## RabbitMQ Context

Earlier RabbitMQ integration was too weak because CV generation was still effectively synchronous.

A more real implementation was started:

- `CvGenerationJob` entity added.
- `CvGenerationJobs` table added via migration.
- CV generation endpoint returns `202 Accepted` when RabbitMQ is enabled.
- Frontend polls `/cv/generation-jobs/{jobId}`.
- RabbitMQ worker consumes from queue `cv.generation.requests`.
- Worker generates the CV and updates the job status.
- Job stores snapshots of profile and opportunity data so the worker does not need the user's bearer token.

Important RabbitMQ fix already made:

- `CvGenerationWorker` originally crashed the whole service if RabbitMQ was not ready.
- It was changed to retry RabbitMQ connection instead of killing `cv-service`.
- Logs may show temporary retry warnings during boot, then:
  - `RabbitMQ CV generation worker is listening on queue cv.generation.requests.`

## CV Generation Current State

CV generation service has been expanded beyond the original monolith state.

Implemented or improved:

- Backend generates PDF, not HTML.
- More professional ATS-style CV formula.
- CV preview and downloaded PDF were previously inconsistent; the backend PDF output is now the real target.
- Role alignment content was considered too confusing and was removed/reworked from the CV formula.
- CV generation now uses RabbitMQ async job flow when enabled.
- MinIO integration exists for generated PDF storage.

Recent critical bug fixed:

- CV generation failed with:
  - `relation "CvGenerationJobs" does not exist`
- Cause:
  - The migration `.cs` existed, and the model snapshot was updated, but the EF migration designer metadata file was missing.
  - EF did not discover/apply `20260603174000_AddCvGenerationJobs` properly.
- Fix:
  - Added `20260603174000_AddCvGenerationJobs.Designer.cs`.
  - Rebuilt/restarted `cv-service`.
  - Verified Postgres table list includes:
    - `CvGenerationJobs`
    - `GeneratedCvs`
    - `__EFMigrationsHistory`

## Frontend Current State

Frontend is React/Vite.

Important files:

- `frontend/src/shared/providers/PrototypeAppProvider.jsx`
  - Main frontend state bridge.
  - Loads profile, opportunities, CVs.
  - Maps backend DTOs to frontend page models.
  - Handles CV generation and polling async CV jobs.
- `frontend/src/modules/dashboard/pages/DashboardPage.jsx`
  - Main dashboard.
- `frontend/src/modules/landing/pages/LandingPage.jsx`
  - Landing page.
- `frontend/src/modules/cv/pages/GenerateCvPage.jsx`
  - CV generation UI.
- `frontend/src/modules/cv/pages/MyCvsPage.jsx`
  - CV library.
- `frontend/src/modules/cv/pages/CvPreviewPage.jsx`
  - CV preview and download page.
- `frontend/src/modules/auth/AuthProvider.jsx`
  - Keycloak auth state.
- `frontend/src/modules/auth/ProtectedRoute.jsx`
  - Protected route behavior.
- `frontend/src/styles/globals.css`
  - Main styling and design tokens.

## Frontend Performance/UI Work Already Done

The user complained the UI felt heavy/laggy and not improved enough.

Changes already made:

- Used `ui-ux-pro-max` skill for design direction.
- Removed heavy runtime dependencies:
  - `framer-motion`
  - `gsap`
  - `three`
- Removed Three.js landing visual.
- Removed route animation.
- Removed expensive blur/noise/shadow effects.
- Reduced animated/global background cost.
- Replaced heavy skeleton look on dashboard.
- Landing page now integrates:
  - `frontend/src/assets/ATS-Friendly-Resume-Template.jpg`
- The resume image is 1240x1754 and is used as the landing page resume preview.

Bundle after optimization:

- Production JS around `406 kB`.
- Resume image emitted separately around `164 kB`.

## Recent Dashboard Loading Fix

Problem:

- After login, dashboard showed large blue skeleton blocks for too long.
- User could easily screenshot it, proving it was visibly slow.

Cause:

- `PrototypeAppProvider` waited for all backend hydration before setting `hydrated=true`.
- It fetched profile, opportunity list, opportunity details, and CV list before rendering the dashboard.

Fix:

- Dashboard now renders immediately with safe/default profile state.
- Backend data loads afterward and updates the UI.
- CV list and opportunity list now load in parallel.
- Basic opportunity list renders before all opportunity detail requests finish.
- Big skeleton blocks were replaced with a smaller loading panel.

Files changed:

- `frontend/src/shared/providers/PrototypeAppProvider.jsx`
- `frontend/src/modules/dashboard/pages/DashboardPage.jsx`

## Docker Compose Context

Main compose file for the microservice/final stack:

- `docker-compose.final.yml`

Main command:

```powershell
docker compose -f docker-compose.final.yml up -d --build
```

Useful targeted rebuilds:

```powershell
docker compose -f docker-compose.final.yml up -d --build cv-service
docker compose -f docker-compose.final.yml up -d --build frontend bff-gateway
```

Useful checks:

```powershell
docker compose -f docker-compose.final.yml ps
docker compose -f docker-compose.final.yml logs --tail=120 cv-service
Invoke-WebRequest -Uri http://localhost:5100/health/downstream -UseBasicParsing | Select-Object -ExpandProperty Content
```

Known healthy BFF downstream response:

```json
{"status":"healthy","services":{"profile":"healthy","opportunity":"healthy","cv":"healthy","ai":"healthy"}}
```

## Verification Commands Already Used

Frontend:

```powershell
cd frontend
npm run build
npm run lint
npm run test
```

Recent frontend verification:

- `npm run build`: passed.
- `npm run lint`: passed with existing warnings only.
- `npm run test`: 10 tests passed.

CV service:

```powershell
dotnet build services\cv-service\src\AiCv.CvService\AiCv.CvService.csproj --no-restore --verbosity minimal
dotnet test services\cv-service\tests\AiCv.CvService.Tests\AiCv.CvService.Tests.csproj --no-restore --verbosity minimal
```

Recent CV service verification:

- Build passed.
- Tests passed: 5 tests.

Docker:

- All containers were running after latest rebuild:
  - `aicv-frontend`
  - `aicv-bff-gateway`
  - `aicv-profile-service`
  - `aicv-opportunity-service`
  - `aicv-cv-service`
  - `aicv-ai-service`
  - `aicv-postgres`
  - `aicv-rabbitmq`
  - `aicv-minio`
  - `aicv-keycloak`

## Git/Branch Context

- User wants to push to a branch called `Migration`.
- They previously accidentally committed on `dev-assaad`.
- There were earlier git conflict/deleted-by-us concerns.
- Be careful with git.
- Do not run destructive commands unless user explicitly approves.
- Do not revert unrelated changes.

## Important Files Added/Changed Recently

Backend:

- `services/cv-service/src/AiCv.CvService/Data/Migrations/20260603174000_AddCvGenerationJobs.cs`
- `services/cv-service/src/AiCv.CvService/Data/Migrations/20260603174000_AddCvGenerationJobs.Designer.cs`
- `services/cv-service/src/AiCv.CvService/Data/Migrations/CvDbContextModelSnapshot.cs`
- `services/cv-service/src/AiCv.CvService/Modules/Cvs/Entities/CvGenerationJob.cs`
- `services/cv-service/src/AiCv.CvService/Modules/Cvs/Services/CvGenerationJobService.cs`
- `services/cv-service/src/AiCv.CvService/Modules/Messaging/*`
- `services/cv-service/src/AiCv.CvService/Modules/Cvs/CvController.cs`
- `services/cv-service/src/AiCv.CvService/Modules/Cvs/Services/CvGenerationService.cs`
- `services/cv-service/src/AiCv.CvService/Program.cs`
- `services/opportunity-service/src/AiCv.OpportunityService/Modules/Messaging/*`
- `services/bff-gateway/src/AiCv.BffGateway/Endpoints/ProxyEndpoints.cs`
- `services/bff-gateway/src/AiCv.BffGateway/Middlewares/ExceptionMiddleware.cs`

Frontend:

- `frontend/src/assets/ATS-Friendly-Resume-Template.jpg`
- `frontend/src/modules/landing/pages/LandingPage.jsx`
- `frontend/src/shared/providers/PrototypeAppProvider.jsx`
- `frontend/src/modules/dashboard/pages/DashboardPage.jsx`
- `frontend/src/modules/cv/pages/GenerateCvPage.jsx`
- `frontend/src/modules/cv/pages/MyCvsPage.jsx`
- `frontend/src/shared/components/app-ui.jsx`
- `frontend/src/shared/layouts/DashboardLayout.jsx`
- `frontend/src/styles/globals.css`
- `frontend/package.json`
- `frontend/package-lock.json`

Config:

- `docker-compose.final.yml`
- `.env.example`
- `.gitignore`

## Current Known Warnings

Frontend lint has warnings but no errors:

- Fast Refresh warnings in some provider/component files.
- `profile-editor.jsx` has a React hook dependency warning for `fields`.
- React Router tests show future flag warnings.

These warnings are pre-existing/non-blocking unless the user asks to clean them.

## Current Recommended Next Steps

If continuing from here:

1. Ask the user to retry CV generation from the UI after hard refresh.
2. If CV generation still fails, inspect fresh logs:

```powershell
docker compose -f docker-compose.final.yml logs --tail=160 cv-service
docker compose -f docker-compose.final.yml logs --tail=120 ai-service
docker compose -f docker-compose.final.yml logs --tail=120 bff-gateway
```

3. Verify MinIO storage after successful CV generation.
4. If UI still feels slow, inspect browser performance with the local app and focus on:
   - excessive re-renders in `PrototypeAppProvider`
   - large page-level state updates
   - unnecessary opportunity detail fetches on dashboard
   - image loading strategy on landing page
5. If preparing final delivery, document:
   - current architecture
   - service responsibilities
   - RabbitMQ async CV generation flow
   - Docker Compose deployment
   - tests and verification commands

