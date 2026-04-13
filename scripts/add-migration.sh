#!/usr/bin/env bash
set -euo pipefail

usage() {
  cat <<'EOF'
Usage:
  bash scripts/add-migration.sh <MigrationName> [provider]

Examples:
  bash scripts/add-migration.sh AddProjectTags
  bash scripts/add-migration.sh AddProjectTags mysql
  bash scripts/add-migration.sh AddProjectTags sqlite

The default provider is mysql so new migration snapshots stay consistent.
EOF
}

if [[ $# -lt 1 || $# -gt 2 ]]; then
  usage >&2
  exit 1
fi

migration_name="$1"
provider="${2:-mysql}"

case "$provider" in
  mysql|sqlite)
    ;;
  *)
    echo "Unsupported provider: $provider" >&2
    usage >&2
    exit 1
    ;;
esac

dotnet ef migrations add "$migration_name" \
  --project PortfolioSite.csproj \
  --context PortfolioDbContext \
  -- --provider "$provider"
