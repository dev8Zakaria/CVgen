import { apiClient } from "@/shared/services/apiClient";

export const cvService = {
  listCvs: () => apiClient.get("/cvs"),
  getCv: (id) => apiClient.get(`/cvs/${id}`),
  generateCv: (payload) => {
    const formData = new FormData();
    formData.append("opportunityId", payload.opportunityId);

    if (payload.assetFile) {
      formData.append("assetFile", payload.assetFile);
    }

    return apiClient.post("/cv/generate", formData);
  },
  downloadCv: (id) => apiClient.get(`/cvs/${id}/download`, { responseType: "blob" }),
  deleteCv: (id) => apiClient.delete(`/cvs/${id}`),
};
