from fastapi import APIRouter, Depends, HTTPException

from app.schemas.job_schema import JobOfferRequest, JobOfferResponse
from app.services.job_analysis_service import (
    JobAnalysisConfigurationError,
    JobAnalysisProviderError,
    JobAnalysisService,
)

router = APIRouter()


def get_job_analysis_service() -> JobAnalysisService:
    try:
        return JobAnalysisService()
    except JobAnalysisConfigurationError as exc:
        raise HTTPException(status_code=503, detail=str(exc)) from exc


@router.post("/analyze-job", response_model=JobOfferResponse)
async def analyze_job(
    request: JobOfferRequest,
    service: JobAnalysisService = Depends(get_job_analysis_service),
) -> JobOfferResponse:
    try:
        return await service.analyze(request)
    except JobAnalysisConfigurationError as exc:
        raise HTTPException(status_code=503, detail=str(exc)) from exc
    except JobAnalysisProviderError as exc:
        raise HTTPException(status_code=502, detail=str(exc)) from exc
