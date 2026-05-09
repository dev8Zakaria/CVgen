import { Check, ChevronRight, MoonStar, SunMedium, X } from "lucide-react";
import { Link } from "react-router-dom";

import { Button } from "@/components/ui/button";
import { cn } from "@/shared/utils/cn";

export function AppLogo({ compact = false }) {
  return (
    <Link to="/" className="flex items-center gap-3">
      <div className="flex h-11 w-11 items-center justify-center rounded-[18px] bg-gradient-to-br from-primary via-blue-500 to-accent text-primary-foreground shadow-[0_18px_36px_rgba(37,99,235,0.24)]">
        <span className="font-display text-lg font-bold uppercase tracking-[0.14em]">CV</span>
      </div>
      {!compact ? (
        <div>
          <p className="font-mono text-[0.68rem] uppercase tracking-[0.3em] text-primary/80">AI CV Generator</p>
          <p className="font-display text-[1.35rem] font-bold tracking-[-0.05em] text-foreground">Career OS</p>
        </div>
      ) : null}
    </Link>
  );
}

export function ThemeToggle({ theme, toggleTheme }) {
  return (
    <Button type="button" variant="outline" size="icon" onClick={toggleTheme} aria-label="Toggle theme">
      {theme === "dark" ? <SunMedium className="h-4 w-4" /> : <MoonStar className="h-4 w-4" />}
    </Button>
  );
}

export function StatusPill({ children, tone = "default", className }) {
  const toneMap = {
    default: "border-border bg-white/70 text-muted-foreground dark:bg-white/[0.04]",
    success: "border-success/20 bg-success/10 text-success",
    warning: "border-warning/20 bg-warning/15 text-warning-foreground",
    danger: "border-destructive/20 bg-destructive/10 text-destructive",
    accent: "border-accent/20 bg-accent/10 text-accent-foreground",
    primary: "border-primary/20 bg-primary/10 text-primary",
  };

  return <span className={cn("pill", toneMap[tone], className)}>{children}</span>;
}

export function SectionHeading({ eyebrow, title, description, action, className }) {
  return (
    <div className={cn("flex flex-col gap-5 md:flex-row md:items-end md:justify-between", className)}>
      <div className="space-y-3">
        {eyebrow ? <p className="font-mono text-[0.72rem] uppercase tracking-[0.3em] text-primary/80">{eyebrow}</p> : null}
        <div>
          <h1 className="max-w-4xl font-display text-4xl font-bold tracking-[-0.06em] text-foreground md:text-5xl">{title}</h1>
          {description ? <p className="mt-3 max-w-2xl text-sm leading-7 text-muted-foreground">{description}</p> : null}
        </div>
      </div>
      {action ? <div className="flex shrink-0 items-center gap-3">{action}</div> : null}
    </div>
  );
}

export function StatCard({ label, value, meta, accent, className }) {
  return (
    <div className={cn("paper-panel grain-card p-6", className)}>
      <div className="flex items-start justify-between gap-4">
        <div>
          <p className="font-mono text-[0.7rem] uppercase tracking-[0.28em] text-primary/80">{label}</p>
          <p className="mt-3 font-display text-5xl font-bold tracking-[-0.07em] text-foreground">{value}</p>
        </div>
        {accent ? <div className="rounded-xl border border-primary/15 bg-primary/10 px-3 py-1 text-xs font-semibold text-primary">{accent}</div> : null}
      </div>
      {meta ? <p className="mt-4 text-sm leading-6 text-muted-foreground">{meta}</p> : null}
    </div>
  );
}

export function EmptyState({ title, description, action, className }) {
  return (
    <div className={cn("paper-panel grain-card flex flex-col items-center justify-center px-6 py-14 text-center", className)}>
      <div className="flex h-14 w-14 items-center justify-center rounded-2xl border border-primary/15 bg-primary/10">
        <Check className="h-5 w-5 text-primary" />
      </div>
      <h3 className="mt-5 font-display text-3xl font-bold tracking-[-0.05em] text-foreground">{title}</h3>
      <p className="mt-3 max-w-xl text-sm leading-7 text-muted-foreground">{description}</p>
      {action ? <div className="mt-6">{action}</div> : null}
    </div>
  );
}

