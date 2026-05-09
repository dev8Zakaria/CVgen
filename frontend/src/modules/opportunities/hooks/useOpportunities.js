import { useEffect, useState } from "react";

import { opportunityService } from "@/modules/opportunities/services/opportunityService";

export function useOpportunities() {
  const [opportunities, setOpportunities] = useState([]);
  const [loading, setLoading] = useState(false);
  const [creating, setCreating] = useState(false);
  const [analyzing, setAnalyzing] = useState(false);
  const [updating, setUpdating] = useState(false);
  const [deleting, setDeleting] = useState(false);
  const [detailLoading, setDetailLoading] = useState(false);
  const [selectedOpportunity, setSelectedOpportunity] = useState(null);
  const [error, setError] = useState(null);

  const toListItem = (opportunity) => ({
    id: opportunity.id,
    title: opportunity.title,
    companyName: opportunity.companyName,
    analysisStatus: opportunity.analysisStatus,
    createdAt: opportunity.createdAt,
    updatedAt: opportunity.updatedAt,
  });

  const loadOpportunities = async () => {
    setLoading(true);
    setError(null);

    try {
      const response = await opportunityService.listOpportunities();
      setOpportunities(response.data);
      setSelectedOpportunity((current) => {
        if (!response.data.length) {
          return null;
        }

        if (!current) {
          return null;
        }

        const selectedStillExists = response.data.some((opportunity) => opportunity.id === current.id);
        return selectedStillExists ? current : null;
      });

      return response.data;
    } catch (requestError) {
      setError(requestError);
      throw requestError;
    } finally {
      setLoading(false);
    }
  };

  const loadOpportunityDetail = async (id) => {
    setDetailLoading(true);
    setError(null);

    try {
      const response = await opportunityService.getOpportunity(id);
      setSelectedOpportunity(response.data);
      return response.data;
    } catch (requestError) {
      setError(requestError);
      throw requestError;
    } finally {
      setDetailLoading(false);
    }
  };

  const createOpportunity = async (payload) => {
    setCreating(true);
    setError(null);

    try {
      const response = await opportunityService.createOpportunity(payload);
      setOpportunities((current) => [toListItem(response.data), ...current.filter((opportunity) => opportunity.id !== response.data.id)]);
      setSelectedOpportunity(response.data);
      return response.data;
    } catch (requestError) {
      setError(requestError);
      throw requestError;
    } finally {
      setCreating(false);
    }
  };

  const updateOpportunity = async (id, payload) => {
    setUpdating(true);
    setError(null);

    try {
      const response = await opportunityService.updateOpportunity(id, payload);
      setOpportunities((current) =>
        current.map((opportunity) => (opportunity.id === response.data.id ? toListItem(response.data) : opportunity)),
      );
      setSelectedOpportunity(response.data);
      return response.data;
    } catch (requestError) {
      setError(requestError);
      throw requestError;
    } finally {
      setUpdating(false);
    }
  };

  const analyzeOpportunity = async (id) => {
    setAnalyzing(true);
    setError(null);

    const processingTimestamp = new Date().toISOString();
    setOpportunities((current) =>
      current.map((opportunity) =>
        opportunity.id === id
          ? {
              ...opportunity,
              analysisStatus: "processing",
              updatedAt: processingTimestamp,
            }
          : opportunity,
      ),
    );
    setSelectedOpportunity((current) =>
      current?.id === id
        ? {
            ...current,
            analysisStatus: "processing",
            updatedAt: processingTimestamp,
          }
        : current,
    );

    try {
      const response = await opportunityService.analyzeOpportunity(id);
      setOpportunities((current) =>
        current.map((opportunity) => (opportunity.id === response.data.id ? toListItem(response.data) : opportunity)),
      );
      setSelectedOpportunity(response.data);
      return response.data;
    } catch (requestError) {
      setOpportunities((current) =>
        current.map((opportunity) =>
          opportunity.id === id
            ? {
                ...opportunity,
                analysisStatus: "failed",
                updatedAt: new Date().toISOString(),
              }
            : opportunity,
        ),
      );
      setSelectedOpportunity((current) =>
        current?.id === id
          ? {
              ...current,
              analysisStatus: "failed",
              updatedAt: new Date().toISOString(),
            }
          : current,
      );
      setError(requestError);
      throw requestError;
    } finally {
      setAnalyzing(false);
    }
  };

  const deleteOpportunity = async (id) => {
    setDeleting(true);
    setError(null);

    try {
      await opportunityService.deleteOpportunity(id);
      setOpportunities((current) => current.filter((opportunity) => opportunity.id !== id));
      setSelectedOpportunity((current) => (current?.id === id ? null : current));
    } catch (requestError) {
      setError(requestError);
      throw requestError;
    } finally {
      setDeleting(false);
    }
  };

  useEffect(() => {
    loadOpportunities().catch(() => {});
  }, []);

  return {
    opportunities,
    loading,
    creating,
    analyzing,
    updating,
    deleting,
    detailLoading,
    selectedOpportunity,
    error,
    reload: loadOpportunities,
    loadOpportunityDetail,
    createOpportunity,
    analyzeOpportunity,
    updateOpportunity,
    deleteOpportunity,
  };
}
