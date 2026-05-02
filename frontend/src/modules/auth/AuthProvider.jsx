import { createContext, useContext, useEffect, useMemo, useState } from "react";

import { isKeycloakConfigured, keycloak } from "@/modules/auth/keycloak";

const AuthContext = createContext(null);

const authEnabled = import.meta.env.VITE_AUTH_ENABLED === "true";

export function AuthProvider({ children }) {
  const [initialized, setInitialized] = useState(!authEnabled);
  const [authenticated, setAuthenticated] = useState(!authEnabled);

  useEffect(() => {
    if (!authEnabled) {
      return;
    }

    if (!isKeycloakConfigured) {
      setInitialized(true);
      setAuthenticated(false);
      return;
    }

    keycloak
      .init({ onLoad: "check-sso", pkceMethod: "S256" })
      .then((isAuthenticated) => {
        setAuthenticated(isAuthenticated);
        setInitialized(true);
      })
      .catch(() => {
        setAuthenticated(false);
        setInitialized(true);
      });
  }, []);

  const value = useMemo(
    () => ({
      authEnabled,
      initialized,
      authenticated,
      token: keycloak?.token,
      login: () => keycloak?.login(),
      logout: () => keycloak?.logout(),
    }),
    [authenticated, initialized],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);

  if (!context) {
    throw new Error("useAuth must be used inside AuthProvider");
  }

  return context;
}
