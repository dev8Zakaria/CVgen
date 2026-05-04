import { Link, Outlet, useLocation } from "react-router-dom";

import { Button } from "@/components/ui/button";
import { useAuth } from "@/modules/auth/AuthProvider";
import { ROUTES } from "@/shared/constants/routes";

export function MainLayout() {
  const location = useLocation();
  const { authEnabled, authenticated, initialized, login, register, logout } = useAuth();

  const redirectUri = `${window.location.origin}${location.pathname}`;

  return (
    <div className="min-h-screen bg-background">
      <header className="sticky top-0 z-10 border-b border-white/60 bg-background/80 backdrop-blur">
        <div className="container flex h-16 items-center justify-between px-6">
          <Link to={ROUTES.landing} className="font-display text-lg font-bold tracking-tight text-foreground">
            AI CV Generator
          </Link>
          <div className="flex items-center gap-3">
            {authEnabled && initialized ? (
              authenticated ? (
                <>
                  <Button asChild variant="outline" className="rounded-full">
                    <a href={ROUTES.profile}>Dashboard</a>
                  </Button>
                  <Button
                    type="button"
                    variant="ghost"
                    className="rounded-full"
                    onClick={() => logout({ redirectUri: window.location.origin })}
                  >
                    Log out
                  </Button>
                </>
              ) : (
                <>
                  <Button
                    type="button"
                    variant="outline"
                    className="rounded-full"
                    onClick={() => register({ redirectUri: `${window.location.origin}${ROUTES.profile}` })}
                  >
                    Sign up
                  </Button>
                  <Button
                    type="button"
                    className="rounded-full"
                    onClick={() => login({ redirectUri })}
                  >
                    Log in
                  </Button>
                </>
              )
            ) : null}
          </div>
        </div>
      </header>
      <Outlet />
    </div>
  );
}
