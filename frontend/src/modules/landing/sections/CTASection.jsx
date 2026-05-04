import { Link } from "react-router-dom";

import { Button } from "@/components/ui/button";
import { useAuth } from "@/modules/auth/AuthProvider";
import { ROUTES } from "@/shared/constants/routes";

export function CTASection() {
  const { authEnabled, authenticated, initialized, login } = useAuth();

  return (
    <section className="px-6 py-20">
      <div className="container rounded-[2rem] bg-slate-950 px-8 py-14 text-center text-white">
        <h2 className="font-display text-3xl font-bold">Ready for the first real workflow?</h2>
        <p className="mx-auto mt-4 max-w-xl text-slate-300">
          The app is now wired for authentication, protected routes, and a live profile fetch from the .NET backend.
        </p>
        {authEnabled && initialized && !authenticated ? (
          <Button
            type="button"
            size="lg"
            className="mt-8 rounded-full bg-white text-slate-950 transition hover:-translate-y-0.5 hover:bg-white/90"
            onClick={() => login({ redirectUri: `${window.location.origin}${ROUTES.profile}` })}
          >
            Log in to continue
          </Button>
        ) : (
          <Button
            asChild
            size="lg"
            className="mt-8 rounded-full bg-white text-slate-950 transition hover:-translate-y-0.5 hover:bg-white/90"
          >
            <Link to={ROUTES.cv}>Open CV module</Link>
          </Button>
        )}
      </div>
    </section>
  );
}
