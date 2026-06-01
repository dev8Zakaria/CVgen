import asyncio
import json
from dataclasses import dataclass
from typing import Protocol

from app.core.config import settings
from app.schemas.job_schema import JobOfferRequest, JobOfferResponse


class JobAnalysisConfigurationError(RuntimeError):
    pass


class JobAnalysisProviderError(RuntimeError):
    pass


class JobAnalysisProvider(Protocol):
    async def analyze_job(self, request: JobOfferRequest) -> JobOfferResponse:
        ...


class MockJobAnalysisProvider:
    async def analyze_job(self, request: JobOfferRequest) -> JobOfferResponse:
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
            reasoning_summary="The analysis is driven by backend engineering, API development, and containerized workflows.",
        )


@dataclass
class GeminiJobAnalysisProvider:
    api_key: str
    model: str
    timeout_seconds: int

    def __post_init__(self) -> None:
        try:
            from google import genai
        except ImportError as exc:
            raise JobAnalysisConfigurationError(
                "Gemini support requires the google-genai package to be installed."
            ) from exc

        self._genai = genai
        self._client = genai.Client(api_key=self.api_key)

    async def analyze_job(self, request: JobOfferRequest) -> JobOfferResponse:
        prompt = self._build_prompt(request.job_description)

        try:
            response = await asyncio.wait_for(
                asyncio.to_thread(self._generate_content, prompt),
                timeout=self.timeout_seconds,
            )
        except asyncio.TimeoutError as exc:
            raise JobAnalysisProviderError("Gemini request timed out.") from exc
        except Exception as exc:
            raise JobAnalysisProviderError(f"Gemini request failed: {exc}") from exc

        if not response.text:
            raise JobAnalysisProviderError("Gemini returned an empty response.")

        try:
            payload = json.loads(response.text)
            return JobOfferResponse.model_validate(payload)
        except (json.JSONDecodeError, ValueError) as exc:
            raise JobAnalysisProviderError("Gemini returned invalid structured JSON.") from exc

    def _generate_content(self, prompt: str):
        return self._client.models.generate_content(
            model=self.model,
            contents=prompt,
            config={
                "response_mime_type": "application/json",
                "response_schema": JobOfferResponse,
                "temperature": 0.2,
            },
        )

    @staticmethod
    def _build_prompt(job_description: str) -> str:
        return f"""
You are a senior technical recruiter analyzing a software engineering job description for CV tailoring.

Analyze the provided job description and return only structured JSON that matches the requested schema.

Rules:
- Base the analysis only on the job description.
- If information is missing, use:
  - "unknown" for detected experience level or contract type.
  - "Unknown" for detected location.
  - [] for missing lists.
- Keep list items concise, specific, and useful for ATS matching.
- Separate hard requirements from soft signals.
- Put only truly mandatory or strongly implied screening criteria in must_have_requirements.
- Put optional, differentiating, or nice-to-have criteria in nice_to_have_requirements.
- extracted_keywords should include ATS/search terms a recruiter or ATS would likely use.
- detected_technologies should include concrete tools, languages, frameworks, platforms, and infrastructure keywords.
- extracted_responsibilities should describe what the hired person will actually do, not generic corporate wording.
- cv_focus_points should be concrete recruiter guidance for the CV generator. Focus on evidence to surface, not vague advice.
- candidate_risks should mention realistic fit risks implied by the posting, such as missing production experience, cloud depth, async messaging, ownership, or seniority expectations.
- reasoning_summary must be one short sentence.
- detected_experience_level must be one of: intern, junior, mid, mid-senior, senior, lead, unknown.
- detected_contract_type must be one of: full-time, part-time, internship, freelance, contract, unknown.
- Do not overfit. If a technology is only mentioned as optional, do not treat it as mandatory.
- Do not hallucinate salary, degree requirements, years of experience, or location if not present.

Job description:
{job_description.strip()}
""".strip()


def build_job_analysis_provider() -> JobAnalysisProvider:
    if settings.AI_PROVIDER == "mock":
        return MockJobAnalysisProvider()

    if settings.AI_PROVIDER == "gemini":
        if not settings.GEMINI_API_KEY:
            raise JobAnalysisConfigurationError(
                "GEMINI_API_KEY is required when AI_PROVIDER is set to 'gemini'."
            )

        return GeminiJobAnalysisProvider(
            api_key=settings.GEMINI_API_KEY,
            model=settings.AI_MODEL,
            timeout_seconds=settings.AI_REQUEST_TIMEOUT_SECONDS,
        )

    raise JobAnalysisConfigurationError(f"Unsupported AI provider: {settings.AI_PROVIDER}")


class JobAnalysisService:
    def __init__(self, provider: JobAnalysisProvider | None = None) -> None:
        self._provider = provider or build_job_analysis_provider()

    async def analyze(self, request: JobOfferRequest) -> JobOfferResponse:
        return await self._provider.analyze_job(request)
