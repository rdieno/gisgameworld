using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BuildingUtility
{
    // combines multiple individual triangles into a single polygonal mesh
    public static Mesh TrianglesToMesh(List<Triangle> geometry)
    {
        List<Mesh> triangleMeshes = new List<Mesh>();

        for (int i = 0; i < geometry.Count; i++)
        {
            Mesh m = new Mesh();

            Triangle t = geometry[i];

            Vector3[] vertices = new Vector3[3];
            int[] triangles = new int[3];
            Vector2[] uv = new Vector2[3];
            Vector3[] normals = new Vector3[3];

            vertices[0] = t.v1.position;
            vertices[1] = t.v2.position;
            vertices[2] = t.v3.position;

            triangles[0] = 0;
            triangles[1] = 1;
            triangles[2] = 2;

            uv[0] = new Vector2(0, 0);
            uv[1] = new Vector2(0, 1);
            uv[2] = new Vector2(1, 0);

            normals[0] = Vector3.up;
            normals[1] = Vector3.up;
            normals[2] = Vector3.up;

            m.vertices = vertices;
            m.triangles = triangles;
            m.uv = uv;
            m.normals = normals;

            triangleMeshes.Add(m);
        }

        return BuildingUtility.CombineMeshes(triangleMeshes);
    }


    // combines mmultiple individual meshes into a single mesh object
    public static Mesh CombineMeshes(List<Mesh> meshes)
    {
        CombineInstance[] combine = new CombineInstance[meshes.Count];
        int i = 0;
        while (i < meshes.Count)
        {
            combine[i].mesh = meshes[i];
            combine[i].transform = Matrix4x4.zero;
            i++;
        }

        Mesh polygon = new Mesh();
        polygon.CombineMeshes(combine, true, false);

        return polygon;
    }

}
