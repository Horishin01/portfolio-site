from __future__ import annotations

import argparse
import getpass
import sys
from pathlib import Path

PROJECT_DIR = Path(__file__).resolve().parent.parent

if str(PROJECT_DIR) not in sys.path:
    sys.path.insert(0, str(PROJECT_DIR))

from backend.security import hash_password


def main() -> None:
    parser = argparse.ArgumentParser(
        description="Generate a scrypt hash for PORTFOLIO_ADMIN_PASSWORD_HASH."
    )
    parser.add_argument(
        "--password",
        help="Plain text password. Omit to enter it securely.",
    )
    args = parser.parse_args()

    password = args.password or getpass.getpass("Admin password: ")

    if not password:
        raise SystemExit("Password is required.")

    print(hash_password(password))


if __name__ == "__main__":
    main()
