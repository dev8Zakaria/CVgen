import logging

from fastapi import APIRouter, Depends, HTTPException

from app.schemas.cv_schema import CvGenerationRequest, CvGenerationResponse
from app.services.cv_generation_service import (
    CvGenerationConfigurationError,
    CvGenerationProviderError,
    CvGenerationService,
)

router = APIRouter()
logger = logging.getLogger(__name__)


def get_cv_generation_service() -> CvGenerationService:
    try:
        return CvGenerationService()
    except CvGenerationConfigurationError as exc:
        raise HTTPException(status_code=503, detail=str(exc)) from exc


@router.post("/generate-cv", response_model=CvGenerationResponse)
async def generate_cv(
    request: CvGenerationRequest,
    service: CvGenerationService = Depends(get_cv_generation_service),
) -> CvGenerationResponse:
    try:
        return await service.generate(request)
    except CvGenerationConfigurationError as exc:
        logger.exception("CV generation configuration error.")
        raise HTTPException(status_code=503, detail=str(exc)) from exc
    except CvGenerationProviderError as exc:
        logger.exception("CV generation provider error.")
        raise HTTPException(status_code=502, detail=str(exc)) from exc
