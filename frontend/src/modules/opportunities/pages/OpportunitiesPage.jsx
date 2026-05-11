import { useState } from "react";
import { ArrowRight, Trash2 } from "lucide-react";
import { Link, useNavigate } from "react-router-dom";

import { Button } from "@/components/ui/button";
import { ConfirmDialog, EmptyState, SectionHeading, SkeletonBlock, StatusPill } from "@/shared/components/app-ui";
import { ROUTES, getJobAnalysisRoute } from "@/shared/constants/routes";
import { usePrototypeApp } from "@/shared/providers/PrototypeAppProvider";
import { useToast } from "@/shared/providers/ToastProvider";

function formatDate(date) {
  return new Intl.DateTimeFormat("en", { dateStyle: "medium" }).format(new Date(date));
}

function getStatusTone(status) {
  const normalizedStatus = String(status || "").toLowerCase();

  if (normalizedStatus === "analyzed") {
    return "success";
  }

  if (normalizedStatus === "failed") {
    return "danger";
  }

  return "warning";
}

export function OpportunitiesPage() {
  const navigate = useNavigate();
  const { hydrated, jobOffers, deleteJobOffer, analyzeJobOffer } = usePrototypeApp();
  const toast = useToast();
  const [pendingDelete, setPendingDelete] = useState(null);
  const [deletingId, setDeletingId] = useState(null);
  const [analyzingId, setAnalyzingId] = useState(null);

  if (!hydrated) {
    return (
      <div className="grid gap-5">
        <SkeletonBlock className="h-32" />
        <SkeletonBlock className="h-28" />
        <SkeletonBlock className="h-28" />
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <SectionHeading
        eyebrow="Job Offers"
        title="Track opportunities, analyze them, and route the best ones into CV generation."
        description="These entries now come from your real backend job-offer data. Analysis results are also loaded from the backend for the authenticated user."
        action={
          <Button asChild>
            <Link to={ROUTES.addOpportunity}>Add Job Offer</Link>
          </Button>
        }
      />

      {jobOffers.length === 0 ? (
        <EmptyState
          title="No job offers yet"
          description="Start by adding a job description. The app will analyze it, compare it with your profile, and prepare it for CV generation."
          action={
            <Button asChild>
              <Link to={ROUTES.addOpportunity}>Add your first offer</Link>
            </Button>
          }
        />
      ) : (
        <div className="grid gap-6">
          {jobOffers.map((offer) => {
            const normalizedStatus = String(offer.status || "").toLowerCase();
            const isAnalyzed = Boolean(offer.analysis) && normalizedStatus === "analyzed";
            const isAnalyzing = analyzingId === offer.id || normalizedStatus === "processing";
            const detailParts = [offer.companyName, offer.location, `Added ${formatDate(offer.createdAt)}`].filter(Boolean);
            const actionLabel = normalizedStatus === "failed" ? "Retry Analysis" : "Analyze Offer";

            return (
              <div key={offer.id} className="paper-panel p-6">
                <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
                  <div>
                    <div className="flex flex-wrap items-center gap-3">
                      <h2 className="font-display text-2xl font-bold text-foreground">{offer.jobTitle}</h2>
                      <StatusPill tone={getStatusTone(offer.status)}>{offer.status}</StatusPill>
                    </div>
                    <p className="mt-1 text-sm font-medium text-muted-foreground">{detailParts.join(" - ")}</p>
                    <p className="mt-3 max-w-4xl text-sm leading-relaxed text-foreground">{offer.description}</p>
                    <div className="mt-4 flex flex-wrap gap-2">
                      {offer.analysis?.extractedSkills.slice(0, 4).map((skill) => (
                        <StatusPill key={skill} tone="secondary">
                          {skill}
                        </StatusPill>
                      ))}
                    </div>
                  </div>
                  <div className="mt-4 flex shrink-0 flex-wrap gap-3 lg:mt-0">
                    {isAnalyzed ? (
                      <Button asChild variant="outline">
                        <Link to={getJobAnalysisRoute(offer.id)}>
                          View Analysis
                          <ArrowRight className="ml-2 h-4 w-4" />
                        </Link>
                      </Button>
                    ) : (
                      <Button
                        type="button"
                        variant="outline"
                        disabled={isAnalyzing}
                        onClick={async () => {
                          try {
                            setAnalyzingId(offer.id);
                            const analyzedOffer = await analyzeJobOffer(offer.id);
                            toast.success("Offer analyzed", "The job offer is now ready for CV generation.");
                            navigate(getJobAnalysisRoute(analyzedOffer.id));
                          } catch {
                            toast.error("Analysis failed", "We could not analyze this job offer from the backend.");
                          } finally {
                            setAnalyzingId(null);
                          }
                        }}
                      >
                        {isAnalyzing ? "Analyzing..." : actionLabel}
                        <ArrowRight className="ml-2 h-4 w-4" />
                      </Button>
                    )}
                    <Button type="button" variant="destructive" onClick={() => setPendingDelete(offer)}>
                      <Trash2 className="mr-2 h-4 w-4" />
                      Delete
                    </Button>
                  </div>
                </div>
              </div>
            );
          })}
        </div>
      )}

      <ConfirmDialog
        open={Boolean(pendingDelete)}
        title="Delete job offer?"
        description="This will remove the analyzed role and any CVs generated specifically from it."
        tone="danger"
        onCancel={() => setPendingDelete(null)}
        onConfirm={async () => {
          try {
            setDeletingId(pendingDelete.id);
            await deleteJobOffer(pendingDelete.id);
            toast.success("Job offer deleted", "The role and its related generated CVs were removed.");
            setPendingDelete(null);
          } catch {
            toast.error("Delete failed", "We could not delete this job offer from the backend.");
          } finally {
            setDeletingId(null);
          }
        }}
        confirmLabel={deletingId ? "Deleting..." : "Delete offer"}
      />
    </div>
  );
}
