using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TranslateOperation : IShapeGrammarOperation
{
    private Vector3 distance;
    private CoordSystem coordSystem;

    public TranslateOperation(Vector3 distance, CoordSystem coordSystem)
    {
        this.distance = distance;
        this.coordSystem = coordSystem;
    }

    public static Shape Translate(Shape shape, Vector3 distance, CoordSystem coordSystem = CoordSystem.Local)
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

    ShapeWrapper IShapeGrammarOperation.PerformOperation(List<Shape> input)
    {
        List<Shape> output = new List<Shape>();

        bool test = true;
        LocalTransform originalTransform = null;
        List<bool> tests = new List<bool>();

        foreach (Shape shape in input)
        {
            if (test)
            {
                originalTransform = new LocalTransform(shape.LocalTransform);
            }

            Shape result = Translate(shape, distance, coordSystem);

            if (test)
            {
                bool positionCompare = ComparePositions(originalTransform, result.LocalTransform);
                tests.Add(positionCompare);
            }

            output.Add(result);
        }

        if (test)
        {
            List<OperationTest> operationTests = new List<OperationTest>();
            operationTests.Add(new OperationTest("translate", "part 1", tests));
            return new ShapeWrapper(output, operationTests);
        }

        return new ShapeWrapper(output);
    }
    
    bool ComparePositions(LocalTransform original, LocalTransform toTest)
    {
        Vector3 origin = original.Origin;

        if (coordSystem == CoordSystem.Local)
        {
            origin += distance.x * original.Right;
            origin += distance.y * original.Up;
            origin += distance.z * original.Forward;
        }
        else 
        {
            origin += distance;
        }

        return (origin == toTest.Origin);
    }
}

