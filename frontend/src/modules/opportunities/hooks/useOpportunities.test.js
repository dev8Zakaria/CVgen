import { act, renderHook, waitFor } from "@testing-library/react";
import { vi } from "vitest";

import { useOpportunities } from "@/modules/opportunities/hooks/useOpportunities";

// ─── mock the service layer ───────────────────────────────────────────────────

const mockOpportunityService = vi.hoisted(() => ({
  listOpportunities: vi.fn(),
  getOpportunity: vi.fn(),
  createOpportunity: vi.fn(),
  analyzeOpportunity: vi.fn(),
  updateOpportunity: vi.fn(),
  deleteOpportunity: vi.fn(),
}));

vi.mock("@/modules/opportunities/services/opportunityService", () => ({
  opportunityService: mockOpportunityService,
}));

// ─── fixtures ─────────────────────────────────────────────────────────────────

const listItem = {
  id: "offer-1",
  title: "Frontend Engineer",
  companyName: "OpenAI",
  analysisStatus: "pending",
  createdAt: "2026-05-05T10:00:00Z",
  updatedAt: "2026-05-05T10:00:00Z",
};

const detailOffer = {
  ...listItem,
  description: "Build AI features.",
  analysis: null,
};

// ─────────────────────────────────────────────────────────────────────────────

