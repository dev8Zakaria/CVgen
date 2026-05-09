import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { vi } from "vitest";

import { OpportunitiesPage } from "@/modules/opportunities/pages/OpportunitiesPage";

// ─── mock hook ────────────────────────────────────────────────────────────────

const mockUseOpportunities = vi.fn();

vi.mock("@/modules/opportunities/hooks/useOpportunities", () => ({
  useOpportunities: () => mockUseOpportunities(),
}));

// ─── fixtures ─────────────────────────────────────────────────────────────────

const baseOffer = {
  id: "offer-1",
  title: "Frontend Engineer",
  companyName: "OpenAI",
  description: "Build product experiences around AI.",
  analysisStatus: "pending",
  createdAt: "2026-05-05T10:00:00Z",
  updatedAt: "2026-05-05T10:00:00Z",
};

const completedAnalysis = {
  id: "analysis-1",
  jobOfferId: "offer-1",
  extractedSkills: ["React", "TypeScript"],
  extractedKeywords: ["frontend", "design systems"],
  extractedResponsibilities: ["Ship UI features"],
  detectedExperienceLevel: "Mid-level",
  detectedLocation: "Remote",
  detectedContractType: "Full-time",
  detectedTechnologies: ["Vite"],
  analysisSummary: "Frontend-focused role.",
  rawAnalysisJson: "{}",
  createdAt: "2026-05-05T12:00:00Z",
};

function makeHook(overrides = {}) {
  return {
    opportunities: [],
    loading: false,
    creating: false,
    analyzing: false,
    updating: false,
    deleting: false,
    detailLoading: false,
    selectedOpportunity: null,
    error: null,
    reload: vi.fn(),
    loadOpportunityDetail: vi.fn(),
    createOpportunity: vi.fn().mockResolvedValue(baseOffer),
    analyzeOpportunity: vi.fn().mockResolvedValue({ ...baseOffer, analysisStatus: "completed", analysis: completedAnalysis }),
    updateOpportunity: vi.fn().mockResolvedValue(baseOffer),
    deleteOpportunity: vi.fn().mockResolvedValue(undefined),
    ...overrides,
  };
}

// ─── tests ────────────────────────────────────────────────────────────────────

