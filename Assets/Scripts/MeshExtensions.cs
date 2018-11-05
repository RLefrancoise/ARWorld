using UnityEngine;
using System.Linq;

public static class MeshExtensions
{
    public static void FlipNormals(this Mesh mesh)
    {
        mesh.uv = mesh.uv.Select(o => new Vector2(1 - o.x, o.y)).ToArray();
        mesh.triangles = mesh.triangles.Reverse().ToArray();
        mesh.normals = mesh.normals.Select(o => -o).ToArray();
    }
}