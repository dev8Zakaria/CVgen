import { ArrowRight, FileText, Layers3, Search, Sparkles } from "lucide-react";
import { Link, Navigate } from "react-router-dom";

import resumeTemplateUrl from "@/assets/ATS-Friendly-Resume-Template.jpg";
import { Button } from "@/components/ui/button";
import { useAuth } from "@/modules/auth/AuthProvider";
import { StatusPill } from "@/shared/components/app-ui";
import { ROUTES } from "@/shared/constants/routes";

const cards = [
  {
    icon: Layers3,
    title: "Profile Studio",
    copy: "One structured profile feeds every tailored application.",
  },
  {
    icon: Search,
    title: "Job Analysis",
    copy: "Extract skills, responsibilities, risks, and ATS keywords.",
  },
  {
    icon: FileText,
    title: "ATS CV Output",
    copy: "Generate clean PDF resumes focused on recruiter readability.",
  },
];

export function LandingPage() {
  const { authEnabled, authenticated, initialized, login } = useAuth();

  if (authEnabled && initialized && authenticated) {
    return <Navigate to={ROUTES.dashboard} replace />;
  }

  return (
    <main className="mx-auto max-w-[1320px] px-5 py-8 md:px-8 md:py-12">
      <section className="grid min-h-[calc(100vh-7rem)] gap-10 lg:grid-cols-[0.94fr_1.06fr] lg:items-center">
        <div className="space-y-7">
          <StatusPill tone="accent" className="w-fit">AI CV Generator</StatusPill>

          <div className="space-y-5">
            <h1 className="max-w-4xl font-display text-4xl font-bold leading-[0.98] tracking-tight text-foreground md:text-6xl">
              Turn a profile and job offer into a polished ATS-ready CV.
            </h1>
            <p className="max-w-2xl text-base leading-8 text-muted-foreground md:text-lg">
              Analyze the offer, select the right profile evidence, and generate a professional PDF that looks clean to recruiters and stays readable for ATS systems.
            </p>
          </div>

          <div className="flex flex-wrap gap-3">
            {authenticated ? (
              <Button asChild size="lg" className="h-12 px-7">
                <Link to={ROUTES.dashboard}>
                  Enter the app
                  <ArrowRight className="ml-2 h-4 w-4" />
                </Link>
              </Button>
            ) : authEnabled ? (
              <Button
                type="button"
                size="lg"
                className="h-12 px-7"
                onClick={() => login?.({ redirectUri: `${window.location.origin}${ROUTES.dashboard}` })}
              >
                Login with Keycloak
                <ArrowRight className="ml-2 h-4 w-4" />
              </Button>
            ) : (
              <Button asChild size="lg" className="h-12 px-7">
                <Link to={ROUTES.dashboard}>
                  Open local prototype
                  <Sparkles className="ml-2 h-4 w-4" />
                </Link>
              </Button>
            )}
            <Button asChild variant="outline" size="lg" className="h-12 px-7">
              <Link to={ROUTES.generateCv}>Preview generation flow</Link>
            </Button>
          </div>

          <div className="grid gap-3 pt-2 sm:grid-cols-3">
            {cards.map((card) => (
              <div key={card.title} className="rounded-xl border bg-card/90 p-4 transition-colors hover:border-primary/25">
                <card.icon className="h-5 w-5 text-primary" />
                <h2 className="mt-3 text-sm font-semibold text-foreground">{card.title}</h2>
                <p className="mt-1 text-sm leading-6 text-muted-foreground">{card.copy}</p>
              </div>
            ))}
          </div>
        </div>

        <div className="relative mx-auto w-full max-w-[640px] lg:max-w-none">
          <div className="relative z-10 mx-auto max-w-[470px] rounded-[28px] border bg-white p-3 shadow-[0_24px_80px_rgba(15,23,42,0.14)]">
            <img
              src={resumeTemplateUrl}
              alt="ATS-friendly resume template preview"
              className="aspect-[1240/1754] w-full rounded-[20px] object-cover object-top"
              loading="eager"
              decoding="async"
            />
          </div>
        </div>
      </section>
    </main>
  );
}
