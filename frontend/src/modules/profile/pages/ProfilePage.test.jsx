import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { vi } from "vitest";

import { ProfilePage } from "@/modules/profile/pages/ProfilePage";

// ─── mock hooks ──────────────────────────────────────────────────────────────

const mockUseAuth = vi.fn();
const mockUseProfile = vi.fn();

vi.mock("@/modules/auth/AuthProvider", () => ({
  useAuth: () => mockUseAuth(),
}));

vi.mock("@/modules/profile/hooks/useProfile", () => ({
  useProfile: () => mockUseProfile(),
}));

// ─── shared fixtures ──────────────────────────────────────────────────────────

const baseAuth = {
  user: {
    sub: "kc-123",
    preferred_username: "jdoe",
    given_name: "Jane",
    family_name: "Doe",
  },
};

const baseProfile = {
  fullName: "Jane Doe",
  email: "jane@example.com",
  phone: "+212 600 000 000",
  location: "Casablanca, Morocco",
  title: "Full-Stack Developer",
  summary: "Passionate engineer building great products.",
  role: "User",
};

function makeProfileHook(overrides = {}) {
  return {
    profile: baseProfile,
    loading: false,
    updating: false,
    deleting: false,
    error: null,
    reload: vi.fn(),
    updateProfile: vi.fn().mockResolvedValue(baseProfile),
    deleteProfile: vi.fn().mockResolvedValue(undefined),
    ...overrides,
  };
}

// ─── tests ────────────────────────────────────────────────────────────────────

