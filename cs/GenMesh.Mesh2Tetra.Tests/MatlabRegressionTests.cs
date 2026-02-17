using Xunit;
using GenMesh.Mesh2Tetra.Geometry;
using GenMesh.Mesh2Tetra.Models;
using GenMesh.Mesh2Tetra.Tests.TestData;

namespace GenMesh.Mesh2Tetra.Tests;

public sealed class MatlabRegressionTests
{
    public static IEnumerable<object[]> Fixtures => RegressionFixtureLoader.LoadFixtureCases();

    [Theory]
    [MemberData(nameof(Fixtures))]
    public void ConvertsFixtureWithExpectedParity(RegressionFixture fixture, string _)
    {
        var vertices = fixture.Input.Vertices
            .Select(v => new Vector3d(v[0], v[1], v[2]))
            .ToArray();
        var faces = fixture.Input.Faces
            .Select(f => new Face(f[0], f[1], f[2]))
            .ToArray();

        var options = new Mesh2TetraOptions
        {
            CheckInput = fixture.Options.CheckInput,
            AutoResolveIntersections = fixture.Options.CheckSelfIntersections,
            FailOnSelfIntersections = fixture.Options.CheckSelfIntersections,
            Verbose = fixture.Options.Verbose,
            PlaneDistanceTolerance = fixture.Options.PlaneDistanceTolerance,
            Epsilon = fixture.Options.Epsilon,
        };

        var tets = Mesh2TetraConverter.Convert(vertices, faces, options);

        Assert.Equal(fixture.Expected.TetraCount, tets.Count);

        var outputVolume = tets.Sum(t => Math.Abs(SignedVolume(vertices[t.A], vertices[t.B], vertices[t.C], vertices[t.D])));
        Assert.InRange(
            Math.Abs(outputVolume - fixture.Expected.TetraVolume),
            0,
            fixture.Expected.VolumeTolerance);

        if (fixture.Expected.ExactTetrahedra is not null)
        {
            var expected = fixture.Expected.ExactTetrahedra
                .Select(CanonicalTet)
                .OrderBy(static x => x, StringComparer.Ordinal)
                .ToArray();

            var actual = tets
                .Select(t => CanonicalTet(new[] { t.A, t.B, t.C, t.D }))
                .OrderBy(static x => x, StringComparer.Ordinal)
                .ToArray();

            Assert.Equal(expected, actual);
        }
    }

    private static double SignedVolume(Vector3d p1, Vector3d p2, Vector3d p3, Vector3d p4)
    {
        var a = p2 - p1;
        var b = p3 - p1;
        var c = p4 - p1;
        return Vector3d.Dot(a, Vector3d.Cross(b, c)) / 6d;
    }

    private static string CanonicalTet(IReadOnlyList<int> tet)
    {
        if (tet.Count != 4)
        {
            throw new ArgumentException("Tetrahedron must contain exactly 4 indices.", nameof(tet));
        }

        var values = tet.ToArray();
        Array.Sort(values);
        return string.Join("-", values);
    }
}
