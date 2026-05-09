import { act, renderHook, waitFor } from "@testing-library/react";
import { vi } from "vitest";

import { useProfile } from "@/modules/profile/hooks/useProfile";

// ─── mock the service layer ───────────────────────────────────────────────────

const mockProfileService = vi.hoisted(() => ({
  getCurrentProfile: vi.fn(),
  updateCurrentProfile: vi.fn(),
  deleteCurrentProfile: vi.fn(),
}));

vi.mock("@/modules/profile/services/profileService", () => ({
  profileService: mockProfileService,
}));

// ─── fixtures ─────────────────────────────────────────────────────────────────

const fakeProfile = {
  id: "p-1",
  fullName: "Jane Doe",
  email: "jane@example.com",
  phone: "+212 600 000 000",
  location: "Casablanca, Morocco",
  title: "Full-Stack Developer",
  summary: "Great engineer.",
  role: "User",
};

// ─────────────────────────────────────────────────────────────────────────────

describe("useProfile", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("starts in loading state and resolves the profile on success", async () => {
    mockProfileService.getCurrentProfile.mockResolvedValue({
      data: fakeProfile,
    });

    const { result } = renderHook(() => useProfile());

    expect(result.current.loading).toBe(true);

    await waitFor(() => expect(result.current.loading).toBe(false));

    expect(result.current.profile).toEqual(fakeProfile);
    expect(result.current.error).toBeNull();
  });

  it("sets error and clears loading when the API fails", async () => {
    const apiError = new Error("500");
    mockProfileService.getCurrentProfile.mockRejectedValue(apiError);

    const { result } = renderHook(() => useProfile());

    await waitFor(() => expect(result.current.loading).toBe(false));

    expect(result.current.profile).toBeNull();
    expect(result.current.error).toBe(apiError);
  });

  it("updateProfile sends the payload, updates state, and returns the profile", async () => {
    mockProfileService.getCurrentProfile.mockResolvedValue({
      data: fakeProfile,
    });
    const updated = { ...fakeProfile, title: "Senior Engineer" };
    mockProfileService.updateCurrentProfile.mockResolvedValue({
      data: updated,
    });

    const { result } = renderHook(() => useProfile());
    await waitFor(() => expect(result.current.loading).toBe(false));

    let returnValue;
    await act(async () => {
      returnValue = await result.current.updateProfile({
        title: "Senior Engineer",
      });
    });

    expect(mockProfileService.updateCurrentProfile).toHaveBeenCalledWith({
      title: "Senior Engineer",
    });
    expect(result.current.profile).toEqual(updated);
    expect(returnValue).toEqual(updated);
  });

  it("updateProfile throws and sets error when the request fails", async () => {
    mockProfileService.getCurrentProfile.mockResolvedValue({
      data: fakeProfile,
    });
    const apiError = new Error("422");
    mockProfileService.updateCurrentProfile.mockRejectedValue(apiError);

    const { result } = renderHook(() => useProfile());
    await waitFor(() => expect(result.current.loading).toBe(false));

    await act(async () => {
      try {
        await result.current.updateProfile({});
      } catch {
        // expected rejection
      }
    });

    expect(result.current.error).toBe(apiError);
  });

  it("deleteProfile throws and sets error when the request fails", async () => {
    mockProfileService.getCurrentProfile.mockResolvedValue({
      data: fakeProfile,
    });
    const apiError = new Error("500");
    mockProfileService.deleteCurrentProfile.mockRejectedValue(apiError);

    const { result } = renderHook(() => useProfile());
    await waitFor(() => expect(result.current.loading).toBe(false));

    await act(async () => {
      try {
        await result.current.deleteProfile();
      } catch {
        // expected rejection
      }
    });

    expect(result.current.error).toBe(apiError);
  });

  it("deleteProfile nullifies the profile and returns", async () => {
    mockProfileService.getCurrentProfile.mockResolvedValue({
      data: fakeProfile,
    });
    mockProfileService.deleteCurrentProfile.mockResolvedValue(undefined);

    const { result } = renderHook(() => useProfile());
    await waitFor(() => expect(result.current.loading).toBe(false));

    await act(async () => {
      await result.current.deleteProfile();
    });

    expect(result.current.profile).toBeNull();
    expect(result.current.deleting).toBe(false);
  });

  it("reload triggers a fresh GET and updates the profile", async () => {
    const reloaded = { ...fakeProfile, title: "Reloaded Title" };
    mockProfileService.getCurrentProfile
      .mockResolvedValueOnce({ data: fakeProfile })
      .mockResolvedValueOnce({ data: reloaded });

    const { result } = renderHook(() => useProfile());
    await waitFor(() => expect(result.current.loading).toBe(false));

    act(() => {
      result.current.reload();
    });

    await waitFor(() => expect(result.current.loading).toBe(false));

    expect(result.current.profile).toEqual(reloaded);
    expect(mockProfileService.getCurrentProfile).toHaveBeenCalledTimes(2);
  });

  it("sets updating to true while a PUT is in flight", async () => {
    mockProfileService.getCurrentProfile.mockResolvedValue({
      data: fakeProfile,
    });
    let resolvePut;
    mockProfileService.updateCurrentProfile.mockReturnValue(
      new Promise((res) => {
        resolvePut = res;
      }),
    );

    const { result } = renderHook(() => useProfile());
    await waitFor(() => expect(result.current.loading).toBe(false));

    act(() => {
      result.current.updateProfile({});
    });

    expect(result.current.updating).toBe(true);

    await act(async () => {
      resolvePut({ data: fakeProfile });
    });

    expect(result.current.updating).toBe(false);
  });

  it("sets deleting to true while a DELETE is in flight", async () => {
    mockProfileService.getCurrentProfile.mockResolvedValue({
      data: fakeProfile,
    });
    let resolveDelete;
    mockProfileService.deleteCurrentProfile.mockReturnValue(
      new Promise((res) => {
        resolveDelete = res;
      }),
    );

    const { result } = renderHook(() => useProfile());
    await waitFor(() => expect(result.current.loading).toBe(false));

    act(() => {
      result.current.deleteProfile();
    });

    expect(result.current.deleting).toBe(true);

    await act(async () => {
      resolveDelete();
    });

    expect(result.current.deleting).toBe(false);
  });
});
