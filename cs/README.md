# GenMesh.Mesh2Tetra (.NET 10)

This folder contains an initial C# port of the Matlab `Mesh2Tetra` pipeline.

## Analysis of the Matlab solution

The original Matlab implementation is a 2-phase constrained tetrahedralization pipeline:

1. **DelaunayInside3D**
   - Build an unconstrained 3D Delaunay tetrahedralization.
   - Remove tetrahedra outside the closed boundary mesh.
   - Recursively retry on residual boundary fragments.
2. **BoundaryCollapse3D**
   - Fill residual volumes that cannot be solved by constrained Delaunay alone.
   - Uses edge-collapse / retry heuristics to avoid adding boundary points.

The Matlab code also contains a broad set of geometric predicates (barycentric checks,
triangle-triangle intersection, line-triangle intersection, face/tetra volume checks) and optional MEX accelerators.

## C# architecture

`GenMesh.Mesh2Tetra` mirrors the same high-level decomposition:

- `Mesh2TetraConverter` = top-level API (equivalent to `Mesh2Tetra.m`).
- `Algorithms/DelaunayInside3D` = Delaunay + inside filtering.
- `Algorithms/BoundaryCollapse3D` = fallback extension point.
- `Algorithms/GeometryPredicates` = shared volume and inside/outside tests.
- `Algorithms/MeshValidation` = input validation.

## Current status

- ✅ Delaunay phase is implemented with `MIConvexHull`.
- ✅ Volume checks and mesh-inside tests are implemented.
- ⚠️ Boundary-collapse parity is not yet implemented; the code currently throws if Delaunay does not fully fill the volume.

## Suggested next steps

1. Port `collapse_edge.m` + `process.m` + retry methods into `BoundaryCollapse3D`.
2. Port intersection predicates from `functions/subfunctions/*.m` into deterministic C# predicates.
3. Add regression tests with known meshes and compare tetra counts + volumes against Matlab outputs.
