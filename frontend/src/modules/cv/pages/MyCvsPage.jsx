import { Download, Eye, Trash2 } from "lucide-react";
import { useState } from "react";
import { Link } from "react-router-dom";

import { Button } from "@/components/ui/button";
import { ConfirmDialog, EmptyState, SectionHeading, SkeletonBlock, StatusPill } from "@/shared/components/app-ui";
import { ROUTES, getCvPreviewRoute } from "@/shared/constants/routes";
import { usePrototypeApp } from "@/shared/providers/PrototypeAppProvider";
import { useToast } from "@/shared/providers/ToastProvider";

function formatDate(date) {
  return new Intl.DateTimeFormat("en", { dateStyle: "medium" }).format(new Date(date));
}

export function MyCvsPage() {
  const { hydrated, cvs, deleteCv } = usePrototypeApp();
  const toast = useToast();
  const [pendingDelete, setPendingDelete] = useState(null);

  if (!hydrated) {
    return <SkeletonBlock className="h-80" />;
  }

  const downloadPreview = (cv) => {
    const popup = window.open("", "_blank");
    if (!popup) {
      toast.error("Download blocked", "Please allow popups to use the print-to-PDF prototype flow.");
      return;
    }

    popup.document.write(`
      <html>
        <head><title>${cv.jobTitle}</title></head>
        <body style="font-family: Georgia, serif; padding: 48px; line-height: 1.6;">
          <h1>${cv.content.header.name}</h1>
          <p>${cv.content.header.title}</p>
          <h2>Summary</h2>
          <p>${cv.content.summary}</p>
        </body>
      </html>
    `);
    popup.document.close();
    popup.focus();
    popup.print();
  };

  return (
    <div className="space-y-6">
      <SectionHeading
        eyebrow="My CVs"
        title="Your generated CV library."
        description="Review tailored drafts, reopen them for editing, print them to PDF, or clean up versions you no longer need."
        action={
          <Button asChild>
            <Link to={ROUTES.generateCv}>Generate another CV</Link>
          </Button>
        }
      />

      {cvs.length === 0 ? (
        <EmptyState
          title="No CVs generated yet"
          description="Generate your first tailored CV from an analyzed job offer. It will appear here with preview, download, and delete actions."
          action={
            <Button asChild>
              <Link to={ROUTES.generateCv}>Start the generation flow</Link>
            </Button>
          }
        />
      ) : (
        <div className="grid gap-4">
          {cvs.map((cv) => (
            <div key={cv.id} className="paper-panel grain-card p-6">
              <div className="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
                <div>
                  <div className="flex flex-wrap items-center gap-3">
                    <h2 className="font-display text-3xl tracking-[-0.04em] text-foreground">{cv.jobTitle}</h2>
                    <StatusPill tone="primary">{cv.template}</StatusPill>
                  </div>
                  <p className="mt-2 text-sm text-muted-foreground">{cv.companyName} · Generated {formatDate(cv.createdAt)}</p>
                </div>
                <div className="flex flex-wrap gap-3">
                  <Button asChild variant="outline">
                    <Link to={getCvPreviewRoute(cv.id)}>
                      <Eye className="mr-2 h-4 w-4" />
                      View
                    </Link>
                  </Button>
                  <Button type="button" variant="outline" onClick={() => downloadPreview(cv)}>
                    <Download className="mr-2 h-4 w-4" />
                    Download
                  </Button>
                  <Button type="button" variant="outline" onClick={() => setPendingDelete(cv)}>
                    <Trash2 className="mr-2 h-4 w-4" />
                    Delete
                  </Button>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}

      <ConfirmDialog
        open={Boolean(pendingDelete)}
        title="Delete CV?"
        description="This removes the generated draft from your library."
        confirmLabel="Delete CV"
        tone="danger"
        onCancel={() => setPendingDelete(null)}
        onConfirm={() => {
          deleteCv(pendingDelete.id);
          toast.success("CV deleted");
          setPendingDelete(null);
        }}
      />
    </div>
  );
}
