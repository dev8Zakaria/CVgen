import { ArrowRight, CheckCircle2, CircleOff, Layers3, Sparkles } from "lucide-react";
import { Link, useNavigate, useParams } from "react-router-dom";

import { Button } from "@/components/ui/button";
import { EmptyState, SectionHeading, StatusPill } from "@/shared/components/app-ui";
import { ROUTES } from "@/shared/constants/routes";
import { usePrototypeApp } from "@/shared/providers/PrototypeAppProvider";

export function JobAnalysisPage() {
  const navigate = useNavigate();
  const { offerId } = useParams();
  const { jobOffers } = usePrototypeApp();
  const offer = jobOffers.find((entry) => entry.id === offerId);

  if (!offer || !offer.analysis) {
    return (
      <EmptyState
        title="Analysis not found"
        description="This prototype could not find the requested job analysis."
        action={
          <Button asChild>
            <Link to={ROUTES.opportunities}>Back to job offers</Link>
          </Button>
        }
      />
    );
  }

  const analysis = offer.analysis;

  return (
    <div className="space-y-6">
      <SectionHeading
        eyebrow="Job Analysis Result"
        title={`${offer.jobTitle} at ${offer.companyName}`}
        description="The role has been translated into structured guidance for the AI-powered CV generation flow."
        action={
          <Button type="button" onClick={() => navigate(ROUTES.generateCv, { state: { offerId: offer.id } })}>
            Generate CV based on this job
            <ArrowRight className="ml-2 h-4 w-4" />
          </Button>
        }
      />

      <div className="grid gap-5 lg:grid-cols-3">
        <div className="paper-panel p-6">
          <p className="font-mono text-[0.68rem] uppercase tracking-[0.3em] text-muted-foreground">AI Match Score</p>
          <p className="mt-4 font-display text-6xl tracking-[-0.07em] text-foreground">{analysis.matchScore}%</p>
          <p className="mt-4 text-sm leading-7 text-muted-foreground">{analysis.insight}</p>
        </div>
        <div className="paper-panel p-6">
          <div className="flex items-center gap-3">
            <CheckCircle2 className="h-5 w-5 text-success" />
            <div>
              <p className="font-mono text-[0.68rem] uppercase tracking-[0.3em] text-muted-foreground">Matched Skills</p>
              <p className="mt-2 font-display text-4xl tracking-[-0.05em]">{analysis.matchedSkills.length}</p>
            </div>
          </div>
          <div className="mt-4 flex flex-wrap gap-2">
            {analysis.matchedSkills.map((skill) => (
              <StatusPill key={skill} tone="success">
                {skill}
              </StatusPill>
            ))}
          </div>
        </div>
        <div className="paper-panel p-6">
          <div className="flex items-center gap-3">
            <CircleOff className="h-5 w-5 text-warning" />
            <div>
              <p className="font-mono text-[0.68rem] uppercase tracking-[0.3em] text-muted-foreground">Missing Skills</p>
              <p className="mt-2 font-display text-4xl tracking-[-0.05em]">{analysis.missingSkills.length}</p>
            </div>
          </div>
          <div className="mt-4 flex flex-wrap gap-2">
            {analysis.missingSkills.map((skill) => (
              <StatusPill key={skill} tone="warning">
                {skill}
              </StatusPill>
            ))}
          </div>
        </div>
      </div>

      <div className="grid gap-5 xl:grid-cols-2">
        <div className="paper-panel p-6">
          <div className="flex items-center gap-3">
            <Sparkles className="h-5 w-5 text-primary" />
            <h2 className="font-display text-3xl tracking-[-0.04em] text-foreground">Extracted Skills & Keywords</h2>
          </div>
          <div className="mt-6 flex flex-wrap gap-2">
            {analysis.extractedSkills.map((skill) => (
              <StatusPill key={skill} tone="primary">
                {skill}
              </StatusPill>
            ))}
          </div>
          <div className="mt-6">
            <p className="font-mono text-[0.68rem] uppercase tracking-[0.3em] text-muted-foreground">Keywords</p>
            <div className="mt-3 flex flex-wrap gap-2">
              {analysis.keywords.map((keyword) => (
                <StatusPill key={keyword} tone="accent">
                  {keyword}
                </StatusPill>
              ))}
            </div>
          </div>
        </div>

        <div className="paper-panel p-6">
          <div className="flex items-center gap-3">
            <Layers3 className="h-5 w-5 text-primary" />
            <h2 className="font-display text-3xl tracking-[-0.04em] text-foreground">Responsibilities & Technologies</h2>
          </div>
          <div className="mt-6 space-y-3">
            {analysis.responsibilities.map((item) => (
              <div key={item} className="rounded-[22px] border border-border bg-white/60 p-4 text-sm leading-7 text-muted-foreground dark:bg-white/[0.03]">
                {item}
              </div>
            ))}
          </div>
          <div className="mt-6 flex flex-wrap gap-2">
            {analysis.technologies.map((tech) => (
              <StatusPill key={tech} tone="primary">
                {tech}
              </StatusPill>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}
