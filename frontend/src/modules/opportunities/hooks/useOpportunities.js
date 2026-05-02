import { useEffect, useState } from "react";

import { opportunityService } from "@/modules/opportunities/services/opportunityService";

export function useOpportunities() {
  const [opportunities, setOpportunities] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  useEffect(() => {
    setLoading(true);
    opportunityService
      .listOpportunities()
      .then((response) => setOpportunities(response.data))
      .catch(setError)
      .finally(() => setLoading(false));
  }, []);

  return { opportunities, loading, error };
}
