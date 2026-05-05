Sprint 3 — Tickets by Member

Zakaria — Project Lead + Backend + Frontend
- Design the Sprint 3 technical flow for job offer analysis and CV generation.
- Implement the backend endpoint to trigger AI analysis for a saved job offer.
- Connect the .NET backend to the AI service client and store the returned analysis in the database.
- Manage job offer analysis status transitions: pending, processing, completed, failed.
- Implement the first CV generation backend endpoint using the authenticated profile and analyzed job offer.
- Define the response shape for CV preview data and help connect the CV page to the new backend flow.

Assaad — Backend + CI/CD + Keycloak
- Scaffold the AI FastAPI service with a health endpoint and a first job-offer analysis contract.
- Dockerize the AI service and connect it cleanly in Docker Compose with environment variables.
- Stabilize Keycloak and backend environment configuration for local and VM usage.
- Extend backend CI so it restores, builds, and runs backend tests automatically.
- Add Swagger or API documentation notes for the new analysis and CV generation endpoints.
- Help review service-to-service integration between backend API and AI service.

Zentari — Backend + QA
- Expand automated backend integration tests for profile and opportunity APIs.
- Add tests for unauthorized access, ownership isolation, validation failures, and delete/update edge cases.
- Add backend tests for AI analysis status changes and analysis reset behavior after editing a job offer.
- Prepare backend test scenarios for the new analysis and CV generation endpoints.
- Fix backend issues discovered during test execution and document remaining blockers.

Senku — Frontend QA + Hosting Infrastructure
- Add frontend tests for protected routes, profile page states, opportunity page flows, and CV page states.
- Test loading, error, success, and empty states for the main authenticated screens.
- Run manual end-to-end QA on the full stack: login, profile update, opportunity creation, analysis display, and CV flow.
- Verify responsive behavior and browser stability for the main pages before the sprint review.
- Update the deployment QA checklist for the Sprint 3 stack on the VM.

Youssef — Hosting Infrastructure + CI/CD
- Add a frontend CI pipeline that installs dependencies, lints, builds, and runs tests.
- Update deployment scripts and environment templates for the AI service and Sprint 3 stack.
- Prepare restart, logs, and health-check scripts for frontend, backend, Keycloak, PostgreSQL, and AI service.
- Verify Docker volumes, persistence, and recovery after container restart.
- Support VM deployment of the Sprint 3 stack and validate the full deployed workflow.

Sprint 3 Goal
By the end of Sprint 3:
User can log in, manage profile, create a job offer, trigger AI analysis, view structured analysis in the application, start the first CV generation flow, and the project has automated backend and frontend tests running in CI with an updated deployable stack.
