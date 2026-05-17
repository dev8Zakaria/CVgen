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

function toDateInput(value) {
  if (!value) {
    return "";
  }

  if (typeof value === "string") {
    return value.slice(0, 10);
  }

  return "";
}

function formatDateRange(startDate, endDate) {
  const start = toDateInput(startDate);
  const end = toDateInput(endDate);

  if (!start && !end) {
    return "";
  }

  if (!end) {
    return `${start} - Present`;
  }

  return `${start} - ${end}`;
}

function toUtcIsoDate(value) {
  if (!value) {
    return null;
  }

  const trimmed = String(value).trim();
  if (!trimmed) {
    return null;
  }

  if (/^\d{4}-\d{2}-\d{2}$/.test(trimmed)) {
    return `${trimmed}T00:00:00.000Z`;
  }

  return trimmed;
}

function normalizeSkill(skill) {
  if (typeof skill === "string") {
    return {
      id: "",
      name: skill,
      level: "",
      category: "",
    };
  }

  return {
    id: skill?.id || "",
    name: skill?.name || "",
    level: skill?.level || "",
    category: skill?.category || "",
  };
}

function normalizeExperience(entry) {
  const bullets = Array.isArray(entry?.bullets)
    ? entry.bullets
    : String(entry?.description || "")
        .split(/\r?\n/)
        .map((bullet) => bullet.trim())
        .filter(Boolean);

  return {
    id: entry?.id || "",
    company: entry?.company || "",
    position: entry?.position || entry?.role || "",
    startDate: toDateInput(entry?.startDate),
    endDate: toDateInput(entry?.endDate),
    description: entry?.description || bullets.join("\n"),
    role: entry?.position || entry?.role || "",
    period: formatDateRange(entry?.startDate, entry?.endDate) || entry?.period || "",
    location: entry?.location || "",
    bullets,
  };
}

function normalizeEducation(entry) {
  return {
    id: entry?.id || "",
    school: entry?.school || "",
    degree: entry?.degree || "",
    field: entry?.field || entry?.location || "",
    startDate: toDateInput(entry?.startDate),
    endDate: toDateInput(entry?.endDate),
  };
}

function normalizeProject(entry) {
  return {
    id: entry?.id || "",
    name: entry?.name || "",
    description: entry?.description || "",
    technologies: entry?.technologies || entry?.role || "",
    url: entry?.url || "",
  };
}

function normalizeLanguage(entry) {
  return {
    id: entry?.id || "",
    name: entry?.name || "",
    level: entry?.level || "",
  };
}

function normalizeCertification(entry) {
  return {
    id: entry?.id || "",
    title: entry?.title || "",
    issuer: entry?.issuer || "",
    year: entry?.year || "",
  };
}

