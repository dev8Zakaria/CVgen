import { ArrowRight, CheckCircle2, CircleOff, Layers3, Sparkles } from "lucide-react";
import { Link, useNavigate, useParams } from "react-router-dom";

import { Button } from "@/components/ui/button";
import { EmptyState, SectionHeading, StatusPill } from "@/shared/components/app-ui";
import { ROUTES } from "@/shared/constants/routes";
import { usePrototypeApp } from "@/shared/providers/PrototypeAppProvider";

function DetailBlock({ label, value }) {
  return (
    <div className="rounded-[22px] border border-border bg-white/60 p-4 dark:bg-white/[0.03]">
      <p className="font-mono text-[0.68rem] uppercase tracking-[0.3em] text-muted-foreground">{label}</p>
      <p className="mt-3 text-sm font-semibold text-foreground">{value || "Unknown"}</p>
    </div>
  );
}

function BulletList({ items, emptyMessage, tone = "default" }) {
  if (!items.length) {
    return <p className="text-sm leading-7 text-muted-foreground">{emptyMessage}</p>;
  }

  const toneClass =
    tone === "warning"
      ? "border-amber-500/20 bg-amber-500/10 text-amber-800 dark:text-amber-200"
      : "border-border bg-white/60 text-muted-foreground dark:bg-white/[0.03]";

  return (
    <div className="space-y-3">
      {items.map((item) => (
        <div key={item} className={`rounded-[22px] border p-4 text-sm leading-7 ${toneClass}`}>
          {item}
        </div>
      ))}
    </div>
  );
}

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
  const confidencePercent = Math.round((analysis.confidenceScore || 0) * 100);

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

      <div className="grid gap-5 xl:grid-cols-4">
        <div className="paper-panel xl:col-span-2 p-6">
          <p className="font-mono text-[0.68rem] uppercase tracking-[0.3em] text-muted-foreground">AI Match Score</p>
          <div className="mt-4 flex items-end justify-between gap-4">
            <p className="font-display text-6xl tracking-[-0.07em] text-foreground">{analysis.matchScore}%</p>
            <div className="rounded-[22px] border border-primary/15 bg-primary/10 px-4 py-3 text-right">
              <p className="font-mono text-[0.68rem] uppercase tracking-[0.3em] text-primary">Confidence</p>
              <p className="mt-2 font-display text-3xl tracking-[-0.05em] text-foreground">{confidencePercent}%</p>
            </div>
          </div>
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
            {analysis.matchedSkills.length ? (
              analysis.matchedSkills.map((skill) => (
                <StatusPill key={skill} tone="success">
                  {skill}
                </StatusPill>
              ))
            ) : (
              <p className="text-sm leading-7 text-muted-foreground">No direct profile matches were detected yet.</p>
            )}
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
            {analysis.missingSkills.length ? (
              analysis.missingSkills.map((skill) => (
                <StatusPill key={skill} tone="warning">
                  {skill}
                </StatusPill>
              ))
            ) : (
              <p className="text-sm leading-7 text-muted-foreground">No obvious gaps were flagged from the extracted skills.</p>
            )}
          </div>
        </div>
      </div>

      <div className="paper-panel p-6">
        <div className="flex flex-col gap-4 lg:flex-row lg:items-end lg:justify-between">
          <div>
            <p className="font-mono text-[0.68rem] uppercase tracking-[0.3em] text-muted-foreground">Role Snapshot</p>
            <h2 className="mt-3 font-display text-3xl tracking-[-0.04em] text-foreground">How the model classified this role</h2>
          </div>
          <div className="flex flex-wrap gap-2">
            <StatusPill tone="accent">{analysis.detectedExperienceLevel || "unknown level"}</StatusPill>
            <StatusPill tone="accent">{analysis.detectedLocation || "unknown location"}</StatusPill>
            <StatusPill tone="accent">{analysis.detectedContractType || "unknown contract"}</StatusPill>
          </div>
        </div>
        <div className="mt-6 grid gap-4 md:grid-cols-3">
          <DetailBlock label="Experience Level" value={analysis.detectedExperienceLevel} />
          <DetailBlock label="Location Mode" value={analysis.detectedLocation} />
          <DetailBlock label="Contract Type" value={analysis.detectedContractType} />
        </div>
      </div>

      <div className="grid gap-5 xl:grid-cols-2">
        <div className="paper-panel p-6">
          <div className="flex items-center gap-3">
            <Sparkles className="h-5 w-5 text-primary" />
            <h2 className="font-display text-3xl tracking-[-0.04em] text-foreground">Extracted Skills & Keywords</h2>
          </div>
          <div className="mt-6 flex flex-wrap gap-2">
            {analysis.extractedSkills.length ? (
              analysis.extractedSkills.map((skill) => (
                <StatusPill key={skill} tone="primary">
                  {skill}
                </StatusPill>
              ))
            ) : (
              <p className="text-sm leading-7 text-muted-foreground">No extracted skills were returned for this role.</p>
            )}
          </div>
          <div className="mt-6">
            <p className="font-mono text-[0.68rem] uppercase tracking-[0.3em] text-muted-foreground">Keywords</p>
            <div className="mt-3 flex flex-wrap gap-2">
              {analysis.keywords.length ? (
                analysis.keywords.map((keyword) => (
                  <StatusPill key={keyword} tone="accent">
                    {keyword}
                  </StatusPill>
                ))
              ) : (
                <p className="text-sm leading-7 text-muted-foreground">No additional keywords were extracted.</p>
              )}
            </div>
          </div>
        </div>

        <div className="paper-panel p-6">
          <div className="flex items-center gap-3">
            <Layers3 className="h-5 w-5 text-primary" />
            <h2 className="font-display text-3xl tracking-[-0.04em] text-foreground">Responsibilities & Technologies</h2>
          </div>
          <div className="mt-6">
            <BulletList
              items={analysis.responsibilities}
              emptyMessage="No responsibilities were extracted from the job description."
            />
          </div>
          <div className="mt-6">
            <p className="font-mono text-[0.68rem] uppercase tracking-[0.3em] text-muted-foreground">Technologies</p>
            <div className="mt-3 flex flex-wrap gap-2">
              {analysis.technologies.length ? (
                analysis.technologies.map((tech) => (
                  <StatusPill key={tech} tone="primary">
                    {tech}
                  </StatusPill>
                ))
              ) : (
                <p className="text-sm leading-7 text-muted-foreground">No technologies were detected for this role.</p>
              )}
            </div>
          </div>
        </div>
      </div>

      <div className="grid gap-5 xl:grid-cols-2">
        <div className="paper-panel p-6">
          <p className="font-mono text-[0.68rem] uppercase tracking-[0.3em] text-muted-foreground">Requirements</p>
          <h2 className="mt-3 font-display text-3xl tracking-[-0.04em] text-foreground">Must-have vs nice-to-have</h2>
          <div className="mt-6 grid gap-6 md:grid-cols-2">
            <div>
              <p className="text-sm font-semibold text-foreground">Must-have requirements</p>
              <div className="mt-3">
                <BulletList
                  items={analysis.mustHaveRequirements}
                  emptyMessage="No must-have requirements were identified."
                />
              </div>
            </div>
            <div>
              <p className="text-sm font-semibold text-foreground">Nice-to-have requirements</p>
              <div className="mt-3">
                <BulletList
                  items={analysis.niceToHaveRequirements}
                  emptyMessage="No nice-to-have requirements were identified."
                />
              </div>
            </div>
          </div>
        </div>

        <div className="paper-panel p-6">
          <p className="font-mono text-[0.68rem] uppercase tracking-[0.3em] text-muted-foreground">CV Guidance</p>
          <h2 className="mt-3 font-display text-3xl tracking-[-0.04em] text-foreground">What to emphasize before generating</h2>
          <div className="mt-6">
            <p className="text-sm font-semibold text-foreground">CV focus points</p>
            <div className="mt-3">
              <BulletList
                items={analysis.cvFocusPoints}
                emptyMessage="No CV emphasis suggestions were returned yet."
              />
            </div>
          </div>
          <div className="mt-6">
            <p className="text-sm font-semibold text-foreground">Candidate risks</p>
            <div className="mt-3">
              <BulletList
                items={analysis.candidateRisks}
                emptyMessage="No major risks were highlighted for this opportunity."
                tone="warning"
              />
            </div>
          </div>
        </div>
      </div>

    </div>
  );
}
