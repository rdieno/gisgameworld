using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RotateOperation : IShapeGrammarOperation
{
    private Vector3 rotation;
    private CoordSystem coordSystem;

    public RotateOperation(Vector3 rotation, CoordSystem coordSystem)
    {
        this.rotation = rotation;
        this.coordSystem = coordSystem;
    }

    public static Shape Rotate(Shape shape, Vector3 rotation, CoordSystem coordSystem = CoordSystem.Local)
    {
        Mesh mesh = shape.Mesh;
        LocalTransform lt = shape.LocalTransform;

        Vector3[] vertices = mesh.vertices;
        
        Quaternion quatRotation = Quaternion.Euler(rotation.x, rotation.y, rotation.z);

        for (int i = 0; i < vertices.Length; i++)
        {
            if(coordSystem == CoordSystem.Local)
            {
                Vector3 current = vertices[i] - lt.Origin;

                current = Quaternion.AngleAxis(rotation.x, lt.Right) * current;
                current = Quaternion.AngleAxis(rotation.y, lt.Up) * current;
                current = Quaternion.AngleAxis(rotation.z, lt.Forward) * current;

                vertices[i] = current + lt.Origin;
            }
            else
            {
                vertices[i] = quatRotation * vertices[i];
            }
        }

        mesh.vertices = vertices;

        if (coordSystem == CoordSystem.Local)
        {
            lt.Right = Quaternion.AngleAxis(rotation.x, lt.Right) * lt.Right;
            lt.Right = Quaternion.AngleAxis(rotation.y, lt.Up) * lt.Right;
            lt.Right = Quaternion.AngleAxis(rotation.z, lt.Forward) * lt.Right;

            lt.Up = Quaternion.AngleAxis(rotation.x, lt.Right) * lt.Up;
            lt.Up = Quaternion.AngleAxis(rotation.y, lt.Up) * lt.Up;
            lt.Up = Quaternion.AngleAxis(rotation.z, lt.Forward) * lt.Up;

            lt.Forward = Quaternion.AngleAxis(rotation.x, lt.Right) * lt.Forward;
            lt.Forward = Quaternion.AngleAxis(rotation.y, lt.Up) * lt.Forward;
            lt.Forward = Quaternion.AngleAxis(rotation.z, lt.Forward) * lt.Forward;
        }
        else
        {
            lt.Origin = quatRotation * lt.Origin;
            lt.Right = quatRotation * lt.Right;
            lt.Up = quatRotation * lt.Up;
            lt.Forward = quatRotation * lt.Forward;
        }
        
        return new Shape(mesh, lt);
    }

    //public List<Shape> PerformOperation(List<Shape> shapes)
    //{
    //    List<Shape> output = new List<Shape>();

    //    foreach (Shape shape in shapes)
    //    {
    //        output.Add(Rotate(shape, rotation, coordSystem));
    //    }

    //    return output;
    //}

    ShapeWrapper IShapeGrammarOperation.PerformOperation(List<Shape> input)
    {
        List<Shape> output = new List<Shape>();

        foreach (Shape shape in input)
        {
            output.Add(Rotate(shape, rotation, coordSystem));
        }

        return new ShapeWrapper(output);
    }

    //public static Shape Rotate(Shape shape, Vector3 rotation, CoordSystem coordSystem = CoordSystem.Local)
    //{
    //    Mesh mesh = shape.Mesh;
    //    LocalTransform lt = shape.LocalTransform;

    //    //float avgX = (lt.Right.x + lt.Up.x + lt.Forward.x) / 3f;
    //    //float avgY = (lt.Right.y + lt.Up.y + lt.Forward.y) / 3f;
    //    //float avgZ = (lt.Right.z + lt.Up.z + lt.Forward.z) / 3f;

    //    //Vector3 avg = Vector3.zero;
    //    //avg = new Vector3(avgX, avgY, avgZ).normalized;

    //    //Debug.DrawLine(lt.Origin, lt.Origin + (avg), Color.yellow, 1000f);

    //    Vector3[] vertices = mesh.vertices;

    //    //Vector3 origin = MathUtility.FarthestPointInDirection(vertices, -avg);
    //    //Vector3 scaledOrigin = new Vector3(origin.x * scale.x, origin.y * scale.y, origin.z * scale.z);

    //    //Vector3 originalOrigin = lt.Origin;




    //    Quaternion rightRotation = Quaternion.identity;
    //    Quaternion upRotation = Quaternion.identity;
    //    Quaternion forwardRotation = Quaternion.identity;

    //    Vector3 up = lt.Up;
    //    Vector3 right = lt.Right;
    //    Vector3 forward = lt.Forward;

    //    if (up != Vector3.up)
    //    {
    //        upRotation = Quaternion.FromToRotation(up, Vector3.up);
    //        up = upRotation * up;
    //        right = upRotation * right;
    //        forward = upRotation * forward;
    //    }

    //    if (right != Vector3.right)
    //    {
    //        rightRotation = Quaternion.FromToRotation(right, Vector3.right);
    //        right = rightRotation * right;
    //        forward = rightRotation * forward;
    //    }

    //    if (forward != Vector3.forward)
    //    {
    //        forwardRotation = Quaternion.FromToRotation(forward, Vector3.forward);
    //        forward = forwardRotation * forward;
    //    }

    //    Quaternion invForwardRotation = Quaternion.Inverse(forwardRotation);
    //    Quaternion invRightRotation = Quaternion.Inverse(rightRotation);
    //    Quaternion invUpRotation = Quaternion.Inverse(upRotation);

    //    Quaternion quatRotation = Quaternion.Euler(rotation.x, rotation.y, rotation.z);

    //    for (int i = 0; i < vertices.Length; i++)
    //    {
    //        Vector3 current = vertices[i] - lt.Origin;

    //        current = upRotation * current;
    //        current = rightRotation * current;
    //        current = forwardRotation * current;

    //        current = quatRotation * current;

    //        current = invForwardRotation * current;
    //        current = invRightRotation * current;
    //        current = invUpRotation * current;

    //        vertices[i] = current + lt.Origin;
    //    }

    //    //Vector3 newRight = lt.Right - lt.Origin;

    //    //newRight = upRotation * newRight;
    //    //newRight = rightRotation * newRight;
    //    //newRight = forwardRotation * newRight;

    //    //newRight = quatRotation * newRight;

    //    //newRight = invForwardRotation * newRight;
    //    //newRight = invRightRotation * newRight;
    //    //newRight = invUpRotation * newRight;

    //    //newRight = newRight + lt.Origin;

    //    //lt.Right = newRight;

    //    //lt.Right = quatRotation * lt.Right;
    //    //lt.Up = quatRotation * lt.Up;
    //    //lt.Forward = quatRotation * lt.Forward;

    //    lt.Right = Quaternion.AngleAxis(rotation.x, lt.Right) * lt.Right;
    //    lt.Right = Quaternion.AngleAxis(rotation.y, lt.Up) * lt.Right;
    //    lt.Right = Quaternion.AngleAxis(rotation.z, lt.Forward) * lt.Right;

    //    lt.Up = Quaternion.AngleAxis(rotation.x, lt.Right) * lt.Up;
    //    lt.Up = Quaternion.AngleAxis(rotation.y, lt.Up) * lt.Up;
    //    lt.Up = Quaternion.AngleAxis(rotation.z, lt.Forward) * lt.Up;

    //    lt.Forward = Quaternion.AngleAxis(rotation.x, lt.Right) * lt.Forward;
    //    lt.Forward = Quaternion.AngleAxis(rotation.y, lt.Up) * lt.Forward;
    //    lt.Forward = Quaternion.AngleAxis(rotation.z, lt.Forward) * lt.Forward;

    //    mesh.vertices = vertices;

    //    //mesh.RecalculateBounds();
    //    //lt.Origin = mesh.bounds.center;

    //    return new Shape(mesh, lt);
    //}

    //public static Shape Rotate(Shape shape, Vector3 rotation, CoordSystem coordSystem = CoordSystem.Local)
    //{
    //    Mesh mesh = shape.Mesh;
    //    LocalTransform lt = shape.LocalTransform;

    //    Quaternion quatRotation = Quaternion.Euler(rotation.x, rotation.y, rotation.z);

    //    Vector3[] vertices = mesh.vertices;
    //    for (int i = 0; i < vertices.Length; i++)
    //    {
    //        if (coordSystem == CoordSystem.Local)
    //        {
    //            vertices[i] = Quaternion.AngleAxis(rotation.x, lt.Right) * vertices[i];
    //            vertices[i] = Quaternion.AngleAxis(rotation.y, lt.Up) * vertices[i];
    //            vertices[i] = Quaternion.AngleAxis(rotation.z, lt.Forward) * vertices[i];
    //        }
    //        else
    //        {
    //            vertices[i] = quatRotation * vertices[i];
    //        }
    //    }

    //    mesh.vertices = vertices;

    //    if (coordSystem == CoordSystem.Local)
    //    {
    //        lt.Origin = Quaternion.AngleAxis(rotation.x, lt.Right) * lt.Origin;
    //        lt.Origin = Quaternion.AngleAxis(rotation.y, lt.Up) * lt.Origin;
    //        lt.Origin = Quaternion.AngleAxis(rotation.z, lt.Forward) * lt.Origin;

    //        lt.Right = Quaternion.AngleAxis(rotation.x, lt.Right) * lt.Right;
    //        lt.Right = Quaternion.AngleAxis(rotation.y, lt.Up) * lt.Right;
    //        lt.Right = Quaternion.AngleAxis(rotation.z, lt.Forward) * lt.Right;

    //        lt.Up = Quaternion.AngleAxis(rotation.x, lt.Right) * lt.Up;
    //        lt.Up = Quaternion.AngleAxis(rotation.y, lt.Up) * lt.Up;
    //        lt.Up = Quaternion.AngleAxis(rotation.z, lt.Forward) * lt.Up;

    //        lt.Forward = Quaternion.AngleAxis(rotation.x, lt.Right) * lt.Forward;
    //        lt.Forward = Quaternion.AngleAxis(rotation.y, lt.Up) * lt.Forward;
    //        lt.Forward = Quaternion.AngleAxis(rotation.z, lt.Forward) * lt.Forward;
    //    }
    //    else
    //    {
    //        lt.Origin = quatRotation * lt.Origin;
    //        lt.Right = quatRotation * lt.Right;
    //        lt.Up = quatRotation * lt.Up;
    //        lt.Forward = quatRotation * lt.Forward;
    //    }

    //    return new Shape(mesh, lt);
    //}

    //public static Shape Rotate(Shape shape, Vector3 rotation, CoordSystem coordSystem = CoordSystem.Local)
    //{
    //    Mesh mesh = shape.Mesh;
    //    LocalTransform lt = shape.LocalTransform;

    //    Quaternion quatRotation = Quaternion.Euler(rotation.x, rotation.y, rotation.z);

    //    Vector3[] vertices = mesh.vertices;
    //    for (int i = 0; i < vertices.Length; i++)
    //    {
    //        vertices[i] = quatRotation * vertices[i];
    //    }

    //    mesh.vertices = vertices;

    //    lt.Origin = quatRotation * lt.Origin;
    //    lt.Right = quatRotation * lt.Right;
    //    lt.Up = quatRotation * lt.Up;
    //    lt.Forward = quatRotation * lt.Forward;

    //    return new Shape(mesh, lt);
    //}
}
