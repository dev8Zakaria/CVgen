import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { vi } from "vitest";

import { CvPreviewPage } from "@/modules/cv/pages/CvPreviewPage";
import { MyCvsPage } from "@/modules/cv/pages/MyCvsPage";
import { getCvPreviewRoute } from "@/shared/constants/routes";

const mockUsePrototypeApp = vi.fn();
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

const cvSummary = {
  id: "cv-1",
  targetOfferId: "offer-1",
  jobTitle: "Backend Engineer",
  companyName: "Acme",
  createdAt: "2026-06-01T00:00:00.000Z",
  template: "Atelier Ivory",
  status: "Saved",
  content: null,
};

const cvDetails = {
  ...cvSummary,
  content: {
    header: {
      name: "Zakaria",
      title: "Full Stack Developer",
      email: "zakaria@example.com",
      phone: "0600000000",
      address: "Casablanca",
    },
    summary: "Tailored backend CV summary.",
    experience: [
      {
        id: "exp-1",
        company: "Acme",
        role: "Developer",
        period: "2025 - Present",
        location: "Remote",
        bullets: ["Built APIs"],
      },
    ],
    skills: ["ASP.NET Core", "React"],
    notes: [],
    target: {
      role: "Backend Engineer",
      company: "Acme",
    },
  },
};

describe("CV pages", () => {
  beforeEach(() => {
    mockUsePrototypeApp.mockReset();
    mockToastSuccess.mockReset();
    mockToastError.mockReset();
  });

  it("downloads a CV through the provider from the CV library", async () => {
    const user = userEvent.setup();
    const downloadCv = vi.fn().mockResolvedValue();

    mockUsePrototypeApp.mockReturnValue({
      hydrated: true,
      cvs: [cvSummary],
      deleteCv: vi.fn(),
      downloadCv,
    });

    render(
      <MemoryRouter>
        <MyCvsPage />
      </MemoryRouter>,
    );

    await user.click(screen.getByRole("button", { name: /download/i }));

    expect(downloadCv).toHaveBeenCalledWith("cv-1");
  });

  it("deletes a CV through the provider after confirmation", async () => {
    const user = userEvent.setup();
    const deleteCv = vi.fn().mockResolvedValue();

    mockUsePrototypeApp.mockReturnValue({
      hydrated: true,
      cvs: [cvSummary],
      deleteCv,
      downloadCv: vi.fn(),
    });

    render(
      <MemoryRouter>
        <MyCvsPage />
      </MemoryRouter>,
    );

    await user.click(screen.getByRole("button", { name: /delete/i }));
    await user.click(screen.getByRole("button", { name: /delete cv/i }));

    await waitFor(() => expect(deleteCv).toHaveBeenCalledWith("cv-1"));
    expect(mockToastSuccess).toHaveBeenCalledWith("CV deleted");
  });

  it("loads full CV details before showing the preview", async () => {
    const loadCv = vi.fn().mockResolvedValue(cvDetails);

    mockUsePrototypeApp.mockReturnValue({
      hydrated: true,
      cvs: [cvSummary],
      updateCv: vi.fn(),
      loadCv,
      downloadCv: vi.fn(),
    });

    render(
      <MemoryRouter initialEntries={[getCvPreviewRoute("cv-1")]}>
        <Routes>
          <Route path="/my-cvs/:cvId" element={<CvPreviewPage />} />
        </Routes>
      </MemoryRouter>,
    );

    expect(await screen.findAllByText("Tailored backend CV summary.")).toHaveLength(2);
    expect(loadCv).toHaveBeenCalledWith("cv-1");
  });

  it("downloads from the preview through the provider", async () => {
    const user = userEvent.setup();
    const downloadCv = vi.fn().mockResolvedValue();

    mockUsePrototypeApp.mockReturnValue({
      hydrated: true,
      cvs: [cvDetails],
      updateCv: vi.fn(),
      loadCv: vi.fn(),
      downloadCv,
    });

    render(
      <MemoryRouter initialEntries={[getCvPreviewRoute("cv-1")]}>
        <Routes>
          <Route path="/my-cvs/:cvId" element={<CvPreviewPage />} />
        </Routes>
      </MemoryRouter>,
    );

    await user.click(screen.getByRole("button", { name: /download/i }));

    expect(downloadCv).toHaveBeenCalledWith("cv-1");
  });
});
