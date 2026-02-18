#!/usr/bin/env python3
"""Import a Matlab-exported mesh case into regression fixture format.

Expected input JSON (example):
{
  "name": "matlab_irregular_closed_shell_dense_01",
  "vertices": [[0,0,0], [1,0,0], [0,1,0], [0,0,1]],
  "faces": [[1,3,2], [1,2,4], [2,3,4], [1,4,3]],
  "tetrahedra": [[1,2,3,4]],
  "tetraVolume": 0.16666666666666666
}

- `faces` and `tetrahedra` may be 1-based (Matlab style) or 0-based.
- For `--mode failfast`, omit `tetrahedra` and set `expectedExceptionContains`.
"""
from __future__ import annotations

import argparse
import json
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]
FIXTURE_DIR = ROOT / "GenMesh.Mesh2Tetra.Tests" / "Fixtures"


def parse_args(argv: list[str]) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Import Matlab-exported case JSON into fixture JSON.")
    parser.add_argument("input", help="Path to Matlab-exported JSON")
    parser.add_argument("--out-name", help="Override output fixture name")
    parser.add_argument("--mode", choices=["volume", "deterministic", "count", "failfast"], default="volume")
    parser.add_argument("--volume-tolerance", type=float, default=1e-8)
    parser.add_argument("--plane-distance-tolerance", type=float, default=1e-10)
    parser.add_argument("--epsilon", type=float, default=1e-8)
    parser.add_argument("--auto-resolve-intersections", action="store_true", default=True)
    parser.add_argument("--no-auto-resolve-intersections", dest="auto_resolve_intersections", action="store_false")
    parser.add_argument("--fail-on-self-intersections", action="store_true", default=True)
    parser.add_argument("--allow-self-intersections", dest="fail_on_self_intersections", action="store_false")
    parser.add_argument("--expected-exception-contains", default="self-intersections")
    parser.add_argument("--force", action="store_true", help="Overwrite if target fixture exists")
    return parser.parse_args(argv)


def maybe_to_zero_based(items: list[list[int]], width: int, label: str) -> list[list[int]]:
    if not items:
        return []
    for i, row in enumerate(items):
        if not isinstance(row, list) or len(row) != width or not all(isinstance(x, int) for x in row):
            raise ValueError(f"{label}[{i}] must be a list of {width} integers")

    min_value = min(min(row) for row in items)
    if min_value >= 1:
        return [[x - 1 for x in row] for row in items]
    return items


def load_input(path: Path) -> dict:
    data = json.loads(path.read_text())
    if not isinstance(data, dict):
        raise ValueError("Input JSON must be an object")
    return data


def build_fixture(data: dict, args: argparse.Namespace) -> dict:
    name = args.out_name or str(data.get("name", "")).strip()
    if not name:
        raise ValueError("Fixture name missing: provide 'name' in input or --out-name")
    if any(c in name for c in "\\/ "):
        raise ValueError("Fixture name must not include slashes or spaces")

    vertices = data.get("vertices")
    faces = data.get("faces")

    if not isinstance(vertices, list) or len(vertices) < 4:
        raise ValueError("vertices must be an array with at least 4 entries")
    if not isinstance(faces, list) or len(faces) < 4:
        raise ValueError("faces must be an array with at least 4 entries")

    for i, v in enumerate(vertices):
        if not (isinstance(v, list) and len(v) == 3 and all(isinstance(x, (int, float)) for x in v)):
            raise ValueError(f"vertices[{i}] must be [x,y,z] numeric")

    faces_0 = maybe_to_zero_based(faces, 3, "faces")

    expected: dict[str, object] = {
        "tetraVolume": float(data.get("tetraVolume", 0.0)),
        "volumeTolerance": float(args.volume_tolerance),
    }

    if args.mode == "failfast":
        expected["tetraCount"] = 0
        expected["tetraVolume"] = 0.0
        expected["expectedExceptionContains"] = args.expected_exception_contains
    else:
        tetra_count = data.get("tetraCount")
        tetrahedra = data.get("tetrahedra")

        if args.mode in {"deterministic", "count"} and tetra_count is None and tetrahedra is None:
            raise ValueError("mode requires tetraCount or tetrahedra in input JSON")

        if tetrahedra is not None:
            tetrahedra_0 = maybe_to_zero_based(tetrahedra, 4, "tetrahedra")
            if args.mode == "deterministic":
                expected["exactTetrahedra"] = tetrahedra_0
                expected["tetraCount"] = len(tetrahedra_0)
            elif args.mode == "count" and tetra_count is None:
                expected["tetraCount"] = len(tetrahedra_0)

        if tetra_count is not None:
            if not isinstance(tetra_count, int) or tetra_count < 0:
                raise ValueError("tetraCount must be a non-negative integer")
            expected["tetraCount"] = tetra_count

        if "tetraVolume" not in data and args.mode != "failfast":
            raise ValueError("tetraVolume is required in input JSON for non-failfast modes")

    return {
        "name": name,
        "input": {
            "vertices": vertices,
            "faces": faces_0,
        },
        "expected": expected,
        "options": {
            "checkInput": True,
            "autoResolveIntersections": args.auto_resolve_intersections,
            "failOnSelfIntersections": args.fail_on_self_intersections,
            "verbose": False,
            "planeDistanceTolerance": args.plane_distance_tolerance,
            "epsilon": args.epsilon,
        },
    }


def main(argv: list[str]) -> int:
    args = parse_args(argv)
    src = Path(args.input)
    if not src.exists():
        print(f"Input file not found: {src}", file=sys.stderr)
        return 2

    try:
        data = load_input(src)
        fixture = build_fixture(data, args)
    except Exception as exc:  # noqa: BLE001
        print(f"Import failed: {exc}", file=sys.stderr)
        return 3

    out = FIXTURE_DIR / f"{fixture['name']}.json"
    if out.exists() and not args.force:
        print(f"Fixture exists: {out}. Use --force to overwrite.", file=sys.stderr)
        return 4

    out.write_text(json.dumps(fixture, indent=2) + "\n")
    print(f"Created fixture: {out.relative_to(ROOT)}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main(sys.argv[1:]))
