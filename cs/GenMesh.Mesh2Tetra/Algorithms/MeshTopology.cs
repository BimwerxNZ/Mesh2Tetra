using GenMesh.Mesh2Tetra.Models;

namespace GenMesh.Mesh2Tetra.Algorithms;

internal static class MeshTopology
{
    public static List<Face> GetRemainingFaces(IReadOnlyList<Tetrahedron> tetrahedra, IReadOnlyList<Face> boundaryFaces)
    {
        var allFaces = new List<Face>(tetrahedra.Count * 4 + boundaryFaces.Count);
        foreach (var t in tetrahedra)
        {
            allFaces.AddRange(GetTetFaces(t));
        }

        allFaces.AddRange(boundaryFaces);

        var counts = new Dictionary<(int, int, int), int>();
        foreach (var f in allFaces)
        {
            var key = Canonical(f);
            counts.TryGetValue(key, out var c);
            counts[key] = c + 1;
        }

        return allFaces.Where(f => counts[Canonical(f)] == 1).ToList();
    }

    public static IEnumerable<Face> GetTetFaces(Tetrahedron t)
    {
        yield return new Face(t.C, t.B, t.A);
        yield return new Face(t.B, t.D, t.A);
        yield return new Face(t.D, t.C, t.A);
        yield return new Face(t.D, t.B, t.C);
    }

    public static (int, int, int) Canonical(Face f)
    {
        var a = f.A;
        var b = f.B;
        var c = f.C;
        if (a > b) (a, b) = (b, a);
        if (b > c) (b, c) = (c, b);
        if (a > b) (a, b) = (b, a);
        return (a, b, c);
    }

    public static bool ContainsAllVertices(Tetrahedron t, Face f)
    {
        var v = t.Vertices;
        return v.Contains(f.A) && v.Contains(f.B) && v.Contains(f.C);
    }
}
