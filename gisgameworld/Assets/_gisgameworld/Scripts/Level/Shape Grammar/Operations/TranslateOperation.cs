using UnityEngine;
using System.Collections;

public class TranslateOperation
{

    public static Shape Translate(Shape shape, Vector3 distance, CoordSystem coordSystem)
    {
        Mesh mesh = shape.Mesh;
        LocalTransform lt = shape.LocalTransform;

        Vector3[] vertices = mesh.vertices;

        for(int i = 0; i < vertices.Length; i++)
        {
            if(coordSystem == CoordSystem.Local)
            {
                vertices[i] += distance.x * lt.Right;
                vertices[i] += distance.y * lt.Up;
                vertices[i] += distance.z * lt.Forward;
            }
            else
            {
                vertices[i] += distance;
            }

        }

        mesh.vertices = vertices;

        if (coordSystem == CoordSystem.Local)
        {
            lt.Origin += distance.x * lt.Right;
            lt.Origin += distance.y * lt.Up;
            lt.Origin += distance.z * lt.Forward;
        }
        else
        {
            lt.Origin += distance;
        }
        

        return new Shape(mesh, lt);
    }
}
