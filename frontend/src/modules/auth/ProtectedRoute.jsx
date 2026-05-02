import { Navigate, useLocation } from "react-router-dom";

import { useAuth } from "@/modules/auth/AuthProvider";
import { ROUTES } from "@/shared/constants/routes";

export function ProtectedRoute({ children }) {
  const location = useLocation();
  const { authEnabled, authenticated, initialized } = useAuth();

  if (!initialized) {
    return <div className="p-6 text-sm text-muted-foreground">Preparing authentication...</div>;
  }

  if (authEnabled && !authenticated) {
    return <Navigate to={ROUTES.landing} replace state={{ from: location }} />;
  }

  return children;
}
