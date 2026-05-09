export const ROUTES = {
  landing: "/",
  dashboard: "/dashboard",
  profile: "/profile",
  opportunities: "/job-offers",
  addOpportunity: "/job-offers/new",
  cvs: "/my-cvs",
  generateCv: "/generate-cv",
};

export function getJobAnalysisRoute(id) {
  return `${ROUTES.opportunities}/${id}/analysis`;
}

export function getCvPreviewRoute(id) {
  return `${ROUTES.cvs}/${id}`;
}
