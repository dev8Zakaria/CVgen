import { Link } from "react-router-dom";

import { Button } from "@/components/ui/button";
import { ROUTES } from "@/shared/constants/routes";

export function HeroSection() {
  return (
    <section className="relative overflow-hidden bg-[radial-gradient(circle_at_top_left,_#dbeafe,_transparent_35%),linear-gradient(135deg,_#f8fafc,_#eef2ff)] px-6 py-24">
      <div className="container grid gap-8 lg:grid-cols-[1.1fr_0.9fr] lg:items-center">
        <div className="space-y-6">
          <p className="text-sm font-semibold uppercase tracking-[0.3em] text-primary">
            AI CV Generator
          </p>
          <h1 className="font-display text-4xl font-bold tracking-tight text-foreground sm:text-6xl">
            Build a CV that speaks directly to each opportunity.
          </h1>
          <p className="max-w-2xl text-lg text-muted-foreground">
            Create your profile once, paste a job offer, and prepare a tailored CV workflow powered by our .NET and AI backend.
          </p>
          <div className="flex flex-wrap gap-3">
            <Button asChild size="lg" className="rounded-full shadow-lg shadow-slate-900/10 transition hover:-translate-y-0.5">
              <Link to={ROUTES.profile}>Start your profile</Link>
            </Button>
            <Button
              asChild
              size="lg"
              variant="outline"
              className="rounded-full bg-background/80 backdrop-blur transition hover:-translate-y-0.5"
            >
              <Link to={ROUTES.opportunities}>Add an opportunity</Link>
            </Button>
          </div>
        </div>
        <div className="rounded-[2rem] border border-white/70 bg-white/70 p-6 shadow-2xl shadow-slate-900/10 backdrop-blur">
          <div className="rounded-[1.5rem] bg-slate-950 p-5 text-slate-100">
            <p className="text-sm text-slate-400">Generation pipeline</p>
            <div className="mt-6 space-y-4 font-mono text-sm">
              <p>profile.json + opportunity.txt</p>
              <p className="text-cyan-300">analysis.matchScore = 92%</p>
              <p className="text-emerald-300">cv.status = "ready"</p>
            </div>
          </div>
        </div>
      </div>
    </section>
  );
}
