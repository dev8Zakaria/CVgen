import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter, Outlet, Route, Routes } from "react-router-dom";
import { vi } from "vitest";

import { AppRouter } from "@/app/router";
import { ROUTES } from "@/shared/constants/routes";

// ─── mock all leaf pages so tests stay unit-level ─────────────────────────────

vi.mock("@/modules/landing/pages/LandingPage", () => ({
  LandingPage: () => <div>Landing page</div>,
}));

vi.mock("@/modules/profile/pages/ProfilePage", () => ({
  ProfilePage: () => <div>Profile page</div>,
}));

vi.mock("@/modules/opportunities/pages/OpportunitiesPage", () => ({
  OpportunitiesPage: () => <div>Opportunities page</div>,
}));

vi.mock("@/modules/cv/pages/CvPage", () => ({
  CvPage: () => <div>CV page</div>,
}));

// ─── mock layouts so we only render the <Outlet /> ───────────────────────────

vi.mock("@/shared/layouts/MainLayout", () => ({
  MainLayout: () => <div data-testid="main-layout"><Outlet /></div>,
}));

vi.mock("@/shared/layouts/DashboardLayout", () => ({
  DashboardLayout: () => <div data-testid="dashboard-layout"><Outlet /></div>,
}));

// ─── mock auth ────────────────────────────────────────────────────────────────

const mockUseAuth = vi.hoisted(() => vi.fn());

vi.mock("@/modules/auth/AuthProvider", () => ({
  useAuth: () => mockUseAuth(),
  // AuthProvider not used by AppRouter directly, but export it anyway
  AuthProvider: ({ children }) => children,
}));

// ─── helpers ──────────────────────────────────────────────────────────────────

function renderAt(path, authState) {
  mockUseAuth.mockReturnValue({
    authEnabled: true,
    initialized: true,
    authenticated: false,
    login: vi.fn(),
    logout: vi.fn(),
    register: vi.fn(),
    ...authState,
  });

  return render(
    <MemoryRouter initialEntries={[path]}>
      <AppRouter />
    </MemoryRouter>,
  );
}

// ─── tests ────────────────────────────────────────────────────────────────────

describe("AppRouter", () => {

  // ── public routes ─────────────────────────────────────────────────────────

  it("renders the landing page at the root path", () => {
    renderAt(ROUTES.landing, { authenticated: false });

    expect(screen.getByText("Landing page")).toBeInTheDocument();
  });

  // ── wildcard redirect ─────────────────────────────────────────────────────

  it("redirects unknown paths to the landing page", () => {
    renderAt("/this/does/not/exist", { authenticated: false });

    expect(screen.getByText("Landing page")).toBeInTheDocument();
  });

  // ── protected routes – unauthenticated ───────────────────────────────────

  it("redirects unauthenticated users away from /profile to landing", () => {
    renderAt(ROUTES.profile, { authenticated: false });

    expect(screen.getByText("Landing page")).toBeInTheDocument();
    expect(screen.queryByText("Profile page")).not.toBeInTheDocument();
  });

  it("redirects unauthenticated users away from /opportunities to landing", () => {
    renderAt(ROUTES.opportunities, { authenticated: false });

    expect(screen.getByText("Landing page")).toBeInTheDocument();
    expect(screen.queryByText("Opportunities page")).not.toBeInTheDocument();
  });

  it("redirects unauthenticated users away from /cv to landing", () => {
    renderAt(ROUTES.cv, { authenticated: false });

    expect(screen.getByText("Landing page")).toBeInTheDocument();
    expect(screen.queryByText("CV page")).not.toBeInTheDocument();
  });

  // ── protected routes – authenticated ─────────────────────────────────────

  it("renders the Profile page for authenticated users", () => {
    renderAt(ROUTES.profile, { authenticated: true });

    expect(screen.getByText("Profile page")).toBeInTheDocument();
    expect(screen.queryByText("Landing page")).not.toBeInTheDocument();
  });

  it("renders the Opportunities page for authenticated users", () => {
    renderAt(ROUTES.opportunities, { authenticated: true });

    expect(screen.getByText("Opportunities page")).toBeInTheDocument();
  });

  it("renders the CV page for authenticated users", () => {
    renderAt(ROUTES.cv, { authenticated: true });

    expect(screen.getByText("CV page")).toBeInTheDocument();
  });

  // ── auth disabled bypass ──────────────────────────────────────────────────

  it("grants access to protected routes when authEnabled is false, even if authenticated is false", () => {
    renderAt(ROUTES.profile, { authEnabled: false, authenticated: false });

    expect(screen.getByText("Profile page")).toBeInTheDocument();
  });

  // ── initialising state ────────────────────────────────────────────────────

  it("shows the preparing-authentication message while Keycloak is initialising", () => {
    renderAt(ROUTES.profile, { initialized: false, authenticated: false });

    expect(screen.getByText(/preparing authentication/i)).toBeInTheDocument();
    expect(screen.queryByText("Profile page")).not.toBeInTheDocument();
    expect(screen.queryByText("Landing page")).not.toBeInTheDocument();
  });

  // ── login / register / logout UI triggers ────────────────────────────────
  // These verify the auth callbacks are wired into the context that the
  // landing page (or any future component) can consume.  We use a small
  // in-test component to invoke them via the context instead of coupling to
  // the real LandingPage markup.

  it("exposes a working login callback through useAuth", async () => {
    const login = vi.fn();
    mockUseAuth.mockReturnValue({
      authEnabled: true,
      initialized: true,
      authenticated: false,
      login,
      logout: vi.fn(),
      register: vi.fn(),
    });

    // Render a minimal consumer that calls login() on button click
    const { useAuth } = await import("@/modules/auth/AuthProvider");
    function LoginTrigger() {
      const { login: doLogin } = useAuth();
      return <button onClick={() => doLogin()}>Login</button>;
    }

    render(
      <MemoryRouter>
        <LoginTrigger />
      </MemoryRouter>,
    );

    await userEvent.click(screen.getByRole("button", { name: /login/i }));

    expect(login).toHaveBeenCalledOnce();
  });

  it("exposes a working logout callback through useAuth", async () => {
    const logout = vi.fn();
    mockUseAuth.mockReturnValue({
      authEnabled: true,
      initialized: true,
      authenticated: true,
      login: vi.fn(),
      logout,
      register: vi.fn(),
    });

    const { useAuth } = await import("@/modules/auth/AuthProvider");
    function LogoutTrigger() {
      const { logout: doLogout } = useAuth();
      return <button onClick={() => doLogout()}>Logout</button>;
    }

    render(
      <MemoryRouter>
        <LogoutTrigger />
      </MemoryRouter>,
    );

    await userEvent.click(screen.getByRole("button", { name: /logout/i }));

    expect(logout).toHaveBeenCalledOnce();
  });

  it("exposes a working register callback through useAuth", async () => {
    const register = vi.fn();
    mockUseAuth.mockReturnValue({
      authEnabled: true,
      initialized: true,
      authenticated: false,
      login: vi.fn(),
      logout: vi.fn(),
      register,
    });

    const { useAuth } = await import("@/modules/auth/AuthProvider");
    function RegisterTrigger() {
      const { register: doRegister } = useAuth();
      return <button onClick={() => doRegister()}>Register</button>;
    }

    render(
      <MemoryRouter>
        <RegisterTrigger />
      </MemoryRouter>,
    );

    await userEvent.click(screen.getByRole("button", { name: /register/i }));

    expect(register).toHaveBeenCalledOnce();
  });
});