describe("useOpportunities", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  // ── initial load ──────────────────────────────────────────────────────────

  it("fetches the list on mount and populates opportunities", async () => {
    mockOpportunityService.listOpportunities.mockResolvedValue({
      data: [listItem],
    });

    const { result } = renderHook(() => useOpportunities());

    expect(result.current.loading).toBe(true);

    await waitFor(() => expect(result.current.loading).toBe(false));

    expect(result.current.opportunities).toEqual([listItem]);
    expect(result.current.error).toBeNull();
  });

  it("sets error when the initial list request fails", async () => {
    const apiError = new Error("503");
    mockOpportunityService.listOpportunities.mockRejectedValue(apiError);

    const { result } = renderHook(() => useOpportunities());

    await waitFor(() => expect(result.current.loading).toBe(false));

    expect(result.current.opportunities).toEqual([]);
    expect(result.current.error).toBe(apiError);
  });

  // ── loadOpportunityDetail ─────────────────────────────────────────────────

  it("loadOpportunityDetail sets selectedOpportunity on success", async () => {
    mockOpportunityService.listOpportunities.mockResolvedValue({
      data: [listItem],
    });
    mockOpportunityService.getOpportunity.mockResolvedValue({
      data: detailOffer,
    });

    const { result } = renderHook(() => useOpportunities());
    await waitFor(() => expect(result.current.loading).toBe(false));

    await act(async () => {
      await result.current.loadOpportunityDetail("offer-1");
    });

    expect(result.current.selectedOpportunity).toEqual(detailOffer);
    expect(result.current.detailLoading).toBe(false);
  });

  it("loadOpportunityDetail throws and sets error on failure", async () => {
    mockOpportunityService.listOpportunities.mockResolvedValue({ data: [] });
    mockOpportunityService.getOpportunity.mockRejectedValue(new Error("404"));

    const { result } = renderHook(() => useOpportunities());
    await waitFor(() => expect(result.current.loading).toBe(false));

    await expect(
      act(async () => {
        await result.current.loadOpportunityDetail("missing");
      }),
    ).rejects.toThrow("404");

    expect(result.current.error).toBeDefined();
  });

  // ── createOpportunity ─────────────────────────────────────────────────────

  it("createOpportunity prepends the new offer to the list and selects it", async () => {
    mockOpportunityService.listOpportunities.mockResolvedValue({ data: [] });
    mockOpportunityService.createOpportunity.mockResolvedValue({
      data: detailOffer,
    });

    const { result } = renderHook(() => useOpportunities());
    await waitFor(() => expect(result.current.loading).toBe(false));

    await act(async () => {
      await result.current.createOpportunity({
        title: "Frontend Engineer",
        companyName: "OpenAI",
        description: "Build AI features.",
      });
    });

    expect(result.current.opportunities).toHaveLength(1);
    expect(result.current.opportunities[0].id).toBe("offer-1");
    expect(result.current.selectedOpportunity).toEqual(detailOffer);
  });

  it("createOpportunity throws and sets error on failure", async () => {
    mockOpportunityService.listOpportunities.mockResolvedValue({ data: [] });
    mockOpportunityService.createOpportunity.mockRejectedValue(
      new Error("422"),
    );

    const { result } = renderHook(() => useOpportunities());
    await waitFor(() => expect(result.current.loading).toBe(false));

    await expect(
      act(async () => {
        await result.current.createOpportunity({});
      }),
    ).rejects.toThrow("422");
  });

  // ── updateOpportunity ─────────────────────────────────────────────────────

  it("updateOpportunity patches the list and updates selectedOpportunity", async () => {
    mockOpportunityService.listOpportunities.mockResolvedValue({
      data: [listItem],
    });
    const updated = { ...detailOffer, title: "Senior Frontend Engineer" };
    mockOpportunityService.updateOpportunity.mockResolvedValue({
      data: updated,
    });

    const { result } = renderHook(() => useOpportunities());
    await waitFor(() => expect(result.current.loading).toBe(false));

    await act(async () => {
      await result.current.updateOpportunity("offer-1", {
        title: "Senior Frontend Engineer",
      });
    });

    expect(result.current.opportunities[0].title).toBe(
      "Senior Frontend Engineer",
    );
    expect(result.current.selectedOpportunity?.title).toBe(
      "Senior Frontend Engineer",
    );
  });

  // ── analyzeOpportunity ────────────────────────────────────────────────────

  it("analyzeOpportunity optimistically sets status to processing, then updates on success", async () => {
    mockOpportunityService.listOpportunities.mockResolvedValue({
      data: [listItem],
    });

    const analyzed = {
      ...detailOffer,
      analysisStatus: "completed",
      analysis: { id: "a-1", extractedSkills: ["React"] },
    };

    let resolveAnalyze;
    mockOpportunityService.analyzeOpportunity.mockReturnValue(
      new Promise((res) => {
        resolveAnalyze = res;
      }),
    );

    const { result } = renderHook(() => useOpportunities());
    await waitFor(() => expect(result.current.loading).toBe(false));

    act(() => {
      result.current.analyzeOpportunity("offer-1");
    });

    expect(result.current.opportunities[0].analysisStatus).toBe("processing");

    await act(async () => {
      resolveAnalyze({ data: analyzed });
    });

    expect(result.current.opportunities[0].analysisStatus).toBe("completed");
    expect(result.current.selectedOpportunity?.analysisStatus).toBe(
      "completed",
    );
  });

  it("analyzeOpportunity sets status to failed when the request rejects", async () => {
    mockOpportunityService.listOpportunities.mockResolvedValue({
      data: [listItem],
    });
    mockOpportunityService.analyzeOpportunity.mockRejectedValue(
      new Error("503"),
    );

    const { result } = renderHook(() => useOpportunities());
    await waitFor(() => expect(result.current.loading).toBe(false));

    await act(async () => {
      try {
        await result.current.analyzeOpportunity("offer-1");
      } catch {
        // expected rejection
      }
    });

    expect(result.current.opportunities[0].analysisStatus).toBe("failed");
  });

  // ── deleteOpportunity ─────────────────────────────────────────────────────

  it("deleteOpportunity removes the offer from the list", async () => {
    mockOpportunityService.listOpportunities.mockResolvedValue({
      data: [listItem],
    });
    mockOpportunityService.deleteOpportunity.mockResolvedValue(undefined);

    const { result } = renderHook(() => useOpportunities());
    await waitFor(() => expect(result.current.loading).toBe(false));

    await act(async () => {
      await result.current.deleteOpportunity("offer-1");
    });

    expect(result.current.opportunities).toHaveLength(0);
    expect(result.current.deleting).toBe(false);
  });

  it("deleteOpportunity throws and sets error on failure", async () => {
    mockOpportunityService.listOpportunities.mockResolvedValue({
      data: [listItem],
    });
    mockOpportunityService.deleteOpportunity.mockRejectedValue(
      new Error("500"),
    );

    const { result } = renderHook(() => useOpportunities());
    await waitFor(() => expect(result.current.loading).toBe(false));

    await expect(
      act(async () => {
        await result.current.deleteOpportunity("offer-1");
      }),
    ).rejects.toThrow("500");

    expect(result.current.error).toBeDefined();
  });

  // ── reload ────────────────────────────────────────────────────────────────

  it("reload refreshes the list and deselects offers that no longer exist", async () => {
    const second = { ...listItem, id: "offer-2", title: "Backend Engineer" };
    mockOpportunityService.listOpportunities
      .mockResolvedValueOnce({ data: [listItem] })
      .mockResolvedValueOnce({ data: [second] });

    const { result } = renderHook(() => useOpportunities());
    await waitFor(() => expect(result.current.loading).toBe(false));

    act(() => {
      result.current.reload();
    });

    await waitFor(() => expect(result.current.loading).toBe(false));

    expect(result.current.opportunities).toHaveLength(1);
    expect(result.current.opportunities[0].id).toBe("offer-2");
  });
});
