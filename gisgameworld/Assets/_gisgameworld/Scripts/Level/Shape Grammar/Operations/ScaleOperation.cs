using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScaleOperation : IShapeGrammarOperation
{
    private Vector3 scale;

    public ScaleOperation(Vector3 scale)
    {
        this.scale = scale;
    }

    public static Shape Scale(Shape shape, Vector3 scale)
    {
        Mesh mesh = shape.Mesh;
        LocalTransform lt = shape.LocalTransform;

        float avgX = (lt.Right.x + lt.Up.x + lt.Forward.x) / 3f;
        float avgY = (lt.Right.y + lt.Up.y + lt.Forward.y) / 3f;
        float avgZ = (lt.Right.z + lt.Up.z + lt.Forward.z) / 3f;

        Vector3 avg = Vector3.zero;
        avg = new Vector3(avgX, avgY, avgZ).normalized;

        //Debug.DrawLine(lt.Origin, lt.Origin + (avg), Color.yellow, 1000f);

        Vector3[] vertices = mesh.vertices;

        Vector3 origin = MathUtility.FarthestPointInDirection(vertices, -avg);
        Vector3 scaledOrigin = new Vector3(origin.x * scale.x, origin.y * scale.y, origin.z * scale.z);

        Quaternion rightRotation = Quaternion.identity;
        Quaternion upRotation = Quaternion.identity;
        Quaternion forwardRotation = Quaternion.identity;

        Vector3 up = lt.Up;
        Vector3 right = lt.Right;
        Vector3 forward = lt.Forward;

        if(up != Vector3.up)
        {
            upRotation = Quaternion.FromToRotation(up, Vector3.up);
            up = upRotation * up;
            right = upRotation * right;
            forward = upRotation * forward;
        }

        if (right != Vector3.right)
        {
            rightRotation = Quaternion.FromToRotation(right, Vector3.right);
            right = rightRotation * right;
            forward = rightRotation * forward;
        }

        if (forward != Vector3.forward)
        {
            forwardRotation = Quaternion.FromToRotation(forward, Vector3.forward);
            forward = forwardRotation * forward;
        }

        Quaternion invForwardRotation = Quaternion.Inverse(forwardRotation);
        Quaternion invRightRotation = Quaternion.Inverse(rightRotation);
        Quaternion invUpRotation = Quaternion.Inverse(upRotation);

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 current = vertices[i] - origin;

            current = upRotation * current;
            current = rightRotation * current;
            current = forwardRotation * current;

            current.x *= scale.x;
            current.y *= scale.y;
            current.z *= scale.z;

            current = invForwardRotation * current;
            current = invRightRotation * current;
            current = invUpRotation * current;

            vertices[i] = current + origin;
        }

        mesh.vertices = vertices;

        mesh.RecalculateBounds();
        lt.Origin = mesh.bounds.center;

        return new Shape(mesh, lt);
    }

    //public List<Shape> PerformOperation(List<Shape> shapes)
    //{
    //    List<Shape> output = new List<Shape>();

    //    foreach (Shape shape in shapes)
    //    {
    //        output.Add(Scale(shape, scale));
    //    }

    //    return output;
    //}

    ShapeWrapper IShapeGrammarOperation.PerformOperation(List<Shape> input)
    {
        List<Shape> output = new List<Shape>();

        foreach (Shape shape in input)
        {
            output.Add(Scale(shape, scale));
        }

        return new ShapeWrapper(output);
    }
}
