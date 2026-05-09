import { useState } from "react";
import { useNavigate } from "react-router-dom";

import { Button } from "@/components/ui/button";
import { SectionHeading } from "@/shared/components/app-ui";
import { getJobAnalysisRoute } from "@/shared/constants/routes";
import { usePrototypeApp } from "@/shared/providers/PrototypeAppProvider";
import { useToast } from "@/shared/providers/ToastProvider";

export function AddJobOfferPage() {
  const navigate = useNavigate();
  const toast = useToast();
  const { createAnalyzedOffer } = usePrototypeApp();
  const [submitting, setSubmitting] = useState(false);
  const [form, setForm] = useState({
    jobTitle: "",
    companyName: "",
    location: "",
    description: "",
  });

  const handleSubmit = async (event) => {
    event.preventDefault();

    try {
      setSubmitting(true);
      const id = await createAnalyzedOffer(form);
      toast.success("Offer analyzed", "The job description was added and processed into a ready-to-use analysis.");
      navigate(getJobAnalysisRoute(id));
    } catch {
      toast.error("Offer creation failed", "We could not create and analyze this job offer from the backend.");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="space-y-6">
      <SectionHeading
        eyebrow="Add Job Offer"
        title="Paste a role and let the prototype transform it into CV-ready intelligence."
        description="This form captures the raw opportunity text. The next screen turns it into skills, keywords, responsibilities, and a matched-versus-missing view."
      />

      <form onSubmit={handleSubmit} className="paper-panel grid gap-5 p-6">
        <div className="grid gap-5 md:grid-cols-2">
          <label className="space-y-2">
            <span className="text-sm font-semibold text-foreground">Job title</span>
            <input className="field" value={form.jobTitle} onChange={(event) => setForm((current) => ({ ...current, jobTitle: event.target.value }))} required />
          </label>
          <label className="space-y-2">
            <span className="text-sm font-semibold text-foreground">Company name</span>
            <input className="field" value={form.companyName} onChange={(event) => setForm((current) => ({ ...current, companyName: event.target.value }))} required />
          </label>
          <label className="space-y-2 md:col-span-2">
            <span className="text-sm font-semibold text-foreground">Location</span>
            <input className="field" value={form.location} onChange={(event) => setForm((current) => ({ ...current, location: event.target.value }))} required />
          </label>
          <label className="space-y-2 md:col-span-2">
            <span className="text-sm font-semibold text-foreground">Job description</span>
            <textarea
              className="field min-h-[240px]"
              value={form.description}
              onChange={(event) => setForm((current) => ({ ...current, description: event.target.value }))}
              required
            />
          </label>
        </div>
        <div className="flex justify-end">
          <Button type="submit" disabled={submitting}>
            {submitting ? "Analyzing..." : "Analyze Offer"}
          </Button>
        </div>
      </form>
    </div>
  );
}
