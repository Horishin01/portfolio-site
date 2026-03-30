#!/usr/bin/env bash
set -euo pipefail

PROJECT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

sudo apt update
sudo apt install -y python3 python3-venv python3-pip default-mysql-client

python3 -m venv "$PROJECT_DIR/.venv"
"$PROJECT_DIR/.venv/bin/pip" install --upgrade pip
"$PROJECT_DIR/.venv/bin/pip" install -r "$PROJECT_DIR/requirements.txt"

if [ ! -f "$PROJECT_DIR/.env" ]; then
  cp "$PROJECT_DIR/.env.example" "$PROJECT_DIR/.env"
  echo ".env was created from .env.example"
else
  echo ".env already exists. Skipping copy."
fi

echo "Setup complete."
echo "1. Edit $PROJECT_DIR/.env"
echo "2. Prepare MySQL schema: mysql -u root -p < $PROJECT_DIR/sql/mysql/init.sql"
echo "3. Start server: $PROJECT_DIR/.venv/bin/python $PROJECT_DIR/server.py"
