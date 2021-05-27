using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using g3;
using ClipperLib;
using System;

public class OffsetOperation : IShapeGrammarOperation
{
    private float amount;
    Dictionary<string, string> componentNames;

    public OffsetOperation(float amount, Dictionary<string, string> componentNames)
    {
        this.amount = amount;
        this.componentNames = componentNames;
    }

    public Dictionary<string, Shape> Offset(Shape shape, float amount, int increaseAttempts = 200, float amountIncrement = 0.25f)
    {
        // get the original mesh
        Mesh originalMesh = shape.Mesh;
        LocalTransform lt = shape.LocalTransform;

        // flatten if face is not pointing directly upwards
        bool flattened = false; 
        Quaternion rotation = Quaternion.identity;
        Vector3[] vertices = originalMesh.vertices;
        if (lt.Up != Vector3.up)
        {
            rotation = Quaternion.FromToRotation(lt.Up, Vector3.up);

            for (int k = 0; k < vertices.Length; k++)
            {
                vertices[k] = rotation * vertices[k];
            }

            flattened = true;
            originalMesh.vertices = vertices;
        }

        // find edge loop
        DMesh3 dmesh = g3UnityUtils.UnityMeshToDMesh(originalMesh);
        MeshBoundaryLoops mbl = new MeshBoundaryLoops(dmesh);
        List<EdgeLoop> loops = mbl.Loops;
        if (loops.Count != 1)
        {
            Debug.Log("Offset: found zero or > 1 loops: " + loops.Count);
            return null;
        }

        // get edge loop vertices 
        EdgeLoop loop = loops[0];
        int[] loopVertexIndicies = loop.Vertices;
        List<Vector3> loopVertices = new List<Vector3>();

        List<IntPoint> originalLoop = new List<IntPoint>();
        List<List<IntPoint>> deflatedLoop = new List<List<IntPoint>>();
        List<List<IntPoint>> nondeflatedLoop = new List<List<IntPoint>>();

        // use scaling factor to convert floats to int, int64 adds more precision
        Int64 scalingFactor = 100000000000;
        for (int i = 0; i < loopVertexIndicies.Length; i++)
        {
            Vector3 vertex = MathUtility.ConvertToVector3(dmesh.GetVertex(loopVertexIndicies[i]));
            loopVertices.Add(vertex);

            originalLoop.Add(new IntPoint(vertex.x * scalingFactor, vertex.z * scalingFactor));
        }

        // Find offset using clipper library
        ClipperOffset co = new ClipperOffset();
        co.AddPath(originalLoop, JoinType.jtMiter, EndType.etClosedPolygon);

        if(amount >= 0.0f)
        {
            co.Execute(ref deflatedLoop, 0.0);
            co.Execute(ref nondeflatedLoop, amount * scalingFactor);
        }
        else
        {
            co.Execute(ref deflatedLoop, amount * scalingFactor);
            co.Execute(ref nondeflatedLoop, 0.0);
        }
        
        int attempt = 0;

        while ((deflatedLoop.Count < 1 || nondeflatedLoop.Count < 1) && attempt < increaseAttempts)
        {
            Debug.Log("Offset Operation: loop not found. offset too large or too small");

            // attempt to increase amount
            if (amount >= 0.0f)
            {
                amount -= amountIncrement;

                co.Execute(ref deflatedLoop, 0.0);
                co.Execute(ref nondeflatedLoop, amount * scalingFactor);
            }
            else
            {
                amount += amountIncrement;

                co.Execute(ref deflatedLoop, amount * scalingFactor);
                co.Execute(ref nondeflatedLoop, 0.0);
            }

            attempt++;
        }

        if(attempt > 0)
        {
            Debug.Log("Offset Operation: Offset successful in: " + attempt + " attempts. With an amount of: " + amount);
        }

        List <Vector3> innerVertices = new List<Vector3>();
        List<Vector3> outerVertices = new List<Vector3>();

        // convert tapered loop to vector3's and translate by y amount
        for (int i = 0; i < deflatedLoop[0].Count; i++)
        {
            innerVertices.Add(new Vector3(deflatedLoop[0][i].X / (float)scalingFactor, loopVertices[0].y, deflatedLoop[0][i].Y / (float)scalingFactor));
        }

        // convert nontapered loop to vector3's and don't translate by y so we can match the original loop
        // this allows us to find the correct starting vertex to build walls
        for (int i = 0; i < nondeflatedLoop[0].Count; i++)
        {
            outerVertices.Add(new Vector3(nondeflatedLoop[0][i].X / (float)scalingFactor, loopVertices[0].y, nondeflatedLoop[0][i].Y / (float)scalingFactor));
        }
        
        // convert both inner and outer vertices to polygon2d
        List<Vector2d> innerv2d = new List<Vector2d>();
        List<Vector2d> outerv2d = new List<Vector2d>();
        for (int i = 0; i < innerVertices.Count; i++)
        {
            Vector3 v0 = innerVertices[i];
            innerv2d.Add(new Vector2d(v0.x, v0.z));
        }

        for (int i = 0; i < outerVertices.Count; i++)
        {
            Vector3 v0 = outerVertices[i];
            outerv2d.Add(new Vector2d(v0.x, v0.z));
        }

        innerv2d.Reverse();

        GeneralPolygon2d borderPolygon2d = new GeneralPolygon2d(new Polygon2d(outerv2d));
        borderPolygon2d.AddHole(new Polygon2d(innerv2d));

        TriangulatedPolygonGenerator tpg = new TriangulatedPolygonGenerator();
        tpg.Polygon = borderPolygon2d;
        MeshGenerator mGen = tpg.Generate();
        
        Mesh borderFace = g3UnityUtils.DMeshToUnityMesh(mGen.MakeDMesh());
        
        Mesh insideFace = Triangulator.TriangulatePolygon(innerVertices, shape.LocalTransform.Up);
        
        // reverse process of converting to polygon2d
        vertices = borderFace.vertices;
        Vector3[] normals = borderFace.normals;
        for (int k = 0; k < vertices.Length; k++)
        {
            vertices[k] = new Vector3(vertices[k].x, innerVertices[0].y, vertices[k].y);
            normals[k] = lt.Up;
        }
        borderFace.vertices = vertices;
        borderFace.normals = normals;

        // reverse flatten
        if (flattened)
        {
            vertices = borderFace.vertices;

            Quaternion invRotation = Quaternion.Inverse(rotation);

            for (int k = 0; k < vertices.Length; k++)
            {
                vertices[k] = invRotation * vertices[k];
            }

            borderFace.vertices = vertices;

            vertices = insideFace.vertices;

            for (int k = 0; k < vertices.Length; k++)
            {
                vertices[k] = invRotation * vertices[k];
            }

            insideFace.vertices = vertices;
        }

        Dictionary<string, Shape> components = new Dictionary<string, Shape>();

        components.Add("Inside", new Shape(insideFace, lt));
        components.Add("Border", new Shape(borderFace, lt));

        return components;
    }

