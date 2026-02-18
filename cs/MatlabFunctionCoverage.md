# Matlab → C# function coverage

This document tracks the Matlab entry points/subfunctions under `/functions` and their C# equivalents under `cs/GenMesh.Mesh2Tetra`.

## Top-level pipeline

- `Mesh2Tetra.m` → `Mesh2TetraConverter.Convert`
- `DelaunayInside3D.m` → `Algorithms/DelaunayInside3D.Build`
- `BoundaryCollapse3D.m` → `Algorithms/BoundaryCollapse3D.FillResidualVolume`

## Topology/object decomposition

- `GetRemainingFaces.m` → `MeshTopology.GetRemainingFaces`
- `ReturnSepparateFaceObjects.m` → `MeshTopology.SeparateFaceObjects`
- `InsidePoints3D.m` → `MeshTopology.InsidePoints3D`
- `CheckFaceOrientations.m` → `GeometryPredicates.HasOrientationImbalance` + `MeshTopology.FlipOrientation`

## Geometry predicates and intersection checks

- `CheckInsideFace.m` → internal `GeometryPredicates.CheckInsideFace` (2D projected test)
- `BarycentricCoordinatesTriangle.m` → internal `GeometryPredicates.BarycentricCoordinatesTriangle`
- `LineTriangleIntersection.m` → `GeometryPredicates.LineTriangleIntersection`
- `TriangleTriangleIntersection.m` → `GeometryPredicates.TriangleTriangleIntersection`
- `CheckMeshInterSections.m` → `GeometryPredicates.HasMeshIntersections` / `FindIntersectingFacePairs`
- `PointToClosestPointOnPlane.m` → `GeometryPredicates.PointToClosestPointOnPlane`
- `PointToClosestPointOnLine.m` → `GeometryPredicates.PointToClosestPointOnLine`
- `LineLineIntersect.m` (mex equivalent) → `GeometryPredicates.TryLineLineIntersect`

## Tetrahedron / volume checks

- `CheckVolumeFaceMesh.m` → `GeometryPredicates.FaceMeshVolume`
- `CheckVolumeTetraMesh.m` → `GeometryPredicates.TetraMeshVolume`
- `VolumeCheck.m` / `VolumeCheckNew.m` → volume consistency checks in `DelaunayInside3D` and `BoundaryCollapse3D`
- `BarycentricCoordinatesTetrahedron.m` → `GeometryPredicates.BarycentricCoordinatesTetrahedron`
- `CheckInsideTetrahedron.m` → `GeometryPredicates.CheckInsideTetrahedron`
- `CheckPointsInsideTetrahedron.m` → `GeometryPredicates.CheckPointsInsideTetrahedron`

## Boundary-collapse process helpers

- `collapse_edge.m` / `process.m` → `BoundaryCollapse3D.TryCollapseEdge` / `BoundaryCollapse3D.Process`
- `retry_remove_tetrahedrons.m` / `RemoveInvalidTetrahedrons.m` → `BoundaryCollapse3D.RetryRemoveTetrahedrons`
- `CheckMoveInside3D.m` / `CheckVisiblePoint3D.m` / `CheckPointOutInside3D.m` → `GeometryPredicates.CheckMoveInside3D` + point-in-mesh tests
- `solveInterSections.m` / `visibility_matrix_3D.m` → `MeshPreprocessing.SolveIntersectionsByLocalCollapse`
- `make_left_vertice_list.m` → local neighbor extraction in `BoundaryCollapse3D.TryCollapseEdge`

## Additional mex-parity primitive

- `SphereFrom4Points.m` (mex equivalent) → `GeometryPredicates.TrySphereFrom4Points`

