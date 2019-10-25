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

    //public List<Shape> PerformOperation(List<Shape> shapes)
    //{
    //    List<Shape> output = new List<Shape>();

    //    foreach(Shape shape in shapes)
    //    {
    //        output.Add(Translate(shape, distance, coordSystem));
    //    }

    //    return output;
    //}

    ShapeWrapper IShapeGrammarOperation.PerformOperation(List<Shape> input)
    {
        List<Shape> output = new List<Shape>();

        foreach (Shape shape in input)
        {
            output.Add(Translate(shape, distance, coordSystem));
        }

        return new ShapeWrapper(output);
    }
}
