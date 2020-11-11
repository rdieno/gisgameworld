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
        
        Vector3[] vertices = mesh.vertices;

        Vector3 origin = MathUtility.FarthestPointInDirection(vertices, -avg);

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

    ShapeWrapper IShapeGrammarOperation.PerformOperation(List<Shape> input)//, bool test)
    {
        List <Shape> output = new List<Shape>();

        bool test = true;
        Shape originalShape = null;
        List<bool> tests = new List<bool>();

        foreach (Shape shape in input)
        {
            if (test)
            {
                originalShape = new Shape(shape);
            }

            Shape result = Scale(shape, scale);

            if(test)
            {
                bool sizeCompare = CompareSizes(originalShape, result);
                tests.Add(sizeCompare);
            }

            output.Add(result);
        }

        if(test)
        {
            List<OperationTest> operationTests = new List<OperationTest>();
            operationTests.Add(new OperationTest("scale", "part 1", tests));
            return new ShapeWrapper(output, operationTests);
        }

        return new ShapeWrapper(output);
    }

    bool CompareSizes(Shape original, Shape processed)
    {
        Mesh mesh = original.Mesh;
        LocalTransform lt = original.LocalTransform;
        
        float avgX = (lt.Right.x + lt.Up.x + lt.Forward.x) / 3f;
        float avgY = (lt.Right.y + lt.Up.y + lt.Forward.y) / 3f;
        float avgZ = (lt.Right.z + lt.Up.z + lt.Forward.z) / 3f;

        Vector3 avg = Vector3.zero;
        avg = new Vector3(avgX, avgY, avgZ).normalized;

        Vector3[] vertices = mesh.vertices;

        Vector3 origin = MathUtility.FarthestPointInDirection(vertices, -avg);

        Quaternion rightRotation = Quaternion.identity;
        Quaternion upRotation = Quaternion.identity;
        Quaternion forwardRotation = Quaternion.identity;

        Vector3 up = lt.Up;
        Vector3 right = lt.Right;
        Vector3 forward = lt.Forward;

        if (up != Vector3.up)
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

        bool compareResult = processed.Mesh.bounds.size == mesh.bounds.size;
        return compareResult;
    }

}
