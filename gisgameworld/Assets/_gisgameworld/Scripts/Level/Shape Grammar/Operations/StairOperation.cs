using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StairOperation : IShapeGrammarOperation
{
    private Direction direction;
    private int stairCount;

    public StairOperation(Direction direction, int stairCount)
    {
        this.direction = direction;
        this.stairCount = stairCount;
    }
    
    // builds a stair set outwards from the input face
    // input shape should be a single face
    // direction points toward to bottom of stairs
    public static Shape Stair(Shape shape, int stairCount, Vector3 direction)
    {
        Mesh originalMesh = shape.Mesh;
        Mesh sideFaceMesh = shape.Mesh;
        LocalTransform lt = shape.LocalTransform;
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

        Mesh bottomFace = BuildingUtility.TrianglesToMesh(bottomFaceTriangles, true);

        Vector3[] bottomFaceNormals = bottomFace.normals;

        for (int i = 0; i < bottomFaceNormals.Length; i++)
        {
            bottomFaceNormals[i] = -lt.Up;
        }

        bottomFace.normals = bottomFaceNormals;

        faces.Add(bottomFace);

        // create side face from copy of bottom face vertices
        Vector3[] sideFaceVertices = originalMesh.vertices;
        Vector3[] sideFaceNormals = originalMesh.normals;

        // find max value on local direction
        Vector3 maxDir = MathUtility.FarthestPointInDirection(sideFaceVertices, direction);
        Vector3 midMaxDir = Vector3.Project(maxDir - lt.Origin, -direction) + lt.Origin;

        Vector3 angleAxis = (midMaxDir - maxDir).normalized;
        Quaternion rotation = Quaternion.FromToRotation(lt.Up, direction);

        for (int i = 0; i < sideFaceVertices.Length; i++)
        {
            sideFaceVertices[i] = sideFaceVertices[i] - midMaxDir;
            sideFaceVertices[i] = rotation * sideFaceVertices[i] + midMaxDir;
            sideFaceNormals[i] = rotation * sideFaceNormals[i];
        }

        sideFaceMesh.vertices = sideFaceVertices;
        sideFaceMesh.normals = sideFaceNormals;
        faces.Add(sideFaceMesh);

        // make stairs

        // find min value on local direction
        Vector3 minDir = MathUtility.FarthestPointInDirection(originalVertices, -direction);
        Vector3 midMinDir = Vector3.Project(minDir - lt.Origin, -direction) + lt.Origin;

        float width = Vector3.Distance(minDir, midMinDir) * 2;

        Vector3 frontAxis = (midMinDir - minDir).normalized;
        Vector3 backAxis = (midMaxDir - maxDir).normalized;

        Vector3 otherFrontVertex = minDir + (width * frontAxis);
        Vector3 otherBackVertex = maxDir + (width * backAxis);
        
        Vector3 right = Vector3.Cross(direction, lt.Up);
        
        Vector3[] backVertices = new Vector3[] { maxDir, otherBackVertex };
        Vector3[] frontVertices = new Vector3[] { minDir, otherFrontVertex };

        Vector3 v0 = MathUtility.FarthestPointInDirection(backVertices, right);
        Vector3 v1 = MathUtility.FarthestPointInDirection(backVertices, -right);

        Vector3 v2 = MathUtility.FarthestPointInDirection(frontVertices, right);
        Vector3 v3 = MathUtility.FarthestPointInDirection(frontVertices, -right);

        // determine side vertices
        Vector3 edge0 = v0 - v2;
        Vector3 back = edge0.normalized;

        Vector3 edge1 = v1 - v3;
        
        List<Vector3> sideVertices0 = new List<Vector3>();
        List<Vector3> sideVertices1 = new List<Vector3>();
        
        sideVertices0.Add(v0);
        sideVertices0.Add(v2);
        sideVertices1.Add(v1);
        sideVertices1.Add(v3);

        // calc stair length/height
        float spanLength = Vector3.Distance(v0, v2);
        float stairLength = spanLength / stairCount;

        Vector3 refVector0 = v2;
        Vector3 refVector1 = v3;

        // build the top and front faces of the stairs
        for (int i = 0; i < stairCount; i++)
        {
            Vector3 stairV0 = refVector0 + (stairLength * lt.Up);
            Vector3 stairV1 = stairV0 + (stairLength * back);

            Vector3 stairV2 = refVector1 + (stairLength * lt.Up);
            Vector3 stairV3 = stairV2 + (stairLength * back);
            
            List<Vector3> stairFrontFaceVertices = new List<Vector3>() { refVector0, stairV0, stairV2, refVector1};
            List<Vector3> stairTopFaceVertices = new List<Vector3>() { stairV0, stairV1, stairV3, stairV2 };

            Mesh stairFrontFace = Triangulator.TriangulatePolygon(stairFrontFaceVertices, -direction);
            Mesh stairTopFace = Triangulator.TriangulatePolygon(stairTopFaceVertices, lt.Up);

            faces.Add(stairFrontFace);
            faces.Add(stairTopFace);

            refVector0 = stairV1;
            refVector1 = stairV3;

            // save these vertices to construct the side faces later
            sideVertices0.Add(stairV0);
            sideVertices0.Add(stairV1);

            sideVertices1.Add(stairV2);
            sideVertices1.Add(stairV3);
        }
      
        if(BuildingUtility.isPolygonClockwise(sideVertices0))
        {
            sideVertices0.Reverse();
        }

        if (BuildingUtility.isPolygonClockwise(sideVertices1))
        {
            sideVertices1.Reverse();
        }
        
        Mesh sideFace0 = Triangulator.TriangulatePolygon(sideVertices0, right);
        Mesh sideFace1 = Triangulator.TriangulatePolygon(sideVertices1, -right);

        faces.Add(sideFace0);
        faces.Add(sideFace1);
        
        Mesh finalMesh = BuildingUtility.CombineMeshes(faces);

        finalMesh.RecalculateBounds();
        LocalTransform newLt = new LocalTransform(finalMesh.bounds.center, lt.Up, lt.Forward, lt.Right);
        
        return new Shape(finalMesh, newLt);
    }

    ShapeWrapper IShapeGrammarOperation.PerformOperation(List<Shape> input)
    {
        List<Shape> output = new List<Shape>();

        bool test = true;
        List<bool> part1results = new List<bool>();
        int originalTriangleCount = -1;

        foreach (Shape shape in input)
        {
            if(test)
            {
                originalTriangleCount = (int)(shape.Triangles.Length / 3);
            }
            
            Vector3 direction = shape.LocalTransform.DirectionToVector(this.direction);
            Shape result = Stair(shape, stairCount, direction);

            if(test)
            {
                int processedTriangleCount = (int)(result.Triangles.Length / 3);

                int A = originalTriangleCount * 2;
                int B = stairCount * 4;
                int C = (stairCount * 2) * 2;

                bool testResult = processedTriangleCount == (A + B + C);
                part1results.Add(testResult);
            }
            
            output.Add(result);
        }

        if (test)
        {
            List<OperationTest> operationTests = new List<OperationTest>();
            operationTests.Add(new OperationTest("stair", "part 1", part1results));
            return new ShapeWrapper(output, operationTests);
        }
        
        return new ShapeWrapper(output);
    }
}
