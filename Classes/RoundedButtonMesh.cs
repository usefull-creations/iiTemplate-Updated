using System.Collections.Generic;
using UnityEngine;

namespace StupidTemplate.Classes;

// Hey template user! Don't worry too much if you don't understand this code.
// It may be quite complicated for a beginner.
// Simply put its basically doing lots of math to create rounded corners.
// I wouldn't mess with this unless you believe you're an 'advanced' developer.
public static class RoundedButtonMesh
{
    private static readonly Dictionary<(Vector3 Size, float Radius, int Segments), Mesh> MeshCache = new();

    public static void Apply(GameObject target, Vector3 size, float radius, int segments)
    {
        radius   = Mathf.Clamp(radius,   0.001f, Mathf.Min(size.y, size.z) / 2f);
        segments = Mathf.Clamp(segments, 1,      12);

        (Vector3 Size, float Radius, int Segments) key = (size, radius, segments);

        if (!MeshCache.TryGetValue(key, out Mesh mesh))
        {
            mesh = CreateMesh(size, radius, segments);
            MeshCache.Add(key, mesh);
        }

        MeshFilter meshFilter = target.GetComponent<MeshFilter>();
        meshFilter.sharedMesh = mesh;

        BoxCollider collider = target.GetComponent<BoxCollider>();

        if (collider != null)
            collider.size = size;

        target.transform.localScale = Vector3.one;
    }

    private static Mesh CreateMesh(Vector3 size, float radius, int segments)
    {
        float halfX = size.x / 2f;
        float halfY = size.y / 2f;
        float halfZ = size.z / 2f;

        List<Vector2> ring = new();

        AddCorner(
                ring,
                new Vector2(halfY - radius, halfZ - radius),
                radius,
                0f,
                segments);

        AddCorner(
                ring,
                new Vector2(-halfY + radius, halfZ - radius),
                radius,
                90f,
                segments);

        AddCorner(
                ring,
                new Vector2(-halfY + radius, -halfZ + radius),
                radius,
                180f,
                segments);

        AddCorner(
                ring,
                new Vector2(halfY - radius, -halfZ + radius),
                radius,
                270f,
                segments);

        List<Vector3> vertices  = new();
        List<Vector2> uvs       = new();
        List<int>     triangles = new();

        int frontCenter = vertices.Count;
        vertices.Add(new Vector3(halfX, 0f, 0f));
        uvs.Add(new Vector2(0.5f, 0.5f));

        int frontStart = vertices.Count;

        foreach (Vector2 point in ring)
        {
            vertices.Add(new Vector3(halfX, point.x, point.y));
            uvs.Add(GetUv(point, size));
        }

        int backCenter = vertices.Count;
        vertices.Add(new Vector3(-halfX, 0f, 0f));
        uvs.Add(new Vector2(0.5f, 0.5f));

        int backStart = vertices.Count;

        foreach (Vector2 point in ring)
        {
            vertices.Add(new Vector3(-halfX, point.x, point.y));
            uvs.Add(GetUv(point, size));
        }

        for (int i = 0; i < ring.Count; i++)
        {
            int next = (i + 1) % ring.Count;

            triangles.Add(frontCenter);
            triangles.Add(frontStart + i);
            triangles.Add(frontStart + next);

            triangles.Add(backCenter);
            triangles.Add(backStart + next);
            triangles.Add(backStart + i);

            triangles.Add(frontStart + i);
            triangles.Add(backStart  + i);
            triangles.Add(backStart  + next);

            triangles.Add(frontStart + i);
            triangles.Add(backStart  + next);
            triangles.Add(frontStart + next);
        }

        Mesh mesh = new()
        {
                name = $"{Constants.Guid}RoundedButton_{size.y:0.###}_{size.z:0.###}",
        };

        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0);

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    private static void AddCorner(
            List<Vector2> points,
            Vector2       center,
            float         radius,
            float         startAngle,
            int           segments)
    {
        for (int i = 0; i < segments; i++)
        {
            float progress = i                             / (float)segments;
            float angle    = (startAngle + progress * 90f) * Mathf.Deg2Rad;

            points.Add(new Vector2(
                    center.x + Mathf.Cos(angle) * radius,
                    center.y + Mathf.Sin(angle) * radius));
        }
    }

    private static Vector2 GetUv(Vector2 point, Vector3 size) =>
            new(
                    point.x / size.y + 0.5f,
                    point.y / size.z + 0.5f);
}