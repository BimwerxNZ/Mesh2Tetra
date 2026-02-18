#!/usr/bin/env python3
"""Run Mesh2Tetra regression quality gate checks.

Default checks:
1) Fixture schema lint
2) dotnet test

Examples:
  python tools/run_regression_gate.py
  python tools/run_regression_gate.py --skip-dotnet
"""

from __future__ import annotations

import argparse
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]


def run(cmd: list[str], cwd: Path) -> int:
    print(f"\n$ {' '.join(cmd)}")
    completed = subprocess.run(cmd, cwd=cwd)
    return completed.returncode


def parse_args(argv: list[str]) -> argparse.Namespace:
    p = argparse.ArgumentParser(description="Run regression gate checks for Mesh2Tetra.")
    p.add_argument("--skip-dotnet", action="store_true", help="Skip dotnet test execution")
    return p.parse_args(argv)


def main(argv: list[str]) -> int:
    args = parse_args(argv)

    rc = run([sys.executable, "tools/validate_fixtures.py"], ROOT)
    if rc != 0:
        return rc

    if not args.skip_dotnet:
        rc = run(["dotnet", "test", "GenMesh.Mesh2Tetra.sln"], ROOT)
        if rc != 0:
            return rc

    print("\nRegression gate passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main(sys.argv[1:]))
