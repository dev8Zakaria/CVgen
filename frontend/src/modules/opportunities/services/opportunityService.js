import { apiClient } from "@/shared/services/apiClient";

export const opportunityService = {
  listOpportunities: () => apiClient.get("/opportunities"),
  getOpportunity: (id) => apiClient.get(`/opportunities/${id}`),
  createOpportunity: (payload) => apiClient.post("/opportunities", payload),
  analyzeOpportunity: (id) => apiClient.post(`/opportunities/${id}/analyze`),
};
