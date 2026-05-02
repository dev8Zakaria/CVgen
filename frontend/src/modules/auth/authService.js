import { keycloak } from "@/modules/auth/keycloak";

export const authService = {
  login: () => keycloak?.login(),
  logout: () => keycloak?.logout(),
  getToken: () => keycloak?.token,
  updateToken: (minValidity = 30) => keycloak?.updateToken(minValidity),
};
