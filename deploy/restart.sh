#!/usr/bin/env bash

set -euo pipefail

# Chemin du dossier deploy/
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Charger les fonctions communes
source "$SCRIPT_DIR/common.sh"

log "Préparation du redémarrage..."

require_command docker
require_file "$ENV_FILE"
require_file "$COMPOSE_FILE"

log "Vérification de Docker..."
docker info >/dev/null

log "Validation du fichier Docker Compose..."
compose config >/dev/null

# Service optionnel passé en argument
SERVICE="${1:-}"

if [ -z "$SERVICE" ]; then
  log "Redémarrage de tous les services..."
  compose restart
else
  if ! compose config --services | grep -qx "$SERVICE"; then
    log "Service inconnu: $SERVICE"
    log "Services disponibles:"
    compose config --services
    error "Le service '$SERVICE' n'existe pas dans $COMPOSE_FILE."
  fi

  log "Redémarrage du service: $SERVICE"
  compose restart "$SERVICE"
fi

log "État actuel des services:"
compose ps

log "Redémarrage terminé."