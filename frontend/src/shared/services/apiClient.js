import axios from "axios";

import { authService } from "@/modules/auth/authService";

export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
  headers: {
    "Content-Type": "application/json",
  },
});

apiClient.interceptors.request.use(async (config) => {
  let token = authService.getToken();

  if (token) {
    try {
      await authService.updateToken(30);
      token = authService.getToken();
    } catch {
      token = authService.getToken();
    }

    config.headers.Authorization = `Bearer ${token}`;
  }

  return config;
});
