import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { vi } from "vitest";

import { OpportunitiesPage } from "@/modules/opportunities/pages/OpportunitiesPage";

const mockUseOpportunities = vi.fn();

vi.mock("@/modules/opportunities/hooks/useOpportunities", () => ({
  useOpportunities: () => mockUseOpportunities(),
}));

describe("OpportunitiesPage", () => {
  it("only exposes raw input fields in the create form", () => {
    mockUseOpportunities.mockReturnValue({
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
      createOpportunity: vi.fn(),
      analyzeOpportunity: vi.fn(),
      updateOpportunity: vi.fn(),
      deleteOpportunity: vi.fn(),
    });

    render(<OpportunitiesPage />);

    expect(screen.getByRole("textbox", { name: /job title/i })).toBeInTheDocument();
    expect(screen.getByRole("textbox", { name: /company name/i })).toBeInTheDocument();
    expect(screen.getByRole("textbox", { name: /job description/i })).toBeInTheDocument();
    expect(screen.queryByRole("textbox", { name: /experience level/i })).not.toBeInTheDocument();
    expect(screen.queryByRole("textbox", { name: /location/i })).not.toBeInTheDocument();
    expect(screen.queryByRole("textbox", { name: /contract type/i })).not.toBeInTheDocument();
  });

  it("submits a create payload and shows read-only analysis for the selected offer", async () => {
    const user = userEvent.setup();
    const createOpportunity = vi.fn().mockResolvedValue({
      id: "offer-2",
      title: "AI Engineer",
      companyName: "OpenAI",
      description: "Build AI features.",
      analysisStatus: "pending",
      createdAt: "2026-05-05T10:00:00Z",
      updatedAt: "2026-05-05T10:00:00Z",
      analysis: null,
    });

    mockUseOpportunities.mockReturnValue({
      opportunities: [
        {
          id: "offer-1",
          title: "Frontend Engineer",
          companyName: "OpenAI",
          analysisStatus: "completed",
          createdAt: "2026-05-05T10:00:00Z",
          updatedAt: "2026-05-05T10:00:00Z",
        },
      ],
      loading: false,
      creating: false,
      analyzing: false,
      updating: false,
      deleting: false,
      detailLoading: false,
      selectedOpportunity: {
        id: "offer-1",
        title: "Frontend Engineer",
        companyName: "OpenAI",
        description: "Build product experiences around AI.",
        analysisStatus: "completed",
        createdAt: "2026-05-05T10:00:00Z",
        updatedAt: "2026-05-05T11:00:00Z",
        analysis: {
          id: "analysis-1",
          jobOfferId: "offer-1",
          extractedSkills: ["React", "TypeScript"],
          extractedKeywords: ["frontend", "design systems"],
          extractedResponsibilities: ["Ship UI features"],
          detectedExperienceLevel: "Mid-level",
          detectedLocation: "Remote",
          detectedContractType: "Full-time",
          detectedTechnologies: ["React", "Vite"],
          analysisSummary: "Frontend-focused role.",
          rawAnalysisJson: "{}",
          createdAt: "2026-05-05T12:00:00Z",
        },
      },
      error: null,
      reload: vi.fn(),
      loadOpportunityDetail: vi.fn(),
      createOpportunity,
      analyzeOpportunity: vi.fn(),
      updateOpportunity: vi.fn(),
      deleteOpportunity: vi.fn(),
    });

    render(<OpportunitiesPage />);

    await user.type(screen.getAllByRole("textbox", { name: /job title/i })[0], "AI Engineer");
    await user.type(screen.getAllByRole("textbox", { name: /company name/i })[0], "OpenAI");
    await user.type(screen.getAllByRole("textbox", { name: /job description/i })[0], "Build AI features.");
    await user.click(screen.getByRole("button", { name: /create job offer/i }));

    expect(createOpportunity).toHaveBeenCalledWith({
      title: "AI Engineer",
      companyName: "OpenAI",
      description: "Build AI features.",
    });

    expect(screen.getByText(/Read-only enrichment/i)).toBeInTheDocument();
    expect(screen.getByText("TypeScript")).toBeInTheDocument();
    expect(screen.getByText("Frontend-focused role.")).toBeInTheDocument();
    expect(screen.queryByDisplayValue("Mid-level")).not.toBeInTheDocument();
  });

  it("triggers AI analysis from the selected opportunity detail", async () => {
    const user = userEvent.setup();
    const analyzeOpportunity = vi.fn().mockResolvedValue({
      id: "offer-1",
      title: "Frontend Engineer",
      companyName: "OpenAI",
      description: "Build product experiences around AI.",
      analysisStatus: "completed",
      createdAt: "2026-05-05T10:00:00Z",
      updatedAt: "2026-05-05T11:30:00Z",
      analysis: {
        id: "analysis-1",
        jobOfferId: "offer-1",
        extractedSkills: ["React", "TypeScript"],
        extractedKeywords: ["frontend", "design systems"],
        extractedResponsibilities: [],
        detectedExperienceLevel: "",
        detectedLocation: "",
        detectedContractType: "",
        detectedTechnologies: [],
        analysisSummary: "Estimated match score: 85%",
        rawAnalysisJson: "{}",
        createdAt: "2026-05-05T12:00:00Z",
      },
    });

    mockUseOpportunities.mockReturnValue({
      opportunities: [
        {
          id: "offer-1",
          title: "Frontend Engineer",
          companyName: "OpenAI",
          analysisStatus: "pending",
          createdAt: "2026-05-05T10:00:00Z",
          updatedAt: "2026-05-05T10:00:00Z",
        },
      ],
      loading: false,
      creating: false,
      analyzing: false,
      updating: false,
      deleting: false,
      detailLoading: false,
      selectedOpportunity: {
        id: "offer-1",
        title: "Frontend Engineer",
        companyName: "OpenAI",
        description: "Build product experiences around AI.",
        analysisStatus: "pending",
        createdAt: "2026-05-05T10:00:00Z",
        updatedAt: "2026-05-05T10:00:00Z",
        analysis: null,
      },
      error: null,
      reload: vi.fn(),
      loadOpportunityDetail: vi.fn(),
      createOpportunity: vi.fn(),
      analyzeOpportunity,
      updateOpportunity: vi.fn(),
      deleteOpportunity: vi.fn(),
    });

    render(<OpportunitiesPage />);

    await user.click(screen.getByRole("button", { name: /run ai analysis/i }));

    expect(analyzeOpportunity).toHaveBeenCalledWith("offer-1");
    expect(screen.getByText(/AI analysis completed for "Frontend Engineer"/i)).toBeInTheDocument();
  });
});
