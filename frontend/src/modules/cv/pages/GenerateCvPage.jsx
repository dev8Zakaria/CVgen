import { ArrowLeft, ArrowRight, Bot, CheckCircle2, LoaderCircle, Sparkles } from "lucide-react";
import { useEffect, useMemo, useState } from "react";
import { Link, useLocation, useNavigate } from "react-router-dom";

import { Button } from "@/components/ui/button";
import { EmptyState, SectionHeading, SkeletonBlock, StatusPill, StepIndicator } from "@/shared/components/app-ui";
import { ROUTES, getCvPreviewRoute } from "@/shared/constants/routes";
import { usePrototypeApp } from "@/shared/providers/PrototypeAppProvider";
import { useToast } from "@/shared/providers/ToastProvider";

const steps = ["Select profile data", "Select job offer", "AI generation"];
const messages = ["Analyzing job context...", "Optimizing CV structure...", "Refining role-specific narrative..."];

export function GenerateCvPage() {
  const navigate = useNavigate();
  const location = useLocation();
  const toast = useToast();
  const { hydrated, profile, jobOffers, generateCv } = usePrototypeApp();
  const availableOffers = useMemo(() => jobOffers.filter((offer) => offer.analysis), [jobOffers]);
  const initialOfferId = location.state?.offerId || availableOffers[0]?.id || null;
  const [currentStep, setCurrentStep] = useState(0);
  const [selectedOfferId, setSelectedOfferId] = useState(initialOfferId);
  const [progress, setProgress] = useState(12);
  const [messageIndex, setMessageIndex] = useState(0);
  const [generationStarted, setGenerationStarted] = useState(false);

  useEffect(() => {
    if (!selectedOfferId && availableOffers[0]?.id) {
      setSelectedOfferId(availableOffers[0].id);
    }
  }, [availableOffers, selectedOfferId]);

  useEffect(() => {
    if (currentStep !== 2 || !generationStarted) {
      return;
    }

    const progressInterval = window.setInterval(() => {
      setProgress((value) => Math.min(96, value + 11));
    }, 650);
    const messageInterval = window.setInterval(() => {
      setMessageIndex((value) => Math.min(messages.length - 1, value + 1));
    }, 900);
    const finishTimer = window.setTimeout(async () => {
      try {
        const generated = await generateCv(selectedOfferId);
        if (generated) {
          toast.success("CV generated", "Your tailored CV is now available in the preview workspace.");
          navigate(getCvPreviewRoute(generated.id));
        }
      } catch {
        toast.error("CV generation failed", "The backend could not generate a CV for the selected offer.");
        setCurrentStep(1);
      } finally {
        setGenerationStarted(false);
      }
    }, 3200);

    return () => {
      window.clearInterval(progressInterval);
      window.clearInterval(messageInterval);
      window.clearTimeout(finishTimer);
    };
  }, [currentStep, generateCv, generationStarted, navigate, selectedOfferId, toast]);

  if (!hydrated) {
    return <SkeletonBlock className="h-80" />;
  }

  if (availableOffers.length === 0) {
    return (
      <EmptyState
        title="No analyzed job offers available"
        description="You need at least one analyzed opportunity before the CV generation flow can begin."
        action={
          <Button asChild>
            <Link to={ROUTES.addOpportunity}>Add and analyze a job offer</Link>
          </Button>
        }
      />
    );
  }

  return (
    <div className="space-y-6">
      <SectionHeading
        eyebrow="Generate CV"
        title="Move from master profile to tailored application in three deliberate steps."
        description="This flow is designed as a guided sequence: confirm the profile data, choose a target role, then let AI build the first tailored draft."
      />

      <StepIndicator steps={steps} currentStep={currentStep} />

      {currentStep === 0 ? (
        <div className="paper-panel p-6">
          <div className="flex items-center gap-3">
            <CheckCircle2 className="h-5 w-5 text-primary" />
            <div>
              <h2 className="font-display text-3xl tracking-[-0.04em] text-foreground">Step 1: Select profile data</h2>
              <p className="mt-2 text-sm leading-7 text-muted-foreground">The generator will use your current profile details as the foundation.</p>
            </div>
          </div>
          <div className="mt-6 grid gap-4 md:grid-cols-2">
            <div className="rounded-[24px] border border-border bg-white/60 p-5 dark:bg-white/[0.03]">
              <p className="font-semibold text-foreground">{profile.fullName}</p>
              <p className="mt-1 text-sm text-muted-foreground">{profile.professionalTitle}</p>
              <p className="mt-4 text-sm leading-7 text-muted-foreground">{profile.summary}</p>
            </div>
            <div className="rounded-[24px] border border-border bg-white/60 p-5 dark:bg-white/[0.03]">
              <p className="font-mono text-[0.68rem] uppercase tracking-[0.3em] text-muted-foreground">Included Sections</p>
              <div className="mt-4 flex flex-wrap gap-2">
                <StatusPill tone="primary">{profile.experience.length} experiences</StatusPill>
                <StatusPill tone="primary">{profile.education.length} education entries</StatusPill>
                <StatusPill tone="primary">{profile.projects.length} projects</StatusPill>
                <StatusPill tone="primary">{profile.skills.length} skills</StatusPill>
              </div>
            </div>
          </div>
          <div className="mt-6 flex justify-between">
            <Button asChild variant="outline">
              <Link to={ROUTES.profile}>Edit profile first</Link>
            </Button>
            <Button type="button" onClick={() => setCurrentStep(1)}>
              Next step
              <ArrowRight className="ml-2 h-4 w-4" />
            </Button>
          </div>
        </div>
      ) : null}

      {currentStep === 1 ? (
        <div className="paper-panel p-6">
          <div className="flex items-center gap-3">
            <Bot className="h-5 w-5 text-primary" />
            <div>
              <h2 className="font-display text-3xl tracking-[-0.04em] text-foreground">Step 2: Select job offer</h2>
              <p className="mt-2 text-sm leading-7 text-muted-foreground">Choose the analyzed job offer that should guide the CV content.</p>
            </div>
          </div>
          <div className="mt-6 grid gap-4">
            {availableOffers.map((offer) => (
              <button
                key={offer.id}
                type="button"
                onClick={() => setSelectedOfferId(offer.id)}
                className={`rounded-[24px] border p-5 text-left transition ${
                  selectedOfferId === offer.id
                    ? "border-primary/30 bg-primary/10"
                    : "border-border bg-white/60 hover:border-primary/20 dark:bg-white/[0.03]"
                }`}
              >
                <div className="flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
                  <div>
                    <p className="font-semibold text-foreground">{offer.jobTitle}</p>
                    <p className="mt-1 text-sm text-muted-foreground">{offer.companyName} · {offer.location}</p>
                  </div>
                  <StatusPill tone="success">{offer.analysis.matchScore}% match</StatusPill>
                </div>
              </button>
            ))}
          </div>
          <div className="mt-6 flex justify-between">
            <Button type="button" variant="outline" onClick={() => setCurrentStep(0)}>
              <ArrowLeft className="mr-2 h-4 w-4" />
              Back
            </Button>
            <Button
              type="button"
              onClick={() => {
                setProgress(12);
                setMessageIndex(0);
                setGenerationStarted(true);
                setCurrentStep(2);
              }}
            >
              Start AI generation
              <Sparkles className="ml-2 h-4 w-4" />
            </Button>
          </div>
        </div>
      ) : null}

      {currentStep === 2 ? (
        <div className="paper-panel p-8">
          <div className="mx-auto max-w-3xl text-center">
            <div className="mx-auto flex h-16 w-16 animate-float items-center justify-center rounded-full bg-primary/10 text-primary">
              <LoaderCircle className="h-7 w-7 animate-spin" />
            </div>
            <h2 className="mt-6 font-display text-4xl tracking-[-0.05em] text-foreground">AI is shaping your next CV draft</h2>
            <p className="mt-3 text-sm leading-7 text-muted-foreground">{messages[messageIndex]}</p>

            <div className="mt-8 h-3 overflow-hidden rounded-full bg-muted">
              <div className="h-full rounded-full bg-primary transition-all duration-500" style={{ width: `${progress}%` }} />
            </div>
            <p className="mt-3 font-mono text-xs uppercase tracking-[0.28em] text-muted-foreground">{progress}% complete</p>
          </div>
        </div>
      ) : null}
    </div>
  );
}
