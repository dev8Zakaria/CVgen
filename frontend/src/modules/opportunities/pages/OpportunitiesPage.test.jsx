import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { vi } from "vitest";

import { AddJobOfferPage } from "@/modules/opportunities/pages/AddJobOfferPage";
import { JobAnalysisPage } from "@/modules/opportunities/pages/JobAnalysisPage";
import { OpportunitiesPage } from "@/modules/opportunities/pages/OpportunitiesPage";
import { ROUTES, getJobAnalysisRoute } from "@/shared/constants/routes";

const mockUsePrototypeApp = vi.fn();
const mockNavigate = vi.fn();
const mockToastSuccess = vi.fn();
const mockToastError = vi.fn();

vi.mock("@/shared/providers/PrototypeAppProvider", () => ({
  usePrototypeApp: () => mockUsePrototypeApp(),
}));

vi.mock("@/shared/providers/ToastProvider", () => ({
  useToast: () => ({
    success: mockToastSuccess,
    error: mockToastError,
  }),
}));

vi.mock("react-router-dom", async () => {
  const actual = await vi.importActual("react-router-dom");
  return {
    ...actual,
    useNavigate: () => mockNavigate,
  };
});

function renderWithRouter(ui, initialEntries = [ROUTES.opportunities]) {
  return render(<MemoryRouter initialEntries={initialEntries}>{ui}</MemoryRouter>);
}

describe("opportunity pages", () => {
  beforeEach(() => {
    mockUsePrototypeApp.mockReset();
    mockNavigate.mockReset();
    mockToastSuccess.mockReset();
    mockToastError.mockReset();
  });

  it("shows the opportunities empty state when no offers exist", () => {
    mockUsePrototypeApp.mockReturnValue({
      hydrated: true,
      jobOffers: [],
      deleteJobOffer: vi.fn(),
    });

    renderWithRouter(<OpportunitiesPage />);

    expect(screen.getByText(/no job offers yet/i)).toBeInTheDocument();
    expect(screen.getByRole("link", { name: /add your first offer/i })).toHaveAttribute("href", ROUTES.addOpportunity);
  });

  it("submits the add-offer form through the provider and routes to the analysis page", async () => {
    const user = userEvent.setup();
    const createAnalyzedOffer = vi.fn().mockResolvedValue("offer-2");

    mockUsePrototypeApp.mockReturnValue({
      createAnalyzedOffer,
    });

    renderWithRouter(<AddJobOfferPage />, [ROUTES.addOpportunity]);

    await user.type(screen.getByRole("textbox", { name: /job title/i }), "AI Engineer");
    await user.type(screen.getByRole("textbox", { name: /company name/i }), "OpenAI");
    await user.type(screen.getByRole("textbox", { name: /location/i }), "Remote");
    await user.type(screen.getByRole("textbox", { name: /job description/i }), "Build AI features.");
    await user.click(screen.getByRole("button", { name: /analyze offer/i }));

    expect(createAnalyzedOffer).toHaveBeenCalledWith({
      jobTitle: "AI Engineer",
      companyName: "OpenAI",
      location: "Remote",
      description: "Build AI features.",
    });
    expect(mockToastSuccess).toHaveBeenCalledWith(
      "Offer analyzed",
      "The job description was added and processed into a ready-to-use analysis.",
    );
    expect(mockNavigate).toHaveBeenCalledWith(getJobAnalysisRoute("offer-2"));
  });

  it("shows read-only analysis details for the selected offer", () => {
    mockUsePrototypeApp.mockReturnValue({
      jobOffers: [
        {
          id: "offer-1",
          jobTitle: "Frontend Engineer",
          companyName: "OpenAI",
          analysis: {
            matchScore: 85,
            insight: "Frontend-focused role.",
            matchedSkills: ["React"],
            missingSkills: ["TypeScript"],
            extractedSkills: ["React", "TypeScript"],
            keywords: ["frontend", "design systems"],
            responsibilities: ["Ship UI features"],
            technologies: ["Vite"],
          },
        },
      ],
    });

    render(
      <MemoryRouter initialEntries={[getJobAnalysisRoute("offer-1")]}>
        <Routes>
          <Route path="/job-offers/:offerId/analysis" element={<JobAnalysisPage />} />
        </Routes>
      </MemoryRouter>,
    );

    expect(screen.getByText(/frontend engineer at openai/i)).toBeInTheDocument();
    expect(screen.getByText("85%")).toBeInTheDocument();
    expect(screen.getByText("Frontend-focused role.")).toBeInTheDocument();
    expect(screen.getAllByText("TypeScript")).toHaveLength(2);
    expect(screen.getByText("Ship UI features")).toBeInTheDocument();
    expect(screen.queryByRole("textbox")).not.toBeInTheDocument();
  });
});
