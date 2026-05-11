from pydantic import BaseModel

class JobOfferRequest(BaseModel):
    job_description: str

class JobOfferResponse(BaseModel):
    analysis_summary: str
    extracted_keywords: list[str]
    suggested_skills: list[str]
    extracted_responsibilities: list[str]
    detected_technologies: list[str]
    detected_experience_level: str
    detected_location: str
    detected_contract_type: str
    must_have_requirements: list[str]
    nice_to_have_requirements: list[str]
    cv_focus_points: list[str]
    candidate_risks: list[str]
    match_score_estimation: int
    confidence_score: float
    reasoning_summary: str
