# Keycloak Local Development

This folder contains local Keycloak bootstrap files for Docker Compose.

`realm-export.json` is imported when Keycloak starts with `--import-realm`. It prepares the `ai-cv-generator` realm and a public `frontend-client` for local development.

Local URLs:

- Keycloak admin console: http://localhost:8080
- Realm issuer for the browser: http://localhost:8080/realms/ai-cv-generator
- Realm issuer inside Docker containers: http://keycloak:8080/realms/ai-cv-generator

Default admin credentials are configured through the root `.env.example`.
