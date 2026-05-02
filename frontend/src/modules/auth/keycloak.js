import Keycloak from "keycloak-js";

const keycloakConfig = {
  url: import.meta.env.VITE_KEYCLOAK_URL,
  realm: import.meta.env.VITE_KEYCLOAK_REALM,
  clientId: import.meta.env.VITE_KEYCLOAK_CLIENT_ID,
};

export const isKeycloakConfigured = Boolean(
  keycloakConfig.url && keycloakConfig.realm && keycloakConfig.clientId,
);

export const keycloak = isKeycloakConfigured ? new Keycloak(keycloakConfig) : null;
