import { apiClient } from "@/shared/services/apiClient";

export const profileService = {
  getCurrentProfile: () => apiClient.get("/profile/me"),
  createProfile: (payload) => apiClient.post("/profile", payload),
  updateCurrentProfile: (payload) => apiClient.put("/profile/me", payload),
  deleteCurrentProfile: () => apiClient.delete("/profile/me"),
};
