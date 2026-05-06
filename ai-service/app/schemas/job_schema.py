from pydantic import BaseModel

class JobOfferRequest(BaseModel):
    job_description: str

class JobOfferResponse(BaseModel):
    extracted_keywords: list[str]
    suggested_skills: list[str]
    match_score_estimation: int