#!/usr/bin/env python3
"""Report Batch 6 Matlab fixture intake progress.

Usage:
  python cs/tools/batch6_status.py
"""
from __future__ import annotations

from dataclasses import dataclass
from pathlib import Path
import json
import sys

ROOT = Path(__file__).resolve().parents[1]
FIXTURES_DIR = ROOT / "GenMesh.Mesh2Tetra.Tests" / "Fixtures"


@dataclass(frozen=True)
class QueueItem:
    name: str
    expected_mode: str


QUEUE: tuple[QueueItem, ...] = (
    QueueItem("matlab_orientation_mixed_winding_01", "volume-only"),
    QueueItem("matlab_duplicate_faces_cleanup_01", "volume-only"),
    QueueItem("matlab_nonmanifold_edge_fragment_01", "fail-fast"),
    QueueItem("matlab_component_bridge_near_touch_01", "volume-only"),
    QueueItem("matlab_intersection_policy_split_01", "count+volume"),
    QueueItem("matlab_high_aspect_ratio_cell_01", "volume-only"),
    QueueItem("matlab_small_volume_threshold_01", "deterministic"),
    QueueItem("matlab_multi_object_sparse_faces_01", "count+volume"),
    QueueItem("matlab_local_self_intersection_autoresolve_01", "volume-only"),
    QueueItem("matlab_seeded_randomized_case_02", "volume-only"),
)


def detect_mode(expected: dict) -> str:
    if expected.get("expectedExceptionContains"):
        return "fail-fast"
    if expected.get("exactTetrahedra") is not None:
        return "deterministic"
    if expected.get("tetraCount") is not None:
        return "count+volume"
    return "volume-only"


def load_fixtures() -> dict[str, str]:
    if not FIXTURES_DIR.exists():
        raise FileNotFoundError(f"Fixture directory not found: {FIXTURES_DIR}")

    results: dict[str, str] = {}
    for path in sorted(FIXTURES_DIR.glob("*.json")):
        data = json.loads(path.read_text())
        name = str(data.get("name", "")).strip()
        if not name:
            continue
        expected = data.get("expected", {})
        results[name] = detect_mode(expected)
    return results


def main() -> int:
    fixtures = load_fixtures()
    done = 0

    print("Batch 6 progress status")
    print("=======================")

    for idx, item in enumerate(QUEUE, start=1):
        mode = fixtures.get(item.name)
        if mode is None:
            print(f"[{idx:02}] ⏳ pending  {item.name} (expected: {item.expected_mode})")
            continue

        done += 1
        marker = "✅" if mode == item.expected_mode else "⚠️"
        note = "" if mode == item.expected_mode else f" [mode mismatch: actual {mode}]"
        print(f"[{idx:02}] {marker} done     {item.name} (expected: {item.expected_mode}){note}")

    print()
    print(f"Completed: {done}/{len(QUEUE)}")

    if done == len(QUEUE):
        print("Batch 6 queue complete.")

    return 0


if __name__ == "__main__":
    sys.exit(main())
