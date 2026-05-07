#!/usr/bin/env bash

set -euo pipefail

# Chemin du dossier deploy/
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Charger les fonctions communes
source "$SCRIPT_DIR/common.sh"

log "Préparation de l'affichage des logs..."

require_command docker
require_file "$ENV_FILE"
require_file "$COMPOSE_FILE"

log "Vérification de Docker..."
docker info >/dev/null

log "Validation du fichier Docker Compose..."
compose config >/dev/null

# Service optionnel passé en argument
SERVICE="${1:-}"

# Nombre de lignes affichées par défaut
LINES="${LINES:-150}"

# Suivre les logs en temps réel par défaut
FOLLOW="${FOLLOW:-true}"

LOG_OPTIONS=(--tail="$LINES")

if [ "$FOLLOW" = "true" ]; then
  LOG_OPTIONS=(-f "${LOG_OPTIONS[@]}")
fi

if [ -z "$SERVICE" ]; then
  log "Affichage des logs de tous les services..."
  compose logs "${LOG_OPTIONS[@]}"
else
  if ! compose config --services | grep -qx "$SERVICE"; then
    log "Service inconnu: $SERVICE"
    log "Services disponibles:"
    compose config --services
    error "Le service '$SERVICE' n'existe pas dans $COMPOSE_FILE."
  fi

  log "Affichage des logs du service: $SERVICE"
  compose logs "${LOG_OPTIONS[@]}" "$SERVICE"
fi