function mapLocalExtrasToProfileCollections(extras) {
  return {
    education: (extras?.education || []).map(normalizeEducation),
    experience: (extras?.experience || []).map(normalizeExperience),
    skills: (extras?.skills || []).map(normalizeSkill),
    projects: (extras?.projects || []).map(normalizeProject),
    languages: (extras?.languages || []).map(normalizeLanguage),
    certifications: (extras?.certifications || []).map(normalizeCertification),
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
  const profileSkillNames = profile.skills.map((skill) => skill.name).filter(Boolean);
  const matchedSkills = profileSkillNames.filter((skill) =>
    [...(analysis?.extractedSkills || []), ...(analysis?.detectedTechnologies || [])].some(
      (item) => item.toLowerCase().includes(skill.toLowerCase()) || skill.toLowerCase().includes(item.toLowerCase()),
    ),
  );

  const missingSkills = (analysis?.extractedSkills || []).filter((skill) => !matchedSkills.includes(skill)).slice(0, 4);
  const matchScore = analysis?.matchScoreEstimation || 0;

  return {
    extractedSkills: analysis?.extractedSkills || [],
    matchedSkills,
    missingSkills,
    keywords: analysis?.extractedKeywords || [],
    responsibilities: analysis?.extractedResponsibilities || [],
    technologies: analysis?.detectedTechnologies || [],
    detectedExperienceLevel: analysis?.detectedExperienceLevel || "",
    detectedLocation: analysis?.detectedLocation || "",
    detectedContractType: analysis?.detectedContractType || "",
    mustHaveRequirements: analysis?.mustHaveRequirements || [],
    niceToHaveRequirements: analysis?.niceToHaveRequirements || [],
    cvFocusPoints: analysis?.cvFocusPoints || [],
    candidateRisks: analysis?.candidateRisks || [],
    confidenceScore: analysis?.confidenceScore || 0,
    reasoningSummary: analysis?.reasoningSummary || "",
    insight:
      analysis?.analysisSummary ||
      "The backend analysis is available, but it did not return a text summary for this role yet.",
    matchScore,
  };
}

function mapBackendProfile(profileDto, authUser, extras) {
  const localCollections = mapLocalExtrasToProfileCollections(extras);

  if (!profileDto) {
    return {
      id: authUser?.sub || "local-profile",
      fullName:
        authUser?.name ||
        [authUser?.given_name, authUser?.family_name].filter(Boolean).join(" ") ||
        authUser?.preferred_username ||
        "",
      email: authUser?.email || "",
      phone: "",
      address: "",
      professionalTitle: "",
      summary: "",
      role: "",
      ...localCollections,
    };
  }

  return {
    id: profileDto.id || authUser?.sub || "local-profile",
    fullName:
      profileDto.fullName ||
      authUser?.name ||
      [authUser?.given_name, authUser?.family_name].filter(Boolean).join(" ") ||
      authUser?.preferred_username ||
      "",
    email: profileDto.email || authUser?.email || "",
    phone: profileDto.phone || "",
    address: profileDto.location || "",
    professionalTitle: profileDto.title || "",
    summary: profileDto.summary || "",
    role: profileDto.role || "",
    education: (profileDto.educations || []).map(normalizeEducation),
    experience: (profileDto.experiences || []).map(normalizeExperience),
    skills: (profileDto.skills || []).map(normalizeSkill),
    projects: (profileDto.projects || []).map(normalizeProject),
    languages: (profileDto.languages || []).map(normalizeLanguage),
    certifications: (profileDto.certifications || []).map(normalizeCertification),
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
        role: item.position || item.role || "",
        period: formatDateRange(item.startDate, item.endDate) || item.period || "",
        bullets: Array.isArray(item.bullets) && item.bullets.length > 0
          ? item.bullets.slice(0, 3)
          : String(item.description || "")
              .split(/\r?\n/)
              .map((bullet) => bullet.trim())
              .filter(Boolean)
              .slice(0, 3),
      })),
      skills: [...new Set([...(response.highlightedSkills || []), ...(response.matchingKeywords || []), ...profile.skills.map((skill) => skill.name)])].slice(0, 12),
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

  const persistLocalProfile = (nextProfile) => {
    const nextExtras = {
      education: nextProfile.education,
      experience: nextProfile.experience,
      skills: nextProfile.skills,
      projects: nextProfile.projects,
      languages: nextProfile.languages,
      certifications: nextProfile.certifications,
    };

    setExtras(nextExtras);
    setProfile(nextProfile);
  };

  const buildBackendProfilePayload = (profileState) => ({
    title: profileState.professionalTitle,
    summary: profileState.summary,
    phone: profileState.phone,
    location: profileState.address,
    experiences: profileState.experience.map((item) => ({
      id: item.id || null,
      company: item.company,
      position: item.position,
      startDate: toUtcIsoDate(item.startDate),
      endDate: toUtcIsoDate(item.endDate),
      description: item.description,
    })),
    educations: profileState.education.map((item) => ({
      id: item.id || null,
      school: item.school,
      degree: item.degree,
      field: item.field,
      startDate: toUtcIsoDate(item.startDate),
      endDate: toUtcIsoDate(item.endDate),
    })),
    projects: profileState.projects.map((item) => ({
      id: item.id || null,
      name: item.name,
      description: item.description,
      technologies: item.technologies,
      url: item.url,
    })),
    skills: profileState.skills.map((item) => ({
      id: item.id || null,
      name: item.name,
      level: item.level,
      category: item.category,
    })),
    languages: profileState.languages.map((item) => ({
      id: item.id || null,
      name: item.name,
      level: item.level,
    })),
    certifications: profileState.certifications.map((item) => ({
      id: item.id || null,
      title: item.title,
      issuer: item.issuer,
      year: item.year,
    })),
  });

  const saveProfileToBackend = async (nextProfile) => {
    const response = await profileService.updateCurrentProfile(buildBackendProfilePayload(nextProfile));
    const mappedProfile = mapBackendProfile(response.data, user, createEmptyExtras());
    setProfile(mappedProfile);
    return mappedProfile;
  };

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

    if (authEnabled && authenticated) {
      await saveProfileToBackend(nextProfile);
      return;
    }

    persistLocalProfile(nextProfile);
  };

  const saveCollectionItem = async (section, item) => {
    const currentList = profile[section] ?? [];
    const resolvedItem = { ...item, id: item.id || "" };
    const exists = currentList.some((entry) => entry.id === resolvedItem.id);
    const nextList = exists
      ? currentList.map((entry) => (entry.id === resolvedItem.id ? resolvedItem : entry))
      : [...currentList, resolvedItem];
    const nextProfile = {
      ...profile,
      [section]: nextList,
    };

    if (authEnabled && authenticated) {
      await saveProfileToBackend(nextProfile);
      return;
    }

    persistLocalProfile(nextProfile);
  };

  const removeCollectionItem = async (section, id) => {
    const nextList = (profile[section] ?? []).filter((entry) => entry.id !== id);
    const nextProfile = {
      ...profile,
      [section]: nextList,
    };

    if (authEnabled && authenticated) {
      await saveProfileToBackend(nextProfile);
      return;
    }

    persistLocalProfile(nextProfile);
  };

  const addSkill = async (skillName) => {
    const trimmed = skillName.trim();
    if (!trimmed) {
      return;
    }

    if (profile.skills.some((skill) => skill.name.toLowerCase() === trimmed.toLowerCase())) {
      return;
    }

    const nextProfile = {
      ...profile,
      skills: [...profile.skills, { id: "", name: trimmed, level: "", category: "" }],
    };

    if (authEnabled && authenticated) {
      await saveProfileToBackend(nextProfile);
      return;
    }

    persistLocalProfile(nextProfile);
  };

  const removeSkill = async (skillName) => {
    const nextProfile = {
      ...profile,
      skills: profile.skills.filter((entry) => entry.name !== skillName),
    };

    if (authEnabled && authenticated) {
      await saveProfileToBackend(nextProfile);
      return;
    }

    persistLocalProfile(nextProfile);
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
    const analyzedResponse = await opportunityService.analyzeOpportunity(createdId);
    const mappedOffer = mapBackendOpportunity(analyzedResponse.data, offerMeta[createdId], profile);

    setJobOffers((current) => [mappedOffer, ...current.filter((offer) => offer.id !== createdId)]);
    return createdId;
  };

  const analyzeJobOffer = async (id) => {
    if (!authEnabled || !authenticated) {
      throw new Error("Authenticated backend session required");
    }

    setJobOffers((current) =>
      current.map((offer) =>
        offer.id === id
          ? {
              ...offer,
              status: "processing",
            }
          : offer,
      ),
    );

    try {
      const response = await opportunityService.analyzeOpportunity(id);
      const mappedOffer = mapBackendOpportunity(response.data, offerMeta[id], profile);
      setJobOffers((current) => current.map((offer) => (offer.id === id ? mappedOffer : offer)));
      return mappedOffer;
    } catch (error) {
      setJobOffers((current) =>
        current.map((offer) =>
          offer.id === id
            ? {
                ...offer,
                status: "failed",
              }
            : offer,
        ),
      );
      throw error;
    }
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
    analyzeJobOffer,
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
