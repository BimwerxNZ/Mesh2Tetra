# GenMesh.Mesh2Tetra (.NET 10)

This folder contains a C# port of the Matlab `Mesh2Tetra` pipeline.

## Analysis of the Matlab solution

The original Matlab implementation is a 2-phase constrained tetrahedralization pipeline:

1. **DelaunayInside3D**
   - Build an unconstrained 3D Delaunay tetrahedralization.
   - Remove tetrahedra outside the closed boundary mesh.
   - Compute residual boundary faces.
2. **BoundaryCollapse3D**
   - Fill residual volumes that cannot be solved by constrained Delaunay alone.
   - Uses edge-collapse / retry heuristics to avoid adding boundary points.

## C# architecture

`GenMesh.Mesh2Tetra` mirrors the same decomposition:

- `Mesh2TetraConverter` = top-level API (equivalent to `Mesh2Tetra.m`).
- `Algorithms/DelaunayInside3D` = Delaunay + inside filtering + residual face extraction.
- `Algorithms/BoundaryCollapse3D` = boundary-collapse + retry-removal fallback.
- `Algorithms/GeometryPredicates` = shared volume/orientation/inside checks.
- `Algorithms/MeshTopology` = tetra face/topology helpers.
- `Algorithms/MeshValidation` = input validation.

## Current status

- ✅ Delaunay phase is implemented with `MIConvexHull`.
- ✅ Boundary-collapse pass is implemented, including:
  - local edge collapse,
  - boundary/tetra process update,
  - retry tetra removal fallback,
  - volume consistency checks,
  - triangle-triangle intersection parity checks during collapse validation.
- ✅ Triangle-triangle intersection checks are ported and used in boundary collapse candidate filtering.
