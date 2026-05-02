import { apiClient } from "@/shared/services/apiClient";

export const fileService = {
  getFile: (id) => apiClient.get(`/files/${id}`, { responseType: "blob" }),
};
