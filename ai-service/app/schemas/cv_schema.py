from pydantic import BaseModel, Field


class CvProfileInput(BaseModel):
    full_name: str = ""
    email: str = ""
    phone: str = ""
    location: str = ""
    headline: str = ""
    summary: str = ""
    skills: list[str] = Field(default_factory=list)
    experiences: list[dict] = Field(default_factory=list)
    educations: list[dict] = Field(default_factory=list)
    projects: list[dict] = Field(default_factory=list)
    languages: list[dict] = Field(default_factory=list)
    certifications: list[dict] = Field(default_factory=list)


class CvOpportunityInput(BaseModel):
    id: str
    title: str = ""
    company_name: str = ""
    description: str = ""
    analysis_summary: str = ""
    extracted_skills: list[str] = Field(default_factory=list)
    extracted_keywords: list[str] = Field(default_factory=list)
    extracted_responsibilities: list[str] = Field(default_factory=list)
    detected_technologies: list[str] = Field(default_factory=list)
    detected_experience_level: str = ""
    detected_location: str = ""
    detected_contract_type: str = ""
    must_have_requirements: list[str] = Field(default_factory=list)
    nice_to_have_requirements: list[str] = Field(default_factory=list)
    cv_focus_points: list[str] = Field(default_factory=list)
    candidate_risks: list[str] = Field(default_factory=list)


class CvGenerationRequest(BaseModel):
    profile: CvProfileInput
    opportunity: CvOpportunityInput


class CvGenerationResponse(BaseModel):
    professional_summary: str
    highlighted_skills: list[str] = Field(default_factory=list)
    matching_keywords: list[str] = Field(default_factory=list)
    tailored_experience_hints: list[str] = Field(default_factory=list)
    optimized_experiences: list[dict] = Field(default_factory=list)
    optimized_projects: list[dict] = Field(default_factory=list)
