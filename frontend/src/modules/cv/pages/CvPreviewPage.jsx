import { Download, Save, WandSparkles } from "lucide-react";
import { useEffect, useMemo, useState } from "react";
import { useParams } from "react-router-dom";

import { Button } from "@/components/ui/button";
import { EmptyState, SectionHeading, SkeletonBlock } from "@/shared/components/app-ui";
import { usePrototypeApp } from "@/shared/providers/PrototypeAppProvider";
import { useToast } from "@/shared/providers/ToastProvider";

const templates = ["Atelier Ivory", "Monograph Slate"];

function hasItems(items) {
  return Array.isArray(items) && items.length > 0;
}

function renderSimpleItems(items) {
  return items
    .filter((item) => item.name)
    .map((item) => (item.detail ? `${item.name} (${item.detail})` : item.name))
    .join(", ");
}

export function CvPreviewPage() {
  const { cvId } = useParams();
  const { hydrated, cvs, updateCv, loadCv, downloadCv } = usePrototypeApp();
  const toast = useToast();
  const cv = useMemo(() => cvs.find((entry) => entry.id === cvId), [cvs, cvId]);
  const [draft, setDraft] = useState(cv);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (!cvId || cv?.content) {
      return;
    }

    let cancelled = false;
    setLoading(true);

    loadCv(cvId)
      .then((loadedCv) => {
        if (!cancelled && loadedCv) {
          setDraft(loadedCv);
        }
      })
      .catch(() => {
        if (!cancelled) {
          toast.error("CV loading failed", "The backend could not load the selected CV.");
        }
      })
      .finally(() => {
        if (!cancelled) {
          setLoading(false);
        }
      });

    return () => {
      cancelled = true;
    };
  }, [cv?.content, cvId, loadCv, toast]);

  if (!hydrated) {
    return <SkeletonBlock className="h-96" />;
  }

  if (!cv) {
    return <EmptyState title="CV not found" description="The selected CV preview could not be loaded." />;
  }

  const activeDraft = draft ?? cv;

  if (loading || !activeDraft.content) {
    return <SkeletonBlock className="h-96" />;
  }

  const downloadPreview = async () => {
    try {
      await downloadCv(activeDraft.id);
    } catch {
      toast.error("Download failed", "The backend could not download this CV.");
    }
  };

  const saveCv = () => {
    updateCv(activeDraft.id, activeDraft);
    toast.success("CV saved", "Your preview edits are now stored in the current frontend session.");
  };

  return (
    <div className="space-y-6">
      <SectionHeading
        eyebrow="CV Preview"
        title={`${activeDraft.jobTitle} - ${activeDraft.companyName}`}
        description="Edit the content inline, switch templates, then download the backend-generated file or save the latest preview state."
        action={
          <>
            <Button type="button" variant="outline" onClick={downloadPreview}>
              <Download className="mr-2 h-4 w-4" />
              Download
            </Button>
            <Button type="button" onClick={saveCv}>
              <Save className="mr-2 h-4 w-4" />
              Save CV
            </Button>
          </>
        }
      />

      <div className="grid gap-5 xl:grid-cols-[1.15fr_0.85fr]">
        <div className="paper-panel bg-white p-8 text-black">
          <div className="grid gap-4 border-b border-black pb-4 md:grid-cols-[1.4fr_1fr]">
            <div>
              <h2 className="font-serif text-4xl font-bold leading-none">{activeDraft.content.header.name}</h2>
              <p className="mt-2 font-serif text-base font-bold">{activeDraft.content.header.title}</p>
              <p className="mt-1 font-serif text-sm italic text-neutral-700">
                Target role: {activeDraft.jobTitle} at {activeDraft.companyName}
              </p>
            </div>
            <p className="whitespace-pre-line text-left font-serif text-sm leading-6 text-neutral-800 md:text-right">
              {[activeDraft.content.header.phone, activeDraft.content.header.email, activeDraft.content.header.address]
                .filter(Boolean)
                .join("\n")}
            </p>
          </div>

          <div className="mt-5 space-y-5 font-serif text-[0.95rem] leading-6">
            <section>
              <h3 className="border-b border-black font-serif text-lg font-bold uppercase tracking-wide">Professional Summary</h3>
              <p className="mt-2 text-justify">{activeDraft.content.summary}</p>
            </section>

            {hasItems(activeDraft.content.education) ? (
              <section>
                <h3 className="border-b border-black font-serif text-lg font-bold uppercase tracking-wide">Education</h3>
                <div className="mt-2 space-y-3">
                  {activeDraft.content.education.map((item) => (
                    <div key={`${item.degree}-${item.school}`}>
                      <p className="font-bold">{[item.degree, item.field].filter(Boolean).join(" in ")}</p>
                      <p className="italic text-neutral-700">{item.school}</p>
                    </div>
                  ))}
                </div>
              </section>
            ) : null}

            {hasItems(activeDraft.content.projects) ? (
              <section>
                <h3 className="border-b border-black font-serif text-lg font-bold uppercase tracking-wide">Projects</h3>
                <div className="mt-2 space-y-4">
                  {activeDraft.content.projects.map((project) => (
                    <div key={project.name}>
                      <p className="font-bold">{project.name}</p>
                      <p className="italic text-neutral-700">{project.description}</p>
                      {hasItems(project.bullets) ? (
                        <ul className="ml-5 mt-1 list-['-_'] space-y-1">
                          {project.bullets.map((bullet) => (
                            <li key={bullet} className="pl-1">{bullet}</li>
                          ))}
                        </ul>
                      ) : null}
                      {project.technologies ? <p className="mt-1"><span className="font-bold">Technologies:</span> {project.technologies}</p> : null}
                    </div>
                  ))}
                </div>
              </section>
            ) : null}

            <section>
              <h3 className="border-b border-black font-serif text-lg font-bold uppercase tracking-wide">Experience</h3>
              <div className="mt-2 space-y-4">
                {activeDraft.content.experience.map((item) => (
                  <div key={item.id}>
                    <div className="flex gap-4">
                      <p className="flex-1 font-bold">{item.role} - {item.company}</p>
                      <p className="shrink-0 italic text-neutral-700">{item.period}</p>
                    </div>
                    {item.location ? <p className="italic text-neutral-700">{item.location}</p> : null}
                    <ul className="ml-5 mt-1 list-['-_'] space-y-1">
                      {item.bullets.map((bullet) => (
                        <li key={bullet} className="pl-1">{bullet}</li>
                      ))}
                    </ul>
                  </div>
                ))}
              </div>
            </section>

            <section>
              <h3 className="border-b border-black font-serif text-lg font-bold uppercase tracking-wide">Technical Skills</h3>
              <div className="mt-2 space-y-1">
                {hasItems(activeDraft.content.skillGroups) ? (
                  activeDraft.content.skillGroups.map((group) => (
                    <p key={group.label}><span className="font-bold">{group.label}:</span> {group.items.join(", ")}</p>
                  ))
                ) : (
                  <p><span className="font-bold">Core Skills:</span> {activeDraft.content.skills.join(", ")}</p>
                )}
              </div>
            </section>

            {hasItems(activeDraft.content.certifications) ? (
              <section>
                <h3 className="border-b border-black font-serif text-lg font-bold uppercase tracking-wide">Certifications</h3>
                <p className="mt-2">{renderSimpleItems(activeDraft.content.certifications)}</p>
              </section>
            ) : null}

            {hasItems(activeDraft.content.languages) ? (
              <section>
                <h3 className="border-b border-black font-serif text-lg font-bold uppercase tracking-wide">Languages</h3>
                <p className="mt-2">{renderSimpleItems(activeDraft.content.languages)}</p>
              </section>
            ) : null}
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
