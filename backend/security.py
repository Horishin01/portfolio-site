from __future__ import annotations

import base64
import hashlib
import hmac
import secrets

from .config import AppConfig


SCRYPT_PREFIX = "scrypt"
DEFAULT_SCRYPT_N = 2**14
DEFAULT_SCRYPT_R = 8
DEFAULT_SCRYPT_P = 1
DEFAULT_KEY_LENGTH = 32
DEFAULT_SALT_LENGTH = 16


def constant_time_equals(left: str, right: str) -> bool:
    return hmac.compare_digest(left.encode("utf-8"), right.encode("utf-8"))


def hash_password(
    password: str,
    *,
    n: int = DEFAULT_SCRYPT_N,
    r: int = DEFAULT_SCRYPT_R,
    p: int = DEFAULT_SCRYPT_P,
    key_length: int = DEFAULT_KEY_LENGTH,
    salt_length: int = DEFAULT_SALT_LENGTH,
) -> str:
    salt = secrets.token_bytes(salt_length)
    derived_key = hashlib.scrypt(
        password.encode("utf-8"),
        salt=salt,
        n=n,
        r=r,
        p=p,
        dklen=key_length,
    )
    salt_b64 = base64.urlsafe_b64encode(salt).decode("ascii")
    key_b64 = base64.urlsafe_b64encode(derived_key).decode("ascii")

    return f"{SCRYPT_PREFIX}${n}${r}${p}${salt_b64}${key_b64}"


def verify_password(password: str, encoded_hash: str) -> bool:
    if not encoded_hash:
        return False

    try:
        algorithm, n_raw, r_raw, p_raw, salt_b64, key_b64 = encoded_hash.split("$", 5)
        if algorithm != SCRYPT_PREFIX:
            return False

        salt = base64.urlsafe_b64decode(salt_b64.encode("ascii"))
        expected_key = base64.urlsafe_b64decode(key_b64.encode("ascii"))
        derived_key = hashlib.scrypt(
            password.encode("utf-8"),
            salt=salt,
            n=int(n_raw),
            r=int(r_raw),
            p=int(p_raw),
            dklen=len(expected_key),
        )
    except (ValueError, TypeError, base64.binascii.Error):
        return False

    return hmac.compare_digest(derived_key, expected_key)


def verify_admin_credentials(app_config: AppConfig, login_id: str, password: str) -> bool:
    if not constant_time_equals(login_id, app_config.admin_id):
        return False

    if app_config.admin_password_hash:
        return verify_password(password, app_config.admin_password_hash)

    return constant_time_equals(password, app_config.admin_password)
