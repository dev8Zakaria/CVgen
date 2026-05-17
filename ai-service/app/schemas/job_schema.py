from typing import Literal

from pydantic import BaseModel, Field


class JobOfferRequest(BaseModel):
    job_description: str = Field(description="Raw job description text to analyze.")


class JobOfferResponse(BaseModel):
    analysis_summary: str = Field(description="Short summary of the role and its main technical emphasis.")
    extracted_keywords: list[str] = Field(default_factory=list, description="High-signal keywords extracted from the job description.")
    suggested_skills: list[str] = Field(default_factory=list, description="Resume-friendly skill labels suggested from the role requirements.")
    extracted_responsibilities: list[str] = Field(default_factory=list, description="Core responsibilities inferred from the role.")
    detected_technologies: list[str] = Field(default_factory=list, description="Frameworks, languages, tools, and platforms explicitly or implicitly required.")
    detected_experience_level: Literal["intern", "junior", "mid", "mid-senior", "senior", "lead", "unknown"] = Field(
        description="Best-fit experience level classification for the role.",
    )
    detected_location: str = Field(description="Detected work location or work mode such as Remote, Hybrid, On-site, or Unknown.")
    detected_contract_type: Literal["full-time", "part-time", "internship", "freelance", "contract", "unknown"] = Field(
        description="Detected contract type for the role.",
    )
    must_have_requirements: list[str] = Field(default_factory=list, description="Requirements that appear mandatory for the role.")
    nice_to_have_requirements: list[str] = Field(default_factory=list, description="Requirements that seem beneficial but not strictly mandatory.")
    cv_focus_points: list[str] = Field(default_factory=list, description="Concrete suggestions for what the user's CV should emphasize for this role.")
    candidate_risks: list[str] = Field(default_factory=list, description="Potential fit risks or likely gaps suggested by the job description.")
    reasoning_summary: str = Field(description="Short explanation of what most influenced the estimated score.")
