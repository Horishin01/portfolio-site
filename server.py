from __future__ import annotations

import json
import secrets
import threading
from datetime import datetime, timedelta, timezone
from http import HTTPStatus
from http.cookies import SimpleCookie
from http.server import SimpleHTTPRequestHandler, ThreadingHTTPServer
from typing import Any
from urllib.parse import urlparse

from backend.config import PROJECT_DIR, load_app_config
from backend.database import (
    clear_portfolio_document,
    describe_database_target,
    ensure_database,
    fetch_portfolio_document,
    save_portfolio_document,
)
from backend.security import verify_admin_credentials

APP_CONFIG = load_app_config()
PUBLIC_DIR = PROJECT_DIR / "public"

SESSION_COOKIE_NAME = "portfolio_admin_session"
SESSION_TTL_HOURS = 12


class SessionStore:
    def __init__(self) -> None:
        self._lock = threading.Lock()
        self._sessions: dict[str, datetime] = {}

    def create(self) -> tuple[str, datetime]:
        token = secrets.token_urlsafe(32)
        expires_at = datetime.now(timezone.utc) + timedelta(hours=SESSION_TTL_HOURS)

        with self._lock:
            self._sessions[token] = expires_at

        return token, expires_at

    def validate(self, token: str) -> bool:
        if not token:
            return False

        now = datetime.now(timezone.utc)

        with self._lock:
            self._cleanup_locked(now)
            expires_at = self._sessions.get(token)

            if not expires_at:
                return False

            if expires_at <= now:
                self._sessions.pop(token, None)
                return False

            return True

    def delete(self, token: str) -> None:
        if not token:
            return

        with self._lock:
            self._sessions.pop(token, None)

    def _cleanup_locked(self, now: datetime) -> None:
        expired = [token for token, expires_at in self._sessions.items() if expires_at <= now]

        for token in expired:
            self._sessions.pop(token, None)


SESSIONS = SessionStore()


