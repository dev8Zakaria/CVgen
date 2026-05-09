from fastapi import FastAPI
from app.core.config import settings
from app.api.routes import health, analyze_job

# C'EST CETTE VARIABLE QUE DOCKER CHERCHE :
app = FastAPI(
    title=settings.PROJECT_NAME, 
    version=settings.VERSION
)

# Enregistrement des routes
app.include_router(health.router, tags=["Health"])
app.include_router(analyze_job.router, tags=["Job Analysis"])