using Xunit;
using GenMesh.Mesh2Tetra.Algorithms;
using GenMesh.Mesh2Tetra.Geometry;

namespace GenMesh.Mesh2Tetra.Tests;

public sealed class GeometryPredicateParityTests
{
    [Fact]
    public void ComputesBarycentricCoordinatesForTetrahedron()
    {
        var a = new Vector3d(0, 0, 0);
        var b = new Vector3d(1, 0, 0);
        var c = new Vector3d(0, 1, 0);
        var d = new Vector3d(0, 0, 1);
        var p = new Vector3d(0.2, 0.3, 0.1);

        var bary = GeometryPredicates.BarycentricCoordinatesTetrahedron(p, a, b, c, d);

        Assert.InRange(Math.Abs((bary.L1 + bary.L2 + bary.L3 + bary.L4) - 1d), 0d, 1e-12);
        Assert.True(GeometryPredicates.CheckInsideTetrahedron(p, a, b, c, d));
    }

    [Fact]
    public void ComputesClosestPointOnLineSegment()
    {
        var p = GeometryPredicates.PointToClosestPointOnLine(
            new Vector3d(0, 0, 0),
            new Vector3d(2, 0, 0),
            new Vector3d(0.7, 1, 0));

        Assert.Equal(new Vector3d(0.7, 0, 0), p);
    }

    [Fact]
    public void ComputesSphereFromFourPoints()
    {
        var ok = GeometryPredicates.TrySphereFrom4Points(
            new Vector3d(1, 0, 0),
            new Vector3d(0, 1, 0),
            new Vector3d(0, 0, 1),
            new Vector3d(-1, 0, 0),
            out var center,
            out var radius);

        Assert.True(ok);
        Assert.InRange(center.Norm(), 0d, 1e-10);
        Assert.InRange(Math.Abs(radius - 1d), 0d, 1e-10);
    }
}
