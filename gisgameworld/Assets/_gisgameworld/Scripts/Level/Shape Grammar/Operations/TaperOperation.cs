using ClipperLib;
using g3;
using System;
using System.Collections.Generic;
using UnityEngine;

public class TaperOperation : IShapeGrammarOperation
{
    private float yAmount;
    private float xzAmount;

    public TaperOperation(float yAmount, float xzAmount)
    {
        this.yAmount = yAmount;
        this.xzAmount = xzAmount;
    }

    // input should be a single face
    public static Shape Taper(Shape shape, float yAmount, float xzAmount = 0f, int steps = 20, int increaseAttempts = 100, float amountIncrement = 0.25f, float amountIncreaseRatio = 0.9f)
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

        List<Mesh> faces = new List<Mesh>();

        // flip orientation and normal of bottom face so it points outwards
        Vector3[] originalVertices = originalMesh.vertices;
        Vector3[] originalNormals = originalMesh.normals;
        int[] originalTris = originalMesh.triangles;

        List<Triangle> bottomFaceTriangles = new List<Triangle>();

        // convert to triangles and reverse orientation
        for (int i = 0; i < originalTris.Length - 2; i += 3)
        {
            Vector3 p0 = originalVertices[originalTris[i]];
            Vector3 p1 = originalVertices[originalTris[i + 1]];
            Vector3 p2 = originalVertices[originalTris[i + 2]];
            Triangle t = new Triangle(p0, p1, p2);
            t.ChangeOrientation();
            bottomFaceTriangles.Add(t);
        }

        // flip the normals
        for (int i = 0; i < originalNormals.Length; i++)
        {
            originalNormals[i] = -lt.Up;
        }

        Mesh bottomFace = BuildingUtility.TrianglesToMesh(bottomFaceTriangles, true);
        bottomFace.normals = originalNormals;

        faces.Add(bottomFace);

        List<Vector3> taperedVerticesTest = null;
        List<Vector3> nonTaperedVerticesTest = null;

        float stepYAmount = yAmount / steps;
        float stepXZAmount = xzAmount / steps;
        float originalStepXZAmount = stepXZAmount;
        float startingYPos = lt.Origin.y;

        Mesh currentStepMesh = originalMesh;


        List<Mesh>[] sideFacesToConsolidate = null;
        bool sameEdgeCountFound = false;
        bool finalLoop = false;

