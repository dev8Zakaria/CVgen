import { Link } from "react-router-dom";

import { Button } from "@/components/ui/button";
import { ROUTES } from "@/shared/constants/routes";

export function CTASection() {
  return (
    <section className="px-6 py-20">
      <div className="container rounded-[2rem] bg-slate-950 px-8 py-14 text-center text-white">
        <h2 className="font-display text-3xl font-bold">Ready for the first real workflow?</h2>
        <p className="mx-auto mt-4 max-w-xl text-slate-300">
          The structure is prepared for auth, API calls, and future AI-powered CV generation.
        </p>
        <Button asChild size="lg" className="mt-8 rounded-full bg-white text-slate-950 transition hover:-translate-y-0.5 hover:bg-white/90">
          <Link to={ROUTES.cv}>Open CV module</Link>
        </Button>
      </div>
    </section>
  );
}
