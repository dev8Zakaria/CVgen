import asyncio
import json
import logging
from dataclasses import dataclass
from typing import Protocol

from app.core.config import settings
from app.schemas.cv_schema import CvGenerationRequest, CvGenerationResponse

logger = logging.getLogger(__name__)


class CvGenerationConfigurationError(RuntimeError):
    pass


class CvGenerationProviderError(RuntimeError):
    pass


class CvGenerationProvider(Protocol):
    async def generate_cv(self, request: CvGenerationRequest) -> CvGenerationResponse:
        ...


class MockCvGenerationProvider:
    async def generate_cv(self, request: CvGenerationRequest) -> CvGenerationResponse:
        profile = request.profile
        opportunity = request.opportunity
        skills = list(
            dict.fromkeys(
                [*opportunity.extracted_skills, *opportunity.detected_technologies, *profile.skills]
            )
        )[:12]
        keywords = list(
            dict.fromkeys(
                [
                    *opportunity.extracted_keywords,
                    *opportunity.must_have_requirements,
                    *opportunity.extracted_responsibilities,
                ]
            )
        )[:14]
        focus = opportunity.cv_focus_points or [
            "Emphasize the strongest evidence that matches the target role."
        ]
        optimized_experiences = []
        for experience in profile.experiences[:3]:
            role = experience.get("position") or experience.get("role") or "role"
            company = experience.get("company") or "the organization"
            optimized_experiences.append(
                {
                    "id": experience.get("id", ""),
                    "bullets": [
                        f"Delivered backend and integration work relevant to {opportunity.title or 'the target role'} using {', '.join(skills[:3]) if skills else 'the required technical stack'}.",
                        f"Improved maintainability and delivery quality for {company} through testing, clear API contracts, and Docker-based workflows.",
                        f"Applied {role} experience to support APIs, data workflows, and service reliability expected in the posting.",
                    ],
                }
            )

        optimized_projects = []
        for project in profile.projects[:3]:
            optimized_projects.append(
                {
                    "name": project.get("name", ""),
                    "bullets": [
                        f"Built a role-relevant project demonstrating {', '.join(skills[:4]) if skills else 'the core requirements'} in a practical application.",
                        "Structured the implementation to show ownership of backend APIs, integration points, and maintainable delivery.",
                    ],
                }
            )

        return CvGenerationResponse(
            professional_summary=(
                f"{profile.headline or 'Candidate'} with practical experience aligned to "
                f"{opportunity.title or 'the target role'}, including {', '.join(skills[:4]) if skills else 'the role requirements'}. "
                "Combines implementation work, service reliability, and cross-functional delivery in a way that matches technical hiring expectations."
            ),
            highlighted_skills=skills,
            matching_keywords=keywords,
            tailored_experience_hints=focus,
            optimized_experiences=optimized_experiences,
            optimized_projects=optimized_projects,
        )


