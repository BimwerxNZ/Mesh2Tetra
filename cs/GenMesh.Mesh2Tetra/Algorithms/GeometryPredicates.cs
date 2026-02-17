using GenMesh.Mesh2Tetra.Geometry;
using GenMesh.Mesh2Tetra.Models;

namespace GenMesh.Mesh2Tetra.Algorithms;

internal static class GeometryPredicates
{
    public static double SignedTetraVolume(Vector3d p1, Vector3d p2, Vector3d p3, Vector3d p4)
    {
        var a = p2 - p1;
        var b = p3 - p1;
        var c = p4 - p1;
        return Vector3d.Dot(a, Vector3d.Cross(b, c)) / 6d;
    }

    public static bool PointInTetrahedron(Vector3d p, Vector3d a, Vector3d b, Vector3d c, Vector3d d, double eps)
    {
        var v = Math.Abs(SignedTetraVolume(a, b, c, d));
        if (v <= eps) return false;

        var v1 = Math.Abs(SignedTetraVolume(p, b, c, d));
        var v2 = Math.Abs(SignedTetraVolume(a, p, c, d));
        var v3 = Math.Abs(SignedTetraVolume(a, b, p, d));
        var v4 = Math.Abs(SignedTetraVolume(a, b, c, p));

        return Math.Abs((v1 + v2 + v3 + v4) - v) <= eps;
    }

    public static double FaceMeshVolume(IReadOnlyList<Vector3d> vertices, IReadOnlyList<Face> faces)
    {
        var acc = 0d;
        foreach (var f in faces)
        {
            var a = vertices[f.A];
            var b = vertices[f.B];
            var c = vertices[f.C];
            acc += Vector3d.Dot(a, Vector3d.Cross(b, c));
        }

        return Math.Abs(acc / 6d);
    }

    public static double TetraMeshVolume(IReadOnlyList<Vector3d> vertices, IReadOnlyList<Tetrahedron> tets)
    {
        var acc = 0d;
        foreach (var t in tets)
        {
            acc += Math.Abs(SignedTetraVolume(vertices[t.A], vertices[t.B], vertices[t.C], vertices[t.D]));
        }

        return acc;
    }

    public static bool PointInsideClosedMesh(Vector3d p, IReadOnlyList<Vector3d> vertices, IReadOnlyList<Face> faces)
    {
        var dir = new Vector3d(1, 0.137, 0.071);
        var hitCount = 0;

        foreach (var face in faces)
        {
            if (RayIntersectsTriangle(p, dir, vertices[face.A], vertices[face.B], vertices[face.C]))
            {
                hitCount++;
            }
        }

        return (hitCount % 2) == 1;
    }

    private static bool RayIntersectsTriangle(Vector3d origin, Vector3d dir, Vector3d v0, Vector3d v1, Vector3d v2)
    {
        const double eps = 1e-12;
        var e1 = v1 - v0;
        var e2 = v2 - v0;
        var p = Vector3d.Cross(dir, e2);
        var det = Vector3d.Dot(e1, p);
        if (Math.Abs(det) < eps) return false;

        var invDet = 1.0 / det;
        var t = origin - v0;
        var u = Vector3d.Dot(t, p) * invDet;
        if (u < 0 || u > 1) return false;

        var q = Vector3d.Cross(t, e1);
        var v = Vector3d.Dot(dir, q) * invDet;
        if (v < 0 || (u + v) > 1) return false;

        var distance = Vector3d.Dot(e2, q) * invDet;
        return distance > eps;
    }
}
