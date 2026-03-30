from __future__ import annotations

import json
import re
from datetime import datetime, timezone
from typing import Any

import mysql.connector

from .config import SEED_PATH, DatabaseConfig, load_database_config


IDENTIFIER_PATTERN = re.compile(r"^[A-Za-z_][A-Za-z0-9_]*$")


def utc_now_iso() -> str:
    return datetime.now(timezone.utc).replace(microsecond=0).isoformat()


def _quote_identifier(identifier: str) -> str:
    if not IDENTIFIER_PATTERN.fullmatch(identifier):
        raise ValueError(f"Invalid SQL identifier: {identifier}")

    return f"`{identifier}`"


def _connection_kwargs(
    config: DatabaseConfig,
    *,
    include_database: bool,
) -> dict[str, Any]:
    kwargs: dict[str, Any] = {
        "host": config.host,
        "port": config.port,
        "user": config.user,
        "password": config.password,
        "charset": config.charset,
        "connection_timeout": config.connect_timeout,
        "use_unicode": True,
    }

    if include_database:
        kwargs["database"] = config.name

    return kwargs


def _open_connection(
    config: DatabaseConfig,
    *,
    include_database: bool = True,
):
    return mysql.connector.connect(
        **_connection_kwargs(config, include_database=include_database)
    )


def _save_document_with_cursor(
    cursor: Any,
    table_name: str,
    payload: dict[str, Any],
    updated_at: str,
) -> None:
    cursor.execute(
        f"""
        INSERT INTO {table_name} (id, data, updated_at)
        VALUES (%s, %s, %s)
        ON DUPLICATE KEY UPDATE
          data = VALUES(data),
          updated_at = VALUES(updated_at)
        """,
        (1, json.dumps(payload, ensure_ascii=False), updated_at),
    )


def ensure_database() -> None:
    config = load_database_config()
    database_name = _quote_identifier(config.name)
    table_name = _quote_identifier(config.table_name)

    if config.auto_create_database:
        connection = _open_connection(config, include_database=False)
        try:
            cursor = connection.cursor()
            try:
                cursor.execute(
                    f"""
                    CREATE DATABASE IF NOT EXISTS {database_name}
                    CHARACTER SET utf8mb4
                    COLLATE utf8mb4_unicode_ci
                    """
                )
            finally:
                cursor.close()
            connection.commit()
        finally:
            connection.close()

    connection = _open_connection(config)
    try:
        cursor = connection.cursor()
        try:
            cursor.execute(
                f"""
                CREATE TABLE IF NOT EXISTS {table_name} (
                  id TINYINT UNSIGNED NOT NULL PRIMARY KEY,
                  data LONGTEXT NOT NULL,
                  updated_at VARCHAR(32) NOT NULL
                ) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci
                """
            )
            cursor.execute(f"SELECT 1 FROM {table_name} WHERE id = %s", (1,))
            row = cursor.fetchone()

            if not row and SEED_PATH.exists():
                seed_payload = json.loads(SEED_PATH.read_text(encoding="utf-8"))
                _save_document_with_cursor(
                    cursor,
                    table_name,
                    seed_payload,
                    utc_now_iso(),
                )
        finally:
            cursor.close()
        connection.commit()
    finally:
        connection.close()


def fetch_portfolio_document() -> dict[str, Any]:
    config = load_database_config()
    table_name = _quote_identifier(config.table_name)
    connection = _open_connection(config)

    try:
        cursor = connection.cursor()
        try:
            cursor.execute(
                f"SELECT data, updated_at FROM {table_name} WHERE id = %s",
                (1,),
            )
            row = cursor.fetchone()
        finally:
            cursor.close()
    finally:
        connection.close()

    if not row:
        return {"data": {}, "updatedAt": ""}

    raw_data, updated_at = row

    return {
        "data": json.loads(raw_data),
        "updatedAt": updated_at,
    }


def save_portfolio_document(payload: dict[str, Any]) -> dict[str, Any]:
    config = load_database_config()
    table_name = _quote_identifier(config.table_name)
    updated_at = utc_now_iso()
    connection = _open_connection(config)

    try:
        cursor = connection.cursor()
        try:
            _save_document_with_cursor(cursor, table_name, payload, updated_at)
        finally:
            cursor.close()
        connection.commit()
    finally:
        connection.close()

    return {"data": payload, "updatedAt": updated_at}


def clear_portfolio_document() -> None:
    config = load_database_config()
    table_name = _quote_identifier(config.table_name)
    connection = _open_connection(config)

    try:
        cursor = connection.cursor()
        try:
            cursor.execute(f"DELETE FROM {table_name} WHERE id = %s", (1,))
        finally:
            cursor.close()
        connection.commit()
    finally:
        connection.close()


def describe_database_target() -> str:
    config = load_database_config()

    return (
        f"MySQL {config.user}@{config.host}:{config.port}/"
        f"{config.name} ({config.table_name})"
    )
