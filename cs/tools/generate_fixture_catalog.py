#!/usr/bin/env python3
"""Generate a markdown catalog for Mesh2Tetra JSON fixtures."""
from __future__ import annotations

import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
FIXTURES_DIR = ROOT / "GenMesh.Mesh2Tetra.Tests" / "Fixtures"
OUTPUT = ROOT / "FixtureCatalog.md"


def classify_expected(expected: dict) -> str:
    if expected.get("expectedExceptionContains"):
        return "fail-fast"
    if expected.get("exactTetrahedra") is not None:
        return "deterministic"
    if expected.get("tetraCount") is not None:
        return "count+volume"
    return "volume-only"


def row_for_fixture(path: Path) -> tuple[str, str, int, int, str]:
    data = json.loads(path.read_text())
    name = data["name"]
    vertices = len(data["input"]["vertices"])
    faces = len(data["input"]["faces"])
    expected_type = classify_expected(data["expected"])
    return name, path.name, vertices, faces, expected_type


def main() -> int:
    rows = [row_for_fixture(path) for path in sorted(FIXTURES_DIR.glob("*.json"))]

    lines = [
        "# Fixture Catalog",
        "",
        "Auto-generated summary of regression fixtures.",
        "",
        "| Fixture | File | Vertices | Faces | Assertion mode |",
        "|---|---|---:|---:|---|",
    ]

    for name, file_name, vertices, faces, expected_type in rows:
        lines.append(f"| `{name}` | `{file_name}` | {vertices} | {faces} | {expected_type} |")

    lines.extend([
        "",
        f"Total fixtures: **{len(rows)}**.",
        "",
        "Regenerate with:",
        "",
        "```bash",
        "python tools/generate_fixture_catalog.py",
        "```",
        "",
    ])

    OUTPUT.write_text("\n".join(lines))
    print(f"Wrote {OUTPUT.relative_to(ROOT)}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
