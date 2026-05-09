import { Navigate, Route, Routes } from "react-router-dom";

import { DashboardPage } from "@/modules/dashboard/pages/DashboardPage";
import { ProtectedRoute } from "@/modules/auth/ProtectedRoute";
import { CvPreviewPage } from "@/modules/cv/pages/CvPreviewPage";
import { GenerateCvPage } from "@/modules/cv/pages/GenerateCvPage";
import { MyCvsPage } from "@/modules/cv/pages/MyCvsPage";
import { LandingPage } from "@/modules/landing/pages/LandingPage";
import { AddJobOfferPage } from "@/modules/opportunities/pages/AddJobOfferPage";
import { JobAnalysisPage } from "@/modules/opportunities/pages/JobAnalysisPage";
import { OpportunitiesPage } from "@/modules/opportunities/pages/OpportunitiesPage";
import { ProfilePage } from "@/modules/profile/pages/ProfilePage";
import { DashboardLayout } from "@/shared/layouts/DashboardLayout";
import { MainLayout } from "@/shared/layouts/MainLayout";
import { ROUTES } from "@/shared/constants/routes";

export function AppRouter() {
  return (
    <Routes>
      <Route element={<MainLayout />}>
        <Route path={ROUTES.landing} element={<LandingPage />} />
      </Route>

      <Route
        element={
          <ProtectedRoute>
            <DashboardLayout />
          </ProtectedRoute>
        }
      >
        <Route path={ROUTES.dashboard} element={<DashboardPage />} />
        <Route path={ROUTES.profile} element={<ProfilePage />} />
        <Route path={ROUTES.opportunities} element={<OpportunitiesPage />} />
        <Route path={ROUTES.addOpportunity} element={<AddJobOfferPage />} />
        <Route path={`${ROUTES.opportunities}/:offerId/analysis`} element={<JobAnalysisPage />} />
        <Route path={ROUTES.generateCv} element={<GenerateCvPage />} />
        <Route path={ROUTES.cvs} element={<MyCvsPage />} />
        <Route path={`${ROUTES.cvs}/:cvId`} element={<CvPreviewPage />} />
      </Route>

      <Route path="*" element={<Navigate to={ROUTES.landing} replace />} />
    </Routes>
  );
}
