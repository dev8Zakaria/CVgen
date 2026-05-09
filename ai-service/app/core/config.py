from pydantic_settings import BaseSettings

class Settings(BaseSettings):
    PROJECT_NAME: str = "AI CV Generator API"
    VERSION: str = "1.0.0"
    ENVIRONMENT: str = "Development"
    # OPENAI_API_KEY: str = "" # On la décommentera pour le vrai appel IA plus tard

    class Config:
        env_file = ".env"

settings = Settings()