import { Link, NavLink, Outlet } from "react-router-dom";

import { Button } from "@/components/ui/button";
import { useAuth } from "@/modules/auth/AuthProvider";

import { ROUTES } from "@/shared/constants/routes";

const navigation = [
  { label: "Profile", to: ROUTES.profile },
  { label: "Opportunities", to: ROUTES.opportunities },
  { label: "CV", to: ROUTES.cv },
];

export function DashboardLayout() {
  const { logout, user } = useAuth();
  const fullName = [user?.given_name, user?.family_name].filter(Boolean).join(" ");
  const displayName = user?.name || fullName || user?.preferred_username || "Authenticated user";

  return (
    <div className="min-h-screen bg-[linear-gradient(180deg,_rgba(239,246,255,0.9),_rgba(248,250,252,0.65))]">
      <header className="border-b bg-background/90 backdrop-blur">
        <nav className="container flex h-16 items-center justify-between px-6">
          <Link to={ROUTES.landing} className="font-display text-lg font-bold">
            AI CV Generator
          </Link>
          <div className="hidden gap-3 text-sm font-medium md:flex">
            {navigation.map((item) => (
              <NavLink
                key={item.to}
                to={item.to}
                className={({ isActive }) =>
                  `rounded-full px-4 py-2 transition ${
                    isActive ? "bg-primary text-primary-foreground" : "text-muted-foreground hover:text-foreground"
                  }`
                }
              >
                {item.label}
              </NavLink>
            ))}
          </div>
          <div className="flex items-center gap-3">
            <div className="hidden text-right md:block">
              <p className="text-xs font-semibold uppercase tracking-[0.22em] text-muted-foreground">Connected with Keycloak</p>
              <p className="text-sm font-medium text-foreground">{displayName}</p>
            </div>
            <Button
              type="button"
              variant="outline"
              className="rounded-full"
              onClick={() => logout({ redirectUri: window.location.origin })}
            >
              Log out
            </Button>
          </div>
        </nav>
      </header>
      <main className="container px-6 py-10">
        <Outlet />
      </main>
    </div>
  );
}
