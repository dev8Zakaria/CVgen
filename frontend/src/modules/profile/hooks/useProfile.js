import { useEffect, useState } from "react";

import { profileService } from "@/modules/profile/services/profileService";

export function useProfile() {
  const [profile, setProfile] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  useEffect(() => {
    setLoading(true);
    profileService
      .getCurrentProfile()
      .then((response) => setProfile(response.data))
      .catch(setError)
      .finally(() => setLoading(false));
  }, []);

  return { profile, loading, error };
}
