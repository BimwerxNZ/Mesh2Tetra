# Batch 5 Matlab fixture expansion plan

This plan focuses on importing additional **Matlab-exported** fixture data into the JSON regression harness.

## Goals

- Increase parity confidence beyond deterministic synthetic cases.
- Stress solver behavior for irregular closed shells and intersection-heavy meshes.
- Keep fixture assertions practical:
  - use exact tetra sets only where deterministic,
  - use volume-only assertions when tetra ordering/topology can vary.

## Priority fixture queue

1. `matlab_irregular_closed_shell_dense_01`
   - High face count single closed shell.
   - Expected: volume-only.
2. `matlab_irregular_closed_shell_dense_02`
   - Similar complexity, different topology.
   - Expected: volume-only.
3. `matlab_intersections_autoresolve_cluster_01`
   - Intentional local face intersections that Matlab resolves.
   - Expected: volume-only, optional count.
4. `matlab_intersections_fail_fast_01`
   - Non-resolvable intersections.
   - Expected: `expectedExceptionContains`.
5. `matlab_tolerance_near_coplanar_01`
   - Near-coplanar face set; tolerance-sensitive.
   - Expected: count + volume.
6. `matlab_tolerance_small_volume_keep_01`
   - Very small but valid volume with tuned epsilon.
   - Expected: exact tetra set when deterministic.
7. `matlab_multi_component_irregular_02`
   - Multiple disconnected irregular components.
   - Expected: count + volume.
8. `matlab_nested_shell_like_01`
   - Inner/outer shell-like geometry (if supported by source workflow).
   - Expected: volume-only.
9. `matlab_thin_feature_shell_01`
   - Slender features that challenge collapse heuristics.
   - Expected: volume-only.
10. `matlab_randomized_seeded_case_01`
   - Matlab-seeded random but reproducible fixture.
   - Expected: volume-only.

## Progress tracking

Use the status command to see queue completion at any time:

```bash
python cs/tools/batch5_status.py
```

Status legend:
- `⏳ pending`: fixture not yet imported.
- `✅ done`: fixture imported with expected assertion mode.
- `⚠️ done`: fixture exists, but assertion mode differs from queue expectation.

## Export checklist per fixture

1. Export vertices and triangular faces from Matlab.
2. Record Matlab output tetrahedra and/or aggregate tetra volume.
3. Import Matlab export directly (preferred): `python cs/tools/import_matlab_fixture.py <matlab_export.json> --mode volume` (or `deterministic` / `count` / `failfast`).
   - Fallback scaffold: `python cs/tools/new_fixture.py <name> --mode volume` and paste values manually.
4. Replace scaffolded input/expected payload with Matlab-exported values.
5. Choose assertion style:
   - deterministic: `tetraCount` + `exactTetrahedra` + `tetraVolume`
   - non-deterministic: `tetraVolume` (and optional `tetraCount`)
6. Run local validation:
   - `python -m json.tool <fixture>.json`
   - `dotnet test cs/GenMesh.Mesh2Tetra.sln` (when .NET SDK available)

## JSON fixture schema (reference)

Use existing schema fields:

- `name`
- `input.vertices`
- `input.faces`
- `expected.tetraCount` (optional)
- `expected.tetraVolume`
- `expected.volumeTolerance`
- `expected.exactTetrahedra` (optional)
- `expected.expectedExceptionContains` (optional)
- `options.checkInput`
- `options.checkSelfIntersections` (legacy shorthand; maps to both flags below)
- `options.autoResolveIntersections` (optional explicit control)
- `options.failOnSelfIntersections` (optional explicit control)
- `options.verbose`
- `options.planeDistanceTolerance`
- `options.epsilon`

