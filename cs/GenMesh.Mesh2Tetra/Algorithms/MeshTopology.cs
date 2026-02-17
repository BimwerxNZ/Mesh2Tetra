using GenMesh.Mesh2Tetra.Geometry;
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

    public static List<List<Face>> SeparateFaceObjects(IReadOnlyList<Face> faces)
    {
        if (faces.Count == 0) return [];

        var vertToFaces = new Dictionary<int, List<int>>();
        for (var i = 0; i < faces.Count; i++)
        {
            AddVertexFace(faces[i].A, i);
            AddVertexFace(faces[i].B, i);
            AddVertexFace(faces[i].C, i);
        }

        var visited = new bool[faces.Count];
        var objects = new List<List<Face>>();
        for (var i = 0; i < faces.Count; i++)
        {
            if (visited[i]) continue;
            var component = new List<Face>();
            var q = new Queue<int>();
            q.Enqueue(i);
            visited[i] = true;

            while (q.Count > 0)
            {
                var fi = q.Dequeue();
                var face = faces[fi];
                component.Add(face);

                VisitNeighbors(face.A, q);
                VisitNeighbors(face.B, q);
                VisitNeighbors(face.C, q);
            }

            objects.Add(component);
        }

        return objects;

        void AddVertexFace(int vertex, int faceIndex)
        {
            if (!vertToFaces.TryGetValue(vertex, out var list))
            {
                list = [];
                vertToFaces[vertex] = list;
            }

            list.Add(faceIndex);
        }

        void VisitNeighbors(int vertex, Queue<int> queue)
        {
            if (!vertToFaces.TryGetValue(vertex, out var neigh)) return;
            foreach (var ni in neigh)
            {
                if (visited[ni]) continue;
                visited[ni] = true;
                queue.Enqueue(ni);
            }
        }
    }

    public static List<Face> FlipOrientation(IReadOnlyList<Face> faces)
        => faces.Select(f => new Face(f.C, f.B, f.A)).ToList();

    public static (List<Vector3d> Vertices, List<Face> Faces, int[] GlobalVertexIds) InsidePoints3D(
        IReadOnlyList<Vector3d> allVertices,
        IReadOnlyList<Face> objectFaces)
    {
        var globalIds = objectFaces.SelectMany(f => new[] { f.A, f.B, f.C }).Distinct().OrderBy(i => i).ToArray();
        var g2l = new Dictionary<int, int>(globalIds.Length);
        for (var i = 0; i < globalIds.Length; i++)
        {
            g2l[globalIds[i]] = i;
        }

        var localVertices = globalIds.Select(id => allVertices[id]).ToList();
        var localFaces = objectFaces.Select(f => new Face(g2l[f.A], g2l[f.B], g2l[f.C])).ToList();
        return (localVertices, localFaces, globalIds);
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