        for (int i = 0; i < steps; i++)
        {
            // find largest edge loop
            DMesh3 dmesh = g3UnityUtils.UnityMeshToDMesh(currentStepMesh);
            MeshBoundaryLoops mbl = new MeshBoundaryLoops(dmesh);

            if (mbl.Loops.Count < 1)
            {
                Debug.Log("Taper Operation: found zero loops: " + mbl.Loops.Count);
                return null;
            }

            int smallestEdgeLoopIndex = FindSmallestEdgeLoopIndex(dmesh, mbl);

            // get edge loop vertices 
            EdgeLoop loop = mbl.Loops[smallestEdgeLoopIndex];
            int[] loopVertexIndicies = loop.Vertices;
            List<Vector3> loopVertices = new List<Vector3>();
            //List<Vector3> taperedVertices = new List<Vector3>();

            List<IntPoint> originalLoop = new List<IntPoint>();
            List<List<IntPoint>> deflatedLoop = new List<List<IntPoint>>();
            List<List<IntPoint>> nondeflatedLoop = new List<List<IntPoint>>();

            // use scaling factor to convert floats to int, int64 adds more precision
            Int64 scalingFactor = 100000000000;
            for (int j = 0; j < loopVertexIndicies.Length; j++)
            {
                Vector3 vertex = (Vector3)dmesh.GetVertex(loopVertexIndicies[j]);
                loopVertices.Add(vertex);

                originalLoop.Add(new IntPoint(vertex.x * scalingFactor, vertex.z * scalingFactor));
            }

            // Find offset using clipper library
            ClipperOffset co = new ClipperOffset();
            co.AddPath(originalLoop, JoinType.jtMiter, EndType.etClosedPolygon);
            co.Execute(ref deflatedLoop, -stepXZAmount * scalingFactor);
            co.Execute(ref nondeflatedLoop, 0.0);

            // if loop too small attempt to increase its size until a proper loop is found
            int attempt = 0;
            while ((deflatedLoop.Count < 1 || nondeflatedLoop.Count < 1) && attempt < increaseAttempts)
            {
                Debug.Log("Taper Operation: loop not found. offset too large or too small. Attempting to recalculate");

                // attempt to increase amount
                //xzAmount -= amountIncrement;
                //stepXZAmount -= amountIncrement;
                stepXZAmount *= amountIncreaseRatio;
                co.Execute(ref deflatedLoop, -stepXZAmount * scalingFactor);
                co.Execute(ref nondeflatedLoop, 0.0);

                attempt++;
            }

            if (attempt > 0)
            {
                if(deflatedLoop.Count < 1 || nondeflatedLoop.Count < 1)
                {
                    Debug.Log("Taper Operation: Offset unsuccessful. Could not create loop");
                }
                else
                {
                    Debug.Log("Taper Operation: Offset successful in: " + attempt + " attempts. With an amount of: " + stepXZAmount);

                    if (sameEdgeCountFound)
                    {
                        // consoldidate edges
                        for (int j = 0; j < sideFacesToConsolidate.Length; j++)
                        {
                            List<Mesh> currentSideFaces = sideFacesToConsolidate[j];
                            Mesh simplifiedFace = SimplifyEdgeFace(currentSideFaces);

                            faces.Add(simplifiedFace);
                        }

                        sideFacesToConsolidate = null;
                        sameEdgeCountFound = false;
                    }

                    i = steps - 1;
                    finalLoop = true;

                    float newStepRatio = stepXZAmount / originalStepXZAmount;
                    stepYAmount *= newStepRatio;
                }
            }

            taperedVerticesTest = new List<Vector3>();
            nonTaperedVerticesTest = new List<Vector3>();

            // convert tapered loop to vector3's and translate by y amount
            for (int j = 0; j < deflatedLoop[0].Count; j++)
            {
                taperedVerticesTest.Add(new Vector3(deflatedLoop[0][j].X / (float)scalingFactor, loopVertices[0].y, deflatedLoop[0][j].Y / (float)scalingFactor));
            }

            // convert nontapered loop to vector3's and don't translate by y so we can match the original loop
            // this allows us to find the correct starting vertex to build walls
            for (int j = 0; j < nondeflatedLoop[0].Count; j++)
            {
                nonTaperedVerticesTest.Add(new Vector3(nondeflatedLoop[0][j].X / (float)scalingFactor, loopVertices[0].y, nondeflatedLoop[0][j].Y / (float)scalingFactor));
            }

            //// debug draw the tapered loop
            //for (int i = 0; i < taperedVerticesTest.Count; i++)
            //{
            //    Vector3 p0 = taperedVerticesTest[i];
            //    Vector3 p1 = taperedVerticesTest[MathUtility.ClampListIndex(i + 1, taperedVerticesTest.Count)];

            //    Debug.DrawLine(p0, p1, Color.green, 1000f);
            //}



            // triangulate side faces


            // convert both inner and outer vertices to polygon2d
            List<Vector2d> innerv2d = new List<Vector2d>();
            List<Vector2d> outerv2d = new List<Vector2d>();
            for (int j = 0; j < taperedVerticesTest.Count; j++)
            {
                Vector3 v0 = taperedVerticesTest[j];
                //Vector3 v1 = taperedVerticesTest[MathUtility.ClampListIndex(j + 1, taperedVerticesTest.Count)];

                //if (i == 19)
                //{
                //    Debug.DrawLine(v0, v1, Color.red, 1000f);
                //}

                innerv2d.Add(new Vector2d(v0.x, v0.z));
            }

            for (int j = 0; j < nonTaperedVerticesTest.Count; j++)
            {
                Vector3 v0 = nonTaperedVerticesTest[j];
                //Vector3 v1 = nonTaperedVerticesTest[MathUtility.ClampListIndex(j + 1, nonTaperedVerticesTest.Count)];

                //if (i == 19)
                //{
                //    Debug.DrawLine(v0, v1, Color.green, 1000f);
                //}


                outerv2d.Add(new Vector2d(v0.x, v0.z));
            }
            
            if(taperedVerticesTest.Count != nonTaperedVerticesTest.Count)
            {
                //Debug.Log("Different AMOUNT");

                if(sameEdgeCountFound)
                {
                    // consoldidate edges
                    for (int j = 0; j < sideFacesToConsolidate.Length; j++)
                    {
                        List<Mesh> currentSideFaces = sideFacesToConsolidate[j];
                        Mesh simplifiedFace = SimplifyEdgeFace(currentSideFaces);

                        faces.Add(simplifiedFace);
                    }

                    sideFacesToConsolidate = null;
                    sameEdgeCountFound = false;
                }

                innerv2d.Reverse();

                GeneralPolygon2d borderPolygon2d = new GeneralPolygon2d(new Polygon2d(outerv2d));
                borderPolygon2d.AddHole(new Polygon2d(innerv2d));

                TriangulatedPolygonGenerator tpg = new TriangulatedPolygonGenerator();
                tpg.Polygon = borderPolygon2d;
                MeshGenerator mGen = null;
                mGen = tpg.Generate();

                DMesh3 sideFacesDMesh = mGen.MakeDMesh();

                MeshBoundaryLoops sideFacesMeshBoundaries = new MeshBoundaryLoops(sideFacesDMesh);

                int smallerEdgeLoopIndex = FindSmallestEdgeLoopIndex(sideFacesDMesh, sideFacesMeshBoundaries);

                for (int j = 0; j < sideFacesMeshBoundaries.Loops.Count; j++)
                {
                    EdgeLoop el = sideFacesMeshBoundaries.Loops[j];
                    int[] edgeIndices = el.Vertices;

                    for (int k = 0; k < edgeIndices.Length; k++)
                    {
                        Vector3 v0 = (Vector3)sideFacesDMesh.GetVertex(edgeIndices[k]);

                        float y = nonTaperedVerticesTest[0].y;

                        if (j == smallerEdgeLoopIndex)
                        {
                            y += stepYAmount;
                        }

                        Vector3 v1 = new Vector3(v0.x, y, v0.y);

                        sideFacesDMesh.SetVertex(edgeIndices[k], v1);
                    }
                }

                // determine normals of side faces
                List<Triangle> sideFaceTriangles = new List<Triangle>();

                foreach (Index3i triangleIndex in sideFacesDMesh.Triangles())
                {
                    Vector3 v0 = (Vector3)sideFacesDMesh.GetVertex(triangleIndex.a);
                    Vector3 v1 = (Vector3)sideFacesDMesh.GetVertex(triangleIndex.b);
                    Vector3 v2 = (Vector3)sideFacesDMesh.GetVertex(triangleIndex.c);

                    Triangle t = new Triangle(v0, v1, v2);
                    t.CalculateNormal();
                    sideFaceTriangles.Add(t);
                }


                Mesh sideFace = BuildingUtility.TrianglesToMesh(sideFaceTriangles);

                faces.Add(sideFace);

                currentStepMesh = g3UnityUtils.DMeshToUnityMesh(sideFacesDMesh);
            }
            else
            {
                //Debug.Log("SAME AMOUNT");

                sameEdgeCountFound = true;

                // reverse original edge loop so they follow the same rotation (CW/CCW) as tapered loop
                List<Vector3> reversedOriginalVertices = new List<Vector3>(loopVertices);

                if(i == 0)
                {
                    reversedOriginalVertices.Reverse();
                }
                

                // defalted vertices do not preserve ordering
                // find closest vertex and record index
                float minDistance = float.MaxValue;
                int closestPointIndex = -1;

                Vector3 firstOriginalVertex = reversedOriginalVertices[0];

                for (int j = 0; j < reversedOriginalVertices.Count; j++)
                {
                    Vector3 p1 = nonTaperedVerticesTest[j];

                    float distance = Vector3.Distance(firstOriginalVertex, p1);

                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestPointIndex = j;
                    }
                }

                List<Mesh> currentSideFaces = new List<Mesh>();
                List<Triangle> currentSideFaceTriangles = new List<Triangle>();

                Mesh[] sideFaces = new Mesh[loopVertices.Count];

                if(sideFacesToConsolidate == null && !finalLoop)
                {
                    sideFacesToConsolidate = new List<Mesh>[loopVertices.Count];
                    for(int j = 0; j < loopVertices.Count; j++)
                    {
                        sideFacesToConsolidate[j] = new List<Mesh>();
                    }
                }
                
                // for each edge pair, triangulate a new face
                for (int j = 0; j < loopVertices.Count; j++)
                {
                    Vector3 p0 = reversedOriginalVertices[j];
                    Vector3 p1 = reversedOriginalVertices[MathUtility.ClampListIndex(j + 1, loopVertices.Count)];

                    Vector3 p2 = taperedVerticesTest[MathUtility.ClampListIndex(j + closestPointIndex, loopVertices.Count)];
                    Vector3 p3 = taperedVerticesTest[MathUtility.ClampListIndex(j + closestPointIndex + 1, loopVertices.Count)];

                    p2.y += stepYAmount;
                    p3.y += stepYAmount;

                    //if (i == 19 && j == 0)
                    //{
                    //    GameObject a = UnityEngine.Object.Instantiate(Resources.Load("BlueCube"), p0, Quaternion.identity) as GameObject;
                    //    GameObject b = UnityEngine.Object.Instantiate(Resources.Load("PinkCube"), p1, Quaternion.identity) as GameObject;
                    //    GameObject c = UnityEngine.Object.Instantiate(Resources.Load("YellowCube"), p2, Quaternion.identity) as GameObject;
                    //    GameObject d = UnityEngine.Object.Instantiate(Resources.Load("OrangeCube"), p3, Quaternion.identity) as GameObject;

                    //    a.transform.localScale = new Vector3(.005f, .005f, .005f);
                    //    b.transform.localScale = new Vector3(.005f, .005f, .005f);
                    //    c.transform.localScale = new Vector3(.005f, .005f, .005f);
                    //    d.transform.localScale = new Vector3(.005f, .005f, .005f);
                    //}

                    Triangle t0 = new Triangle(p0, p2, p1);
                    Triangle t1 = new Triangle(p2, p3, p1);

                    List<Triangle> newFaceTriangles = new List<Triangle>() { t0, t1 };
                    Mesh newFace = BuildingUtility.TrianglesToMesh(newFaceTriangles, true, 0.00001f);

                    // calculate new normals for face
                    Vector3 normal = -Vector3.Cross((p1 - p0).normalized, (p2 - p0).normalized);

                    // set the new normal for all vertices
                    Vector3[] normals = newFace.normals;
                    for (int k = 0; k < normals.Length; k++)
                    {
                        normals[k] = normal;
                    }
                    newFace.normals = normals;

                    currentSideFaceTriangles.AddRange(newFaceTriangles);

                    //sideFaces[j] = newFace;

                    if(finalLoop)
                    {
                        faces.Add(newFace);
                    }
                    else
                    {
                        sideFacesToConsolidate[j].Add(newFace);
                    }
                    

                    //faces.Add(newFace);
                    //currentSideFaces.Add(newFace);
                }

                //Mesh completeStep = BuildingUtility.CombineMeshes(currentSideFaces);
                //faces.Add(completeStep);

                //sideFacesToConsolidate.Add(sideFaces);

                // completeStep = BuildingUtility.SimplifyFaces(completeStep);

                Mesh welded = BuildingUtility.TrianglesToMesh(currentSideFaceTriangles, true);

                currentStepMesh = welded;
            }
        }

        if (sameEdgeCountFound && !finalLoop)
        {
            // consoldidate edges
            for (int j = 0; j < sideFacesToConsolidate.Length; j++)
            //for (int j = 0; j < 4; j++)
            {
                List<Mesh> currentSideFaces = sideFacesToConsolidate[j];
               // List<Mesh> currentSideFacesTest = new List<Mesh>();
                //currentSideFacesTest.Add(currentSideFaces[currentSideFaces.Count - 1]);

                //Mesh simplifiedFace = SimplifyEdgeFace(currentSideFacesTest);
                Mesh simplifiedFace = SimplifyEdgeFace(currentSideFaces);

                faces.Add(simplifiedFace);
            }

            sideFacesToConsolidate = null;
            sameEdgeCountFound = false;
        }

        for (int i = 0; i < taperedVerticesTest.Count; i++)
        {
            taperedVerticesTest[i] = new Vector3(taperedVerticesTest[i].x, taperedVerticesTest[i].y + stepYAmount, taperedVerticesTest[i].z);
        }


        //for (int i = 0; i < taperedVerticesTest.Count; i++)
        //{
        //    Vector3 v0 = taperedVerticesTest[i];
        //    Vector3 v1 = taperedVerticesTest[MathUtility.ClampListIndex(i + 1, taperedVerticesTest.Count)];

        //    Debug.DrawLine(v0, v1, Color.yellow, 1000f);
        //}

        // create top face
        List<Triangle> topFaceTriangles = Triangulator.TriangulatePolygon(taperedVerticesTest);

        for(int i = 0; i < topFaceTriangles.Count; i++)
        {
            if(!topFaceTriangles[i].IsClockwise(Vector3.up))
                topFaceTriangles[i].ChangeOrientation();
        }

        Mesh topFace = BuildingUtility.TrianglesToMesh(topFaceTriangles, true, 0.00001f);
        faces.Add(topFace);

        Mesh finalMesh = BuildingUtility.CombineMeshes(faces);
        finalMesh.RecalculateBounds();
        lt.Origin = finalMesh.bounds.center;

        // reverse flatten
        if (flattened)
        {
            vertices = finalMesh.vertices;

            Quaternion invRotation = Quaternion.Inverse(rotation);

            for (int k = 0; k < vertices.Length; k++)
            {
                vertices[k] = invRotation * vertices[k];
            }

            finalMesh.vertices = vertices;
        }


        Shape finalShape = new Shape(finalMesh, lt);
        return finalShape;
    }

    private static Mesh SimplifyEdgeFace(List<Mesh> faces)
    {
        Mesh simplifiedFace = BuildingUtility.CombineMeshes(faces);

        if (simplifiedFace.vertexCount == 4 && simplifiedFace.triangles.Length == 6)
        {
            return simplifiedFace;
        }

        simplifiedFace.uv = null;
        MeshWelder mw = new MeshWelder(simplifiedFace);
        mw.MaxPositionDelta = 0.005f;
        mw.MaxAngleDelta = 0.05f;

        //public float MaxPositionDelta = 0.001f;
        //public float MaxAngleDelta = 0.01f;
        simplifiedFace = mw.Weld();

        Vector3 faceNormal = simplifiedFace.normals[0];

        DMesh3 simplifiedFaceDMesh = g3UnityUtils.UnityMeshToDMesh(simplifiedFace);

        MeshBoundaryLoops loopFinder = new MeshBoundaryLoops(simplifiedFaceDMesh);
        EdgeLoop edgeLoop = loopFinder.Loops[0];
        int[] edgeLoopIndices = edgeLoop.Vertices;

        List<Vector3> edgeLoopVertices = new List<Vector3>();

        for (int k = 0; k < edgeLoopIndices.Length; k++)
        {
            edgeLoopVertices.Add((Vector3)simplifiedFaceDMesh.GetVertex(edgeLoopIndices[k]));
        }

        edgeLoopVertices = Triangulator.RemoveUnecessaryVertices(edgeLoopVertices, faceNormal);

        //Debug.Log(edgeLoopVertices.Count);
        //simplifiedFace = BuildingUtility.SimplifyFaces(simplifiedFace);

        simplifiedFace = Triangulator.TriangulatePolygonNormal(edgeLoopVertices, true, faceNormal);
        return simplifiedFace;
    }

    private static int FindSmallestEdgeLoopIndex(DMesh3 mesh, MeshBoundaryLoops meshBoundaryLoops)
    {
        int smallerEdgeLoopIndex = 0;
        double minValue = Double.MaxValue;

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

            if (diagonalLength < minValue)
            {
                smallerEdgeLoopIndex = j;
                minValue = diagonalLength;
            }
        }

        return smallerEdgeLoopIndex;
    }

    ShapeWrapper IShapeGrammarOperation.PerformOperation(List<Shape> input)
    {
        List<Shape> output = new List<Shape>();

        foreach (Shape shape in input)
        {
            output.Add(Taper(shape, yAmount, xzAmount));
        }

        return new ShapeWrapper(output);
    }

    // input should be a single face
    //public static Shape Taper_BACKUP(Shape shape, float yAmount, float xzAmount = 0f, int increaseAttempts = 100, float amountIncrement = 0.25f)
    //{
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

    //    // Debug draw the original loop
    //    //for (int i = 0; i < loopVertices.Count; i++)
    //    //{
    //    //    Vector3 p0 = loopVertices[i];
    //    //    Vector3 p1 = loopVertices[MathUtility.ClampListIndex(i + 1, loopVertices.Count)];

    //    //    Debug.DrawLine(p0, p1, Color.green, 1000f);
    //    //}


    //    // Find offset using clipper library
    //    ClipperOffset co = new ClipperOffset();
    //    co.AddPath(originalLoop, JoinType.jtMiter, EndType.etClosedPolygon);
    //    co.Execute(ref deflatedLoop, -xzAmount * scalingFactor);
    //    co.Execute(ref nondeflatedLoop, 0.0);

    //    //// make sure we've found exactly 1 loop
    //    //if (deflatedLoop.Count != 1 || nondeflatedLoop.Count != 1)
    //    //{
    //    //    Debug.Log("Taper: clipper offset did not produce a useable loop, it's possible that the offset is too big");
    //    //    return null;
    //    //}

    //    int attempt = 0;

    //    while ((deflatedLoop.Count < 1 || nondeflatedLoop.Count < 1) && attempt < increaseAttempts)
    //    {
    //        Debug.Log("Taper Operation: loop not found. offset too large or too small");

    //        // attempt to increase amount

    //        xzAmount -= amountIncrement;
    //        co.Execute(ref deflatedLoop, -xzAmount * scalingFactor);
    //        co.Execute(ref nondeflatedLoop, 0.0);

    //        attempt++;
    //    }

    //    if (attempt > 0)
    //    {
    //        Debug.Log("Taper Operation: Offset successful in: " + attempt + " attempts. With an amount of: " + xzAmount);
    //    }

    //    List<Vector3> taperedVerticesTest = new List<Vector3>();
    //    List<Vector3> nonTaperedVerticesTest = new List<Vector3>();

    //    // convert tapered loop to vector3's and translate by y amount
    //    for (int i = 0; i < deflatedLoop[0].Count; i++)
    //    {
    //        taperedVerticesTest.Add(new Vector3(deflatedLoop[0][i].X / (float)scalingFactor, loopVertices[0].y + yAmount, deflatedLoop[0][i].Y / (float)scalingFactor));
    //    }

    //    // convert nontapered loop to vector3's and don't translate by y so we can match the original loop
    //    // this allows us to find the correct starting vertex to build walls
    //    for (int i = 0; i < nondeflatedLoop[0].Count; i++)
    //    {
    //        nonTaperedVerticesTest.Add(new Vector3(nondeflatedLoop[0][i].X / (float)scalingFactor, loopVertices[0].y, nondeflatedLoop[0][i].Y / (float)scalingFactor));
    //    }

    //    //// debug draw the tapered loop
    //    //for (int i = 0; i < taperedVerticesTest.Count; i++)
    //    //{
    //    //    Vector3 p0 = taperedVerticesTest[i];
    //    //    Vector3 p1 = taperedVerticesTest[MathUtility.ClampListIndex(i + 1, taperedVerticesTest.Count)];

    //    //    Debug.DrawLine(p0, p1, Color.green, 1000f);
    //    //}

    //    List<Mesh> faces = new List<Mesh>();

    //    // flip orientation and normal of bottom face so it points outwards
    //    Vector3[] originalVertices = originalMesh.vertices;
    //    Vector3[] originalNormals = originalMesh.normals;
    //    int[] originalTris = originalMesh.triangles;

    //    List<Triangle> bottomFaceTriangles = new List<Triangle>();

    //    // convert to triangles and reverse orientation
    //    for (int i = 0; i < originalTris.Length - 2; i += 3)
    //    {
    //        Vector3 p0 = originalVertices[originalTris[i]];
    //        Vector3 p1 = originalVertices[originalTris[i + 1]];
    //        Vector3 p2 = originalVertices[originalTris[i + 2]];
    //        Triangle t = new Triangle(p0, p1, p2);
    //        t.ChangeOrientation();
    //        bottomFaceTriangles.Add(t);
    //    }

    //    // flip the normals
    //    for (int i = 0; i < originalNormals.Length; i++)
    //    {
    //        originalNormals[i] = -lt.Up;
    //    }

    //    Mesh bottomFace = BuildingUtility.TrianglesToMesh(bottomFaceTriangles, true);
    //    bottomFace.normals = originalNormals;

    //    faces.Add(bottomFace);


    //    //// reverse original edge loop so they follow the same rotation (CW/CCW) as tapered loop
    //    //List<Vector3> reversedOriginalVertices = new List<Vector3>(loopVertices);
    //    //reversedOriginalVertices.Reverse();

    //    //// defalted vertices do not preserve ordering
    //    //// find closest vertex and record index
    //    //float minDistance = float.MaxValue;
    //    //int closestPointIndex = -1;

    //    //Vector3 firstOriginalVertex = reversedOriginalVertices[0];

    //    //for (int i = 0; i < reversedOriginalVertices.Count; i++)
    //    //{
    //    //    Vector3 p1 = nonTaperedVerticesTest[i];

    //    //    float distance = Vector3.Distance(firstOriginalVertex, p1);

    //    //    if (distance < minDistance)
    //    //    {
    //    //        minDistance = distance;
    //    //        closestPointIndex = i;
    //    //    }
    //    //}


    //    //// for each edge pair, triangulate a new face
    //    //for (int i = 0; i < loopVertices.Count; i++)
    //    //{
    //    //    Vector3 p0 = reversedOriginalVertices[i];
    //    //    Vector3 p1 = reversedOriginalVertices[MathUtility.ClampListIndex(i + 1, loopVertices.Count)];

    //    //    Vector3 p2 = taperedVerticesTest[MathUtility.ClampListIndex(i + closestPointIndex, loopVertices.Count)];
    //    //    Vector3 p3 = taperedVerticesTest[MathUtility.ClampListIndex(i + closestPointIndex + 1, loopVertices.Count)];

    //    //    Triangle t0 = new Triangle(p0, p2, p1);
    //    //    Triangle t1 = new Triangle(p2, p3, p1);

    //    //    List<Triangle> newFaceTriangles = new List<Triangle>() { t0, t1 };
    //    //    Mesh newFace = BuildingUtility.TrianglesToMesh(newFaceTriangles, true);

    //    //    // calculate new normals for face
    //    //    Vector3 normal = -Vector3.Cross((p1 - p0).normalized, (p2 - p0).normalized);

    //    //    // set the new normal for all vertices
    //    //    Vector3[] normals = newFace.normals;
    //    //    for (int j = 0; j < normals.Length; j++)
    //    //    {
    //    //        normals[j] = normal;
    //    //    }
    //    //    newFace.normals = normals;

    //    //    faces.Add(newFace);
    //    //}



    //    // triangulate side faces


    //    // convert both inner and outer vertices to polygon2d
    //    List<Vector2d> innerv2d = new List<Vector2d>();
    //    List<Vector2d> outerv2d = new List<Vector2d>();
    //    for (int i = 0; i < taperedVerticesTest.Count; i++)
    //    {
    //        Vector3 v0 = taperedVerticesTest[i];
    //        innerv2d.Add(new Vector2d(v0.x, v0.z));
    //    }

    //    for (int i = 0; i < nonTaperedVerticesTest.Count; i++)
    //    {
    //        Vector3 v0 = nonTaperedVerticesTest[i];
    //        outerv2d.Add(new Vector2d(v0.x, v0.z));
    //    }

    //    innerv2d.Reverse();

    //    GeneralPolygon2d borderPolygon2d = new GeneralPolygon2d(new Polygon2d(outerv2d));
    //    borderPolygon2d.AddHole(new Polygon2d(innerv2d));

    //    TriangulatedPolygonGenerator tpg = new TriangulatedPolygonGenerator();
    //    tpg.Polygon = borderPolygon2d;
    //    MeshGenerator mGen = tpg.Generate();

    //    DMesh3 sideFacesDMesh = mGen.MakeDMesh();

    //    MeshBoundaryLoops sideFacesMeshBoundaries = new MeshBoundaryLoops(sideFacesDMesh);

    //    int smallerEdgeLoopIndex = 0;
    //    double minValue = Double.MaxValue;

    //    for (int i = 0; i < sideFacesMeshBoundaries.Loops.Count; i++)
    //    {
    //        EdgeLoop elTest = sideFacesMeshBoundaries.Loops[i];

    //        List<Vector2d> edgeVertices = new List<Vector2d>();

    //        int[] edgeIndicesTest = elTest.Vertices;

    //        for(int j = 0; j < elTest.Vertices.Length; j++)
    //        {
    //            Vector3d v0 = sideFacesDMesh.GetVertex(edgeIndicesTest[j]);
    //            edgeVertices.Add(new Vector2d(v0.x, v0.y));
    //        }

    //        Polygon2d p2d = new Polygon2d(edgeVertices);

    //        double diagonalLength = p2d.GetBounds().DiagonalLength;

    //        if(diagonalLength < minValue)
    //        {
    //            smallerEdgeLoopIndex = i;
    //            minValue = diagonalLength;
    //        }

    //        //int[] edgeverts = el.Vertices;

    //        //for (int j = 0; j < edgeverts.Length; j++)
    //        //{
    //        //    Vector3 v0 = (Vector3)sideFacesDMesh.GetVertex(edgeverts[j]);
    //        //    Vector3 v1 = new Vector3(v0.x, 5f, v0.y);


    //        //    sideFacesDMesh.SetVertex(edgeverts[j], v1);
    //        //}

    //        //for (int j = 0; j < edgeverts.Length; j++)
    //        //{
    //        //    Vector3 v0 = (Vector3)sideFacesDMesh.GetVertex(edgeverts[j]);
    //        //    Vector3 v1 = (Vector3)sideFacesDMesh.GetVertex(edgeverts[MathUtility.ClampListIndex(j + 1, edgeverts.Length)]);



    //        //    Debug.DrawLine(v0, v1, Color.green, 1000f);
    //        //}


    //    }

    //    for (int i = 0; i < sideFacesMeshBoundaries.Loops.Count; i++)
    //    {
    //        EdgeLoop el = sideFacesMeshBoundaries.Loops[i];
    //        int[] edgeIndices = el.Vertices;

    //        for (int j = 0; j < edgeIndices.Length; j++)
    //        {
    //            Vector3 v0 = (Vector3)sideFacesDMesh.GetVertex(edgeIndices[j]);

    //            float y = 0f;

    //            if(i == smallerEdgeLoopIndex)
    //            {
    //                y = yAmount;
    //            }

    //            Vector3 v1 = new Vector3(v0.x, y, v0.y);


    //            sideFacesDMesh.SetVertex(edgeIndices[j], v1);
    //        }
    //    }




    //    //Debug.Log("smallest edge index: " + smallerEdgeLoopIndex);

    //    Mesh sideFaces = g3UnityUtils.DMeshToUnityMesh(sideFacesDMesh);

    //    // reverse process of converting to polygon2d
    //    //vertices = sideFaces.vertices;

    //    //for (int k = 0; k < vertices.Length; k++)
    //    //{
    //    //    vertices[k] = new Vector3(vertices[k].x, 0f, vertices[k].y);
    //    //}
    //    //sideFaces.vertices = vertices;

    //    faces.Add(sideFaces);



    //    // create top face
    //    List<Triangle> topFaceTriangles = Triangulator.TriangulatePolygon(taperedVerticesTest);

    //    Mesh topFace = BuildingUtility.TrianglesToMesh(topFaceTriangles, true);
    //    faces.Add(topFace);

    //    Mesh finalMesh = BuildingUtility.CombineMeshes(faces);
    //    finalMesh.RecalculateBounds();
    //    lt.Origin = finalMesh.bounds.center;


    //    // reverse flatten
    //    if (flattened)
    //    {
    //        vertices = finalMesh.vertices;

    //        Quaternion invRotation = Quaternion.Inverse(rotation);

    //        for (int k = 0; k < vertices.Length; k++)
    //        {
    //            vertices[k] = invRotation * vertices[k];
    //        }

    //        finalMesh.vertices = vertices;
    //    }


    //    Shape finalShape = new Shape(finalMesh, lt);
    //    return finalShape;
    //}

    //public static Shape Taper(Shape shape, float yAmount, float xzAmount = 0f)
    //{
    //    // get the original mesh
    //    Mesh originalMesh = shape.Mesh;
    //    originalMesh.RecalculateBounds();

    //    // find edge loop
    //    DMesh3 dmesh = g3UnityUtils.UnityMeshToDMesh(originalMesh);
    //    MeshBoundaryLoops mbl = new MeshBoundaryLoops(dmesh);
    //    List<EdgeLoop> loops = mbl.Loops;
    //    if(loops.Count != 1)
    //    {
    //        Debug.Log("Taper: found zero or > 1 loops: " + loops.Count);
    //        return null;
    //    }

    //    // get edge loop vertices 
    //    EdgeLoop loop = loops[0];
    //    int[] loopVertexIndicies = loop.Vertices;
    //    List<Vector3> loopVertices = new List<Vector3>();
    //    List<Vector3> taperedVertices = new List<Vector3>();

    //    for (int i = 0; i < loopVertexIndicies.Length; i++)
    //    {
    //        loopVertices.Add((Vector3) dmesh.GetVertex(loopVertexIndicies[i]));
    //    }

    //    // find center
    //    LocalTransform lt = shape.LocalTransform;
    //    Vector3 yOffset = lt.Up * yAmount;
    //    Vector3 topCenter = originalMesh.bounds.center + yOffset;

    //    // move new loop vertices up and inward
    //    for(int i = 0; i < loopVertices.Count; i++)
    //    {
    //        Vector3 towardCenter = (topCenter - loopVertices[i]).normalized;

    //        Vector3 tapered = loopVertices[i];
    //        tapered += (towardCenter * xzAmount);
    //        tapered += yOffset;

    //        taperedVertices.Add(tapered);
    //    }

    //    List<Mesh> faces = new List<Mesh>();

    //    // flip orientation and normal of bottom face so it points outwards
    //    Vector3[] originalVertices = originalMesh.vertices;
    //    Vector3[] originalNormals = originalMesh.normals;
    //    int[] originalTris = originalMesh.triangles;

    //    List<Triangle> bottomFaceTriangles = new List<Triangle>();

    //    // convert to triangles and reverse orientation
    //    for(int i = 0; i < originalTris.Length - 2; i += 3)
    //    {
    //        Vector3 p0 = originalVertices[originalTris[i]];
    //        Vector3 p1 = originalVertices[originalTris[i + 1]];
    //        Vector3 p2 = originalVertices[originalTris[i + 2]];
    //        Triangle t = new Triangle(p0, p1, p2);
    //        t.ChangeOrientation();
    //        bottomFaceTriangles.Add(t);
    //    }

    //    // flip the normals
    //    for(int i = 0; i < originalNormals.Length; i++)
    //    {
    //        originalNormals[i] = -lt.Up;
    //    }

    //    Mesh bottomFace = BuildingUtility.TrianglesToMesh(bottomFaceTriangles, true);
    //    bottomFace.normals = originalNormals;

    //    faces.Add(bottomFace);

    //    // for each edge pair, triangulate a new face
    //    for (int i = 0; i < loopVertices.Count; i++)
    //    {
    //        Vector3 p0 = loopVertices[i];
    //        Vector3 p1 = loopVertices[MathUtility.ClampListIndex(i + 1, loopVertices.Count)];

    //        Vector3 p2 = taperedVertices[i];
    //        Vector3 p3 = taperedVertices[MathUtility.ClampListIndex(i + 1, loopVertices.Count)];

    //        Triangle t0 = new Triangle(p0, p1, p2);
    //        Triangle t1 = new Triangle(p2, p1, p3);

    //        List<Triangle> newFaceTriangles = new List<Triangle>() { t0, t1 };
    //        Mesh newFace = BuildingUtility.TrianglesToMesh(newFaceTriangles, true);

    //        // calculate new normals for face
    //        Vector3 normal = Vector3.Cross((p1 - p0).normalized, (p2 - p0).normalized);

    //        // set the new normal for all vertices
    //        Vector3[] normals = newFace.normals;
    //        for (int j = 0; j < normals.Length; j++)
    //        {
    //            normals[j] = normal;
    //        }
    //        newFace.normals = normals;

    //        faces.Add(newFace);
    //    }

    //    // create top face
    //    List<Triangle> topFaceTriangles = Triangulator.TriangulatePolygonN(taperedVertices, true, lt.Up);
    //    Mesh topFace = BuildingUtility.TrianglesToMesh(topFaceTriangles, true);
    //    faces.Add(topFace);

    //    Mesh finalMesh = BuildingUtility.CombineMeshes(faces);
    //    finalMesh.RecalculateBounds();
    //    lt.Origin = finalMesh.bounds.center;

    //    Shape finalShape = new Shape(BuildingUtility.CombineMeshes(faces), lt);

    //    return finalShape;
    //}
}
