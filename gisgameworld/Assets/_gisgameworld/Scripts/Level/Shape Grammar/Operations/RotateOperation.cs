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
        Vector3[] normals = mesh.normals;
        
        Quaternion quatRotation = Quaternion.Euler(rotation.x, rotation.y, rotation.z);

        for (int i = 0; i < vertices.Length; i++)
        {
            if(coordSystem == CoordSystem.Local)
            {
                Vector3 currentVertex = vertices[i] - lt.Origin;
                Vector3 currentNormal = normals[i];

                currentVertex = Quaternion.AngleAxis(rotation.x, lt.Right) * currentVertex;
                currentVertex = Quaternion.AngleAxis(rotation.y, lt.Up) * currentVertex;
                currentVertex = Quaternion.AngleAxis(rotation.z, lt.Forward) * currentVertex;

                currentNormal = Quaternion.AngleAxis(rotation.x, lt.Right) * currentNormal;
                currentNormal = Quaternion.AngleAxis(rotation.y, lt.Up) * currentNormal;
                currentNormal = Quaternion.AngleAxis(rotation.z, lt.Forward) * currentNormal;

                vertices[i] = currentVertex + lt.Origin;
                normals[i] = currentNormal;
            }
            else
            {
                vertices[i] = quatRotation * vertices[i];
                normals[i] = quatRotation * normals[i];
            }
        }

        mesh.vertices = vertices;
        mesh.normals = normals;

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
    
    ShapeWrapper IShapeGrammarOperation.PerformOperation(List<Shape> input)
    {
        List<Shape> output = new List<Shape>();
        
        bool test = true;
        LocalTransform originalTransform = null;
        List<bool> tests = new List<bool>();

        foreach (Shape shape in input)
        {
            if(test)
            {
                originalTransform = new LocalTransform(shape.LocalTransform);
            }
            
            Shape result = Rotate(shape, rotation, coordSystem);

            if (test)
            {
                bool rotationCompare = CompareRotations(originalTransform, result.LocalTransform);
                tests.Add(rotationCompare);
            }
            
            output.Add(result);
        }

        if (test)
        {
            List<OperationTest> operationTests = new List<OperationTest>();
            operationTests.Add(new OperationTest("rotate", "part 1", tests));
            return new ShapeWrapper(output, operationTests);
        }

        return new ShapeWrapper(output);
    }


    bool CompareRotations(LocalTransform original, LocalTransform toTest)
    {
        LocalTransform lt = original;

        Quaternion quatRotation = Quaternion.Euler(rotation.x, rotation.y, rotation.z);

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

        bool testResult = true;

        testResult = lt.Origin == toTest.Origin;
        testResult = lt.Forward == toTest.Forward;
        testResult = lt.Up == toTest.Up;
        testResult = lt.Right == toTest.Right;

        return testResult;

    }



}
