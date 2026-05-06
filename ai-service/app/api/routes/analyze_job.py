from fastapi import APIRouter, Depends
from app.schemas.job_schema import JobOfferRequest, JobOfferResponse
from app.services.job_analysis_service import JobAnalysisService

router = APIRouter()

# Fonction d'injection de dépendance
def get_job_analysis_service():
    return JobAnalysisService()

@router.post("/analyze-job", response_model=JobOfferResponse)
async def analyze_job(
    request: JobOfferRequest,
    service: JobAnalysisService = Depends(get_job_analysis_service)
):
    return await service.analyze(request)