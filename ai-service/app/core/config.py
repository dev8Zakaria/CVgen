from typing import Literal

from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    PROJECT_NAME: str = "AI CV Generator API"
    VERSION: str = "1.0.0"
    ENVIRONMENT: str = "Development"

    AI_PROVIDER: Literal["gemini", "mock"] = "gemini"
    AI_MODEL: str = "gemini-3-flash-preview"
    GEMINI_API_KEY: str | None = None
    AI_REQUEST_TIMEOUT_SECONDS: int = 60
    AI_FALLBACK_TO_MOCK: bool = True

    model_config = SettingsConfigDict(env_file=".env", extra="ignore")


settings = Settings()
