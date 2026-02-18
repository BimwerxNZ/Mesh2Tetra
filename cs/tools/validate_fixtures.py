#!/usr/bin/env python3
"""Lightweight validator for JSON regression fixtures.

Usage:
  python cs/tools/validate_fixtures.py
"""
from __future__ import annotations

import json
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]
FIXTURES = ROOT / "GenMesh.Mesh2Tetra.Tests" / "Fixtures"

REQUIRED_TOP = {"name", "input", "expected", "options"}
REQUIRED_INPUT = {"vertices", "faces"}
REQUIRED_EXPECTED = {"tetraVolume", "volumeTolerance"}
REQUIRED_OPTIONS = {"checkInput", "checkSelfIntersections", "planeDistanceTolerance", "epsilon"}


def fail(path: Path, message: str) -> None:
    raise ValueError(f"{path.name}: {message}")


def validate_fixture(path: Path) -> None:
    data = json.loads(path.read_text())

    missing = REQUIRED_TOP - set(data.keys())
    if missing:
        fail(path, f"missing top-level keys: {sorted(missing)}")

    if not isinstance(data["name"], str) or not data["name"].strip():
        fail(path, "name must be a non-empty string")

    fi = data["input"]
    missing = REQUIRED_INPUT - set(fi.keys())
    if missing:
        fail(path, f"input missing keys: {sorted(missing)}")

    vertices = fi["vertices"]
    faces = fi["faces"]
    if not isinstance(vertices, list) or len(vertices) < 4:
        fail(path, "input.vertices must be an array with at least 4 entries")
    if not isinstance(faces, list) or len(faces) < 4:
        fail(path, "input.faces must be an array with at least 4 entries")

    for i, v in enumerate(vertices):
        if not (isinstance(v, list) and len(v) == 3 and all(isinstance(x, (int, float)) for x in v)):
            fail(path, f"vertex[{i}] must be [x,y,z] numeric")

    n_vertices = len(vertices)
    for i, f in enumerate(faces):
        if not (isinstance(f, list) and len(f) == 3 and all(isinstance(x, int) for x in f)):
            fail(path, f"face[{i}] must be [a,b,c] integer indices")
        if any(x < 0 or x >= n_vertices for x in f):
            fail(path, f"face[{i}] has out-of-range indices")

    expected = data["expected"]
    missing = REQUIRED_EXPECTED - set(expected.keys())
    if missing:
        fail(path, f"expected missing keys: {sorted(missing)}")

    if not isinstance(expected["tetraVolume"], (int, float)):
        fail(path, "expected.tetraVolume must be numeric")
    if not isinstance(expected["volumeTolerance"], (int, float)) or expected["volumeTolerance"] < 0:
        fail(path, "expected.volumeTolerance must be non-negative numeric")

    tetra_count = expected.get("tetraCount")
    exact = expected.get("exactTetrahedra")
    exception = expected.get("expectedExceptionContains")

    if tetra_count is not None and (not isinstance(tetra_count, int) or tetra_count < 0):
        fail(path, "expected.tetraCount must be null/omitted or a non-negative integer")

    if exact is not None:
        if not isinstance(exact, list):
            fail(path, "expected.exactTetrahedra must be an array when present")
        for i, tet in enumerate(exact):
            if not (isinstance(tet, list) and len(tet) == 4 and all(isinstance(x, int) for x in tet)):
                fail(path, f"expected.exactTetrahedra[{i}] must be [a,b,c,d] integer indices")
        if tetra_count is not None and tetra_count != len(exact):
            fail(path, "expected.tetraCount must equal expected.exactTetrahedra length when both are present")

    if exception is not None and (not isinstance(exception, str) or not exception.strip()):
        fail(path, "expected.expectedExceptionContains must be a non-empty string when present")

    options = data["options"]
    missing = REQUIRED_OPTIONS - set(options.keys())
    if missing:
        fail(path, f"options missing keys: {sorted(missing)}")

    for key in ("checkInput", "checkSelfIntersections"):
        if not isinstance(options[key], bool):
            fail(path, f"options.{key} must be boolean")

    for key in ("planeDistanceTolerance", "epsilon"):
        if not isinstance(options[key], (int, float)) or options[key] < 0:
            fail(path, f"options.{key} must be non-negative numeric")


def main() -> int:
    fixture_files = sorted(FIXTURES.glob("*.json"))
    if not fixture_files:
        print("No fixture files found.")
        return 1

    errors = []
    for path in fixture_files:
        try:
            validate_fixture(path)
        except Exception as exc:  # noqa: BLE001
            errors.append(str(exc))

    if errors:
        print("Fixture validation failed:")
        for e in errors:
            print(f" - {e}")
        return 2

    print(f"Validated {len(fixture_files)} fixtures successfully.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
