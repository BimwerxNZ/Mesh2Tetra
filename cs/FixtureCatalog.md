# Fixture Catalog

Auto-generated summary of regression fixtures.

| Fixture | File | Vertices | Faces | Assertion mode |
|---|---|---:|---:|---|
| `intersecting_overlapping_tetra_pair` | `intersecting_overlapping_tetra_pair.json` | 8 | 8 | count+volume |
| `intersecting_shells_autoresolve_single_component` | `intersecting_shells_fail_fast.json` | 8 | 8 | count+volume |
| `intersecting_shells_fail_fast_explicit` | `intersecting_shells_fail_fast_explicit.json` | 8 | 8 | fail-fast |
| `irregular_closed_shell_prism_like` | `irregular_closed_shell_prism_like.json` | 6 | 8 | volume-only |
| `irregular_closed_shell_skewed_hexahedron` | `irregular_closed_shell_skewed_hexahedron.json` | 8 | 12 | volume-only |
| `matlab_intersections_autoresolve_cluster_01` | `matlab_intersections_autoresolve_cluster_01.json` | 8 | 8 | volume-only |
| `matlab_intersections_fail_fast_01` | `matlab_intersections_fail_fast_01.json` | 8 | 8 | fail-fast |
| `matlab_irregular_closed_shell_dense_01` | `matlab_irregular_closed_shell_dense_01.json` | 8 | 12 | volume-only |
| `matlab_irregular_closed_shell_dense_02` | `matlab_irregular_closed_shell_dense_02.json` | 8 | 12 | volume-only |
| `matlab_multi_component_irregular_02` | `matlab_multi_component_irregular_02.json` | 12 | 12 | count+volume |
| `matlab_nested_shell_like_01` | `matlab_nested_shell_like_01.json` | 8 | 8 | volume-only |
| `matlab_orientation_mixed_winding_01` | `matlab_orientation_mixed_winding_01.json` | 4 | 4 | volume-only |
| `matlab_randomized_seeded_case_01` | `matlab_randomized_seeded_case_01.json` | 8 | 8 | volume-only |
| `matlab_thin_feature_shell_01` | `matlab_thin_feature_shell_01.json` | 4 | 4 | volume-only |
| `matlab_tolerance_near_coplanar_01` | `matlab_tolerance_near_coplanar_01.json` | 4 | 4 | count+volume |
| `matlab_tolerance_small_volume_keep_01` | `matlab_tolerance_small_volume_keep_01.json` | 4 | 4 | deterministic |
| `multi_component_four_tetra` | `multi_component_four_tetra.json` | 16 | 16 | deterministic |
| `multi_component_mixed_scale_three_tetra` | `multi_component_mixed_scale_three_tetra.json` | 12 | 12 | deterministic |
| `multi_component_three_tetra` | `multi_component_three_tetra.json` | 12 | 12 | deterministic |
| `multi_component_two_tetra` | `multi_component_two_tetra.json` | 8 | 8 | deterministic |
| `scaled_unit_tetra` | `scaled_unit_tetra.json` | 4 | 4 | deterministic |
| `unit_tetra` | `tetra_unit.json` | 4 | 4 | deterministic |
| `tolerance_small_tetra_accepted` | `tolerance_small_tetra_accepted.json` | 4 | 4 | deterministic |
| `tolerance_small_tetra_with_default_epsilon` | `tolerance_small_tetra_rejected.json` | 4 | 4 | count+volume |

Total fixtures: **24**.

Regenerate with:

```bash
python tools/generate_fixture_catalog.py
```
