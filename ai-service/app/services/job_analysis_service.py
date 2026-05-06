from app.schemas.job_schema import JobOfferRequest, JobOfferResponse

class JobAnalysisService:
    async def analyze(self, request: JobOfferRequest) -> JobOfferResponse:
        # Mock de l'IA pour valider l'intégration Service-to-Service de ce Sprint
        return JobOfferResponse(
            extracted_keywords=["FastAPI", "Microservices", "Docker", "Clean Architecture"],
            suggested_skills=["Architecture C#", "Python", "DevOps"],
            match_score_estimation=85
        )