import { Download, Save, WandSparkles } from "lucide-react";
import { useMemo, useState } from "react";
import { useParams } from "react-router-dom";

import { Button } from "@/components/ui/button";
import { EmptyState, SectionHeading, StatusPill } from "@/shared/components/app-ui";
import { usePrototypeApp } from "@/shared/providers/PrototypeAppProvider";
import { useToast } from "@/shared/providers/ToastProvider";

const templates = ["Atelier Ivory", "Monograph Slate"];

export function CvPreviewPage() {
  const { cvId } = useParams();
  const { cvs, updateCv } = usePrototypeApp();
  const toast = useToast();
  const cv = useMemo(() => cvs.find((entry) => entry.id === cvId), [cvs, cvId]);
  const [draft, setDraft] = useState(cv);

  if (!cv) {
    return <EmptyState title="CV not found" description="The selected CV preview could not be loaded." />;
  }

  const activeDraft = draft ?? cv;

  const downloadPreview = () => {
    const popup = window.open("", "_blank");
    if (!popup) {
      toast.error("Download blocked", "Please allow popups to use the print-to-PDF preview flow.");
      return;
    }

    popup.document.write(`
      <html>
        <head><title>${activeDraft.jobTitle}</title></head>
        <body style="font-family: Georgia, serif; padding: 48px; line-height: 1.6; max-width: 820px; margin: 0 auto;">
          <h1>${activeDraft.content.header.name}</h1>
          <p>${activeDraft.content.header.title}</p>
          <p>${activeDraft.content.header.email} · ${activeDraft.content.header.phone} · ${activeDraft.content.header.address}</p>
          <h2>Summary</h2>
          <p>${activeDraft.content.summary}</p>
        </body>
      </html>
    `);
    popup.document.close();
    popup.focus();
    popup.print();
  };

  const saveCv = () => {
    updateCv(activeDraft.id, activeDraft);
    toast.success("CV saved", "Your preview edits are now stored in the prototype.");
  };

  return (
    <div className="space-y-6">
      <SectionHeading
        eyebrow="CV Preview"
        title={`${activeDraft.jobTitle} · ${activeDraft.companyName}`}
        description="Edit the content inline, switch templates, then print to PDF or save the latest version."
        action={
          <>
            <Button type="button" variant="outline" onClick={downloadPreview}>
              <Download className="mr-2 h-4 w-4" />
              Download PDF
            </Button>
            <Button type="button" onClick={saveCv}>
              <Save className="mr-2 h-4 w-4" />
              Save CV
            </Button>
          </>
        }
      />

      <div className="grid gap-5 xl:grid-cols-[1.1fr_0.9fr]">
        <div className={`paper-panel p-8 ${activeDraft.template === "Monograph Slate" ? "bg-foreground text-background dark:border-white/0" : ""}`}>
          <div className="border-b border-border/70 pb-6">
            <h2 className="font-display text-5xl tracking-[-0.06em]">{activeDraft.content.header.name}</h2>
            <p className="mt-2 text-lg">{activeDraft.content.header.title}</p>
            <p className="mt-3 text-sm opacity-80">
              {activeDraft.content.header.email} · {activeDraft.content.header.phone} · {activeDraft.content.header.address}
            </p>
          </div>

          <div className="mt-8 space-y-8">
            <section>
              <p className="font-mono text-[0.68rem] uppercase tracking-[0.3em] opacity-60">Professional Summary</p>
              <p className="mt-3 text-sm leading-7">{activeDraft.content.summary}</p>
            </section>

            <section>
              <p className="font-mono text-[0.68rem] uppercase tracking-[0.3em] opacity-60">Experience</p>
              <div className="mt-4 space-y-5">
                {activeDraft.content.experience.map((item) => (
                  <div key={item.id}>
                    <p className="font-semibold">{item.role} · {item.company}</p>
                    <p className="text-sm opacity-70">{item.period} · {item.location}</p>
                    <ul className="mt-3 space-y-2 text-sm leading-7">
                      {item.bullets.map((bullet) => (
                        <li key={bullet}>• {bullet}</li>
                      ))}
                    </ul>
                  </div>
                ))}
              </div>
            </section>

            <section>
              <p className="font-mono text-[0.68rem] uppercase tracking-[0.3em] opacity-60">Skills</p>
              <div className="mt-3 flex flex-wrap gap-2">
                {activeDraft.content.skills.map((skill) => (
                  <StatusPill key={skill} tone={activeDraft.template === "Monograph Slate" ? "accent" : "primary"}>
                    {skill}
                  </StatusPill>
                ))}
              </div>
            </section>
          </div>
        </div>

        <div className="space-y-5">
          <div className="paper-panel p-6">
            <div className="flex items-center gap-3">
              <WandSparkles className="h-5 w-5 text-primary" />
              <div>
                <h2 className="font-display text-3xl tracking-[-0.04em] text-foreground">Edit Content</h2>
                <p className="mt-2 text-sm leading-7 text-muted-foreground">Use this side panel to refine the preview without leaving the document view.</p>
              </div>
            </div>

            <div className="mt-5 grid gap-4">
              <label className="space-y-2">
                <span className="text-sm font-semibold text-foreground">Professional title</span>
                <input
                  className="field"
                  value={activeDraft.content.header.title}
                  onChange={(event) =>
                    setDraft((current) => ({
                      ...(current ?? cv),
                      content: {
                        ...(current ?? cv).content,
                        header: { ...(current ?? cv).content.header, title: event.target.value },
                      },
                    }))
                  }
                />
              </label>
              <label className="space-y-2">
                <span className="text-sm font-semibold text-foreground">Summary</span>
                <textarea
                  className="field min-h-[180px]"
                  value={activeDraft.content.summary}
                  onChange={(event) =>
                    setDraft((current) => ({
                      ...(current ?? cv),
                      content: {
                        ...(current ?? cv).content,
                        summary: event.target.value,
                      },
                    }))
                  }
                />
              </label>
            </div>
          </div>

          <div className="paper-panel p-6">
            <p className="font-mono text-[0.68rem] uppercase tracking-[0.3em] text-muted-foreground">Template Switcher</p>
            <div className="mt-4 grid gap-3">
              {templates.map((template) => (
                <button
                  key={template}
                  type="button"
                  onClick={() => setDraft((current) => ({ ...(current ?? cv), template }))}
                  className={`rounded-[24px] border p-4 text-left transition ${
                    activeDraft.template === template
                      ? "border-primary/30 bg-primary/10"
                      : "border-border bg-white/60 dark:bg-white/[0.03]"
                  }`}
                >
                  <p className="font-semibold text-foreground">{template}</p>
                  <p className="mt-1 text-sm text-muted-foreground">
                    {template === "Atelier Ivory" ? "Warm editorial layout with soft paper tones." : "Sharper contrast with a darker monograph feel."}
                  </p>
                </button>
              ))}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
