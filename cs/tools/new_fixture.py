#!/usr/bin/env python3
"""Scaffold a new Mesh2Tetra regression fixture JSON file.

Examples:
  python cs/tools/new_fixture.py matlab_irregular_closed_shell_dense_01 --mode volume
  python cs/tools/new_fixture.py matlab_intersections_fail_fast_01 --mode failfast
  python cs/tools/new_fixture.py unit_like_case --mode deterministic
"""

from __future__ import annotations

import argparse
import json
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]
FIXTURE_DIR = ROOT / "GenMesh.Mesh2Tetra.Tests" / "Fixtures"


def base_fixture(name: str) -> dict:
    return {
        "name": name,
        "input": {
            "vertices": [
                [0.0, 0.0, 0.0],
                [1.0, 0.0, 0.0],
                [0.0, 1.0, 0.0],
                [0.0, 0.0, 1.0],
            ],
            "faces": [
                [0, 2, 1],
                [0, 1, 3],
                [1, 2, 3],
                [0, 3, 2],
            ],
        },
        "expected": {
            "tetraVolume": 0.16666666666666666,
            "volumeTolerance": 1e-8,
        },
        "options": {
            "checkInput": True,
            "autoResolveIntersections": True,
            "failOnSelfIntersections": True,
            "verbose": False,
            "planeDistanceTolerance": 1e-10,
            "epsilon": 1e-8,
        },
    }


def apply_mode(fixture: dict, mode: str) -> None:
    expected = fixture["expected"]

    if mode == "deterministic":
        expected["tetraCount"] = 1
        expected["exactTetrahedra"] = [[0, 1, 2, 3]]
    elif mode == "volume":
        # Non-deterministic triangulation path: keep volume-only assertions.
        expected.pop("tetraCount", None)
        expected.pop("exactTetrahedra", None)
    elif mode == "failfast":
        expected["tetraCount"] = 0
        expected["tetraVolume"] = 0.0
        expected["expectedExceptionContains"] = "self-intersections"
        fixture["options"]["autoResolveIntersections"] = False
        fixture["options"]["failOnSelfIntersections"] = True
    else:
        raise ValueError(f"Unsupported mode: {mode}")


def parse_args(argv: list[str]) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Create a fixture JSON skeleton.")
    parser.add_argument("name", help="Fixture base name, without .json")
    parser.add_argument(
        "--mode",
        choices=["deterministic", "volume", "failfast"],
        default="volume",
        help="Assertion style to scaffold",
    )
    parser.add_argument(
        "--force",
        action="store_true",
        help="Overwrite existing fixture file",
    )
    return parser.parse_args(argv)


def main(argv: list[str]) -> int:
    args = parse_args(argv)

    if any(c in args.name for c in "\\/ "):
        print("Fixture name must not contain slashes or spaces.", file=sys.stderr)
        return 2

    path = FIXTURE_DIR / f"{args.name}.json"
    if path.exists() and not args.force:
        print(f"Fixture already exists: {path}. Use --force to overwrite.", file=sys.stderr)
        return 3

    fixture = base_fixture(args.name)
    apply_mode(fixture, args.mode)

    path.write_text(json.dumps(fixture, indent=2) + "\n")
    print(f"Created fixture: {path.relative_to(ROOT)}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main(sys.argv[1:]))