@dataclass
class GeminiCvGenerationProvider:
    api_key: str
    model: str
    timeout_seconds: int

    def __post_init__(self) -> None:
        try:
            from google import genai
        except ImportError as exc:
            raise CvGenerationConfigurationError(
                "Gemini support requires the google-genai package to be installed."
            ) from exc

        self._client = genai.Client(api_key=self.api_key)

    async def generate_cv(self, request: CvGenerationRequest) -> CvGenerationResponse:
        prompt = self._build_prompt(request)

        try:
            response = await asyncio.wait_for(
                asyncio.to_thread(self._generate_content, prompt),
                timeout=self.timeout_seconds,
            )
        except asyncio.TimeoutError as exc:
            raise CvGenerationProviderError("Gemini request timed out.") from exc
        except Exception as exc:
            raise CvGenerationProviderError(f"Gemini request failed: {exc}") from exc

        if not response.text:
            raise CvGenerationProviderError("Gemini returned an empty response.")

        try:
            payload = json.loads(response.text)
            return CvGenerationResponse.model_validate(payload)
        except (json.JSONDecodeError, ValueError) as exc:
            raise CvGenerationProviderError("Gemini returned invalid structured JSON.") from exc

    def _generate_content(self, prompt: str):
        from app.schemas.cv_schema import CvGenerationResponse

        return self._client.models.generate_content(
            model=self.model,
            contents=prompt,
            config={
                "response_mime_type": "application/json",
                "response_schema": CvGenerationResponse,
                "temperature": 0.45,
            },
        )

    @staticmethod
    def _build_prompt(request: CvGenerationRequest) -> str:
        return f"""
You are a senior technical recruiter and hiring manager with experience screening software engineering CVs.

Generate final CV content that would pass an ATS scan and still read well to a technical hiring manager.
Your goal is not to decorate the CV. Your goal is to select and rewrite the strongest truthful evidence for the target role.

Rules:
- Return only JSON matching the schema.
- Do not invent companies, degrees, or roles not present in the profile.
- Do not invent metrics, tools, certifications, employers, dates, or responsibilities that are not supported by the profile.
- You may rewrite weak profile wording into stronger professional language, but it must remain truthful.
- professional_summary must be 2 polished sentences max, written for the exact target role.
- highlighted_skills must include only ATS-relevant skills from the profile, opportunity analysis, or clear job description wording.
- matching_keywords must come from the opportunity analysis and job description.
- must_have_requirements should drive the summary and the first bullets when supported by the profile.
- nice_to_have_requirements may appear only when supported by profile/projects.
- candidate_risks should help you choose safer wording; never hide gaps by inventing experience.
- detected_experience_level should influence tone. Junior profiles should sound credible, not inflated.
- optimized_experiences must contain objects with:
  - id: the original experience id when available
  - bullets: 2-4 final CV bullets for that experience
- optimized_projects must contain objects with:
  - name: the original project name
  - bullets: 2-3 final CV bullets for that project
- Bullets must be final CV bullets, not advice. Never write "emphasize", "highlight", "mention", or "focus on".
- Start bullets with strong action verbs: Built, Developed, Integrated, Improved, Designed, Automated, Implemented, Tested, Deployed.
- Prefer concrete technical evidence: APIs, authentication, data models, testing, deployment, service boundaries, CI, Docker, PostgreSQL, React, .NET, Python.
- If the candidate lacks a requirement, do not fake it. Instead, strengthen adjacent truthful evidence.
- Keep bullets concise: ideally 14-24 words each.
- tailored_experience_hints may contain internal advice, but it will not be printed in the final CV.

Input JSON:
{request.model_dump_json(indent=2)}
""".strip()


def build_cv_generation_provider() -> CvGenerationProvider:
    if settings.AI_PROVIDER == "mock":
        return MockCvGenerationProvider()

    if settings.AI_PROVIDER == "gemini":
        if not settings.GEMINI_API_KEY:
            raise CvGenerationConfigurationError(
                "GEMINI_API_KEY is required when AI_PROVIDER is set to 'gemini'."
            )

        return GeminiCvGenerationProvider(
            api_key=settings.GEMINI_API_KEY,
            model=settings.AI_MODEL,
            timeout_seconds=settings.AI_REQUEST_TIMEOUT_SECONDS,
        )

    raise CvGenerationConfigurationError(f"Unsupported AI provider: {settings.AI_PROVIDER}")


class CvGenerationService:
    def __init__(self, provider: CvGenerationProvider | None = None) -> None:
        self._provider = provider or build_cv_generation_provider()

    async def generate(self, request: CvGenerationRequest) -> CvGenerationResponse:
        try:
            return await self._provider.generate_cv(request)
        except CvGenerationProviderError:
            logger.exception("Primary CV generation provider failed.")
            if settings.AI_FALLBACK_TO_MOCK:
                logger.warning("Falling back to deterministic mock CV generation provider.")
                return await MockCvGenerationProvider().generate_cv(request)

            raise
