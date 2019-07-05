using ClipperLib;
using g3;
using System;
using System.Collections.Generic;
using UnityEngine;

public class TaperOperation
{
    // input should be a single face
    public static Shape Taper(Shape shape, float yAmount, float xzAmount = 0f)
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
            Debug.Log("Taper: found zero or > 1 loops: " + loops.Count);
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
            Vector3 vertex = (Vector3)dmesh.GetVertex(loopVertexIndicies[i]);
            loopVertices.Add(vertex);

            originalLoop.Add(new IntPoint(vertex.x * scalingFactor, vertex.z * scalingFactor));
        }

        // Debug draw the original loop
        //for (int i = 0; i < loopVertices.Count; i++)
        //{
        //    Vector3 p0 = loopVertices[i];
        //    Vector3 p1 = loopVertices[MathUtility.ClampListIndex(i + 1, loopVertices.Count)];

        //    Debug.DrawLine(p0, p1, Color.green, 1000f);
        //}


        // Find offset using clipper library
        ClipperOffset co = new ClipperOffset();
        co.AddPath(originalLoop, JoinType.jtMiter, EndType.etClosedPolygon);
        co.Execute(ref deflatedLoop, -xzAmount * scalingFactor);
        co.Execute(ref nondeflatedLoop, 0.0);

        // make sure we've found exactly 1 loop
        if (deflatedLoop.Count != 1 || nondeflatedLoop.Count != 1)
        {
            Debug.Log("Taper: clipper offset did not produce a useable loop, it's possible that the offset is too big");
            return null;
        }

        List<Vector3> taperedVerticesTest = new List<Vector3>();
        List<Vector3> nonTaperedVerticesTest = new List<Vector3>();

        // convert tapered loop to vector3's and translate by y amount
        for (int i = 0; i < deflatedLoop[0].Count; i++)
        {
            taperedVerticesTest.Add(new Vector3(deflatedLoop[0][i].X / (float)scalingFactor, loopVertices[0].y + yAmount, deflatedLoop[0][i].Y / (float)scalingFactor));
        }

        // convert nontapered loop to vector3's and don't translate by y so we can match the original loop
        // this allows us to find the correct starting vertex to build walls
        for (int i = 0; i < nondeflatedLoop[0].Count; i++)
        {
            nonTaperedVerticesTest.Add(new Vector3(nondeflatedLoop[0][i].X / (float)scalingFactor, loopVertices[0].y, nondeflatedLoop[0][i].Y / (float)scalingFactor));
        }

        //// debug draw the tapered loop
        //for (int i = 0; i < taperedVerticesTest.Count; i++)
        //{
        //    Vector3 p0 = taperedVerticesTest[i];
        //    Vector3 p1 = taperedVerticesTest[MathUtility.ClampListIndex(i + 1, taperedVerticesTest.Count)];

        //    Debug.DrawLine(p0, p1, Color.green, 1000f);
        //}

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


        // reverse original edge loop so they follow the same rotation (CW/CCW) as tapered loop
        List<Vector3> reversedOriginalVertices = new List<Vector3>(loopVertices);
        reversedOriginalVertices.Reverse();

        // defalted vertices do not preserve ordering
        // find closest vertex and record index
        float minDistance = float.MaxValue;
        int closestPointIndex = -1;

        Vector3 firstOriginalVertex = reversedOriginalVertices[0];

        for (int i = 0; i < reversedOriginalVertices.Count; i++)
        {
            Vector3 p1 = nonTaperedVerticesTest[i];

            float distance = Vector3.Distance(firstOriginalVertex, p1);

            if (distance < minDistance)
            {
                minDistance = distance;
                closestPointIndex = i;
            }
        }


        // for each edge pair, triangulate a new face
        for (int i = 0; i < loopVertices.Count; i++)
        {
            Vector3 p0 = reversedOriginalVertices[i];
            Vector3 p1 = reversedOriginalVertices[MathUtility.ClampListIndex(i + 1, loopVertices.Count)];

            Vector3 p2 = taperedVerticesTest[MathUtility.ClampListIndex(i + closestPointIndex, loopVertices.Count)];
            Vector3 p3 = taperedVerticesTest[MathUtility.ClampListIndex(i + closestPointIndex + 1, loopVertices.Count)];

            Triangle t0 = new Triangle(p0, p2, p1);
            Triangle t1 = new Triangle(p2, p3, p1);

            List<Triangle> newFaceTriangles = new List<Triangle>() { t0, t1 };
            Mesh newFace = BuildingUtility.TrianglesToMesh(newFaceTriangles, true);

            // calculate new normals for face
            Vector3 normal = -Vector3.Cross((p1 - p0).normalized, (p2 - p0).normalized);

            // set the new normal for all vertices
            Vector3[] normals = newFace.normals;
            for (int j = 0; j < normals.Length; j++)
            {
                normals[j] = normal;
            }
            newFace.normals = normals;

            faces.Add(newFace);
        }

        // create top face
        List<Triangle> topFaceTriangles = Triangulator.TriangulatePolygon(taperedVerticesTest);

        Mesh topFace = BuildingUtility.TrianglesToMesh(topFaceTriangles, true);
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
