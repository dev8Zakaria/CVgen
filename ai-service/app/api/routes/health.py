import time

from fastapi import APIRouter

from app.core.config import settings

router = APIRouter()

START_TIME = time.time()


@router.get("/health")
async def health_check():
    uptime_seconds = time.time() - START_TIME
    ai_provider_ready = settings.AI_PROVIDER == "mock" or bool(settings.GEMINI_API_KEY)

    return {
        "status": "online",
        "service": settings.PROJECT_NAME,
        "version": settings.VERSION,
        "environment": settings.ENVIRONMENT,
        "metrics": {
            "uptime_seconds": round(uptime_seconds, 2),
            "uptime_minutes": round(uptime_seconds / 60, 2),
        },
        "dependencies": {
            "ai_provider": {
                "name": settings.AI_PROVIDER,
                "model": settings.AI_MODEL,
                "configured": ai_provider_ready,
            }
        },
    }
