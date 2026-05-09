import { ArrowRight, Sparkles } from "lucide-react";
import { Link, Outlet, useLocation } from "react-router-dom";

import { Button } from "@/components/ui/button";
import { useAuth } from "@/modules/auth/AuthProvider";
import { AppLogo, ThemeToggle } from "@/shared/components/app-ui";
import { ROUTES } from "@/shared/constants/routes";
import { useTheme } from "@/shared/providers/ThemeProvider";

export function MainLayout() {
  const location = useLocation();
  const { theme, toggleTheme } = useTheme();
  const { authEnabled, authenticated, initialized, login, logout } = useAuth();

  const redirectUri = `${window.location.origin}${location.pathname}`;

  return (
    <div className="min-h-screen px-4 py-4 md:px-6">
      <div className="app-shell min-h-[calc(100vh-2rem)]">
        <header className="border-b border-border/70 px-6 py-5 md:px-10">
          <div className="mx-auto flex max-w-[1360px] items-center justify-between gap-4">
            <AppLogo />
            <div className="flex items-center gap-3">
              <ThemeToggle theme={theme} toggleTheme={toggleTheme} />
              {initialized && authenticated ? (
                <>
                  <Button asChild variant="outline">
                    <Link to={ROUTES.dashboard}>Enter App</Link>
                  </Button>
                  <Button type="button" variant="ghost" onClick={() => logout?.({ redirectUri: window.location.origin })}>
                    Logout
                  </Button>
                </>
              ) : authEnabled ? (
                <Button type="button" onClick={() => login?.({ redirectUri })}>
                  Login
                  <ArrowRight className="ml-2 h-4 w-4" />
                </Button>
              ) : (
                <Button asChild>
                  <Link to={ROUTES.dashboard}>
                    Open Prototype
                    <Sparkles className="ml-2 h-4 w-4" />
                  </Link>
                </Button>
              )}
            </div>
          </div>
        </header>
        <Outlet />
      </div>
    </div>
  );
}
