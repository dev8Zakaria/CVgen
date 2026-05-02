import { AuthProvider } from "@/modules/auth/AuthProvider";

export function AppProviders({ children }) {
  return <AuthProvider>{children}</AuthProvider>;
}
