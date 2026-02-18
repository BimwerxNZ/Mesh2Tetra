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

        var boundaryFaces = MeshPreprocessing.PreprocessBoundaryFaces(vertices, faces, options);

        var sourceVolume = GeometryPredicates.FaceMeshVolume(vertices, boundaryFaces);
        if (options.Verbose)
        {
            Console.WriteLine($"[Mesh2Tetra] Input volume: {sourceVolume:0.########}");
            Console.WriteLine($"[Mesh2Tetra] Boundary faces after preprocessing: {boundaryFaces.Count}");
        }

        var (delaunayTets, remainingFaces) = DelaunayInside3D.Build(vertices, boundaryFaces, options);
        if (options.Verbose)
        {
            Console.WriteLine($"[Mesh2Tetra] Delaunay tets: {delaunayTets.Count}");
            Console.WriteLine($"[Mesh2Tetra] Residual faces: {remainingFaces.Count}");
        }

        IReadOnlyList<Tetrahedron> final;
        try
        {
            final = BoundaryCollapse3D.FillResidualVolume(vertices, remainingFaces, delaunayTets, options);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Boundary collapse failed", StringComparison.OrdinalIgnoreCase))
        {
            // Fallback for stubborn residual shells: keep the validated Delaunay phase result
            // when boundary-collapse heuristics cannot make progress.
            if (delaunayTets.Count > 0)
            {
                final = delaunayTets;
            }
            else
            {
                throw;
            }
        }

        if (options.Verbose)
        {
            Console.WriteLine($"[Mesh2Tetra] Final tets: {final.Count}");
        }

        return final;
    }
}
