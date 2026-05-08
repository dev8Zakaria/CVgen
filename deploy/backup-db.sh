#!/usr/bin/env bash

set -euo pipefail

# Chemin du dossier deploy/
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Charger les fonctions communes
source "$SCRIPT_DIR/common.sh"

log "Préparation du backup PostgreSQL..."

require_command docker
require_command gzip
require_file "$ENV_FILE"
require_file "$COMPOSE_FILE"

load_env

log "Vérification de Docker..."
docker info >/dev/null

log "Validation du fichier Docker Compose..."
compose config >/dev/null

# Variables à adapter selon votre docker-compose.yml et votre .env
POSTGRES_SERVICE="${POSTGRES_SERVICE:-postgres}"
POSTGRES_DB="${POSTGRES_DB:-aicv_db}"
POSTGRES_USER="${POSTGRES_USER:-aicv_user}"
POSTGRES_PASSWORD="${POSTGRES_PASSWORD:-aicv_password}"
BACKUP_DIR="${BACKUP_DIR:-$PROJECT_DIR/backups}"

# Vérifier que le service PostgreSQL existe dans docker-compose.yml
if ! compose config --services | grep -qx "$POSTGRES_SERVICE"; then
  log "Service PostgreSQL introuvable: $POSTGRES_SERVICE"
  log "Services disponibles:"
  compose config --services
  error "Adapte POSTGRES_SERVICE dans ton .env ou dans backup-db.sh."
fi

# Créer le dossier backups/ s'il n'existe pas
mkdir -p "$BACKUP_DIR"

# Nom du fichier backup
BACKUP_FILE="$BACKUP_DIR/${POSTGRES_DB}_$(date '+%Y%m%d_%H%M%S').sql.gz"

log "Lancement du backup..."
log "Service PostgreSQL: $POSTGRES_SERVICE"
log "Base de données: $POSTGRES_DB"
log "Utilisateur: $POSTGRES_USER"
log "Fichier de sortie: $BACKUP_FILE"

if [ -n "$POSTGRES_PASSWORD" ]; then
  compose exec -T "$POSTGRES_SERVICE" env PGPASSWORD="$POSTGRES_PASSWORD" \
    pg_dump -U "$POSTGRES_USER" -d "$POSTGRES_DB" | gzip > "$BACKUP_FILE"
else
  compose exec -T "$POSTGRES_SERVICE" \
    pg_dump -U "$POSTGRES_USER" -d "$POSTGRES_DB" | gzip > "$BACKUP_FILE"
fi

log "Backup terminé avec succès."
log "Fichier créé: $BACKUP_FILE"