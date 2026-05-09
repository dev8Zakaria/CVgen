import time
import os
from fastapi import APIRouter
from app.core.config import settings

router = APIRouter()

# On enregistre l'heure exacte à laquelle le conteneur a démarré
START_TIME = time.time()

@router.get("/health")
async def health_check():
    # 1. Calculer depuis combien de temps le service tourne (Uptime)
    uptime_seconds = time.time() - START_TIME
    
    # 2. Vérifier si les dépendances critiques sont là (ex: La clé OpenAI)
    # Si la clé n'est pas là, on sait qu'on tourne en mode "Mock"
    ai_key = os.getenv("OPENAI_API_KEY")
    ai_status = "ready" if ai_key else "mock_mode (no api key)"

    # 3. Construire une vraie réponse d'état
    response = {
        "status": "online",
        "service": settings.PROJECT_NAME,
        "version": settings.VERSION,
        "environment": settings.ENVIRONMENT,
        "metrics": {
            "uptime_seconds": round(uptime_seconds, 2),
            "uptime_minutes": round(uptime_seconds / 60, 2)
        },
        "dependencies": {
            "ai_provider": ai_status
        }
    }
    
    return response