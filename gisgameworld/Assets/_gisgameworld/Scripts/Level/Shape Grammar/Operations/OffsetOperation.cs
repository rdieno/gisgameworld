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
        Dictionary<string, Shape> components = new Dictionary<string, Shape>();

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
        //List<Vector3> taperedVertices = new List<Vector3>();

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

        //List<Mesh> outerFaces = new List<Mesh>();

        //for(int i = 0; i < innerVertices.Count; i++)
        //{
        //    Vector3 v0 = innerVertices[i];
        //    Vector3 v1 = innerVertices[MathUtility.ClampListIndex(i+1, innerVertices.Count)];

        //    Debug.DrawLine(v0, v1, Color.yellow, 1000f);


        //    GameObject go = UnityEngine.Object.Instantiate(Resources.Load("YellowCube"), innerVertices[i], Quaternion.identity) as GameObject;
        //}

        //for (int i = 0; i < outerVertices.Count; i++)
        //{
        //    Vector3 v0 = outerVertices[i];
        //    Vector3 v1 = outerVertices[MathUtility.ClampListIndex(i + 1, outerVertices.Count)];

        //    Debug.DrawLine(v0, v1, Color.blue, 1000f);

        //    GameObject go = UnityEngine.Object.Instantiate(Resources.Load("BlueCube"), outerVertices[i], Quaternion.identity) as GameObject;
        //}

        //// reverse original edge loop so they follow the same rotation (CW/CCW) as tapered loop
        //List<Vector3> reversedOriginalVertices = new List<Vector3>(loopVertices);
        //reversedOriginalVertices.Reverse();

        // defalted vertices do not preserve ordering
        // find closest vertex and record index
        //float minDistance = float.MaxValue;
        //int closestPointIndex = -1;

        //Vector3 firstOriginalVertex = reversedOriginalVertices[0];

        //for (int i = 0; i < reversedOriginalVertices.Count; i++)
        //{
        //    Vector3 p1 = outerVertices[i];

        //    float distance = Vector3.Distance(firstOriginalVertex, p1);

        //    if (distance < minDistance)
        //    {
        //        minDistance = distance;
        //        closestPointIndex = i;
        //    }
        //}


        // convert both inner and outer vertices to polygon2d
        List<Vector2d> innerv2d = new List<Vector2d>();
        List<Vector2d> outerv2d = new List<Vector2d>();
        for (int i = 0; i < innerVertices.Count; i++)
        {
            Vector3 v0 = innerVertices[i];
            //Vector3 v1 = innerVertices[MathUtility.ClampListIndex(i+1, innerVertices.Count)];

            //Debug.DrawLine(v0, v1, Color.green, 1000f);

            innerv2d.Add(new Vector2d(v0.x, v0.z));
        }

        for (int i = 0; i < outerVertices.Count; i++)
        {
            Vector3 v0 = outerVertices[i];
            //Vector3 v1 = outerVertices[MathUtility.ClampListIndex(i + 1, outerVertices.Count)];

            //Debug.DrawLine(v0, v1, Color.yellow, 1000f);

            outerv2d.Add(new Vector2d(v0.x, v0.z));
        }

        innerv2d.Reverse();

        GeneralPolygon2d borderPolygon2d = new GeneralPolygon2d(new Polygon2d(outerv2d));
        borderPolygon2d.AddHole(new Polygon2d(innerv2d));

        TriangulatedPolygonGenerator tpg = new TriangulatedPolygonGenerator();
        tpg.Polygon = borderPolygon2d;
        MeshGenerator mGen = tpg.Generate();


        //DMesh3 dmesh2 = mGen.MakeDMesh();

        //MeshBoundaryLoops mbl2 = new MeshBoundaryLoops(dmesh2);

        //for(int i = 1; i < 2; i++)
        //{
        //    EdgeLoop el = mbl2.Loops[i];

        //    bool test = el.IsInternalLoop();

        //    AxisAlignedBox3d bb = el.Mesh.GetBounds();

        //    double size = bb.DiagonalLength;

        //    int f = 34;

        //    int[] edgeverts = el.Vertices;

        //    for (int j = 0; j < edgeverts.Length; j++)
        //    {
        //        Vector3 v0 = (Vector3) dmesh2.GetVertex(edgeverts[j]);
        //        Vector3 v1 = new Vector3(v0.x, 5f, v0.y);


        //        dmesh2.SetVertex(edgeverts[j], v1);
        //    }

        //    for (int j = 0; j < edgeverts.Length; j++)
        //    {
        //        Vector3 v0 = (Vector3) dmesh2.GetVertex(edgeverts[j]);
        //        Vector3 v1 = (Vector3) dmesh2.GetVertex(edgeverts[MathUtility.ClampListIndex(j+1, edgeverts.Length)]);

                

        //        Debug.DrawLine(v0, v1, Color.green, 1000f);
        //    }

        //    int q = 5;
        //}



        Mesh borderFace = g3UnityUtils.DMeshToUnityMesh(mGen.MakeDMesh());

        // for each edge pair, triangulate a new face
        //for (int i = 0; i < loopVertices.Count; i++)
        //{
        //    Vector3 p0 = outerVertices[i];
        //    Vector3 p1 = outerVertices[MathUtility.ClampListIndex(i + 1, loopVertices.Count)];


        //    Vector3 p2 = innerVertices[i];
        //    Vector3 p3 = innerVertices[MathUtility.ClampListIndex(i + 1, loopVertices.Count)];

        //    //Vector3 p2 = innerVertices[MathUtility.ClampListIndex(i + closestPointIndex, loopVertices.Count)];
        //    //Vector3 p3 = innerVertices[MathUtility.ClampListIndex(i + closestPointIndex + 1, loopVertices.Count)];

        //    Triangle t0 = new Triangle(p0, p2, p1);
        //    Triangle t1 = new Triangle(p2, p3, p1);

        //    List<Triangle> newFaceTriangles = new List<Triangle>() { t0, t1 };
        //    Mesh newFace = BuildingUtility.TrianglesToMesh(newFaceTriangles, true);

        //    // calculate new normals for face
        //    Vector3 normal = -Vector3.Cross((p1 - p0).normalized, (p2 - p0).normalized);

        //    // set the new normal for all vertices
        //    Vector3[] normals = newFace.normals;
        //    for (int j = 0; j < normals.Length; j++)
        //    {
        //        normals[j] = normal;
        //    }
        //    newFace.normals = normals;

        //    outerFaces.Add(newFace);
        //}

        //Mesh borderFace = BuildingUtility.CombineMeshes(outerFaces);


        // create inside face
        //List<Triangle> insideFaceTriangles = Triangulator.TriangulatePolygon(innerVertices);

        //for (int i = 0; i < insideFaceTriangles.Count; i++)
        //{ 
        //    //if (!insideFaceTriangles[i].IsClockwise(lt.Up))
        //    if (!insideFaceTriangles[i].IsClockwise(Vector3.up))
        //    {
        //        insideFaceTriangles[i].ChangeOrientation();
        //    }
        //}

        //Mesh insideFace = BuildingUtility.TrianglesToMesh(insideFaceTriangles, true);
        Mesh insideFace = Triangulator.TriangulatePolygon(innerVertices, shape.LocalTransform.Up);

        //List<Vector3> testVerts = new List<Vector3>();

        //int[] testIndices = insideFace.triangles;

        //for(int i = 0; i < testIndices.Length; i++)
        //{
        //    testVerts.Add(insideFace.vertices[testIndices[i]]); 
        //}

        //bool clockwise = BuildingUtility.isPolygonClockwiseZ(testVerts, lt.Up);
        //Debug.Log("clockwise:  " + clockwise);

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

        components.Add("Inside", new Shape(insideFace, lt));
        components.Add("Border", new Shape(borderFace, lt));
        //components.Add("Border", new Shape(new Mesh(), lt));

        return components;
    }

    ShapeWrapper IShapeGrammarOperation.PerformOperation(List<Shape> input)
    {
        Dictionary<string, List<Shape>> output = new Dictionary<string, List<Shape>>();

        foreach (Shape shape in input)
        {
            Dictionary<string, Shape> currentResult = Offset(shape, amount);

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

        return new ShapeWrapper(output, true);
    }

    //public static Dictionary<string, Shape> Offset_BACKUP(Shape shape, float amount)
    //{
    //    Dictionary<string, Shape> components = new Dictionary<string, Shape>();

    //    // get the original mesh
    //    Mesh originalMesh = shape.Mesh;
    //    LocalTransform lt = shape.LocalTransform;

    //    // flatten if face is not pointing directly upwards
    //    bool flattened = false;
    //    Quaternion rotation = Quaternion.identity;
    //    Vector3[] vertices = originalMesh.vertices;
    //    if (lt.Up != Vector3.up)
    //    {
    //        rotation = Quaternion.FromToRotation(lt.Up, Vector3.up);

    //        for (int k = 0; k < vertices.Length; k++)
    //        {
    //            vertices[k] = rotation * vertices[k];
    //        }

    //        flattened = true;
    //        originalMesh.vertices = vertices;
    //    }

    //    // find edge loop
    //    DMesh3 dmesh = g3UnityUtils.UnityMeshToDMesh(originalMesh);
    //    MeshBoundaryLoops mbl = new MeshBoundaryLoops(dmesh);
    //    List<EdgeLoop> loops = mbl.Loops;
    //    if (loops.Count != 1)
    //    {
    //        Debug.Log("Taper: found zero or > 1 loops: " + loops.Count);
    //        return null;
    //    }

    //    // get edge loop vertices 
    //    EdgeLoop loop = loops[0];
    //    int[] loopVertexIndicies = loop.Vertices;
    //    List<Vector3> loopVertices = new List<Vector3>();
    //    //List<Vector3> taperedVertices = new List<Vector3>();

    //    List<IntPoint> originalLoop = new List<IntPoint>();
    //    List<List<IntPoint>> deflatedLoop = new List<List<IntPoint>>();
    //    List<List<IntPoint>> nondeflatedLoop = new List<List<IntPoint>>();

    //    // use scaling factor to convert floats to int, int64 adds more precision
    //    Int64 scalingFactor = 100000000000;
    //    for (int i = 0; i < loopVertexIndicies.Length; i++)
    //    {
    //        Vector3 vertex = (Vector3)dmesh.GetVertex(loopVertexIndicies[i]);
    //        loopVertices.Add(vertex);

    //        originalLoop.Add(new IntPoint(vertex.x * scalingFactor, vertex.z * scalingFactor));
    //    }

    //    // Find offset using clipper library
    //    ClipperOffset co = new ClipperOffset();
    //    co.AddPath(originalLoop, JoinType.jtMiter, EndType.etClosedPolygon);

    //    if(amount >= 0.0f)
    //    {
    //        co.Execute(ref deflatedLoop, 0.0);
    //        co.Execute(ref nondeflatedLoop, amount * scalingFactor);
    //    }
    //    else
    //    {
    //        co.Execute(ref deflatedLoop, amount * scalingFactor);
    //        co.Execute(ref nondeflatedLoop, 0.0);
    //    }

    //    List<Vector3> innerVertices = new List<Vector3>();
    //    List<Vector3> outerVertices = new List<Vector3>();

    //    // convert tapered loop to vector3's and translate by y amount
    //    for (int i = 0; i < deflatedLoop[0].Count; i++)
    //    {
    //        innerVertices.Add(new Vector3(deflatedLoop[0][i].X / (float)scalingFactor, loopVertices[0].y, deflatedLoop[0][i].Y / (float)scalingFactor));
    //    }

    //    // convert nontapered loop to vector3's and don't translate by y so we can match the original loop
    //    // this allows us to find the correct starting vertex to build walls
    //    for (int i = 0; i < nondeflatedLoop[0].Count; i++)
    //    {
    //        outerVertices.Add(new Vector3(nondeflatedLoop[0][i].X / (float)scalingFactor, loopVertices[0].y, nondeflatedLoop[0][i].Y / (float)scalingFactor));
    //    }

    //    List<Mesh> outerFaces = new List<Mesh>();

    //    //// reverse original edge loop so they follow the same rotation (CW/CCW) as tapered loop
    //    //List<Vector3> reversedOriginalVertices = new List<Vector3>(loopVertices);
    //    //reversedOriginalVertices.Reverse();

    //    // defalted vertices do not preserve ordering
    //    // find closest vertex and record index
    //    //float minDistance = float.MaxValue;
    //    //int closestPointIndex = -1;

    //    //Vector3 firstOriginalVertex = reversedOriginalVertices[0];

    //    //for (int i = 0; i < reversedOriginalVertices.Count; i++)
    //    //{
    //    //    Vector3 p1 = outerVertices[i];

    //    //    float distance = Vector3.Distance(firstOriginalVertex, p1);

    //    //    if (distance < minDistance)
    //    //    {
    //    //        minDistance = distance;
    //    //        closestPointIndex = i;
    //    //    }
    //    //}

    //    TriangulatedPolygonGenerator tpg = new TriangulatedPolygonGenerator();



    //    // for each edge pair, triangulate a new face
    //    for (int i = 0; i < loopVertices.Count; i++)
    //    {
    //        Vector3 p0 = outerVertices[i];
    //        Vector3 p1 = outerVertices[MathUtility.ClampListIndex(i + 1, loopVertices.Count)];


    //        Vector3 p2 = innerVertices[i];
    //        Vector3 p3 = innerVertices[MathUtility.ClampListIndex(i + 1, loopVertices.Count)];

    //        //Vector3 p2 = innerVertices[MathUtility.ClampListIndex(i + closestPointIndex, loopVertices.Count)];
    //        //Vector3 p3 = innerVertices[MathUtility.ClampListIndex(i + closestPointIndex + 1, loopVertices.Count)];

    //        Triangle t0 = new Triangle(p0, p2, p1);
    //        Triangle t1 = new Triangle(p2, p3, p1);

    //        List<Triangle> newFaceTriangles = new List<Triangle>() { t0, t1 };
    //        Mesh newFace = BuildingUtility.TrianglesToMesh(newFaceTriangles, true);

    //        // calculate new normals for face
    //        Vector3 normal = -Vector3.Cross((p1 - p0).normalized, (p2 - p0).normalized);

    //        // set the new normal for all vertices
    //        Vector3[] normals = newFace.normals;
    //        for (int j = 0; j < normals.Length; j++)
    //        {
    //            normals[j] = normal;
    //        }
    //        newFace.normals = normals;

    //        outerFaces.Add(newFace);
    //    }

    //    Mesh borderFace = BuildingUtility.CombineMeshes(outerFaces);


    //    // create inside face
    //    List<Triangle> insideFaceTriangles = Triangulator.TriangulatePolygon(innerVertices);
    //    Mesh insideFace = BuildingUtility.TrianglesToMesh(insideFaceTriangles, true);


    //    // reverse flatten
    //    if (flattened)
    //    {
    //        vertices = borderFace.vertices;

    //        Quaternion invRotation = Quaternion.Inverse(rotation);

    //        for (int k = 0; k < vertices.Length; k++)
    //        {
    //            vertices[k] = invRotation * vertices[k];
    //        }

    //        borderFace.vertices = vertices;


    //        vertices = insideFace.vertices;

    //        for (int k = 0; k < vertices.Length; k++)
    //        {
    //            vertices[k] = invRotation * vertices[k];
    //        }

    //        insideFace.vertices = vertices;
    //    }

    //    components.Add("Inside", new Shape(insideFace, lt));
    //    components.Add("Border", new Shape(borderFace, lt));

    //    return components;
    //}
}
