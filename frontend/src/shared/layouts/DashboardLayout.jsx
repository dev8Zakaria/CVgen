import { Bell, FileText, LayoutDashboard, LogOut, PlusCircle, Search, User } from "lucide-react";
import { NavLink, Outlet } from "react-router-dom";

import { Button } from "@/components/ui/button";
import { useAuth } from "@/modules/auth/AuthProvider";
import { AppLogo, ThemeToggle } from "@/shared/components/app-ui";
import { ROUTES } from "@/shared/constants/routes";
import { useTheme } from "@/shared/providers/ThemeProvider";
import { usePrototypeApp } from "@/shared/providers/PrototypeAppProvider";

const navigation = [
  { label: "Dashboard", icon: LayoutDashboard, to: ROUTES.dashboard },
  { label: "My Profile", icon: User, to: ROUTES.profile },
  { label: "Job Offers", icon: Search, to: ROUTES.opportunities },
  { label: "My CVs", icon: FileText, to: ROUTES.cvs },
  { label: "Generate CV", icon: PlusCircle, to: ROUTES.generateCv },
];

export function DashboardLayout() {
  const { theme, toggleTheme } = useTheme();
  const { profile } = usePrototypeApp();
  const { logout, user } = useAuth();

  const displayName = profile.fullName || user?.name || user?.preferred_username || "Creative User";
  const initials = displayName
    .split(" ")
    .slice(0, 2)
    .map((part) => part[0])
    .join("")
    .toUpperCase();

  return (
    <div className="grid min-h-[calc(100vh-2rem)] gap-0 lg:grid-cols-[296px_minmax(0,1fr)]">
      <aside className="border-b border-border/70 bg-white/52 px-5 py-5 backdrop-blur-xl dark:bg-slate-950/55 lg:border-b-0 lg:border-r">
        <div className="flex items-center justify-between lg:block">
          <AppLogo compact />
          <div className="flex items-center gap-2 lg:hidden">
            <ThemeToggle theme={theme} toggleTheme={toggleTheme} />
            <Button type="button" variant="ghost" size="icon" onClick={() => logout?.({ redirectUri: window.location.origin })}>
              <LogOut className="h-4 w-4" />
            </Button>
          </div>
        </div>

        <nav className="mt-6 grid gap-2 lg:mt-10">
          {navigation.map((item) => (
            <NavLink
              key={item.label}
              to={item.to}
              className={({ isActive }) =>
                `flex items-center gap-3 rounded-md px-3 py-2 text-sm font-medium transition-colors ${
                  isActive
                    ? "bg-secondary text-secondary-foreground"
                    : "text-muted-foreground hover:bg-secondary/50 hover:text-foreground"
                }`
              }
            >
              <item.icon className="h-4 w-4" />
              {item.label}
            </NavLink>
          ))}
          <Button
            type="button"
            variant="ghost"
            className="justify-start rounded-2xl px-4"
            onClick={() => logout?.({ redirectUri: window.location.origin })}
          >
            <LogOut className="mr-3 h-4 w-4" />
            Logout
          </Button>
        </nav>
      </aside>

      <div className="min-w-0">
        <header className="sticky top-0 z-10 flex h-16 items-center border-b bg-background/95 px-6 backdrop-blur md:px-8">
          <div className="flex w-full items-center justify-between">
            <div>
              <h1 className="font-display text-lg font-semibold tracking-tight text-foreground">Welcome, {displayName.split(" ")[0]}</h1>
            </div>

            <div className="flex items-center gap-4">
              <div className="hidden items-center gap-2 text-sm text-muted-foreground md:flex">
                <Bell className="h-4 w-4" />
                Notifications
              </div>
              <ThemeToggle theme={theme} toggleTheme={toggleTheme} />
              <div className="flex h-9 w-9 items-center justify-center rounded-full bg-primary text-sm font-medium text-primary-foreground">
                {initials}
              </div>
            </div>
          </div>
        </header>

        <main className="px-5 py-6 md:px-8 md:py-8">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
