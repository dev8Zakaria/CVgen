import { Navigate, useLocation } from "react-router-dom";

import { useAuth } from "@/modules/auth/AuthProvider";
import { ROUTES } from "@/shared/constants/routes";

export function ProtectedRoute({ children }) {
  const location = useLocation();
  const { authEnabled, authenticated, initialized } = useAuth();

  if (!initialized) {
    return (
      <div className="min-h-screen px-6 py-10">
        <div className="paper-panel mx-auto max-w-xl p-10 text-center">
          <p className="font-mono text-xs uppercase tracking-[0.3em] text-muted-foreground">Authentication</p>
          <h1 className="mt-4 font-display text-4xl tracking-[-0.05em] text-foreground">Preparing your studio</h1>
          <p className="mt-3 text-sm text-muted-foreground">Checking your external session before loading the app shell.</p>
        </div>
      </div>
    );
  }

  if (authEnabled && !authenticated) {
    return <Navigate to={ROUTES.landing} replace state={{ from: location }} />;
  }

  return children;
}
