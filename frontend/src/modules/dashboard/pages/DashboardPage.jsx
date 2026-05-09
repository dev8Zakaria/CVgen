import { ArrowRight, BriefcaseBusiness, Clock3, FileText, Sparkles } from "lucide-react";
import { Link } from "react-router-dom";

import { Button } from "@/components/ui/button";
import { EmptyState, SectionHeading, SkeletonBlock, StatCard, StatusPill } from "@/shared/components/app-ui";
import { ROUTES, getCvPreviewRoute, getJobAnalysisRoute } from "@/shared/constants/routes";
import { usePrototypeApp } from "@/shared/providers/PrototypeAppProvider";

export function DashboardPage() {
  const { hydrated, profile, jobOffers, cvs } = usePrototypeApp();
  const recentCv = cvs.slice(0, 3);
  const recentOffers = jobOffers.slice(0, 3);

  if (!hydrated) {
    return (
      <div className="grid gap-5">
        <SkeletonBlock className="h-44" />
        <div className="grid gap-5 lg:grid-cols-3">
          <SkeletonBlock className="h-44" />
          <SkeletonBlock className="h-44" />
          <SkeletonBlock className="h-44" />
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <SectionHeading
        eyebrow="Dashboard"
        title={`Welcome back, ${profile.fullName.split(" ")[0]}.`}
        description="This studio keeps your profile, job offers, and generated CVs in one polished workflow so every application feels deliberate."
        action={
          <Button asChild>
            <Link to={ROUTES.generateCv}>
              Generate New CV
              <Sparkles className="ml-2 h-4 w-4" />
            </Link>
          </Button>
        }
      />

      <div className="grid gap-5 lg:grid-cols-3">
        <StatCard label="CVs Generated" value={cvs.length} meta="Saved drafts and tailored applications ready to preview or export." accent="Library" />
        <StatCard label="Job Offers" value={jobOffers.length} meta="Tracked roles analyzed against your profile and keyword baseline." accent="Tracked" />
        <StatCard label="Current Focus" value={recentOffers[0]?.companyName || "Profile"} meta="Move from role analysis to CV generation with one click." accent="Live" />
      </div>

      <div className="grid gap-5 xl:grid-cols-[1.15fr_0.85fr]">
        <div className="paper-panel p-6">
          <div className="flex items-center justify-between gap-3">
            <div>
              <p className="font-mono text-[0.68rem] uppercase tracking-[0.3em] text-muted-foreground">Recent Activity</p>
              <h2 className="mt-3 font-display text-3xl tracking-[-0.04em] text-foreground">Last generated CVs</h2>
            </div>
            <Button asChild variant="outline">
              <Link to={ROUTES.cvs}>View all</Link>
            </Button>
          </div>

          {recentCv.length === 0 ? (
            <EmptyState
              className="mt-6"
              title="No CVs yet"
              description="Start with a job offer analysis, then generate a tailored CV. Your recent work will appear here."
              action={
                <Button asChild>
                  <Link to={ROUTES.generateCv}>Generate your first CV</Link>
                </Button>
              }
            />
          ) : (
            <div className="mt-6 space-y-3">
              {recentCv.map((cv) => (
                <Link
                  key={cv.id}
                  to={getCvPreviewRoute(cv.id)}
                  className="flex items-center justify-between rounded-[26px] border border-border bg-white/70 p-5 transition hover:translate-x-1 hover:border-primary/20 hover:bg-white dark:bg-white/[0.03]"
                >
                  <div>
                    <p className="font-semibold text-foreground">{cv.jobTitle}</p>
                    <p className="mt-1 text-sm text-muted-foreground">{cv.companyName}</p>
                  </div>
                  <div className="flex items-center gap-3">
                    <StatusPill tone="primary">{cv.template}</StatusPill>
                    <ArrowRight className="h-4 w-4 text-muted-foreground" />
                  </div>
                </Link>
              ))}
            </div>
          )}
        </div>

        <div className="grid gap-5">
          <div className="paper-panel p-6">
            <div className="flex items-center gap-3">
              <Clock3 className="h-5 w-5 text-primary" />
              <div>
                <p className="font-mono text-[0.68rem] uppercase tracking-[0.3em] text-muted-foreground">Profile Completeness</p>
                <h2 className="mt-2 font-display text-3xl tracking-[-0.04em] text-foreground">Everything required to generate premium CVs.</h2>
              </div>
            </div>
            <p className="mt-4 text-sm leading-7 text-muted-foreground">
              Personal data, experience, projects, and certifications are all editable inside the profile studio.
            </p>
            <Button asChild variant="outline" className="mt-5">
              <Link to={ROUTES.profile}>Open profile studio</Link>
            </Button>
          </div>

          <div className="paper-panel p-6">
            <div className="flex items-center gap-3">
              <BriefcaseBusiness className="h-5 w-5 text-primary" />
              <div>
                <p className="font-mono text-[0.68rem] uppercase tracking-[0.3em] text-muted-foreground">Tracked Offers</p>
                <h2 className="mt-2 font-display text-3xl tracking-[-0.04em] text-foreground">Latest opportunities</h2>
              </div>
            </div>
            <div className="mt-5 space-y-3">
              {recentOffers.map((offer) => (
                <Link
                  key={offer.id}
                  to={getJobAnalysisRoute(offer.id)}
                  className="flex items-center justify-between rounded-[24px] border border-border bg-white/70 p-4 transition hover:border-primary/20 dark:bg-white/[0.03]"
                >
                  <div>
                    <p className="font-semibold text-foreground">{offer.jobTitle}</p>
                    <p className="mt-1 text-sm text-muted-foreground">{offer.companyName}</p>
                  </div>
                  <StatusPill tone="success">Analyzed</StatusPill>
                </Link>
              ))}
            </div>
            <Button asChild variant="outline" className="mt-5">
              <Link to={ROUTES.opportunities}>
                Manage job offers
                <ArrowRight className="ml-2 h-4 w-4" />
              </Link>
            </Button>
          </div>
        </div>
      </div>
    </div>
  );
}
