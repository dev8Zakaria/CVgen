#!/usr/bin/env bash

set -euo pipefail

# Chemin du dossier deploy/
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Charger les fonctions communes
source "$SCRIPT_DIR/common.sh"

log "Début du déploiement..."

require_command docker
require_command git
require_file "$ENV_FILE"
require_file "$COMPOSE_FILE"

log "Vérification de Docker..."
docker info >/dev/null

log "Vérification de Docker Compose..."
docker compose version >/dev/null

if git rev-parse --is-inside-work-tree >/dev/null 2>&1; then
  CURRENT_BRANCH="$(git branch --show-current)"

  if [ -z "$CURRENT_BRANCH" ]; then
    error "Impossible de détecter la branche Git actuelle."
  fi

  if [ -n "$(git status --porcelain --untracked-files=no)" ]; then
    error "Il y a des modifications locales suivies par Git. Commit, stash ou annule-les avant de déployer."
  fi

  log "Branche actuelle: $CURRENT_BRANCH"
  log "Récupération de la dernière version du code..."

  git fetch origin "$CURRENT_BRANCH"
  git pull --ff-only origin "$CURRENT_BRANCH"
else
  log "Ce dossier n'est pas un dépôt Git. Étape git pull ignorée."
fi

log "Validation du fichier Docker Compose..."
compose config >/dev/null

log "Construction des images Docker..."
compose build --pull

log "Démarrage des services..."
compose up -d --remove-orphans

log "État des services:"
compose ps

if [ -x "$SCRIPT_DIR/healthcheck.sh" ]; then
  log "Lancement du health-check..."
  if "$SCRIPT_DIR/healthcheck.sh"; then
    log "Health-check réussi."
  else
    log "Attention: certains services ne répondent pas correctement. Vérifie les logs."
  fi
else
  log "healthcheck.sh n'est pas encore exécutable ou n'existe pas. Health-check ignoré."
fi

log "Déploiement terminé."