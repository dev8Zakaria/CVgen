#!/usr/bin/env bash

set -euo pipefail

# Chemin du dossier deploy/
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Charger les fonctions communes
source "$SCRIPT_DIR/common.sh"

log "Préparation du health-check..."

require_command docker
require_command curl
require_file "$ENV_FILE"
require_file "$COMPOSE_FILE"

load_env

log "Vérification de Docker..."
docker info >/dev/null

log "Validation du fichier Docker Compose..."
compose config >/dev/null

FAILED=0
CURL_TIMEOUT="${CURL_TIMEOUT:-5}"

check_running_services() {
  log "Vérification des services Docker Compose..."

  RUNNING_SERVICES="$(compose ps --services --filter "status=running" || true)"

  while IFS= read -r SERVICE; do
    if [ -z "$SERVICE" ]; then
      continue
    fi

    if echo "$RUNNING_SERVICES" | grep -qx "$SERVICE"; then
      echo "OK: $SERVICE est en cours d'exécution."
    else
      echo "FAIL: $SERVICE n'est pas en cours d'exécution."
      FAILED=1
    fi
  done < <(compose config --services)
}

check_url() {
  local NAME="$1"
  local URL="${2:-}"

  if [ -z "$URL" ]; then
    echo "SKIP: $NAME aucune URL configurée."
    return
  fi

  echo "Vérification $NAME -> $URL"

  if curl -fsSL --max-time "$CURL_TIMEOUT" "$URL" >/dev/null 2>&1; then
    echo "OK: $NAME répond correctement."
  else
    echo "FAIL: $NAME ne répond pas correctement."
    FAILED=1
  fi
}

check_running_services

FRONTEND_HEALTH_URL="${FRONTEND_HEALTH_URL:- http://localhost:5173}"
BACKEND_HEALTH_URL="${BACKEND_HEALTH_URL:-http://localhost:5000/health}"
AI_HEALTH_URL="${AI_HEALTH_URL:-}"
KEYCLOAK_HEALTH_URL="${KEYCLOAK_HEALTH_URL:-http://localhost:8080}"

log "Vérification des URLs HTTP..."

check_url "Frontend" "$FRONTEND_HEALTH_URL"
check_url "Backend API" "$BACKEND_HEALTH_URL"
check_url "AI Service" "$AI_HEALTH_URL"
check_url "Keycloak" "$KEYCLOAK_HEALTH_URL"

if [ "$FAILED" -eq 0 ]; then
  log "Health-check réussi. Tous les services vérifiés sont OK."
else
  log "Health-check échoué. Un ou plusieurs services ont un problème."
fi

exit "$FAILED"