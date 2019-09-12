using UnityEngine;
using System.Collections;

public class RotateOperation
{
    public static Shape Rotate(Shape shape, Vector3 rotation)
    {
        Mesh mesh = shape.Mesh;
        LocalTransform lt = shape.LocalTransform;

        Quaternion quatRotation = Quaternion.Euler(rotation.x, rotation.y, rotation.z);

        Vector3[] vertices = mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = quatRotation * vertices[i];
        }

        mesh.vertices = vertices;

        lt.Origin = quatRotation * lt.Origin;
        lt.Right = quatRotation * lt.Right;
        lt.Up = quatRotation * lt.Up;
        lt.Forward = quatRotation * lt.Forward;

        return new Shape(mesh, lt);
    }
}
