import { apiClient } from "@/shared/services/apiClient";

export const opportunityService = {
  listOpportunities: () => apiClient.get("/opportunity"),
  getOpportunity: (id) => apiClient.get(`/opportunity/${id}`),
  createOpportunity: (payload) => apiClient.post("/opportunity", payload),
  analyzeOpportunity: (id) => apiClient.post(`/opportunity/${id}/analyze`),
  updateOpportunity: (id, payload) => apiClient.put(`/opportunity/${id}`, payload),
  deleteOpportunity: (id) => apiClient.delete(`/opportunity/${id}`),
};