export function StepIndicator({ steps, currentStep }) {
  return (
    <div className="flex flex-wrap items-center gap-3">
      {steps.map((step, index) => {
        const state = index < currentStep ? "done" : index === currentStep ? "active" : "idle";

        return (
          <div key={step} className="flex items-center gap-3">
            <div
              className={cn(
                "flex items-center gap-3 rounded-2xl border px-4 py-2.5 text-sm",
                state === "done" && "border-primary/20 bg-primary/10 text-primary",
                state === "active" && "border-primary/25 bg-primary/[0.08] text-foreground shadow-sm",
                state === "idle" && "border-border bg-white/60 text-muted-foreground dark:bg-white/[0.03]",
              )}
            >
              <span
                className={cn(
                  "flex h-7 w-7 items-center justify-center rounded-xl text-xs font-semibold",
                  state === "done" && "bg-primary text-primary-foreground",
                  state === "active" && "bg-primary text-primary-foreground",
                  state === "idle" && "bg-muted text-muted-foreground",
                )}
              >
                {index + 1}
              </span>
              {step}
            </div>
            {index < steps.length - 1 ? <ChevronRight className="h-4 w-4 text-muted-foreground" /> : null}
          </div>
        );
      })}
    </div>
  );
}

export function SkeletonBlock({ className }) {
  return <div className={cn("loading-sheen animate-shimmer rounded-[24px]", className)} />;
}

export function ConfirmDialog({ open, title, description, confirmLabel, onConfirm, onCancel, tone = "default" }) {
  if (!open) {
    return null;
  }

  const toneClass = tone === "danger" ? "border-destructive/25" : "border-border";

  return (
    <div className="fixed inset-0 z-[80] flex items-center justify-center bg-black/30 px-4 backdrop-blur-sm">
      <div className={`paper-panel w-full max-w-md p-6 ${toneClass}`}>
        <div className="flex items-start justify-between gap-3">
          <div>
            <h3 className="font-display text-3xl font-bold tracking-[-0.05em] text-foreground">{title}</h3>
            <p className="mt-3 text-sm leading-7 text-muted-foreground">{description}</p>
          </div>
          <button type="button" onClick={onCancel} className="rounded-2xl border border-border p-2 text-muted-foreground transition hover:border-primary/20 hover:text-foreground">
            <X className="h-4 w-4" />
          </button>
        </div>
        <div className="mt-6 flex justify-end gap-3">
          <Button type="button" variant="outline" onClick={onCancel}>
            Cancel
          </Button>
          <Button type="button" variant={tone === "danger" ? "destructive" : "default"} onClick={onConfirm}>
            {confirmLabel}
          </Button>
        </div>
      </div>
    </div>
  );
}

export function InlineTagEditor({ items, onRemove, onAdd, placeholder = "Add tag" }) {
  return (
    <div className="rounded-2xl border border-border bg-white/60 p-4 dark:bg-white/[0.03]">
      <div className="flex flex-wrap gap-2">
        {items.map((item) => (
          <button
            key={item}
            type="button"
            onClick={() => onRemove(item)}
            className="pill transition hover:border-destructive/20 hover:text-destructive"
          >
            {item}
          </button>
        ))}
      </div>
      <form
        className="mt-4 flex gap-3"
        onSubmit={(event) => {
          event.preventDefault();
          const formData = new FormData(event.currentTarget);
          const value = String(formData.get("value") || "");
          onAdd(value);
          event.currentTarget.reset();
        }}
      >
        <input name="value" placeholder={placeholder} className="field" />
        <Button type="submit" variant="outline">
          Add
        </Button>
      </form>
    </div>
  );
}
