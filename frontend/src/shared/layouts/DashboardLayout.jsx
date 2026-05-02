import { Link, Outlet } from "react-router-dom";

import { ROUTES } from "@/shared/constants/routes";

const navigation = [
  { label: "Profile", to: ROUTES.profile },
  { label: "Opportunities", to: ROUTES.opportunities },
  { label: "CV", to: ROUTES.cv },
];

export function DashboardLayout() {
  return (
    <div className="min-h-screen bg-muted/40">
      <header className="border-b bg-background">
        <nav className="container flex h-16 items-center justify-between">
          <Link to={ROUTES.landing} className="font-display text-lg font-bold">
            AI CV Generator
          </Link>
          <div className="flex gap-3 text-sm font-medium">
            {navigation.map((item) => (
              <Link key={item.to} to={item.to} className="text-muted-foreground transition hover:text-foreground">
                {item.label}
              </Link>
            ))}
          </div>
        </nav>
      </header>
      <main className="container py-10">
        <Outlet />
      </main>
    </div>
  );
}
