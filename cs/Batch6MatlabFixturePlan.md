# Batch 6 Matlab fixture expansion plan

This batch focuses on **robustness-oriented Matlab exports** that stress preprocessing, tolerance handling, and object decomposition behavior after Batch 5 completion.

## Goals

- Expand parity coverage for edge-case meshes likely to appear in real CAD/BIM sources.
- Exercise option combinations around self-intersection policy and tolerance controls.
- Keep assertions practical:
  - use `volume-only` for non-deterministic triangulation paths,
  - use `count+volume` for stable decompositions,
  - reserve deterministic tetra lists for very small stable cases only.

## Priority fixture queue

1. `matlab_orientation_mixed_winding_01`
   - Closed shell with mixed triangle winding in source export.
   - Expected: volume-only.
2. `matlab_duplicate_faces_cleanup_01`
   - Duplicate/coplanar duplicate faces requiring preprocessing cleanup.
   - Expected: volume-only.
3. `matlab_nonmanifold_edge_fragment_01`
   - Local non-manifold edge fragment that should fail-fast when strict.
   - Expected: fail-fast.
4. `matlab_component_bridge_near_touch_01`
   - Two components with near-touching bridge geometry.
   - Expected: volume-only.
5. `matlab_intersection_policy_split_01`
   - Same geometry evaluated under explicit intersection policy flags.
   - Expected: count+volume.
6. `matlab_high_aspect_ratio_cell_01`
   - High aspect-ratio shell section; tolerance-sensitive.
   - Expected: volume-only.
7. `matlab_small_volume_threshold_01`
   - Very small valid enclosed region requiring tuned epsilon.
   - Expected: deterministic.
8. `matlab_multi_object_sparse_faces_01`
   - Sparse triangularization across multiple disconnected objects.
   - Expected: count+volume.
9. `matlab_local_self_intersection_autoresolve_01`
   - Localized self-intersection resolvable by preprocessing collapse.
   - Expected: volume-only.
10. `matlab_seeded_randomized_case_02`
   - Reproducible seeded case for broad shape diversity.
   - Expected: volume-only.

## Progress tracking

Use the status command to see queue completion at any time:

```bash
python cs/tools/batch6_status.py
```

Status legend:
- `⏳ pending`: fixture not yet imported.
- `✅ done`: fixture imported with expected assertion mode.
- `⚠️ done`: fixture exists, but assertion mode differs from queue expectation.

## Export checklist per fixture

1. Export vertices/faces from Matlab source case.
2. Capture tetra output and/or aggregate tetra volume.
3. Import directly where possible:
   - `python cs/tools/import_matlab_fixture.py <matlab_export.json> --mode volume`
4. Choose assertion mode (`volume`, `count`, `deterministic`, `failfast`) per stability.
5. Validate:
   - `python cs/tools/validate_fixtures.py`
   - `python -m json.tool <fixture>.json`
   - `dotnet test cs/GenMesh.Mesh2Tetra.sln` (when .NET SDK available)
