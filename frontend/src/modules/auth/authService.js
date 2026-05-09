import { keycloak } from "@/modules/auth/keycloak";

export const authService = {
  login: (options) => keycloak?.login(options),
  logout: (options) => keycloak?.logout(options),
  register: (options) => keycloak?.register(options),
  getToken: () => keycloak?.token,
  updateToken: (minValidity = 30) => keycloak?.updateToken(minValidity),
  getUser: () => keycloak?.tokenParsed ?? null,
};
