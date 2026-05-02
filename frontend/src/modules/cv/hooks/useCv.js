import { useEffect, useState } from "react";

import { cvService } from "@/modules/cv/services/cvService";

export function useCv() {
  const [cvs, setCvs] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  useEffect(() => {
    setLoading(true);
    cvService
      .listCvs()
      .then((response) => setCvs(response.data))
      .catch(setError)
      .finally(() => setLoading(false));
  }, []);

  return { cvs, loading, error };
}