describe("OpportunitiesPage", () => {

  // ── already-existing tests (kept for completeness) ────────────────────────

  it("only exposes raw input fields in the create form", () => {
    mockUseOpportunities.mockReturnValue(makeHook());

    render(<OpportunitiesPage />);

    expect(screen.getByRole("textbox", { name: /job title/i })).toBeInTheDocument();
    expect(screen.getByRole("textbox", { name: /company name/i })).toBeInTheDocument();
    expect(screen.getByRole("textbox", { name: /job description/i })).toBeInTheDocument();
    expect(screen.queryByRole("textbox", { name: /experience level/i })).not.toBeInTheDocument();
    expect(screen.queryByRole("textbox", { name: /contract type/i })).not.toBeInTheDocument();
  });

  it("submits the expected create payload", async () => {
    const user = userEvent.setup();
    const createOpportunity = vi.fn().mockResolvedValue(baseOffer);
    mockUseOpportunities.mockReturnValue(makeHook({ createOpportunity }));

    render(<OpportunitiesPage />);

    await user.type(screen.getByRole("textbox", { name: /job title/i }), "AI Engineer");
    await user.type(screen.getByRole("textbox", { name: /company name/i }), "Anthropic");
    await user.type(screen.getByRole("textbox", { name: /job description/i }), "Build safe AI.");
    await user.click(screen.getByRole("button", { name: /create job offer/i }));

    expect(createOpportunity).toHaveBeenCalledWith({
      title: "AI Engineer",
      companyName: "Anthropic",
      description: "Build safe AI.",
    });
  });

  it("shows a success feedback message after creating an offer", async () => {
    const user = userEvent.setup();
    mockUseOpportunities.mockReturnValue(makeHook());

    render(<OpportunitiesPage />);

    await user.type(screen.getByRole("textbox", { name: /job title/i }), "Frontend Engineer");
    await user.type(screen.getByRole("textbox", { name: /company name/i }), "OpenAI");
    await user.click(screen.getByRole("button", { name: /create job offer/i }));

    await waitFor(() =>
      expect(screen.getByText(/was created\. ai analysis is now pending/i)).toBeInTheDocument(),
    );
  });

  it("shows an error message when createOpportunity rejects", async () => {
    const user = userEvent.setup();
    const createOpportunity = vi.fn().mockRejectedValue(new Error("422"));
    mockUseOpportunities.mockReturnValue(makeHook({ createOpportunity }));

    render(<OpportunitiesPage />);

    await user.type(screen.getByRole("textbox", { name: /job title/i }), "Bad offer");
    await user.type(screen.getByRole("textbox", { name: /company name/i }), "Broken Co");
    await user.click(screen.getByRole("button", { name: /create job offer/i }));

    await waitFor(() =>
      expect(screen.getByText(/we could not create the job offer/i)).toBeInTheDocument(),
    );
  });

  // ── loading state ─────────────────────────────────────────────────────────

  it("shows a loading message in the list while opportunities are loading", () => {
    mockUseOpportunities.mockReturnValue(makeHook({ loading: true }));

    render(<OpportunitiesPage />);

    expect(screen.getByText(/loading your job offers/i)).toBeInTheDocument();
  });

  it("shows an empty state message when there are no saved offers", () => {
    mockUseOpportunities.mockReturnValue(makeHook({ opportunities: [] }));

    render(<OpportunitiesPage />);

    expect(screen.getByText(/no job offers saved yet/i)).toBeInTheDocument();
  });

  // ── detail selection ──────────────────────────────────────────────────────

  it("calls loadOpportunityDetail with the correct id when an offer is clicked", async () => {
    const user = userEvent.setup();
    const loadOpportunityDetail = vi.fn().mockResolvedValue(baseOffer);
    mockUseOpportunities.mockReturnValue(
      makeHook({ opportunities: [baseOffer], loadOpportunityDetail }),
    );

    render(<OpportunitiesPage />);

    await user.click(screen.getByText("Frontend Engineer"));

    expect(loadOpportunityDetail).toHaveBeenCalledWith("offer-1");
  });

  it("shows the detail panel for the selected opportunity", () => {
    mockUseOpportunities.mockReturnValue(
      makeHook({
        opportunities: [baseOffer],
        selectedOpportunity: {
          ...baseOffer,
          analysis: null,
        },
      }),
    );

    render(<OpportunitiesPage />);

    // description shows in the detail panel
  expect(screen.getByText("Build product experiences around AI.", { selector: "p" })).toBeInTheDocument();
    // edit form should be pre-filled
    expect(screen.getByDisplayValue("Frontend Engineer")).toBeInTheDocument();
    expect(screen.getByDisplayValue("OpenAI")).toBeInTheDocument();
  });

  it("shows a loading spinner inside the detail panel while detailLoading is true", () => {
    mockUseOpportunities.mockReturnValue(
      makeHook({ opportunities: [baseOffer], detailLoading: true }),
    );

    render(<OpportunitiesPage />);

    expect(screen.getByText(/loading full detail/i)).toBeInTheDocument();
  });

  it("shows an error message when loadOpportunityDetail rejects", async () => {
    const user = userEvent.setup();
    const loadOpportunityDetail = vi.fn().mockRejectedValue(new Error("404"));
    mockUseOpportunities.mockReturnValue(
      makeHook({ opportunities: [baseOffer], loadOpportunityDetail }),
    );

    render(<OpportunitiesPage />);

    await user.click(screen.getByText("Frontend Engineer"));

    await waitFor(() =>
      expect(screen.getByText(/we could not load this job offer detail/i)).toBeInTheDocument(),
    );
  });

  // ── pending-analysis state ────────────────────────────────────────────────

  it("shows the pending-analysis placeholder when selectedOpportunity has no analysis", () => {
    mockUseOpportunities.mockReturnValue(
      makeHook({
        opportunities: [baseOffer],
        selectedOpportunity: { ...baseOffer, analysis: null },
      }),
    );

    render(<OpportunitiesPage />);

    expect(screen.getByText(/ai analysis has not been completed yet/i)).toBeInTheDocument();
    expect(screen.queryByText(/read-only enrichment/i)).not.toBeInTheDocument();
  });

  it("shows 'Run AI analysis' button when analysis is null", () => {
    mockUseOpportunities.mockReturnValue(
      makeHook({
        opportunities: [baseOffer],
        selectedOpportunity: { ...baseOffer, analysis: null },
      }),
    );

    render(<OpportunitiesPage />);

    expect(screen.getByRole("button", { name: /run ai analysis/i })).toBeInTheDocument();
  });

  it("shows 'Re-run AI analysis' button when analysis already exists", () => {
    mockUseOpportunities.mockReturnValue(
      makeHook({
        opportunities: [baseOffer],
        selectedOpportunity: {
          ...baseOffer,
          analysisStatus: "completed",
          analysis: completedAnalysis,
        },
      }),
    );

    render(<OpportunitiesPage />);

    expect(screen.getByRole("button", { name: /re-run ai analysis/i })).toBeInTheDocument();
  });

  // ── AI analysis trigger ───────────────────────────────────────────────────

  it("calls analyzeOpportunity with the selected offer id", async () => {
    const user = userEvent.setup();
    const analyzeOpportunity = vi.fn().mockResolvedValue({
      ...baseOffer,
      analysisStatus: "completed",
      analysis: completedAnalysis,
    });

    mockUseOpportunities.mockReturnValue(
      makeHook({
        opportunities: [baseOffer],
        selectedOpportunity: { ...baseOffer, analysis: null },
        analyzeOpportunity,
      }),
    );

    render(<OpportunitiesPage />);

    await user.click(screen.getByRole("button", { name: /run ai analysis/i }));

    expect(analyzeOpportunity).toHaveBeenCalledWith("offer-1");
  });

  it("shows a success message after analysis completes", async () => {
    const user = userEvent.setup();
    mockUseOpportunities.mockReturnValue(
      makeHook({
        opportunities: [baseOffer],
        selectedOpportunity: { ...baseOffer, analysis: null },
      }),
    );

    render(<OpportunitiesPage />);

    await user.click(screen.getByRole("button", { name: /run ai analysis/i }));

    await waitFor(() =>
      expect(screen.getByText(/ai analysis completed for "Frontend Engineer"/i)).toBeInTheDocument(),
    );
  });

  it("shows an error message when analyzeOpportunity rejects", async () => {
    const user = userEvent.setup();
    const analyzeOpportunity = vi.fn().mockRejectedValue(new Error("503"));
    const loadOpportunityDetail = vi.fn().mockResolvedValue(baseOffer);

    mockUseOpportunities.mockReturnValue(
      makeHook({
        opportunities: [baseOffer],
        selectedOpportunity: { ...baseOffer, analysis: null },
        analyzeOpportunity,
        loadOpportunityDetail,
      }),
    );

    render(<OpportunitiesPage />);

    await user.click(screen.getByRole("button", { name: /run ai analysis/i }));

    await waitFor(() =>
      expect(screen.getByText(/we could not complete ai analysis/i)).toBeInTheDocument(),
    );
  });

  // ── analysis result display ───────────────────────────────────────────────

  it("renders extracted skills and summary from a completed analysis", () => {
    mockUseOpportunities.mockReturnValue(
      makeHook({
        opportunities: [baseOffer],
        selectedOpportunity: {
          ...baseOffer,
          analysisStatus: "completed",
          analysis: completedAnalysis,
        },
      }),
    );

    render(<OpportunitiesPage />);

    expect(screen.getByText(/read-only enrichment/i)).toBeInTheDocument();
    expect(screen.getByText("React")).toBeInTheDocument();
    expect(screen.getByText("TypeScript")).toBeInTheDocument();
    expect(screen.getByText("Frontend-focused role.")).toBeInTheDocument();
    expect(screen.getByText("Mid-level")).toBeInTheDocument();
    expect(screen.getByText("Remote")).toBeInTheDocument();
    expect(screen.getByText("Full-time")).toBeInTheDocument();
  });

  it("does NOT render editable inputs for analysis fields", () => {
    mockUseOpportunities.mockReturnValue(
      makeHook({
        opportunities: [baseOffer],
        selectedOpportunity: {
          ...baseOffer,
          analysisStatus: "completed",
          analysis: completedAnalysis,
        },
      }),
    );

    render(<OpportunitiesPage />);

    expect(screen.queryByDisplayValue("Mid-level")).not.toBeInTheDocument();
    expect(screen.queryByDisplayValue("Remote")).not.toBeInTheDocument();
  });

  // ── update flow ───────────────────────────────────────────────────────────

  it("calls updateOpportunity with trimmed field values on edit-form submit", async () => {
    const user = userEvent.setup();
    const updateOpportunity = vi.fn().mockResolvedValue({
      ...baseOffer,
      title: "Senior Frontend Engineer",
    });

    mockUseOpportunities.mockReturnValue(
      makeHook({
        opportunities: [baseOffer],
        selectedOpportunity: { ...baseOffer, analysis: null },
        updateOpportunity,
      }),
    );

    render(<OpportunitiesPage />);

    // The edit form uses a separate set of fields; grab by current value
    const titleInputs = screen.getAllByRole("textbox", { name: /job title/i });
    // second one is in the detail edit form
    const editTitleInput = titleInputs[1];
    await user.clear(editTitleInput);
    await user.type(editTitleInput, "  Senior Frontend Engineer  ");

    await user.click(screen.getByRole("button", { name: /save changes/i }));

    expect(updateOpportunity).toHaveBeenCalledWith(
      "offer-1",
      expect.objectContaining({ title: "Senior Frontend Engineer" }),
    );
  });

  it("shows a success message after updating an offer", async () => {
    const user = userEvent.setup();
    const updateOpportunity = vi.fn().mockResolvedValue({
      ...baseOffer,
      title: "Frontend Engineer",
    });

    mockUseOpportunities.mockReturnValue(
      makeHook({
        opportunities: [baseOffer],
        selectedOpportunity: { ...baseOffer, analysis: null },
        updateOpportunity,
      }),
    );

    render(<OpportunitiesPage />);

    await user.click(screen.getByRole("button", { name: /save changes/i }));

    await waitFor(() =>
      expect(screen.getByText(/was updated/i)).toBeInTheDocument(),
    );
  });

  it("shows an error message when updateOpportunity rejects", async () => {
    const user = userEvent.setup();
    const updateOpportunity = vi.fn().mockRejectedValue(new Error("500"));

    mockUseOpportunities.mockReturnValue(
      makeHook({
        opportunities: [baseOffer],
        selectedOpportunity: { ...baseOffer, analysis: null },
        updateOpportunity,
      }),
    );

    render(<OpportunitiesPage />);

    await user.click(screen.getByRole("button", { name: /save changes/i }));

    await waitFor(() =>
      expect(screen.getByText(/we could not save the job offer changes/i)).toBeInTheDocument(),
    );
  });

  it("resets the edit form fields when Reset fields is clicked", async () => {
    const user = userEvent.setup();

    mockUseOpportunities.mockReturnValue(
      makeHook({
        opportunities: [baseOffer],
        selectedOpportunity: { ...baseOffer, analysis: null },
      }),
    );

    render(<OpportunitiesPage />);

    const titleInputs = screen.getAllByRole("textbox", { name: /job title/i });
    const editTitleInput = titleInputs[1];
    await user.clear(editTitleInput);
    await user.type(editTitleInput, "Totally Different Title");

    await user.click(screen.getByRole("button", { name: /reset fields/i }));

    expect(screen.getByDisplayValue("Frontend Engineer")).toBeInTheDocument();
  });

  // ── delete flow ───────────────────────────────────────────────────────────

  it("calls deleteOpportunity with the offer id after confirming", async () => {
    const user = userEvent.setup();
    const deleteOpportunity = vi.fn().mockResolvedValue(undefined);
    vi.spyOn(window, "confirm").mockReturnValue(true);

    mockUseOpportunities.mockReturnValue(
      makeHook({
        opportunities: [baseOffer],
        selectedOpportunity: { ...baseOffer, analysis: null },
        deleteOpportunity,
      }),
    );

    render(<OpportunitiesPage />);

    await user.click(screen.getByRole("button", { name: /delete job offer/i }));

    expect(deleteOpportunity).toHaveBeenCalledWith("offer-1");
  });

  it("does NOT call deleteOpportunity when the user cancels the dialog", async () => {
    const user = userEvent.setup();
    const deleteOpportunity = vi.fn();
    vi.spyOn(window, "confirm").mockReturnValue(false);

    mockUseOpportunities.mockReturnValue(
      makeHook({
        opportunities: [baseOffer],
        selectedOpportunity: { ...baseOffer, analysis: null },
        deleteOpportunity,
      }),
    );

    render(<OpportunitiesPage />);

    await user.click(screen.getByRole("button", { name: /delete job offer/i }));

    expect(deleteOpportunity).not.toHaveBeenCalled();
  });

  it("shows a success message after deleting an offer", async () => {
    const user = userEvent.setup();
    vi.spyOn(window, "confirm").mockReturnValue(true);

    mockUseOpportunities.mockReturnValue(
      makeHook({
        opportunities: [baseOffer],
        selectedOpportunity: { ...baseOffer, analysis: null },
      }),
    );

    render(<OpportunitiesPage />);

    await user.click(screen.getByRole("button", { name: /delete job offer/i }));

    await waitFor(() =>
      expect(screen.getByText(/was deleted/i)).toBeInTheDocument(),
    );
  });

  it("shows an error message when deleteOpportunity rejects", async () => {
    const user = userEvent.setup();
    const deleteOpportunity = vi.fn().mockRejectedValue(new Error("500"));
    vi.spyOn(window, "confirm").mockReturnValue(true);

    mockUseOpportunities.mockReturnValue(
      makeHook({
        opportunities: [baseOffer],
        selectedOpportunity: { ...baseOffer, analysis: null },
        deleteOpportunity,
      }),
    );

    render(<OpportunitiesPage />);

    await user.click(screen.getByRole("button", { name: /delete job offer/i }));

    await waitFor(() =>
      expect(screen.getByText(/we could not delete this job offer/i)).toBeInTheDocument(),
    );
  });

  // ── disabled states ───────────────────────────────────────────────────────

  it("disables the Create button while creating is true", () => {
    mockUseOpportunities.mockReturnValue(makeHook({ creating: true }));

    render(<OpportunitiesPage />);

    expect(screen.getByRole("button", { name: /creating\.\.\./i })).toBeDisabled();
  });

  it("disables Save and Delete buttons in the detail panel while updating is true", () => {
    mockUseOpportunities.mockReturnValue(
      makeHook({
        opportunities: [baseOffer],
        selectedOpportunity: { ...baseOffer, analysis: null },
        updating: true,
      }),
    );

    render(<OpportunitiesPage />);

    expect(screen.getByRole("button", { name: /saving\.\.\./i })).toBeDisabled();
    expect(screen.getByRole("button", { name: /delete job offer/i })).toBeDisabled();
  });

  // ── global error banner ───────────────────────────────────────────────────

  it("shows the global error banner when error is set and no feedback message is shown", () => {
    mockUseOpportunities.mockReturnValue(
      makeHook({ error: new Error("Network error") }),
    );

    render(<OpportunitiesPage />);

    expect(screen.getByText(/the job opportunity request failed/i)).toBeInTheDocument();
  });
});