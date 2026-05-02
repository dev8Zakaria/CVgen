import { apiClient } from "@/shared/services/apiClient";

export const cvService = {
  listCvs: () => apiClient.get("/cvs"),
  getCv: (id) => apiClient.get(`/cvs/${id}`),
  generateCv: (payload) => apiClient.post("/cvs/generate", payload),
  downloadCv: (id) => apiClient.get(`/cvs/${id}/download`, { responseType: "blob" }),
  deleteCv: (id) => apiClient.delete(`/cvs/${id}`),
};
