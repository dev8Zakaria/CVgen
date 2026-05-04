import { useEffect, useState } from "react";

import { profileService } from "@/modules/profile/services/profileService";

export function useProfile() {
  const [profile, setProfile] = useState(null);
  const [loading, setLoading] = useState(false);
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

  useEffect(() => {
    loadProfile();
  }, []);

  return { profile, loading, error, reload: loadProfile };
}
