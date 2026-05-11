from app.schemas.job_schema import JobOfferRequest, JobOfferResponse


class JobAnalysisService:
    async def analyze(self, request: JobOfferRequest) -> JobOfferResponse:
        # Mock AI response until the Kimi integration replaces this implementation.
        return JobOfferResponse(
            analysis_summary="Backend-focused role emphasizing APIs, Docker, and distributed systems.",
            extracted_keywords=["FastAPI", "Microservices", "Docker", "Clean Architecture"],
            suggested_skills=["Architecture C#", "Python", "DevOps"],
            extracted_responsibilities=[
                "Design and maintain backend services",
                "Collaborate with cross-functional teams",
                "Improve performance and reliability",
            ],
            detected_technologies=["ASP.NET Core", "Docker", "PostgreSQL", "GitHub Actions"],
            detected_experience_level="mid-senior",
            detected_location="Remote",
            detected_contract_type="full-time",
            must_have_requirements=[
                "Strong C# and .NET experience",
                "Experience building APIs",
                "Comfort with Docker-based workflows",
            ],
            nice_to_have_requirements=[
                "Cloud deployment experience",
                "CI/CD familiarity",
                "Microservices exposure",
            ],
            cv_focus_points=[
                "Highlight backend API projects",
                "Emphasize Docker and deployment experience",
                "Show measurable impact on reliability or performance",
            ],
            candidate_risks=["Role may expect deeper cloud production experience"],
            match_score_estimation=85,
            confidence_score=0.88,
            reasoning_summary="The score is driven by alignment with backend engineering, API development, and containerized workflows.",
        )
