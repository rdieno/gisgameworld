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
    public static Shape Taper(Shape shape, float yAmount, float xzAmount = 0f, int steps = 5, int increaseAttempts = 100, float amountIncrement = 0.25f, float amountIncreaseRatio = 0.9f)
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
                Vector3 vertex = MathUtility.ConvertToVector3(dmesh.GetVertex(loopVertexIndicies[j]));
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

            // triangulate side faces

            // convert both inner and outer vertices to polygon2d
            List<Vector2d> innerv2d = new List<Vector2d>();
            List<Vector2d> outerv2d = new List<Vector2d>();
            for (int j = 0; j < taperedVerticesTest.Count; j++)
            {
                Vector3 v0 = taperedVerticesTest[j];
                innerv2d.Add(new Vector2d(v0.x, v0.z));
            }

            for (int j = 0; j < nonTaperedVerticesTest.Count; j++)
            {
                Vector3 v0 = nonTaperedVerticesTest[j];
                outerv2d.Add(new Vector2d(v0.x, v0.z));
            }
            
            if(taperedVerticesTest.Count != nonTaperedVerticesTest.Count)
            {
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
                        Vector3 v0 = MathUtility.ConvertToVector3(sideFacesDMesh.GetVertex(edgeIndices[k]));

                        float y = nonTaperedVerticesTest[0].y;

                        if (j == smallerEdgeLoopIndex)
                        {
                            y += stepYAmount;
                        }

                        Vector3 v1 = new Vector3(v0.x, y, v0.y);

                        sideFacesDMesh.SetVertex(edgeIndices[k], (Vector3d) v1);
                    }
                }

                // determine normals of side faces
                List<Triangle> sideFaceTriangles = new List<Triangle>();

                foreach (Index3i triangleIndex in sideFacesDMesh.Triangles())
                {
                    Vector3 v0 = MathUtility.ConvertToVector3(sideFacesDMesh.GetVertex(triangleIndex.a));
                    Vector3 v1 = MathUtility.ConvertToVector3(sideFacesDMesh.GetVertex(triangleIndex.b));
                    Vector3 v2 = MathUtility.ConvertToVector3(sideFacesDMesh.GetVertex(triangleIndex.c));

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
                sameEdgeCountFound = true;

                // reverse original edge loop so they follow the same rotation (CW/CCW) as tapered loop
                List<Vector3> reversedOriginalVertices = new List<Vector3>(nonTaperedVerticesTest);

                // deflated vertices do not preserve ordering
                // find closest vertex and record index
                float minDistance = float.MaxValue;
                int closestPointIndex = -1;

                Vector3 firstOriginalVertex = reversedOriginalVertices[0];

                for (int j = 0; j < nonTaperedVerticesTest.Count; j++)
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

                Mesh[] sideFaces = new Mesh[nonTaperedVerticesTest.Count];

                if(sideFacesToConsolidate == null && !finalLoop)
                {
                    sideFacesToConsolidate = new List<Mesh>[nonTaperedVerticesTest.Count];
                    for(int j = 0; j < nonTaperedVerticesTest.Count; j++)
                    {
                        sideFacesToConsolidate[j] = new List<Mesh>();
                    }
                }
                
                // for each edge pair, triangulate a new face
                for (int j = 0; j < nonTaperedVerticesTest.Count; j++)
                {
                    Vector3 p0 = reversedOriginalVertices[j];
                    Vector3 p1 = reversedOriginalVertices[MathUtility.ClampListIndex(j + 1, reversedOriginalVertices.Count)];

                    Vector3 p2 = taperedVerticesTest[MathUtility.ClampListIndex(j + closestPointIndex, taperedVerticesTest.Count)];
                    Vector3 p3 = taperedVerticesTest[MathUtility.ClampListIndex(j + closestPointIndex + 1, taperedVerticesTest.Count)];

                    p2.y += stepYAmount;
                    p3.y += stepYAmount;

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

                    if(finalLoop)
                    {
                        faces.Add(newFace);
                    }
                    else
                    {
                        sideFacesToConsolidate[j].Add(newFace);
                    }
                }

                Mesh welded = BuildingUtility.TrianglesToMesh(currentSideFaceTriangles, true);

                currentStepMesh = welded;
            }
        }

        if (sameEdgeCountFound && !finalLoop)
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

        for (int i = 0; i < taperedVerticesTest.Count; i++)
        {
            taperedVerticesTest[i] = new Vector3(taperedVerticesTest[i].x, taperedVerticesTest[i].y + stepYAmount, taperedVerticesTest[i].z);
        }

        Mesh topFace = Triangulator.TriangulatePolygon(taperedVerticesTest, shape.LocalTransform.Up);
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

        simplifiedFace = mw.Weld();

        Vector3 faceNormal = simplifiedFace.normals[0];

        DMesh3 simplifiedFaceDMesh = g3UnityUtils.UnityMeshToDMesh(simplifiedFace);

        MeshBoundaryLoops loopFinder = new MeshBoundaryLoops(simplifiedFaceDMesh);
        EdgeLoop edgeLoop = loopFinder.Loops[0];
        int[] edgeLoopIndices = edgeLoop.Vertices;

        List<Vector3> edgeLoopVertices = new List<Vector3>();

        for (int k = 0; k < edgeLoopIndices.Length; k++)
        {
            edgeLoopVertices.Add(MathUtility.ConvertToVector3(simplifiedFaceDMesh.GetVertex(edgeLoopIndices[k])));
        }

        edgeLoopVertices = Triangulator.RemoveUnecessaryVertices(edgeLoopVertices, faceNormal);

        simplifiedFace = Triangulator.TriangulatePolygon(edgeLoopVertices, faceNormal);
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
}
