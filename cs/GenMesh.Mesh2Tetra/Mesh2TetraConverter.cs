using GenMesh.Mesh2Tetra.Algorithms;
using GenMesh.Mesh2Tetra.Geometry;
using GenMesh.Mesh2Tetra.Models;

namespace GenMesh.Mesh2Tetra;

public static class Mesh2TetraConverter
{
    public static IReadOnlyList<Tetrahedron> Convert(
        IReadOnlyList<Vector3d> vertices,
        IReadOnlyList<Face> faces,
        Mesh2TetraOptions? options = null)
    {
        options ??= new Mesh2TetraOptions();
        if (options.CheckInput)
        {
            MeshValidation.ValidateInput(vertices, faces);
        }

        var sourceVolume = GeometryPredicates.FaceMeshVolume(vertices, faces);
        if (options.Verbose)
        {
            Console.WriteLine($"[Mesh2Tetra] Input volume: {sourceVolume:0.########}");
        }

        var delaunay = DelaunayInside3D.Build(vertices, faces, options);
        if (options.Verbose)
        {
            Console.WriteLine($"[Mesh2Tetra] Delaunay tets: {delaunay.Count}");
        }

        var final = BoundaryCollapse3D.FillResidualVolume(vertices, faces, delaunay, options);
        if (options.Verbose)
        {
            Console.WriteLine($"[Mesh2Tetra] Final tets: {final.Count}");
        }

        return final;
    }
}
