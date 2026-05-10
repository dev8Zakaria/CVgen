Sprint 4 — Tickets by Member

Zakaria — Project Lead + Backend + Frontend
- Complete the AI job analysis feature end to end, including the remaining backend logic and AI service integration.
- Finalize the integration between the .NET backend and the AI agent/service used for job analysis.
- Build and refine the job analysis UI so users can trigger the flow and view results clearly.
- Help coordinate the Sprint 4 technical flow across backend, AI service, and frontend work.

Assaad — Backend + File Storage
- Build the backend pieces required for the first CV generation flow without AI-generated CV content.
- Use the CV endpoint results to produce the first CV output structure and returned data.
- Implement the MinIO file storage logic required for CV-related files and assets.
- Support integration between CV generation endpoints and file storage services.

Zentari — Backend QA
- Add backend integration tests for the current features that still do not have automated coverage.
- Cover the remaining backend modules with success, failure, validation, and regression scenarios.
- Help identify backend issues discovered during Sprint 4 test execution and document remaining gaps.

Senku — Frontend QA + Deployment Verification
- Add frontend integration tests for the current features that still do not have automated coverage.
- Cover the remaining frontend modules and user flows with success, error, loading, and regression scenarios.
- Verify that all team members can access the application through the VM IP address.
- Run deployed-environment checks on the main application flows and report access or stability issues.

Youssef — CI/CD + Deployment
- Complete the full CI/CD pipeline for the project.
- Split the pipeline into multiple clear jobs instead of placing everything in a single job.
- Ensure the pipeline covers the main build, test, and deployment steps for the current stack.
- Reuse the existing deployment scripts inside the CI/CD pipeline where appropriate, especially for deployment, restart, logs, and health-check steps.
- Help validate that the deployed application remains accessible through the VM IP address.

Sprint 4 Goal
By the end of Sprint 4:
The AI job analysis feature is complete, the first CV generation flow works using the current CV endpoint results, the remaining backend and frontend areas gain stronger integration-test coverage, the CI/CD pipeline is fully structured, and all team members can access the deployed application through the VM IP address.
