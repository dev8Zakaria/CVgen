import { Navigate, Route, Routes } from "react-router-dom";

import { ProtectedRoute } from "@/modules/auth/ProtectedRoute";
import { CvPage } from "@/modules/cv/pages/CvPage";
import { LandingPage } from "@/modules/landing/pages/LandingPage";
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
        <Route path={ROUTES.profile} element={<ProfilePage />} />
        <Route path={ROUTES.opportunities} element={<OpportunitiesPage />} />
        <Route path={ROUTES.cv} element={<CvPage />} />
      </Route>

      <Route path="*" element={<Navigate to={ROUTES.landing} replace />} />
    </Routes>
  );
}