    ShapeWrapper IShapeGrammarOperation.PerformOperation(List<Shape> input)
    {
        Dictionary<string, List<Shape>> output = new Dictionary<string, List<Shape>>();

        bool test = true;
        Shape insideShape = null;
        Shape borderShape = null;
        List<bool> part1results = new List<bool>();
        
        foreach (Shape shape in input)
        {
            Dictionary<string, Shape> currentResult = Offset(shape, amount);

            if(test)
            {
                bool foundInsideKey = currentResult.ContainsKey("Inside");
                if (foundInsideKey)
                {
                    insideShape = currentResult["Inside"];
                }

                bool foundBorderKey = currentResult.ContainsKey("Border");
                if (foundBorderKey)
                {
                    borderShape = currentResult["Border"];
                }

                bool borderTest = CheckIfInnerVerticesWithinBorder(insideShape, borderShape);
                part1results.Add(borderTest);
            }
            
            foreach (KeyValuePair<string, Shape> component in currentResult)
            {
                if (output.ContainsKey(componentNames[component.Key]))
                {
                    output[componentNames[component.Key]].Add(component.Value);
                }
                else
                {
                    output.Add(componentNames[component.Key], new List<Shape>() { component.Value });
                }
            }
        }

        if (test)
        {
            List<OperationTest> operationTests = new List<OperationTest>();
            operationTests.Add(new OperationTest("offset", "part 1", part1results));
            return new ShapeWrapper(output, operationTests);
        }

        return new ShapeWrapper(output, true);
    }

