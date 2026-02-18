# GenMesh.Mesh2Tetra (.NET 10)

This folder contains a C# port of the Matlab `Mesh2Tetra` pipeline.

## Analysis of the Matlab solution

The original Matlab implementation is a 2-phase constrained tetrahedralization pipeline:

1. **DelaunayInside3D**
   - Build an unconstrained 3D Delaunay tetrahedralization.
   - Remove tetrahedra outside the closed boundary mesh.
   - Compute residual boundary faces.
   - Recursively process residual components.
2. **BoundaryCollapse3D**
   - Fill residual volumes that cannot be solved by constrained Delaunay alone.
   - Uses edge-collapse / retry heuristics to avoid adding boundary points.

## C# architecture

`GenMesh.Mesh2Tetra` mirrors the same decomposition:

- `Mesh2TetraConverter` = top-level API (equivalent to `Mesh2Tetra.m`).
- `Algorithms/DelaunayInside3D` = Delaunay + inside filtering + residual face extraction + recursive object processing.
- `Algorithms/BoundaryCollapse3D` = boundary-collapse + retry-removal fallback.
- `Algorithms/GeometryPredicates` = shared volume/orientation/inside/intersection checks.
- `Algorithms/MeshTopology` = tetra face/topology/object helpers.
- `Algorithms/MeshValidation` = input validation.
- `Algorithms/MeshPreprocessing` = boundary face cleanup and intersection handling (including local-collapse intersection solving).

## Current status

- ✅ Delaunay phase is implemented with `MIConvexHull`, including `PlaneDistanceTolerance` support for current API signatures.
- ✅ Delaunay residual recursion and disconnected face-object handling are implemented.
- ✅ Boundary-collapse pass is implemented, including:
  - local edge collapse,
  - boundary/tetra process update,
  - retry tetra removal fallback,
  - volume consistency checks,
  - triangle-triangle intersection parity checks during collapse validation.

## Remaining work

- ✅ `solveInterSections` parity path is implemented via iterative local edge-collapse style intersection reduction with conservative fallback cleanup.
- ✅ Regression harness for Matlab fixture parity now exists in `GenMesh.Mesh2Tetra.Tests` with JSON-driven fixtures and strict tetra/volume assertions.
- ✅ Batch 3 fixture expansion added irregular closed-shell JSON fixtures with volume-only assertions where tetra ordering can vary.
- ✅ Batch 4 fixture expansion added deterministic scale/translation multi-component cases.
- ⚠️ Continue adding Matlab-exported fixtures as they are generated to broaden regression breadth.
- ✅ Batch 5 Matlab fixture intake plan documented in `Batch5MatlabFixturePlan.md`.
- ✅ Batch 6 Matlab fixture intake plan documented in `Batch6MatlabFixturePlan.md`.

## Regression fixtures (Matlab parity)

Run from `cs/`:

```bash
dotnet test GenMesh.Mesh2Tetra.sln
```

Fixtures are discovered automatically from `GenMesh.Mesh2Tetra.Tests/Fixtures/*.json`. Each fixture can assert:

Quick fixture lint:

```bash
python tools/validate_fixtures.py
```

Quick fixture scaffold:

```bash
python tools/new_fixture.py <fixture_name> --mode volume
```

Import Matlab-exported fixture payload:

```bash
python tools/import_matlab_fixture.py <matlab_export.json> --mode volume
```

Batch 5 queue progress snapshot:

```bash
python tools/batch5_status.py
```

Batch 6 queue progress snapshot:

```bash
python tools/batch6_status.py
```

Quality gate (fixture lint + catalog freshness + unit tests):

```bash
python tools/run_regression_gate.py
```

Optional flags:
- `--skip-dotnet`
- `--skip-catalog`

Fixture catalog generation:

```bash
python tools/generate_fixture_catalog.py
```


- input vertices/faces,
- expected tetra count,
- expected total tetra volume (+ tolerance),
- optional exact tetra index sets for deterministic small cases,
- optional `expectedExceptionContains` for fail-fast fixtures (e.g. intentional self-intersection cases).


## Matlab function coverage index

A full function-by-function mapping now lives in `MatlabFunctionCoverage.md`.


## Fixture catalog

A generated fixture summary is available in `FixtureCatalog.md`.


## CI

GitHub Actions workflow `cs-regression-gate` runs fixture lint, fixture catalog freshness, and `dotnet test` for `cs/**` changes.
