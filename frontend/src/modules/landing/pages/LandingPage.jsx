import { ArrowRight, Bot, FileText, Layers3, Sparkles, Target } from "lucide-react";
import { Link, Navigate } from "react-router-dom";

import { Button } from "@/components/ui/button";
import { useAuth } from "@/modules/auth/AuthProvider";
import { SectionHeading, StatCard, StatusPill } from "@/shared/components/app-ui";
import { ROUTES } from "@/shared/constants/routes";

const cards = [
  {
    icon: Layers3,
    title: "Profile Studio",
    copy: "Build a rich master profile with education, experience, projects, certifications, and narrative detail that AI can actually use.",
  },
  {
    icon: Target,
    title: "Job Analysis",
    copy: "Turn every job description into skills, responsibilities, technologies, and matched versus missing capabilities.",
  },
  {
    icon: FileText,
    title: "Tailored CVs",
    copy: "Generate polished, role-specific resumes, edit them inline, switch templates, and keep a clean archive of versions.",
  },
];

export function LandingPage() {
  const { authEnabled, authenticated, initialized, login } = useAuth();

  if (authEnabled && initialized && authenticated) {
    return <Navigate to={ROUTES.dashboard} replace />;
  }

  return (
    <main className="mx-auto max-w-[1360px] px-6 py-10 md:px-10 md:py-16">
      <section className="grid gap-10 lg:grid-cols-[1.05fr_0.95fr] lg:items-center">
        <div className="space-y-8 animate-reveal">
          <StatusPill tone="accent">Premium prototype direction</StatusPill>
          <SectionHeading
            eyebrow="AI-Powered Career Studio"
            title="Design each application like a crafted document."
            description="This platform helps ambitious candidates build a master professional profile, analyze job offers with AI, and generate high-signal CVs tailored to each opportunity."
          />

          <div className="flex flex-wrap gap-4">
            {authenticated ? (
              <Button asChild size="lg" className="h-12 px-8">
                <Link to={ROUTES.dashboard}>
                  Enter the app
                  <ArrowRight className="ml-2 h-4 w-4" />
                </Link>
              </Button>
            ) : authEnabled ? (
              <Button type="button" size="lg" className="h-12 px-8" onClick={() => login?.({ redirectUri: `${window.location.origin}${ROUTES.dashboard}` })}>
                Login with Keycloak
                <ArrowRight className="ml-2 h-4 w-4" />
              </Button>
            ) : (
              <Button asChild size="lg" className="h-12 px-8">
                <Link to={ROUTES.dashboard}>
                  Open local prototype
                  <Sparkles className="ml-2 h-4 w-4" />
                </Link>
              </Button>
            )}
            <Button asChild variant="outline" size="lg" className="h-12 px-8">
              <Link to={ROUTES.generateCv}>Preview flow</Link>
            </Button>
          </div>

          <div className="grid gap-4 md:grid-cols-3">
            <StatCard label="Scope" value="9" meta="Core screens" accent="Complete" />
            <StatCard label="Design" value="1" meta="Clean shadcn UI" accent="Consistent" />
            <StatCard label="Versions" value="∞" meta="Tailored applications" accent="Flexible" />
          </div>
        </div>

        <div className="paper-panel relative overflow-hidden p-6 md:p-8">
          <div className="absolute -right-20 -top-20 h-64 w-64 rounded-full bg-primary/5 blur-3xl" />
          <div className="relative grid gap-6">
            <div className="rounded-lg border bg-card p-6 shadow-sm">
              <div className="flex items-start justify-between gap-4">
                <div>
                  <p className="text-xs font-medium text-primary">Active Job</p>
                  <h3 className="mt-1 font-display text-xl font-semibold text-foreground">Senior Product Designer, AI Tools</h3>
                  <p className="mt-1 text-sm text-muted-foreground">Notion · Remote EMEA</p>
                </div>
                <StatusPill tone="success">Analyzed</StatusPill>
              </div>
              <div className="mt-4 flex flex-wrap gap-2">
                <StatusPill tone="primary">Design Systems</StatusPill>
                <StatusPill tone="primary">Storytelling</StatusPill>
                <StatusPill tone="accent">AI Tooling</StatusPill>
              </div>
            </div>

            <div className="rounded-lg border border-primary/20 bg-primary/5 p-6">
              <div className="flex items-center gap-2">
                <Bot className="h-5 w-5 text-primary" />
                <p className="font-semibold text-primary">AI Insight</p>
              </div>
              <p className="mt-2 text-sm leading-relaxed text-foreground">
                Your strongest angle is translating complex systems into elegant decisions. Emphasize high-trust product thinking, recruiter clarity, and measurable outcomes.
              </p>
            </div>

            <div className="grid gap-4 md:grid-cols-2">
              {cards.map((card) => (
                <div key={card.title} className="rounded-lg border bg-card p-5 shadow-sm">
                  <card.icon className="h-5 w-5 text-primary" />
                  <h4 className="mt-3 font-semibold text-foreground">{card.title}</h4>
                  <p className="mt-2 text-sm text-muted-foreground">{card.copy}</p>
                </div>
              ))}
            </div>
          </div>
        </div>
      </section>
    </main>
  );
}
