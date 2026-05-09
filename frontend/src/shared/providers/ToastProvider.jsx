import { createContext, useCallback, useContext, useMemo, useState } from "react";

const ToastContext = createContext(null);

export function ToastProvider({ children }) {
  const [toasts, setToasts] = useState([]);

  const dismiss = useCallback((id) => {
    setToasts((current) => current.filter((toast) => toast.id !== id));
  }, []);

  const push = useCallback(({ title, description, tone = "default" }) => {
    const id = crypto.randomUUID();
    setToasts((current) => [...current, { id, title, description, tone }]);
    window.setTimeout(() => dismiss(id), 3800);
  }, [dismiss]);

  const value = useMemo(
    () => ({
      toast: {
        push,
        success: (title, description) => push({ title, description, tone: "success" }),
        error: (title, description) => push({ title, description, tone: "error" }),
      },
    }),
    [push],
  );

  return (
    <ToastContext.Provider value={value}>
      {children}
      <div className="fixed right-5 top-5 z-[70] flex w-full max-w-sm flex-col gap-3">
        {toasts.map((toast) => {
          const toneClasses =
            toast.tone === "success"
              ? "border-success/20 bg-success/10"
              : toast.tone === "error"
                ? "border-destructive/20 bg-destructive/10"
                : "border-border bg-card/90";

          return (
            <div key={toast.id} className={`paper-panel animate-in slide-in-from-top-3 p-4 ${toneClasses}`}>
              <div className="flex items-start justify-between gap-3">
                <div>
                  <p className="text-sm font-semibold text-foreground">{toast.title}</p>
                  {toast.description ? <p className="mt-1 text-sm text-muted-foreground">{toast.description}</p> : null}
                </div>
                <button type="button" onClick={() => dismiss(toast.id)} className="text-xs text-muted-foreground">
                  Close
                </button>
              </div>
            </div>
          );
        })}
      </div>
    </ToastContext.Provider>
  );
}

export function useToast() {
  const context = useContext(ToastContext);
  if (!context) {
    throw new Error("useToast must be used inside ToastProvider");
  }

  return context.toast;
}
