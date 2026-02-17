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

        var boundaryFaces = faces.ToList();
        if (options.AutoFixFaceOrientation && GeometryPredicates.HasOrientationImbalance(boundaryFaces))
        {
            if (options.Verbose)
            {
                Console.WriteLine("[Mesh2Tetra] Detected orientation imbalance; flipping face orientation.");
            }

            boundaryFaces = MeshTopology.FlipOrientation(boundaryFaces);
        }

        var sourceVolume = GeometryPredicates.FaceMeshVolume(vertices, boundaryFaces);
        if (options.Verbose)
        {
            Console.WriteLine($"[Mesh2Tetra] Input volume: {sourceVolume:0.########}");
        }

        var (delaunayTets, remainingFaces) = DelaunayInside3D.Build(vertices, boundaryFaces, options);
        if (options.Verbose)
        {
            Console.WriteLine($"[Mesh2Tetra] Delaunay tets: {delaunayTets.Count}");
            Console.WriteLine($"[Mesh2Tetra] Residual faces: {remainingFaces.Count}");
        }

        var final = BoundaryCollapse3D.FillResidualVolume(vertices, remainingFaces, delaunayTets, options);
        if (options.Verbose)
        {
            Console.WriteLine($"[Mesh2Tetra] Final tets: {final.Count}");
        }

        return final;
    }
}
