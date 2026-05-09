import { render, screen } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { vi } from "vitest";

import { ProtectedRoute } from "@/modules/auth/ProtectedRoute";
import { ROUTES } from "@/shared/constants/routes";

const mockUseAuth = vi.fn();

vi.mock("@/modules/auth/AuthProvider", () => ({
  useAuth: () => mockUseAuth(),
}));

describe("ProtectedRoute", () => {
  it("redirects guests to the landing page when auth is enabled", () => {
    mockUseAuth.mockReturnValue({
      authEnabled: true,
      initialized: true,
      authenticated: false,
    });

    render(
      <MemoryRouter initialEntries={[ROUTES.profile]}>
        <Routes>
          <Route path={ROUTES.landing} element={<div>Landing page</div>} />
          <Route
            path={ROUTES.profile}
            element={
              <ProtectedRoute>
                <div>Protected profile</div>
              </ProtectedRoute>
            }
          />
        </Routes>
      </MemoryRouter>,
    );

    expect(screen.getByText("Landing page")).toBeInTheDocument();
    expect(screen.queryByText("Protected profile")).not.toBeInTheDocument();
  });

  it("renders children for authenticated users", () => {
    mockUseAuth.mockReturnValue({
      authEnabled: true,
      initialized: true,
      authenticated: true,
    });

    render(
      <MemoryRouter initialEntries={[ROUTES.profile]}>
        <Routes>
          <Route
            path={ROUTES.profile}
            element={
              <ProtectedRoute>
                <div>Protected profile</div>
              </ProtectedRoute>
            }
          />
        </Routes>
      </MemoryRouter>,
    );

    expect(screen.getByText("Protected profile")).toBeInTheDocument();
  });
});