describe("ProfilePage", () => {
  beforeEach(() => {
    mockUseAuth.mockReturnValue(baseAuth);
  });

  // ── loading state ────────────────────────────────────────────────────────

  it("shows a loading spinner while the profile is being fetched", () => {
    mockUseProfile.mockReturnValue(makeProfileHook({ profile: null, loading: true }));

    render(<ProfilePage />);

    expect(screen.getByText(/loading your profile/i)).toBeInTheDocument();
    // edit form must not be present yet
    expect(screen.queryByRole("button", { name: /save changes/i })).not.toBeInTheDocument();
  });

  // ── error state ──────────────────────────────────────────────────────────

  it("shows an error panel with a retry button when the API call fails", () => {
    mockUseProfile.mockReturnValue(
      makeProfileHook({ profile: null, error: new Error("500") }),
    );

    render(<ProfilePage />);

    expect(screen.getByText(/we could not load the profile api/i)).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /retry request/i })).toBeInTheDocument();
  });

  it("calls reload when the user clicks Retry request", async () => {
    const reload = vi.fn();
    mockUseProfile.mockReturnValue(
      makeProfileHook({ profile: null, error: new Error("500"), reload }),
    );

    render(<ProfilePage />);

    await userEvent.click(screen.getByRole("button", { name: /retry request/i }));

    expect(reload).toHaveBeenCalledOnce();
  });

  // ── success / display state ───────────────────────────────────────────────

  it("renders profile info cards when data loads successfully", () => {
    mockUseProfile.mockReturnValue(makeProfileHook());

    render(<ProfilePage />);

    expect(screen.getByText("jane@example.com")).toBeInTheDocument();
    expect(screen.getByText("+212 600 000 000")).toBeInTheDocument();
    expect(screen.getByText("Casablanca, Morocco")).toBeInTheDocument();
    expect(screen.getByText("Passionate engineer building great products.", { selector: "p" })).toBeInTheDocument();
  });

  it("pre-fills the edit form fields with current profile values", () => {
    mockUseProfile.mockReturnValue(makeProfileHook());

    render(<ProfilePage />);

    expect(screen.getByDisplayValue("Full-Stack Developer")).toBeInTheDocument();
    expect(screen.getByDisplayValue("Passionate engineer building great products.")).toBeInTheDocument();
    expect(screen.getByDisplayValue("+212 600 000 000")).toBeInTheDocument();
    expect(screen.getByDisplayValue("Casablanca, Morocco")).toBeInTheDocument();
  });

  it("displays the display name from the profile fullName", () => {
    mockUseProfile.mockReturnValue(makeProfileHook());

    render(<ProfilePage />);

    expect(screen.getByRole("heading", { name: /jane doe/i })).toBeInTheDocument();
  });

  it("falls back to Keycloak given_name + family_name when fullName is absent", () => {
    mockUseProfile.mockReturnValue(makeProfileHook({ profile: { ...baseProfile, fullName: undefined } }));

    render(<ProfilePage />);

    expect(screen.getByRole("heading", { name: /jane doe/i })).toBeInTheDocument();
  });

  // ── update form submission ────────────────────────────────────────────────

  it("calls updateProfile with the form values when Save changes is clicked", async () => {
    const user = userEvent.setup();
    const updateProfile = vi.fn().mockResolvedValue(baseProfile);
    mockUseProfile.mockReturnValue(makeProfileHook({ updateProfile }));

    render(<ProfilePage />);

    // clear the title field and type a new value
    const titleInput = screen.getByDisplayValue("Full-Stack Developer");
    await user.clear(titleInput);
    await user.type(titleInput, "Senior Engineer");

    await user.click(screen.getByRole("button", { name: /save changes/i }));

    expect(updateProfile).toHaveBeenCalledWith(
      expect.objectContaining({ title: "Senior Engineer" }),
    );
  });

  it("shows a success message after a successful save", async () => {
    const user = userEvent.setup();
    mockUseProfile.mockReturnValue(makeProfileHook());

    render(<ProfilePage />);

    await user.click(screen.getByRole("button", { name: /save changes/i }));

    await waitFor(() =>
      expect(screen.getByText(/profile changes saved successfully/i)).toBeInTheDocument(),
    );
  });

  it("shows an error message when updateProfile rejects", async () => {
    const user = userEvent.setup();
    const updateProfile = vi.fn().mockRejectedValue(new Error("422"));
    mockUseProfile.mockReturnValue(makeProfileHook({ updateProfile }));

    render(<ProfilePage />);

    await user.click(screen.getByRole("button", { name: /save changes/i }));

    await waitFor(() =>
      expect(screen.getByText(/we could not save the profile changes/i)).toBeInTheDocument(),
    );
  });

  it("calls reload when Reset from API is clicked", async () => {
    const user = userEvent.setup();
    const reload = vi.fn();
    mockUseProfile.mockReturnValue(makeProfileHook({ reload }));

    render(<ProfilePage />);

    await user.click(screen.getByRole("button", { name: /reset from api/i }));

    expect(reload).toHaveBeenCalledOnce();
  });

  // ── delete flow ───────────────────────────────────────────────────────────

  it("calls deleteProfile after the user confirms the dialog", async () => {
    const user = userEvent.setup();
    const deleteProfile = vi.fn().mockResolvedValue(undefined);
    vi.spyOn(window, "confirm").mockReturnValue(true);
    mockUseProfile.mockReturnValue(makeProfileHook({ deleteProfile }));

    render(<ProfilePage />);

    await user.click(screen.getByRole("button", { name: /delete local profile/i }));

    expect(deleteProfile).toHaveBeenCalledOnce();
  });

  it("does NOT call deleteProfile when the user cancels the dialog", async () => {
    const user = userEvent.setup();
    const deleteProfile = vi.fn();
    vi.spyOn(window, "confirm").mockReturnValue(false);
    mockUseProfile.mockReturnValue(makeProfileHook({ deleteProfile }));

    render(<ProfilePage />);

    await user.click(screen.getByRole("button", { name: /delete local profile/i }));

    expect(deleteProfile).not.toHaveBeenCalled();
  });

  it("shows a success message after successful deletion", async () => {
    const user = userEvent.setup();
    vi.spyOn(window, "confirm").mockReturnValue(true);
    mockUseProfile.mockReturnValue(makeProfileHook());

    render(<ProfilePage />);

    await user.click(screen.getByRole("button", { name: /delete local profile/i }));

    await waitFor(() =>
      expect(screen.getByText(/the local profile was deleted/i)).toBeInTheDocument(),
    );
  });

  it("shows an error message when deleteProfile rejects", async () => {
    const user = userEvent.setup();
    const deleteProfile = vi.fn().mockRejectedValue(new Error("500"));
    vi.spyOn(window, "confirm").mockReturnValue(true);
    mockUseProfile.mockReturnValue(makeProfileHook({ deleteProfile }));

    render(<ProfilePage />);

    await user.click(screen.getByRole("button", { name: /delete local profile/i }));

    await waitFor(() =>
      expect(screen.getByText(/we could not delete the profile/i)).toBeInTheDocument(),
    );
  });

  // ── recreate flow (after deletion) ───────────────────────────────────────

  it("shows the recreate panel when profile is null and profileDeleted is true", async () => {
    const user = userEvent.setup();
    vi.spyOn(window, "confirm").mockReturnValue(true);

    // deleteProfile sets profile to null in the real hook; simulate by
    // returning null profile after deletion resolves
    const deleteProfile = vi.fn().mockResolvedValue(undefined);
    mockUseProfile.mockReturnValue(
      makeProfileHook({ profile: null, deleteProfile }),
    );

    render(<ProfilePage />);

    await user.click(screen.queryByRole("button", { name: /delete local profile/i }) ?? screen.getByText(/delete local profile/i));

    // The component transitions to the deleted state once profileDeleted = true
    // and profile is still null (hook hasn't reloaded yet).
    await waitFor(() =>
      expect(screen.getByText(/local profile deleted/i)).toBeInTheDocument(),
    );
  });

  it("calls reload when Recreate profile from API is clicked after deletion", async () => {
    const user = userEvent.setup();
    vi.spyOn(window, "confirm").mockReturnValue(true);
    const reload = vi.fn();
    const deleteProfile = vi.fn().mockResolvedValue(undefined);

    mockUseProfile.mockReturnValue(
      makeProfileHook({ profile: null, deleteProfile, reload }),
    );

    render(<ProfilePage />);

    // trigger deletion to show the recreate panel
    await user.click(
      screen.queryByRole("button", { name: /delete local profile/i }) ??
      screen.getByText(/delete local profile/i),
    );

    await waitFor(() =>
      expect(screen.getByRole("button", { name: /recreate profile from api/i })).toBeInTheDocument(),
    );

    await user.click(screen.getByRole("button", { name: /recreate profile from api/i }));

    expect(reload).toHaveBeenCalled();
  });

  // ── disabled states ───────────────────────────────────────────────────────

  it("disables Save and Reset buttons while updating is true", () => {
    mockUseProfile.mockReturnValue(makeProfileHook({ updating: true }));

    render(<ProfilePage />);

    expect(screen.getByRole("button", { name: /saving\.\.\./i })).toBeDisabled();
    expect(screen.getByRole("button", { name: /reset from api/i })).toBeDisabled();
  });

  it("disables Delete button while deleting is true", () => {
    mockUseProfile.mockReturnValue(makeProfileHook({ deleting: true }));

    render(<ProfilePage />);

    expect(screen.getByRole("button", { name: /deleting\.\.\./i })).toBeDisabled();
  });
});