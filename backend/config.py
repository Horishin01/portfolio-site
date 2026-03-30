from __future__ import annotations

import os
from dataclasses import dataclass
from pathlib import Path


PROJECT_DIR = Path(__file__).resolve().parent.parent
ENV_PATH = PROJECT_DIR / ".env"
SEED_PATH = PROJECT_DIR / "portfolio-seed.json"


@dataclass(frozen=True)
class AppConfig:
    host: str
    port: int
    admin_id: str
    admin_password: str
    admin_password_hash: str
    cookie_secure: bool


@dataclass(frozen=True)
class DatabaseConfig:
    host: str
    port: int
    name: str
    user: str
    password: str
    charset: str
    table_name: str
    connect_timeout: int
    auto_create_database: bool


def _load_env_file(path: Path) -> None:
    if not path.exists():
        return

    for raw_line in path.read_text(encoding="utf-8").splitlines():
        line = raw_line.strip()

        if not line or line.startswith("#") or "=" not in line:
            continue

        key, value = line.split("=", 1)
        parsed_key = key.strip()
        parsed_value = value.strip().strip('"').strip("'")

        if parsed_key:
            os.environ.setdefault(parsed_key, parsed_value)


def _get_int_env(name: str, default: int) -> int:
    raw_value = os.getenv(name)

    if raw_value is None or raw_value == "":
        return default

    return int(raw_value)


def _get_bool_env(name: str, default: bool) -> bool:
    raw_value = os.getenv(name)

    if raw_value is None or raw_value == "":
        return default

    return raw_value.strip().lower() in {"1", "true", "yes", "on"}


_load_env_file(ENV_PATH)


def load_app_config() -> AppConfig:
    return AppConfig(
        host=os.getenv("PORTFOLIO_HOST", "0.0.0.0"),
        port=_get_int_env("PORTFOLIO_PORT", 8000),
        admin_id=os.getenv("PORTFOLIO_ADMIN_ID", "admin"),
        admin_password=os.getenv("PORTFOLIO_ADMIN_PASSWORD", "0000"),
        admin_password_hash=os.getenv("PORTFOLIO_ADMIN_PASSWORD_HASH", ""),
        cookie_secure=_get_bool_env("PORTFOLIO_COOKIE_SECURE", False),
    )


def load_database_config() -> DatabaseConfig:
    return DatabaseConfig(
        host=os.getenv("PORTFOLIO_DB_HOST", "127.0.0.1"),
        port=_get_int_env("PORTFOLIO_DB_PORT", 3306),
        name=os.getenv("PORTFOLIO_DB_NAME", "portfolio_site"),
        user=os.getenv("PORTFOLIO_DB_USER", "portfolio_app"),
        password=os.getenv("PORTFOLIO_DB_PASSWORD", "change-me"),
        charset=os.getenv("PORTFOLIO_DB_CHARSET", "utf8mb4"),
        table_name=os.getenv("PORTFOLIO_DB_TABLE", "portfolio_documents"),
        connect_timeout=_get_int_env("PORTFOLIO_DB_CONNECT_TIMEOUT", 10),
        auto_create_database=_get_bool_env("PORTFOLIO_DB_AUTO_CREATE", True),
    )