    private bool CheckIfInnerVerticesWithinBorder(Shape inside, Shape border)
    {
        // make polygon out of border shape outer vertices
        Mesh borderMesh = border.Mesh;

        // flatten if face is not pointing directly upwards
        //bool flattened = false;
        Quaternion rotation = Quaternion.identity;
        Vector3[] insideVertices = inside.Vertices;
        Vector3[] borderVertices = border.Vertices;
        LocalTransform insideTransform = inside.LocalTransform;
        if (insideTransform.Up != Vector3.up)
        {
            rotation = Quaternion.FromToRotation(insideTransform.Up, Vector3.up);

            for (int k = 0; k < insideVertices.Length; k++)
            {
                insideVertices[k] = rotation * insideVertices[k];
                borderVertices[k] = rotation * borderVertices[k];
            }

            //flattened = true;
            //originalMesh.vertices = vertices;
            borderMesh.vertices = borderVertices;
        }
        

        // find largest edge loop
        DMesh3 dmesh = g3UnityUtils.UnityMeshToDMesh(borderMesh);

        MeshBoundaryLoops mbl = new MeshBoundaryLoops(dmesh);

        if (mbl.Loops.Count < 1)
        {
            Debug.Log("Offset Operation: Test: found zero loops: " + mbl.Loops.Count);
            return false;
        }

        int largestEdgeLoopIndex = FindLargestEdgeLoopIndex(dmesh, mbl);

        EdgeLoop loop = mbl.Loops[largestEdgeLoopIndex];
        int[] loopVertexIndicies = loop.Vertices;

        List<Vector3> outerLoopVertices = new List<Vector3>();

        for (int i = 0; i < loopVertexIndicies.Length; i++)
        {
            Vector3 vertex = MathUtility.ConvertToVector3(dmesh.GetVertex(loopVertexIndicies[i]));
            outerLoopVertices.Add(vertex);
        }


        // check if inside vertices reside within the 2d polygon created from the border shape
        bool result = true;

        foreach(Vector3 innerPoint in insideVertices)
        {
            bool withinPolygonTest = MathUtility.IsPointInPolygonZ(innerPoint, outerLoopVertices);
            if (!withinPolygonTest)
            {
                result = false;
            }

        }


        return result;
    }

    private int FindLargestEdgeLoopIndex(DMesh3 mesh, MeshBoundaryLoops meshBoundaryLoops)
    {
        int largestEdgeLoopIndex = 0;
        double maxValue = Double.MinValue;

        for (int j = 0; j < meshBoundaryLoops.Loops.Count; j++)
        {
            EdgeLoop elTest = meshBoundaryLoops.Loops[j];

            List<Vector2d> edgeVertices = new List<Vector2d>();

            int[] edgeIndices = elTest.Vertices;

            for (int k = 0; k < elTest.Vertices.Length; k++)
            {
                Vector3d v0 = mesh.GetVertex(edgeIndices[k]);
                edgeVertices.Add(new Vector2d(v0.x, v0.y));
            }

            Polygon2d p2d = new Polygon2d(edgeVertices);

            double diagonalLength = p2d.GetBounds().DiagonalLength;

            if (diagonalLength > maxValue)
            {
                largestEdgeLoopIndex = j;
                maxValue = diagonalLength;
            }
        }

        return largestEdgeLoopIndex;
    }
}
