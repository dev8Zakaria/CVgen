#!/usr/bin/env bash

set -euo pipefail

# Chemin du dossier deploy/
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Chemin de la racine du projet
PROJECT_DIR="${PROJECT_DIR:-$(cd "$SCRIPT_DIR/.." && pwd)}"

# Nom du fichier Docker Compose
COMPOSE_FILE="${COMPOSE_FILE:-docker-compose.yml}"

# Nom du fichier d'environnement utilisé sur la VM
ENV_FILE="${ENV_FILE:-.env}"

# On se place toujours à la racine du projet
cd "$PROJECT_DIR"

log() {
  echo "[$(date '+%Y-%m-%d %H:%M:%S')] $1"
}

error() {
  echo "[ERROR] $1" >&2
  exit 1
}

require_command() {
  if ! command -v "$1" >/dev/null 2>&1; then
    error "Commande manquante: $1"
  fi
}

require_file() {
  if [ ! -f "$1" ]; then
    error "Fichier manquant: $1"
  fi
}

compose() {
  docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" "$@"
}

load_env() {
  if [ -f "$ENV_FILE" ]; then
    set -a
    source "$ENV_FILE"
    set +a
  fi
}