class PortfolioRequestHandler(SimpleHTTPRequestHandler):
    server_version = "PortfolioServer/1.0"

    def __init__(self, *args: Any, **kwargs: Any) -> None:
        super().__init__(*args, directory=str(PUBLIC_DIR), **kwargs)

    def do_OPTIONS(self) -> None:
        self.send_response(HTTPStatus.NO_CONTENT)
        self._send_common_headers()
        self.end_headers()

    def do_GET(self) -> None:
        path = urlparse(self.path).path

        if path == "/api/portfolio":
            self._handle_get_public_portfolio()
            return

        if path == "/api/admin/portfolio":
            self._handle_get_admin_portfolio()
            return

        if path == "/api/admin/session":
            self._handle_get_admin_session()
            return

        super().do_GET()

    def do_POST(self) -> None:
        path = urlparse(self.path).path

        if path == "/api/admin/login":
            self._handle_admin_login()
            return

        if path == "/api/admin/logout":
            self._handle_admin_logout()
            return

        self._send_json({"message": "Not found."}, HTTPStatus.NOT_FOUND)

    def do_PUT(self) -> None:
        path = urlparse(self.path).path

        if path == "/api/admin/portfolio":
            self._handle_put_admin_portfolio()
            return

        self._send_json({"message": "Not found."}, HTTPStatus.NOT_FOUND)

    def do_DELETE(self) -> None:
        path = urlparse(self.path).path

        if path == "/api/admin/portfolio":
            self._handle_delete_admin_portfolio()
            return

        self._send_json({"message": "Not found."}, HTTPStatus.NOT_FOUND)

    def log_message(self, format: str, *args: Any) -> None:
        super().log_message(format, *args)

    def _handle_get_public_portfolio(self) -> None:
        self._send_json(fetch_portfolio_document())

    def _handle_get_admin_portfolio(self) -> None:
        if not self._require_admin_session():
            return

        self._send_json(fetch_portfolio_document())

    def _handle_get_admin_session(self) -> None:
        self._send_json({"authenticated": self._is_authenticated()})

    def _handle_admin_login(self) -> None:
        payload = self._read_json_body()

        if payload is None:
            return

        login_id = str(payload.get("id", "")).strip()
        password = str(payload.get("password", "")).strip()

        if not verify_admin_credentials(APP_CONFIG, login_id, password):
            self._send_json(
                {"authenticated": False, "message": "ID またはパスワードが違います。"},
                HTTPStatus.UNAUTHORIZED,
            )
            return

        token, expires_at = SESSIONS.create()
        cookie = SimpleCookie()
        cookie[SESSION_COOKIE_NAME] = token
        cookie[SESSION_COOKIE_NAME]["path"] = "/"
        cookie[SESSION_COOKIE_NAME]["httponly"] = True
        cookie[SESSION_COOKIE_NAME]["samesite"] = "Lax"
        if APP_CONFIG.cookie_secure:
            cookie[SESSION_COOKIE_NAME]["secure"] = True
        cookie[SESSION_COOKIE_NAME]["expires"] = expires_at.strftime(
            "%a, %d %b %Y %H:%M:%S GMT"
        )
        cookie[SESSION_COOKIE_NAME]["max-age"] = str(SESSION_TTL_HOURS * 3600)

        self._send_json(
            {
                "authenticated": True,
                "expiresAt": expires_at.isoformat(),
            },
            HTTPStatus.OK,
            {"Set-Cookie": cookie.output(header="").strip()},
        )

    def _handle_admin_logout(self) -> None:
        token = self._get_session_token()
        SESSIONS.delete(token)

        cookie = SimpleCookie()
        cookie[SESSION_COOKIE_NAME] = ""
        cookie[SESSION_COOKIE_NAME]["path"] = "/"
        cookie[SESSION_COOKIE_NAME]["httponly"] = True
        cookie[SESSION_COOKIE_NAME]["samesite"] = "Lax"
        if APP_CONFIG.cookie_secure:
            cookie[SESSION_COOKIE_NAME]["secure"] = True
        cookie[SESSION_COOKIE_NAME]["expires"] = "Thu, 01 Jan 1970 00:00:00 GMT"
        cookie[SESSION_COOKIE_NAME]["max-age"] = "0"

        self._send_json(
            {"authenticated": False},
            HTTPStatus.OK,
            {"Set-Cookie": cookie.output(header="").strip()},
        )

    def _handle_put_admin_portfolio(self) -> None:
        if not self._require_admin_session():
            return

        payload = self._read_json_body()

        if payload is None:
            return

        document = payload.get("data", payload)

        if not isinstance(document, dict):
            self._send_json(
                {"message": "Request body must be a JSON object."},
                HTTPStatus.BAD_REQUEST,
            )
            return

        self._send_json(save_portfolio_document(document), HTTPStatus.OK)

    def _handle_delete_admin_portfolio(self) -> None:
        if not self._require_admin_session():
            return

        clear_portfolio_document()
        self._send_json({"cleared": True}, HTTPStatus.OK)

    def _is_authenticated(self) -> bool:
        return SESSIONS.validate(self._get_session_token())

    def _require_admin_session(self) -> bool:
        if self._is_authenticated():
            return True

        self._send_json(
            {"authenticated": False, "message": "Authentication required."},
            HTTPStatus.UNAUTHORIZED,
        )
        return False

    def _get_session_token(self) -> str:
        raw_cookie = self.headers.get("Cookie", "")

        if not raw_cookie:
            return ""

        cookie = SimpleCookie()
        cookie.load(raw_cookie)
        morsel = cookie.get(SESSION_COOKIE_NAME)

        return morsel.value if morsel else ""

    def _read_json_body(self) -> dict[str, Any] | None:
        content_length = int(self.headers.get("Content-Length", "0") or "0")
        raw_body = self.rfile.read(content_length) if content_length > 0 else b""

        if not raw_body:
            return {}

        try:
            parsed = json.loads(raw_body.decode("utf-8"))
        except json.JSONDecodeError:
            self._send_json(
                {"message": "Request body must be valid JSON."},
                HTTPStatus.BAD_REQUEST,
            )
            return None

        if not isinstance(parsed, dict):
            self._send_json(
                {"message": "Request body must be a JSON object."},
                HTTPStatus.BAD_REQUEST,
            )
            return None

        return parsed

    def _send_json(
        self,
        payload: dict[str, Any],
        status: HTTPStatus = HTTPStatus.OK,
        extra_headers: dict[str, str] | None = None,
    ) -> None:
        body = json.dumps(payload, ensure_ascii=False).encode("utf-8")
        self.send_response(status)
        self._send_common_headers()
        for header_name, header_value in (extra_headers or {}).items():
            self.send_header(header_name, header_value)
        self.send_header("Content-Type", "application/json; charset=utf-8")
        self.send_header("Content-Length", str(len(body)))
        self.end_headers()
        self.wfile.write(body)

    def _send_common_headers(self) -> None:
        self.send_header("Cache-Control", "no-store")


def main() -> None:
    try:
        ensure_database()
    except Exception as exc:
        raise SystemExit(f"MySQL initialization failed: {exc}") from exc

    server = ThreadingHTTPServer(
        (APP_CONFIG.host, APP_CONFIG.port),
        PortfolioRequestHandler,
    )

    print(f"Portfolio server running on http://{APP_CONFIG.host}:{APP_CONFIG.port}")
    print(f"Database: {describe_database_target()}")
    server.serve_forever()


if __name__ == "__main__":
    main()
