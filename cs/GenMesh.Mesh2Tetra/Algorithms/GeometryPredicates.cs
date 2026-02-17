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

    public static bool HasOrientationImbalance(IReadOnlyList<Face> faces)
    {
        var edgeCounts = new Dictionary<(int, int), int>();
        foreach (var f in faces)
        {
            AddEdge(f.A, f.B);
            AddEdge(f.B, f.C);
            AddEdge(f.C, f.A);
        }

        foreach (var kv in edgeCounts)
        {
            var reverse = (kv.Key.Item2, kv.Key.Item1);
            edgeCounts.TryGetValue(reverse, out var rev);
            if (kv.Value != rev)
            {
                return true;
            }
        }

        return false;

        void AddEdge(int a, int b)
        {
            var key = (a, b);
            edgeCounts.TryGetValue(key, out var c);
            edgeCounts[key] = c + 1;
        }
    }

    public static bool CheckMoveInside3D(IReadOnlyList<Vector3d> vertices, IReadOnlyList<Face> newFaces, int vertexId)
    {
        var p = vertices[vertexId];
        foreach (var f in newFaces)
        {
            var a = vertices[f.A];
            var b = vertices[f.B];
            var c = vertices[f.C];

            var normal = Vector3d.Cross(a - c, b - c);
            var normalNorm = normal.Norm();
            if (normalNorm <= 1e-12) return false;
            normal /= normalNorm;

            var planeClosest = PointToClosestPointOnPlane(a, b, c, p);
            var normal2 = p - planeClosest;
            var normal2Norm = normal2.Norm();
            if (normal2Norm <= 1e-12) return false;
            normal2 /= normal2Norm;

            var diff = normal - normal2;
            if ((diff.X * diff.X + diff.Y * diff.Y + diff.Z * diff.Z) >= 1e-5)
            {
                return false;
            }
        }

        return true;
    }

    public static bool HasMeshIntersections(IReadOnlyList<Vector3d> vertices, IReadOnlyList<Face> faces, int maxOuterFaces = -1)
        => FindIntersectingFacePairs(vertices, faces, maxOuterFaces).Count > 0;

    public static List<(int I, int J)> FindIntersectingFacePairs(IReadOnlyList<Vector3d> vertices, IReadOnlyList<Face> faces, int maxOuterFaces = -1)
    {
        var pairs = new List<(int I, int J)>();
        var nF = faces.Count;
        if (nF <= 1) return pairs;
        var nMax = maxOuterFaces < 0 ? nF - 1 : Math.Min(maxOuterFaces, nF - 1);

        for (var j = 0; j < nMax; j++)
        {
            var fj = faces[j];
            var o1 = vertices[fj.A];
            var o2 = vertices[fj.B];
            var o3 = vertices[fj.C];

            for (var i = j + 1; i < nF; i++)
            {
                var fi = faces[i];
                if (SameFace(fi, fj)) continue;

                var p1 = vertices[fi.A];
                var p2 = vertices[fi.B];
                var p3 = vertices[fi.C];
                if (TriangleTriangleIntersection(p1, p2, p3, o1, o2, o3, ignoreCorners: true))
                {
                    pairs.Add((j, i));
                }
            }
        }

        return pairs;
    }

    public static bool TriangleTriangleIntersection(
        Vector3d p1,
        Vector3d p2,
        Vector3d p3,
        Vector3d o1,
        Vector3d o2,
        Vector3d o3,
        bool ignoreCorners)
    {
        if (SeparatedByAabb(p1, p2, p3, o1, o2, o3))
        {
            return false;
        }

        return LineTriangleIntersection(p1, p2, p3, o1, o2, ignoreCorners)
            || LineTriangleIntersection(p1, p2, p3, o2, o3, ignoreCorners)
            || LineTriangleIntersection(p1, p2, p3, o3, o1, ignoreCorners)
            || LineTriangleIntersection(o1, o2, o3, p1, p2, ignoreCorners)
            || LineTriangleIntersection(o1, o2, o3, p2, p3, ignoreCorners)
            || LineTriangleIntersection(o1, o2, o3, p3, p1, ignoreCorners);
    }

    private static bool LineTriangleIntersection(Vector3d a, Vector3d b, Vector3d c, Vector3d p1, Vector3d p2, bool ignoreCorners)
    {
        if (SeparatedByAabb(a, b, c, p1, p2, p2))
        {
            return false;
        }

        var n = Vector3d.Cross(a - c, b - c);
        var nNorm = n.Norm();
        if (nNorm <= 1e-12) return false;
        n /= nNorm;

        var vLine = p2 - p1;
        var numerator = Vector3d.Dot(n, c - p1);
        var denominator = Vector3d.Dot(n, vLine) + 1e-16;
        var t = numerator / denominator;

        if (t < 0 || t > 1)
        {
            return false;
        }

        var p = p1 + new Vector3d(vLine.X * t, vLine.Y * t, vLine.Z * t);

        var absX = Math.Abs(n.X);
        var absY = Math.Abs(n.Y);
        var absZ = Math.Abs(n.Z);

        int i;
        int j;
        if (absX > absY)
        {
            if (absX > absZ)
            {
                i = 1;
                j = 2;
            }
            else
            {
                i = 0;
                j = 1;
            }
        }
        else
        {
            if (absY > absZ)
            {
                i = 0;
                j = 2;
            }
            else
            {
                i = 0;
                j = 1;
            }
        }

        var p2d = Pick(p, i, j);
        var a2d = Pick(a, i, j);
        var b2d = Pick(b, i, j);
        var c2d = Pick(c, i, j);

        var minX = Math.Min(a2d.X, Math.Min(b2d.X, c2d.X));
        var minY = Math.Min(a2d.Y, Math.Min(b2d.Y, c2d.Y));
        var maxX = Math.Max(a2d.X, Math.Max(b2d.X, c2d.X));
        var maxY = Math.Max(a2d.Y, Math.Max(b2d.Y, c2d.Y));

        return CheckInsideFace(a2d, b2d, c2d, p2d.X, p2d.Y, minX, minY, maxX, maxY, ignoreCorners);
    }

    private static bool CheckInsideFace(Vector2d v0, Vector2d v1, Vector2d v2, double rx, double ry, double minX, double minY, double maxX, double maxY, bool ignoreCorners)
    {
        if (rx < minX || rx > maxX || ry < minY || ry > maxY)
        {
            return false;
        }

        var lambda = BarycentricCoordinatesTriangle(v0, v1, v2, rx, ry);
        var mv = ignoreCorners ? 1e-8 : 0d;
        var vv = ignoreCorners ? (1d - 1e-8) : 1d;

        return lambda.L1 >= mv && lambda.L1 <= vv
            && lambda.L2 >= mv && lambda.L2 <= vv
            && lambda.L3 >= mv && lambda.L3 <= vv;
    }

    private static (double L1, double L2, double L3) BarycentricCoordinatesTriangle(Vector2d v0, Vector2d v1, Vector2d v2, double rx, double ry)
    {
        var f12 = ((v1.Y - v2.Y) * v0.X) + ((v2.X - v1.X) * v0.Y) + (v1.X * v2.Y) - (v2.X * v1.Y);
        var f20 = ((v2.Y - v0.Y) * v1.X) + ((v0.X - v2.X) * v1.Y) + (v2.X * v0.Y) - (v0.X * v2.Y);
        var f01 = ((v0.Y - v1.Y) * v2.X) + ((v1.X - v0.X) * v2.Y) + (v0.X * v1.Y) - (v1.X * v0.Y);

        if (Math.Abs(f12) < 1e-16 || Math.Abs(f20) < 1e-16 || Math.Abs(f01) < 1e-16)
        {
            return (double.NaN, double.NaN, double.NaN);
        }

        var l1 = (((v1.Y - v2.Y) / f12) * rx) + (((v2.X - v1.X) / f12) * ry) + (((v1.X * v2.Y) - (v2.X * v1.Y)) / f12);
        var l2 = (((v2.Y - v0.Y) / f20) * rx) + (((v0.X - v2.X) / f20) * ry) + (((v2.X * v0.Y) - (v0.X * v2.Y)) / f20);
        var l3 = (((v0.Y - v1.Y) / f01) * rx) + (((v1.X - v0.X) / f01) * ry) + (((v0.X * v1.Y) - (v1.X * v0.Y)) / f01);
        return (l1, l2, l3);
    }

    private static bool SameFace(Face a, Face b)
    {
        var ca = MeshTopology.Canonical(a);
        var cb = MeshTopology.Canonical(b);
        return ca == cb;
    }

    private static bool SeparatedByAabb(Vector3d p1, Vector3d p2, Vector3d p3, Vector3d o1, Vector3d o2, Vector3d o3)
    {
        var pMinX = Math.Min(p1.X, Math.Min(p2.X, p3.X));
        var pMinY = Math.Min(p1.Y, Math.Min(p2.Y, p3.Y));
        var pMinZ = Math.Min(p1.Z, Math.Min(p2.Z, p3.Z));
        var pMaxX = Math.Max(p1.X, Math.Max(p2.X, p3.X));
        var pMaxY = Math.Max(p1.Y, Math.Max(p2.Y, p3.Y));
        var pMaxZ = Math.Max(p1.Z, Math.Max(p2.Z, p3.Z));

        var oMinX = Math.Min(o1.X, Math.Min(o2.X, o3.X));
        var oMinY = Math.Min(o1.Y, Math.Min(o2.Y, o3.Y));
        var oMinZ = Math.Min(o1.Z, Math.Min(o2.Z, o3.Z));
        var oMaxX = Math.Max(o1.X, Math.Max(o2.X, o3.X));
        var oMaxY = Math.Max(o1.Y, Math.Max(o2.Y, o3.Y));
        var oMaxZ = Math.Max(o1.Z, Math.Max(o2.Z, o3.Z));

        return pMinX > oMaxX || pMinY > oMaxY || pMinZ > oMaxZ || pMaxX < oMinX || pMaxY < oMinY || pMaxZ < oMinZ;
    }

    private static Vector2d Pick(Vector3d v, int i, int j)
    {
        var values = new[] { v.X, v.Y, v.Z };
        return new Vector2d(values[i], values[j]);
    }

    private static Vector3d PointToClosestPointOnPlane(Vector3d a, Vector3d b, Vector3d c, Vector3d p)
    {
        var n = Vector3d.Cross(b - a, c - a);
        var nNorm = n.Norm();
        if (nNorm <= 1e-12) return p;
        n /= nNorm;
        var distance = Vector3d.Dot(p - a, n);
        return p - new Vector3d(n.X * distance, n.Y * distance, n.Z * distance);
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

    private readonly record struct Vector2d(double X, double Y);
}
