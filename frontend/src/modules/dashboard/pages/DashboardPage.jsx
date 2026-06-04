import { ArrowRight, BriefcaseBusiness, Clock3, FileText, Sparkles } from "lucide-react";
import { Link } from "react-router-dom";

import { Button } from "@/components/ui/button";
import { EmptyState, SectionHeading, StatCard, StatusPill } from "@/shared/components/app-ui";
import { ROUTES, getCvPreviewRoute, getJobAnalysisRoute } from "@/shared/constants/routes";
import { usePrototypeApp } from "@/shared/providers/PrototypeAppProvider";

export function DashboardPage() {
  const { hydrated, profile, jobOffers, cvs } = usePrototypeApp();
  const recentCv = cvs.slice(0, 3);
  const recentOffers = jobOffers.slice(0, 3);
  const analyzedOffers = jobOffers.filter((offer) => offer.analysis).length;
  const firstName = profile.fullName?.trim().split(" ")[0] || "there";
  const profileSections = [
    profile.experience.length > 0,
    profile.education.length > 0,
    profile.projects.length > 0,
    profile.skills.length > 0,
  ];
  const profileScore = Math.round((profileSections.filter(Boolean).length / profileSections.length) * 100);

  if (!hydrated) {
    return (
      <div className="paper-panel p-6">
        <p className="text-sm font-medium text-muted-foreground">Loading workspace data...</p>
        <h1 className="mt-2 font-display text-3xl font-bold tracking-tight text-foreground">Preparing your dashboard</h1>
        <p className="mt-2 max-w-xl text-sm text-muted-foreground">
          Your profile shell is ready. Backend data will appear here as soon as the services respond.
        </p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <SectionHeading
        eyebrow="Dashboard"
        title={`Welcome back, ${firstName}.`}
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
        <StatCard label="CVs Generated" value={cvs.length} meta="Saved PDFs and tailored drafts ready to preview." accent="Library" />
        <StatCard label="Analyzed Offers" value={`${analyzedOffers}/${jobOffers.length}`} meta="Roles with extracted skills, keywords, and focus points." accent="Signal" />
        <StatCard label="Profile Readiness" value={`${profileScore}%`} meta="Experience, education, projects, and skills coverage." accent="Quality" />
      </div>

      <div className="grid gap-6 xl:grid-cols-[1.15fr_0.85fr]">
        <div className="paper-panel p-6">
          <div className="flex items-center justify-between gap-4 border-b pb-4">
            <div>
              <p className="text-sm font-medium text-muted-foreground">Recent Activity</p>
              <h2 className="mt-1 font-display text-2xl font-bold text-foreground">Last generated CVs</h2>
            </div>
            <Button asChild variant="outline" size="sm">
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
                  className="flex items-center justify-between rounded-lg border bg-card p-4 transition-colors hover:border-primary/25 hover:bg-primary/[0.03]"
                >
                  <div>
                    <p className="font-semibold text-foreground">{cv.jobTitle}</p>
                    <p className="text-sm text-muted-foreground">{cv.companyName}</p>
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

        <div className="grid gap-6">
          <div className="paper-panel border-primary/20 bg-primary/[0.03] p-6">
            <div className="flex items-center gap-3 border-b pb-4">
              <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-primary/10">
                <Clock3 className="h-5 w-5 text-primary" />
              </div>
              <div>
                <p className="text-sm font-medium text-muted-foreground">Next best action</p>
                <h2 className="font-display text-xl font-bold text-foreground">
                  {analyzedOffers > 0 ? "Generate a targeted CV" : "Analyze a job offer"}
                </h2>
              </div>
            </div>
            <p className="mt-4 text-sm text-muted-foreground">
              {analyzedOffers > 0
                ? "You already have analyzed opportunities. Pick one and let the CV service create a role-specific PDF."
                : "Start by adding a job description so the platform can extract ATS keywords and role expectations."}
            </p>
            <Button asChild className="mt-6 w-full">
              <Link to={analyzedOffers > 0 ? ROUTES.generateCv : ROUTES.addOpportunity}>
                {analyzedOffers > 0 ? "Generate CV" : "Add job offer"}
                <ArrowRight className="ml-2 h-4 w-4" />
              </Link>
            </Button>
          </div>

          <div className="paper-panel p-6">
            <div className="flex items-center gap-3 border-b pb-4">
              <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-primary/10">
                <BriefcaseBusiness className="h-5 w-5 text-primary" />
              </div>
              <div>
                <p className="text-sm font-medium text-muted-foreground">Tracked Offers</p>
                <h2 className="font-display text-xl font-bold text-foreground">Latest Opportunities</h2>
              </div>
            </div>
            <div className="mt-6 space-y-3">
              {recentOffers.map((offer) => (
                <Link
                  key={offer.id}
                  to={getJobAnalysisRoute(offer.id)}
                  className="flex items-center justify-between rounded-lg border bg-card p-3 transition-colors hover:border-primary/25 hover:bg-primary/[0.03]"
                >
                  <div>
                    <p className="font-medium text-foreground">{offer.jobTitle}</p>
                    <p className="text-xs text-muted-foreground">{offer.companyName}</p>
                  </div>
                  <StatusPill tone="success">Analyzed</StatusPill>
                </Link>
              ))}
            </div>
            <Button asChild variant="outline" className="mt-6 w-full">
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
