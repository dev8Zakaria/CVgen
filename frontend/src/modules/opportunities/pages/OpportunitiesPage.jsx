import { useEffect, useMemo, useState } from "react";

import {
  AlertCircle,
  BriefcaseBusiness,
  Building2,
  CalendarClock,
  Clock3,
  FileText,
  LoaderCircle,
  MapPin,
  PencilLine,
  Plus,
  RefreshCw,
  SearchCode,
  Sparkles,
  Trash2,
} from "lucide-react";

import { Button } from "@/components/ui/button";
import { useOpportunities } from "@/modules/opportunities/hooks/useOpportunities";

function formatDate(value) {
  if (!value) {
    return "Unknown date";
  }

  return new Intl.DateTimeFormat("en", {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(new Date(value));
}

function StatusBadge({ status }) {
  const normalized = (status || "pending").toLowerCase();
  const tone =
    normalized === "completed"
      ? "bg-emerald-100 text-emerald-700"
      : normalized === "failed"
        ? "bg-rose-100 text-rose-700"
        : "bg-amber-100 text-amber-700";

  return <span className={`rounded-full px-3 py-1 text-xs font-semibold uppercase tracking-[0.18em] ${tone}`}>{normalized}</span>;
}

function TagList({ items, emptyLabel }) {
  if (!items?.length) {
    return <p className="text-sm text-muted-foreground">{emptyLabel}</p>;
  }

  return (
    <div className="flex flex-wrap gap-2">
      {items.map((item) => (
        <span key={item} className="rounded-full bg-secondary px-3 py-1 text-xs font-medium text-secondary-foreground">
          {item}
        </span>
      ))}
    </div>
  );
}

function AnalysisCard({ title, icon: Icon, children }) {
  return (
    <div className="rounded-3xl border bg-background p-4">
      <div className="flex items-center gap-2 text-sm font-semibold uppercase tracking-[0.18em] text-muted-foreground">
        <Icon className="h-4 w-4" />
        <span>{title}</span>
      </div>
      <div className="mt-3">{children}</div>
    </div>
  );
}

export function OpportunitiesPage() {
  const {
    opportunities,
    loading,
    creating,
    analyzing,
    updating,
    deleting,
    detailLoading,
    selectedOpportunity,
    error,
    reload,
    loadOpportunityDetail,
    createOpportunity,
    analyzeOpportunity,
    updateOpportunity,
    deleteOpportunity,
  } = useOpportunities();
  const [form, setForm] = useState({
    title: "",
    companyName: "",
    description: "",
  });
  const [editForm, setEditForm] = useState({
    title: "",
    companyName: "",
    description: "",
  });
  const [feedback, setFeedback] = useState({ tone: null, message: "" });

  const selectedOpportunityId = selectedOpportunity?.id ?? null;
  const selectedAnalysis = selectedOpportunity?.analysis ?? null;

  useEffect(() => {
    if (!selectedOpportunity) {
      setEditForm({
        title: "",
        companyName: "",
        description: "",
      });
      return;
    }

    setEditForm({
      title: selectedOpportunity.title ?? "",
      companyName: selectedOpportunity.companyName ?? "",
      description: selectedOpportunity.description ?? "",
    });
  }, [selectedOpportunity]);

  const listStateMessage = useMemo(() => {
    if (loading) {
      return "Loading your job offers...";
    }

    if (opportunities.length === 0) {
      return "No job offers saved yet. Create your first one from the form.";
    }

    return null;
  }, [loading, opportunities.length]);

  const handleFieldChange = (event) => {
    const { name, value } = event.target;
    setForm((current) => ({ ...current, [name]: value }));
  };

  const handleEditFieldChange = (event) => {
    const { name, value } = event.target;
    setEditForm((current) => ({ ...current, [name]: value }));
  };

  const handleCreate = async (event) => {
    event.preventDefault();
    setFeedback({ tone: null, message: "" });

    const payload = {
      title: form.title.trim(),
      companyName: form.companyName.trim(),
      description: form.description.trim(),
    };

    try {
      const created = await createOpportunity(payload);
      setForm({
        title: "",
        companyName: "",
        description: "",
      });
      setFeedback({
        tone: "success",
        message: `Job offer "${created.title}" was created. AI analysis is now pending.`,
      });
    } catch {
      setFeedback({
        tone: "error",
        message: "We could not create the job offer. Make sure your profile exists and retry.",
      });
    }
  };

  const handleSelect = async (id) => {
    setFeedback({ tone: null, message: "" });

    try {
      await loadOpportunityDetail(id);
    } catch {
      setFeedback({
        tone: "error",
        message: "We could not load this job offer detail. Please retry.",
      });
    }
  };

  const handleUpdate = async (event) => {
    event.preventDefault();

    if (!selectedOpportunity) {
      return;
    }

    setFeedback({ tone: null, message: "" });

    const payload = {
      title: editForm.title.trim(),
      companyName: editForm.companyName.trim(),
      description: editForm.description.trim(),
    };

    try {
      const updated = await updateOpportunity(selectedOpportunity.id, payload);
      setFeedback({
        tone: "success",
        message: `Job offer "${updated.title}" was updated. Any previous AI enrichment is now marked pending until analysis runs again.`,
      });
    } catch {
      setFeedback({
        tone: "error",
        message: "We could not save the job offer changes. Please retry.",
      });
    }
  };

  const handleAnalyze = async () => {
    if (!selectedOpportunity) {
      return;
    }

    setFeedback({ tone: null, message: "" });

    try {
      const analyzed = await analyzeOpportunity(selectedOpportunity.id);
      setFeedback({
        tone: "success",
        message: `AI analysis completed for "${analyzed.title}". The new enrichment is now available below.`,
      });
    } catch {
      await loadOpportunityDetail(selectedOpportunity.id).catch(() => {});
      setFeedback({
        tone: "error",
        message: `We could not complete AI analysis for "${selectedOpportunity.title}". Check the AI service and retry.`,
      });
    }
  };

  const handleResetSelected = () => {
    if (!selectedOpportunity) {
      return;
    }

    setEditForm({
      title: selectedOpportunity.title ?? "",
      companyName: selectedOpportunity.companyName ?? "",
      description: selectedOpportunity.description ?? "",
    });
    setFeedback({ tone: null, message: "" });
  };

  const handleDelete = async () => {
    if (!selectedOpportunity) {
      return;
    }

    const confirmed = window.confirm(
      `Delete the job offer "${selectedOpportunity.title}"? This will also remove its stored AI analysis result.`,
    );

    if (!confirmed) {
      return;
    }

    setFeedback({ tone: null, message: "" });

    try {
      const deletedTitle = selectedOpportunity.title;
      await deleteOpportunity(selectedOpportunity.id);
      setFeedback({
        tone: "success",
        message: `Job offer "${deletedTitle}" was deleted.`,
      });
    } catch {
      setFeedback({
        tone: "error",
        message: "We could not delete this job offer. Please retry.",
      });
    }
  };

  return (
    <section className="space-y-8">
      <div className="rounded-[2rem] border bg-[radial-gradient(circle_at_top_left,_rgba(196,181,253,0.28),_transparent_32%),linear-gradient(135deg,_rgba(255,255,255,0.96),_rgba(248,250,252,0.92))] p-8 shadow-sm shadow-slate-900/5">
        <div className="flex flex-col gap-5 lg:flex-row lg:items-end lg:justify-between">
          <div className="space-y-4">
            <p className="text-sm font-semibold uppercase tracking-[0.25em] text-primary">Job opportunities</p>
            <h1 className="font-display text-4xl font-bold tracking-tight">Capture the raw offer once, let AI enrich it later</h1>
            <p className="max-w-2xl text-muted-foreground">
              The create form now only stores the original job title, company name, and description. Any extracted skills, responsibilities, or metadata are reserved for the future AI analysis result and shown here as read-only output.
            </p>
          </div>
          <div className="rounded-3xl border bg-white/80 px-5 py-4 shadow-sm backdrop-blur">
            <p className="text-xs font-semibold uppercase tracking-[0.2em] text-muted-foreground">Current workflow</p>
            <p className="mt-2 text-lg font-semibold text-foreground">Raw input first, analysis later</p>
          </div>
        </div>
      </div>

      {feedback.message ? (
        <div
          className={`rounded-3xl border px-5 py-4 text-sm shadow-sm shadow-slate-900/5 ${
            feedback.tone === "error"
              ? "border-destructive/20 bg-destructive/5 text-destructive"
              : "border-emerald-200 bg-emerald-50 text-emerald-700"
          }`}
        >
          {feedback.message}
        </div>
      ) : null}

      {error && !feedback.message ? (
        <div className="rounded-3xl border border-destructive/20 bg-destructive/5 p-5 text-sm text-destructive shadow-sm shadow-slate-900/5">
          <div className="flex items-start gap-3">
            <AlertCircle className="mt-0.5 h-5 w-5" />
            <div>
              <p className="font-semibold">The job opportunity request failed.</p>
              <p className="mt-1 text-muted-foreground">
                If this is your first authenticated session, load the profile page once so the backend can create your local user record.
              </p>
            </div>
          </div>
        </div>
      ) : null}

      <div className="grid gap-5 xl:grid-cols-[0.95fr_1.05fr]">
        <form onSubmit={handleCreate} className="rounded-[2rem] border bg-card p-6 shadow-sm shadow-slate-900/5">
          <div className="flex items-center gap-3">
            <Plus className="h-5 w-5 text-primary" />
            <h2 className="font-display text-2xl font-bold">Create a job offer</h2>
          </div>
          <p className="mt-3 text-sm text-muted-foreground">
            Only enter the original offer details here. The structured analysis fields will be generated automatically by AI later and shown as read-only.
          </p>

          <div className="mt-6 grid gap-5">
            <label className="space-y-2">
              <span className="text-sm font-semibold text-foreground">Job title</span>
              <input
                name="title"
                value={form.title}
                onChange={handleFieldChange}
                placeholder="Frontend Engineer"
                required
                className="w-full rounded-2xl border bg-background px-4 py-3 text-sm outline-none transition focus:border-primary focus:ring-2 focus:ring-primary/15"
              />
            </label>

            <label className="space-y-2">
              <span className="text-sm font-semibold text-foreground">Company name</span>
              <input
                name="companyName"
                value={form.companyName}
                onChange={handleFieldChange}
                placeholder="OpenAI"
                required
                className="w-full rounded-2xl border bg-background px-4 py-3 text-sm outline-none transition focus:border-primary focus:ring-2 focus:ring-primary/15"
              />
            </label>

            <label className="space-y-2">
              <span className="text-sm font-semibold text-foreground">Job description</span>
              <textarea
                name="description"
                value={form.description}
                onChange={handleFieldChange}
                rows={8}
                placeholder="Paste the original job offer description or your condensed notes."
                className="w-full rounded-2xl border bg-background px-4 py-3 text-sm outline-none transition focus:border-primary focus:ring-2 focus:ring-primary/15"
              />
            </label>
          </div>

          <div className="mt-6 flex flex-wrap gap-3">
            <Button type="submit" className="rounded-full" disabled={creating || loading || detailLoading}>
              {creating ? (
                <>
                  <LoaderCircle className="mr-2 h-4 w-4 animate-spin" />
                  Creating...
                </>
              ) : (
                "Create job offer"
              )}
            </Button>
            <Button type="button" variant="outline" className="rounded-full" onClick={reload} disabled={creating || loading || detailLoading}>
              <RefreshCw className="mr-2 h-4 w-4" />
              Refresh list
            </Button>
          </div>
        </form>

        <div className="space-y-5">
          <div className="rounded-[2rem] border bg-card p-6 shadow-sm shadow-slate-900/5">
            <div className="flex items-center gap-3">
              <BriefcaseBusiness className="h-5 w-5 text-primary" />
              <h2 className="font-display text-2xl font-bold">Saved job offers</h2>
            </div>

            {listStateMessage ? (
              <p className="mt-4 text-sm text-muted-foreground">{listStateMessage}</p>
            ) : (
              <div className="mt-5 space-y-3">
                {opportunities.map((opportunity) => {
                  const isSelected = selectedOpportunityId === opportunity.id;

                  return (
                    <button
                      key={opportunity.id}
                      type="button"
                      onClick={() => handleSelect(opportunity.id)}
                      className={`w-full rounded-3xl border px-5 py-4 text-left transition ${
                        isSelected
                          ? "border-primary bg-primary/5 shadow-sm shadow-slate-900/5"
                          : "bg-background hover:border-primary/40 hover:bg-primary/5"
                      }`}
                    >
                      <div className="flex flex-col gap-3 md:flex-row md:items-start md:justify-between">
                        <div>
                          <p className="font-semibold text-foreground">{opportunity.title}</p>
                          <p className="mt-1 text-sm text-muted-foreground">{opportunity.companyName}</p>
                        </div>
                        <div className="flex flex-wrap items-center gap-2">
                          <StatusBadge status={opportunity.analysisStatus} />
                          <span className="rounded-full bg-secondary px-3 py-1 text-xs font-medium text-secondary-foreground">
                            {formatDate(opportunity.createdAt)}
                          </span>
                        </div>
                      </div>
                    </button>
                  );
                })}
              </div>
            )}
          </div>

          <div className="rounded-[2rem] border bg-card p-6 shadow-sm shadow-slate-900/5">
            <div className="flex items-center gap-3">
              <SearchCode className="h-5 w-5 text-primary" />
              <h2 className="font-display text-2xl font-bold">Job offer detail</h2>
            </div>

            {detailLoading ? (
              <div className="mt-5 flex items-center gap-3 text-sm text-muted-foreground">
                <LoaderCircle className="h-4 w-4 animate-spin" />
                <span>Loading full detail...</span>
              </div>
            ) : selectedOpportunity ? (
              <div className="mt-5 space-y-5">
                <div className="grid gap-4 md:grid-cols-2">
                  <AnalysisCard title="Title" icon={BriefcaseBusiness}>
                    <p className="font-medium text-foreground">{selectedOpportunity.title}</p>
                  </AnalysisCard>

                  <AnalysisCard title="Company" icon={Building2}>
                    <p className="font-medium text-foreground">{selectedOpportunity.companyName}</p>
                  </AnalysisCard>
                </div>

                <AnalysisCard title="Description" icon={FileText}>
                  <p className="whitespace-pre-wrap leading-7 text-foreground">
                    {selectedOpportunity.description || "No description was saved for this job offer."}
                  </p>
                </AnalysisCard>

                <div className="grid gap-4 md:grid-cols-3">
                  <AnalysisCard title="Analysis status" icon={Sparkles}>
                    <StatusBadge status={selectedOpportunity.analysisStatus} />
                  </AnalysisCard>

                  <AnalysisCard title="Created at" icon={CalendarClock}>
                    <p className="text-foreground">{formatDate(selectedOpportunity.createdAt)}</p>
                  </AnalysisCard>

                  <AnalysisCard title="Updated at" icon={Clock3}>
                    <p className="text-foreground">{formatDate(selectedOpportunity.updatedAt)}</p>
                  </AnalysisCard>
                </div>

                <div className="rounded-[2rem] border bg-background p-5">
                  <div className="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
                    <div>
                      <p className="text-sm font-semibold uppercase tracking-[0.22em] text-primary">AI action</p>
                      <h3 className="mt-2 font-display text-2xl font-bold">Run analysis on this raw job offer</h3>
                      <p className="mt-2 max-w-2xl text-sm text-muted-foreground">
                        Trigger the backend AI analyzer to extract keywords and skills from the saved description. Running analysis again refreshes the enrichment from the latest raw input.
                      </p>
                    </div>
                    <Button
                      type="button"
                      className="rounded-full"
                      onClick={handleAnalyze}
                      disabled={analyzing || updating || deleting || detailLoading}
                    >
                      {analyzing ? (
                        <>
                          <LoaderCircle className="mr-2 h-4 w-4 animate-spin" />
                          Analyzing...
                        </>
                      ) : selectedAnalysis ? (
                        <>
                          <Sparkles className="mr-2 h-4 w-4" />
                          Re-run AI analysis
                        </>
                      ) : (
                        <>
                          <Sparkles className="mr-2 h-4 w-4" />
                          Run AI analysis
                        </>
                      )}
                    </Button>
                  </div>
                </div>

                <form onSubmit={handleUpdate} className="rounded-[2rem] border bg-background p-5">
                  <div className="flex items-center gap-3">
                    <PencilLine className="h-5 w-5 text-primary" />
                    <h3 className="font-display text-2xl font-bold">Edit raw job offer</h3>
                  </div>
                  <p className="mt-3 text-sm text-muted-foreground">
                    This form updates only the original job title, company name, and description. Saving changes resets the analysis status to pending so the AI result can be regenerated from the fresh raw input later.
                  </p>

                  <div className="mt-6 grid gap-5">
                    <label className="space-y-2">
                      <span className="text-sm font-semibold text-foreground">Job title</span>
                      <input
                        name="title"
                        value={editForm.title}
                        onChange={handleEditFieldChange}
                        required
                        className="w-full rounded-2xl border bg-card px-4 py-3 text-sm outline-none transition focus:border-primary focus:ring-2 focus:ring-primary/15"
                      />
                    </label>

                    <label className="space-y-2">
                      <span className="text-sm font-semibold text-foreground">Company name</span>
                      <input
                        name="companyName"
                        value={editForm.companyName}
                        onChange={handleEditFieldChange}
                        required
                        className="w-full rounded-2xl border bg-card px-4 py-3 text-sm outline-none transition focus:border-primary focus:ring-2 focus:ring-primary/15"
                      />
                    </label>

                    <label className="space-y-2">
                      <span className="text-sm font-semibold text-foreground">Job description</span>
                      <textarea
                        name="description"
                        value={editForm.description}
                        onChange={handleEditFieldChange}
                        rows={8}
                        className="w-full rounded-2xl border bg-card px-4 py-3 text-sm outline-none transition focus:border-primary focus:ring-2 focus:ring-primary/15"
                      />
                    </label>
                  </div>

                  <div className="mt-6 flex flex-wrap gap-3">
                    <Button type="submit" className="rounded-full" disabled={updating || deleting || detailLoading}>
                      {updating ? (
                        <>
                          <LoaderCircle className="mr-2 h-4 w-4 animate-spin" />
                          Saving...
                        </>
                      ) : (
                        "Save changes"
                      )}
                    </Button>
                    <Button
                      type="button"
                      variant="outline"
                      className="rounded-full"
                      onClick={handleResetSelected}
                      disabled={updating || deleting || detailLoading}
                    >
                      <RefreshCw className="mr-2 h-4 w-4" />
                      Reset fields
                    </Button>
                  </div>
                </form>

                {selectedAnalysis ? (
                  <div className="space-y-4 rounded-[2rem] border border-primary/10 bg-primary/5 p-5">
                    <div>
                      <p className="text-sm font-semibold uppercase tracking-[0.22em] text-primary">AI analysis result</p>
                      <h3 className="mt-2 font-display text-2xl font-bold">Read-only enrichment</h3>
                    </div>

                    <div className="grid gap-4 md:grid-cols-2">
                      <AnalysisCard title="Extracted skills" icon={Sparkles}>
                        <TagList items={selectedAnalysis.extractedSkills} emptyLabel="No extracted skills yet." />
                      </AnalysisCard>

                      <AnalysisCard title="Extracted keywords" icon={Sparkles}>
                        <TagList items={selectedAnalysis.extractedKeywords} emptyLabel="No extracted keywords yet." />
                      </AnalysisCard>

                      <AnalysisCard title="Responsibilities" icon={FileText}>
                        <TagList items={selectedAnalysis.extractedResponsibilities} emptyLabel="No responsibilities extracted yet." />
                      </AnalysisCard>

                      <AnalysisCard title="Technologies" icon={Sparkles}>
                        <TagList items={selectedAnalysis.detectedTechnologies} emptyLabel="No technologies detected yet." />
                      </AnalysisCard>
                    </div>

                    <div className="grid gap-4 md:grid-cols-3">
                      <AnalysisCard title="Experience level" icon={BriefcaseBusiness}>
                        <p className="text-foreground">{selectedAnalysis.detectedExperienceLevel || "Not detected yet."}</p>
                      </AnalysisCard>

                      <AnalysisCard title="Location" icon={MapPin}>
                        <p className="text-foreground">{selectedAnalysis.detectedLocation || "Not detected yet."}</p>
                      </AnalysisCard>

                      <AnalysisCard title="Contract type" icon={BriefcaseBusiness}>
                        <p className="text-foreground">{selectedAnalysis.detectedContractType || "Not detected yet."}</p>
                      </AnalysisCard>
                    </div>

                    <AnalysisCard title="Analysis summary" icon={Sparkles}>
                      <p className="whitespace-pre-wrap leading-7 text-foreground">
                        {selectedAnalysis.analysisSummary || "No AI summary has been generated yet."}
                      </p>
                    </AnalysisCard>
                  </div>
                ) : (
                  <div className="rounded-[2rem] border border-dashed bg-background p-5">
                    <div className="flex items-start gap-3">
                      <Clock3 className="mt-0.5 h-5 w-5 text-amber-600" />
                      <div>
                        <p className="font-semibold text-foreground">AI analysis has not been completed yet.</p>
                        <p className="mt-1 text-sm text-muted-foreground">
                          The extracted skills, keywords, responsibilities, experience level, location, contract type, technologies, and summary will appear here automatically after the future AI analysis step finishes.
                        </p>
                      </div>
                    </div>
                  </div>
                )}

                <div className="rounded-[2rem] border border-destructive/20 bg-destructive/5 p-5">
                  <div className="flex items-center gap-3">
                    <Trash2 className="h-5 w-5 text-destructive" />
                    <h3 className="font-display text-2xl font-bold text-foreground">Danger zone</h3>
                  </div>
                  <p className="mt-3 text-sm text-muted-foreground">
                    Deleting the job offer also removes any attached AI analysis record from the backend.
                  </p>
                  <Button
                    type="button"
                    variant="outline"
                    className="mt-6 rounded-full border-destructive/30 text-destructive hover:bg-destructive/10 hover:text-destructive"
                    onClick={handleDelete}
                    disabled={updating || deleting || detailLoading}
                  >
                    {deleting ? (
                      <>
                        <LoaderCircle className="mr-2 h-4 w-4 animate-spin" />
                        Deleting...
                      </>
                    ) : (
                      "Delete job offer"
                    )}
                  </Button>
                </div>
              </div>
            ) : (
              <p className="mt-4 text-sm text-muted-foreground">
                Pick a saved job offer from the list to fetch the full detail and any completed AI analysis.
              </p>
            )}
          </div>
        </div>
      </div>
    </section>
  );
}
