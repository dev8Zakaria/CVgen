import { createContext, useContext, useEffect, useState } from "react";

import { useAuth } from "@/modules/auth/AuthProvider";
import { cvService } from "@/modules/cv/services/cvService";
import { opportunityService } from "@/modules/opportunities/services/opportunityService";
import { profileService } from "@/modules/profile/services/profileService";

const PrototypeAppContext = createContext(null);

const PROFILE_EXTRAS_KEY = "cvgen.profile.extras";
const CV_LIBRARY_KEY = "cvgen.cv.library";
const OFFER_META_KEY = "cvgen.offer.meta";

function createEmptyExtras() {
  return {
    education: [],
    experience: [],
    skills: [],
    projects: [],
    languages: [],
    certifications: [],
  };
}

function getStorageKey(baseKey, userKey) {
  return `${baseKey}.${userKey}`;
}

function readStorage(baseKey, userKey, fallback) {
  try {
    const raw = window.localStorage.getItem(getStorageKey(baseKey, userKey));
    if (!raw) {
      return fallback;
    }

    const parsed = JSON.parse(raw);
    return parsed && typeof parsed === "object" ? parsed : fallback;
  } catch {
    return fallback;
  }
}

function writeStorage(baseKey, userKey, value) {
  window.localStorage.setItem(getStorageKey(baseKey, userKey), JSON.stringify(value));
}

function createAnalysisFromBackend(opportunity, profile) {
  const analysis = opportunity.analysis;
  const matchedSkills = profile.skills.filter((skill) =>
    [...(analysis?.extractedSkills || []), ...(analysis?.detectedTechnologies || [])].some(
      (item) => item.toLowerCase().includes(skill.toLowerCase()) || skill.toLowerCase().includes(item.toLowerCase()),
    ),
  );

  const missingSkills = (analysis?.extractedSkills || []).filter((skill) => !matchedSkills.includes(skill)).slice(0, 4);
  const matchScoreMatch = analysis?.analysisSummary?.match(/(\d+)%/);
  const matchScore = matchScoreMatch ? Number(matchScoreMatch[1]) : 0;

  return {
    extractedSkills: analysis?.extractedSkills || [],
    matchedSkills,
    missingSkills,
    keywords: analysis?.extractedKeywords || [],
    responsibilities: analysis?.extractedResponsibilities || [],
    technologies: analysis?.detectedTechnologies || [],
    insight:
      analysis?.analysisSummary ||
      "The backend analysis is available, but it did not return a text summary for this role yet.",
    matchScore,
  };
}

function mapBackendProfile(profileDto, authUser, extras) {
  return {
    id: profileDto?.id || authUser?.sub || "local-profile",
    fullName:
      profileDto?.fullName ||
      authUser?.name ||
      [authUser?.given_name, authUser?.family_name].filter(Boolean).join(" ") ||
      authUser?.preferred_username ||
      "",
    email: profileDto?.email || authUser?.email || "",
    phone: profileDto?.phone || "",
    address: profileDto?.location || "",
    professionalTitle: profileDto?.title || "",
    summary: profileDto?.summary || "",
    role: profileDto?.role || "",
    education: extras.education || [],
    experience: extras.experience || [],
    skills: extras.skills || [],
    projects: extras.projects || [],
    languages: extras.languages || [],
    certifications: extras.certifications || [],
  };
}

function mapBackendOpportunity(opportunity, meta, profile) {
  return {
    id: opportunity.id,
    jobTitle: opportunity.title,
    companyName: opportunity.companyName,
    location: meta?.location || "",
    description: opportunity.description || "",
    status: opportunity.analysisStatus === "completed" ? "Analyzed" : opportunity.analysisStatus || "Pending",
    createdAt: opportunity.createdAt,
    updatedAt: opportunity.updatedAt,
    analysis: opportunity.analysis ? createAnalysisFromBackend(opportunity, profile) : null,
    rawAnalysis: opportunity.analysis || null,
  };
}

function createLocalCvFromGeneratedResponse(response, offer, profile, existingCount) {
  return {
    id: crypto.randomUUID(),
    targetOfferId: offer.id,
    jobTitle: response.target.jobTitle,
    companyName: response.target.companyName,
    createdAt: response.generatedAt,
    template: existingCount % 2 === 0 ? "Atelier Ivory" : "Monograph Slate",
    status: "Saved",
    content: {
      header: {
        name: response.profile.fullName,
        title: response.profile.headline,
        email: response.profile.email,
        phone: response.profile.phone,
        address: response.profile.location,
      },
      summary: response.professionalSummary,
      experience: profile.experience.map((item) => ({
        ...item,
        bullets: Array.isArray(item.bullets) ? item.bullets.slice(0, 3) : [],
      })),
      skills: [...new Set([...(response.highlightedSkills || []), ...(response.matchingKeywords || []), ...(profile.skills || [])])].slice(0, 12),
      notes: response.tailoredExperienceHints || [],
      target: {
        role: response.target.jobTitle,
        company: response.target.companyName,
        matchScore: offer.analysis?.matchScore || 0,
      },
    },
  };
}

