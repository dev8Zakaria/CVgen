import { AuthProvider } from "@/modules/auth/AuthProvider";
import { PrototypeAppProvider } from "@/shared/providers/PrototypeAppProvider";
import { ThemeProvider } from "@/shared/providers/ThemeProvider";
import { ToastProvider } from "@/shared/providers/ToastProvider";

export function AppProviders({ children }) {
  return (
    <ThemeProvider>
      <AuthProvider>
        <PrototypeAppProvider>
          <ToastProvider>{children}</ToastProvider>
        </PrototypeAppProvider>
      </AuthProvider>
    </ThemeProvider>
  );
}
