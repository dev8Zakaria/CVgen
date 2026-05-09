import { useEffect, useState } from "react";

import { profileService } from "@/modules/profile/services/profileService";

export function useProfile() {
  const [profile, setProfile] = useState(null);
  const [loading, setLoading] = useState(false);
  const [updating, setUpdating] = useState(false);
  const [deleting, setDeleting] = useState(false);
  const [error, setError] = useState(null);

  const loadProfile = () => {
    setLoading(true);
    setError(null);

    profileService
      .getCurrentProfile()
      .then((response) => setProfile(response.data))
      .catch((requestError) => setError(requestError))
      .finally(() => setLoading(false));
  };

  const updateProfile = async (payload) => {
    setUpdating(true);
    setError(null);

    try {
      const response = await profileService.updateCurrentProfile(payload);
      setProfile(response.data);
      return response.data;
    } catch (requestError) {
      setError(requestError);
      throw requestError;
    } finally {
      setUpdating(false);
    }
  };

  const deleteProfile = async () => {
    setDeleting(true);
    setError(null);

    try {
      await profileService.deleteCurrentProfile();
      setProfile(null);
    } catch (requestError) {
      setError(requestError);
      throw requestError;
    } finally {
      setDeleting(false);
    }
  };

  useEffect(() => {
    loadProfile();
  }, []);

  return {
    profile,
    loading,
    updating,
    deleting,
    error,
    reload: loadProfile,
    updateProfile,
    deleteProfile,
  };
}
