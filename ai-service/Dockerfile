FROM python:3.11-slim

WORKDIR /app

COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

# On copie tout le dossier racine dans le conteneur
COPY . /app/

EXPOSE 8000

# Attention au chemin d'exécution : on lance app.main:app
CMD ["uvicorn", "app.main:app", "--host", "0.0.0.0", "--port", "8000"]