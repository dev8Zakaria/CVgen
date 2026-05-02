import { useState } from "react";

import { fileService } from "@/modules/files/services/fileService";

export function useFiles() {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  async function getFile(id) {
    setLoading(true);
    setError(null);

    try {
      const response = await fileService.getFile(id);
      return response.data;
    } catch (requestError) {
      setError(requestError);
      return null;
    } finally {
      setLoading(false);
    }
  }

  return { getFile, loading, error };
}