export function PrototypeAppProvider({ children }) {
  const { authEnabled, authenticated, initialized, user } = useAuth();
  const userKey = user?.sub || user?.email || "local";
  const [hydrated, setHydrated] = useState(false);
  const [profile, setProfile] = useState(() => mapBackendProfile(null, user, createEmptyExtras()));
  const [jobOffers, setJobOffers] = useState([]);
  const [cvs, setCvs] = useState([]);
  const [extras, setExtras] = useState(createEmptyExtras());
  const [offerMeta, setOfferMeta] = useState({});

  useEffect(() => {
    if (!initialized) {
      return;
    }

    let cancelled = false;

    async function hydrateState() {
      setHydrated(false);

      const storedExtras = readStorage(PROFILE_EXTRAS_KEY, userKey, createEmptyExtras());
      const storedMeta = readStorage(OFFER_META_KEY, userKey, {});
      const storedCvs = readStorage(CV_LIBRARY_KEY, userKey, []);

      if (!authEnabled || !authenticated) {
        if (!cancelled) {
          setExtras(storedExtras);
          setOfferMeta(storedMeta);
          setCvs(Array.isArray(storedCvs) ? storedCvs : []);
          setProfile(mapBackendProfile(null, user, storedExtras));
          setJobOffers([]);
          setHydrated(true);
        }
        return;
      }

      try {
        const profileResponse = await profileService.getCurrentProfile();
        const profileData = mapBackendProfile(profileResponse.data, user, storedExtras);

        const listResponse = await opportunityService.listOpportunities();
        const detailedOffers = await Promise.all(
          listResponse.data.map(async (offer) => {
            try {
              const detailResponse = await opportunityService.getOpportunity(offer.id);
              return detailResponse.data;
            } catch {
              return {
                ...offer,
                description: "",
                analysis: null,
              };
            }
          }),
        );

        const mappedOffers = detailedOffers.map((offer) => mapBackendOpportunity(offer, storedMeta[offer.id], profileData));

        if (!cancelled) {
          setExtras(storedExtras);
          setOfferMeta(storedMeta);
          setCvs(Array.isArray(storedCvs) ? storedCvs : []);
          setProfile(profileData);
          setJobOffers(mappedOffers);
          setHydrated(true);
        }
      } catch {
        if (!cancelled) {
          setExtras(storedExtras);
          setOfferMeta(storedMeta);
          setCvs(Array.isArray(storedCvs) ? storedCvs : []);
          setProfile(mapBackendProfile(null, user, storedExtras));
          setJobOffers([]);
          setHydrated(true);
        }
      }
    }

    hydrateState();

    return () => {
      cancelled = true;
    };
  }, [authEnabled, authenticated, initialized, user, userKey]);

  useEffect(() => {
    if (!initialized) {
      return;
    }

    writeStorage(PROFILE_EXTRAS_KEY, userKey, extras);
  }, [extras, initialized, userKey]);

  useEffect(() => {
    if (!initialized) {
      return;
    }

    writeStorage(OFFER_META_KEY, userKey, offerMeta);
  }, [initialized, offerMeta, userKey]);

  useEffect(() => {
    if (!initialized) {
      return;
    }

    writeStorage(CV_LIBRARY_KEY, userKey, cvs);
  }, [cvs, initialized, userKey]);

  const updateProfile = async (payload) => {
    const nextProfile = {
      ...profile,
      phone: payload.phone ?? profile.phone,
      address: payload.address ?? profile.address,
      professionalTitle: payload.professionalTitle ?? profile.professionalTitle,
      summary: payload.summary ?? profile.summary,
    };

    setProfile((currentProfile) => ({
      ...currentProfile,
      ...nextProfile,
    }));

    if (authEnabled && authenticated) {
      const response = await profileService.updateCurrentProfile({
        title: nextProfile.professionalTitle,
        summary: nextProfile.summary,
        phone: nextProfile.phone,
        location: nextProfile.address,
      });

    setProfile(() => mapBackendProfile(response.data, user, extras));
    }
  };

  const saveCollectionItem = (section, item) => {
    const currentList = extras[section] ?? [];
    const resolvedItem = { ...item, id: item.id || crypto.randomUUID() };
    const exists = currentList.some((entry) => entry.id === resolvedItem.id);
    const nextList = exists
      ? currentList.map((entry) => (entry.id === resolvedItem.id ? resolvedItem : entry))
      : [...currentList, resolvedItem];

    setExtras((current) => ({
      ...current,
      [section]: nextList,
    }));
    setProfile((current) => ({
      ...current,
      [section]: nextList,
    }));
  };

  const removeCollectionItem = (section, id) => {
    const nextList = (extras[section] ?? []).filter((entry) => entry.id !== id);

    setExtras((current) => ({
      ...current,
      [section]: nextList,
    }));
    setProfile((current) => ({
      ...current,
      [section]: nextList,
    }));
  };

  const addSkill = (skill) => {
    const trimmed = skill.trim();
    if (!trimmed) {
      return;
    }

    const nextSkills = [...new Set([...(extras.skills || []), trimmed])];
    setExtras((current) => ({
      ...current,
      skills: nextSkills,
    }));
    setProfile((current) => ({
      ...current,
      skills: nextSkills,
    }));
  };

  const removeSkill = (skill) => {
    const nextSkills = (extras.skills || []).filter((entry) => entry !== skill);
    setExtras((current) => ({
      ...current,
      skills: nextSkills,
    }));
    setProfile((current) => ({
      ...current,
      skills: nextSkills,
    }));
  };

  const createAnalyzedOffer = async (payload) => {
    if (!authEnabled || !authenticated) {
      throw new Error("Authenticated backend session required");
    }

    const createResponse = await opportunityService.createOpportunity({
      title: payload.jobTitle,
      companyName: payload.companyName,
      description: payload.description,
    });

    const createdId = createResponse.data.id;
    const nextMeta = {
      ...offerMeta,
      [createdId]: {
        location: payload.location,
      },
    };
    setOfferMeta(nextMeta);

    const analyzedResponse = await opportunityService.analyzeOpportunity(createdId);
    const mappedOffer = mapBackendOpportunity(analyzedResponse.data, nextMeta[createdId], profile);

    setJobOffers((current) => [mappedOffer, ...current.filter((offer) => offer.id !== createdId)]);
    return createdId;
  };

  const updateJobOffer = async (id, payload) => {
    if (!authEnabled || !authenticated) {
      throw new Error("Authenticated backend session required");
    }

    const response = await opportunityService.updateOpportunity(id, {
      title: payload.jobTitle ?? payload.title,
      companyName: payload.companyName,
      description: payload.description,
    });

    if (payload.location !== undefined) {
      setOfferMeta((current) => ({
        ...current,
        [id]: {
          ...(current[id] || {}),
          location: payload.location,
        },
      }));
    }

    const mappedOffer = mapBackendOpportunity(
      response.data,
      payload.location !== undefined ? { ...(offerMeta[id] || {}), location: payload.location } : offerMeta[id],
      profile,
    );

    setJobOffers((current) => current.map((offer) => (offer.id === id ? mappedOffer : offer)));
    return mappedOffer;
  };

  const deleteJobOffer = async (id) => {
    if (authEnabled && authenticated) {
      await opportunityService.deleteOpportunity(id);
    }

    setJobOffers((current) => current.filter((offer) => offer.id !== id));
    setCvs((current) => current.filter((cv) => cv.targetOfferId !== id));
    setOfferMeta((current) => {
      const next = { ...current };
      delete next[id];
      return next;
    });
  };

  const generateCv = async (offerId) => {
    const offer = jobOffers.find((entry) => entry.id === offerId);
    if (!offer) {
      return null;
    }

    if (!authEnabled || !authenticated) {
      throw new Error("Authenticated backend session required");
    }

    const response = await cvService.generateCv({ opportunityId: offerId });
    const localCv = createLocalCvFromGeneratedResponse(response.data, offer, profile, cvs.length);
    setCvs((current) => [localCv, ...current]);
    return localCv;
  };

  const updateCv = (id, payload) => {
    setCvs((current) => current.map((cv) => (cv.id === id ? { ...cv, ...payload } : cv)));
  };

  const deleteCv = (id) => {
    setCvs((current) => current.filter((cv) => cv.id !== id));
  };

  const value = {
    hydrated,
    profile,
    jobOffers,
    cvs,
    updateProfile,
    saveCollectionItem,
    removeCollectionItem,
    addSkill,
    removeSkill,
    createAnalyzedOffer,
    updateJobOffer,
    deleteJobOffer,
    generateCv,
    updateCv,
    deleteCv,
    usingBackendData: authEnabled && authenticated,
  };

  return <PrototypeAppContext.Provider value={value}>{children}</PrototypeAppContext.Provider>;
}

export function usePrototypeApp() {
  const context = useContext(PrototypeAppContext);
  if (!context) {
    throw new Error("usePrototypeApp must be used inside PrototypeAppProvider");
  }

  return context;
}
