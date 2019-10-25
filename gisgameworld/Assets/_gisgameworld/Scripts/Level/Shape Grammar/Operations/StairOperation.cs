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
        for (int i = 0; i < originalNormals.Length; i++)
        {
            originalNormals[i] = -lt.Up;
        }

        Mesh bottomFace = BuildingUtility.TrianglesToMesh(bottomFaceTriangles, true);
        bottomFace.normals = originalNormals;
        faces.Add(bottomFace);

        // create side face from copy of bottom face vertices
        Vector3[] sideFaceVertices = originalMesh.vertices;
        Vector3[] sideFaceNormals = originalMesh.normals;

        // find max value on local direction
        Vector3 maxDir = MathUtility.FarthestPointInDirection(sideFaceVertices, direction);
        Vector3 midMaxDir = Vector3.Project(maxDir - lt.Origin, -direction) + lt.Origin;

        //GameObject a = UnityEngine.Object.Instantiate(Resources.Load("BlueCube"), maxDir, Quaternion.identity) as GameObject;
        //GameObject b = UnityEngine.Object.Instantiate(Resources.Load("PinkCube"), midMaxDir , Quaternion.identity) as GameObject;
        //GameObject c = UnityEngine.Object.Instantiate(Resources.Load("YellowCube"), lt.Origin, Quaternion.identity) as GameObject;
        ////Debug.DrawLine(maxDir, midMaxDir, Color.magenta, 1000.0f);
        //Debug.DrawLine(lt.Origin, lt.Origin + direction, Color.magenta, 1000.0f);
        //Debug.DrawLine(lt.Origin, maxDir, Color.cyan, 100---0.0f);

        Vector3 angleAxis = (midMaxDir - maxDir).normalized;
        //Quaternion rotation = Quaternion.AngleAxis(90.0f, angleAxis);
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


        // float distance = Vector3.Distance(midMaxDir, midMinDir);
        float width = Vector3.Distance(minDir, midMinDir) * 2;

        Vector3 frontAxis = (midMinDir - minDir).normalized;
        Vector3 backAxis = (midMaxDir - maxDir).normalized;

        Vector3 otherFrontVertex = minDir + (width * frontAxis);
        Vector3 otherBackVertex = maxDir + (width * backAxis);





        //organize vectors by rightside/leftside

        //Vector3 minDir = MathUtility.FarthestPointInDirection(originalVertices, -direction);

        //Quaternion degree90Rot = Quaternion.AngleAxis(90.0f, lt.Up);
        //Vector3 right = Quaternion.AngleAxis(90.0f, lt.Up) * direction;
        //Quaternion.AngleAxis(90.0f, lt.Up) * direction;
        Vector3 right = Vector3.Cross(direction, lt.Up);

        //Debug.DrawLine(maxDir, maxDir + (right * 25.0f), Color.magenta, 1000.0f);
        //Debug.DrawLine(maxDir, maxDir + (lt.Up * 25.0f), Color.red, 1000.0f);
        //Debug.DrawLine(maxDir, maxDir + (direction * 25.0f), Color.blue, 1000.0f);

        //Vector3 v0 = Vector3.zero;
        //Vector3 v1 = Vector3.zero;
        //Vector3 v2 = Vector3.zero;
        //Vector3 v3 = Vector3.zero;

        Vector3[] backVertices = new Vector3[] { maxDir, otherBackVertex };
        Vector3[] frontVertices = new Vector3[] { minDir, otherFrontVertex };

        Vector3 v0 = MathUtility.FarthestPointInDirection(backVertices, right);
        Vector3 v1 = MathUtility.FarthestPointInDirection(backVertices, -right);

        Vector3 v2 = MathUtility.FarthestPointInDirection(frontVertices, right);
        Vector3 v3 = MathUtility.FarthestPointInDirection(frontVertices, -right);


        //GameObject a = UnityEngine.Object.Instantiate(Resources.Load("BlueCube"), maxDir, Quaternion.identity) as GameObject;
        //GameObject c = UnityEngine.Object.Instantiate(Resources.Load("PinkCube"), otherBackVertex, Quaternion.identity) as GameObject;

        //GameObject d = UnityEngine.Object.Instantiate(Resources.Load("OrangeCube"), minDir, Quaternion.identity) as GameObject;
        //GameObject b = UnityEngine.Object.Instantiate(Resources.Load("YellowCube"), otherFrontVertex, Quaternion.identity) as GameObject;

        //GameObject a = UnityEngine.Object.Instantiate(Resources.Load("BlueCube"), v0, Quaternion.identity) as GameObject;
        //GameObject c = UnityEngine.Object.Instantiate(Resources.Load("PinkCube"), v1, Quaternion.identity) as GameObject;

        //GameObject d = UnityEngine.Object.Instantiate(Resources.Load("OrangeCube"), v2, Quaternion.identity) as GameObject;
        //GameObject b = UnityEngine.Object.Instantiate(Resources.Load("YellowCube"), v3, Quaternion.identity) as GameObject;


        // determine side vertices
        Vector3 edge0 = v0 - v2;
        Vector3 back = edge0.normalized;

        Vector3 edge1 = v1 - v3;

        //Debug.DrawLine(v2, v2 + back, Color.green, 1000.0f);
        //Debug.DrawLine(v3, v3 + back, Color.green, 1000.0f);
        //Debug.DrawLine(v2, v2 + lt.Up, Color.green, 1000.0f);
        //Debug.DrawLine(v3, v3 + lt.Up, Color.green, 1000.0f);

        List<Vector3> sideVertices0 = new List<Vector3>();
        List<Vector3> sideVertices1 = new List<Vector3>();

        //List<Vector3> stairFrontFaceVertices0 = new List<Vector3>();
        //List<Vector3> stairFrontFaceVertices1 = new List<Vector3>();

        //List<Vector3> stairTopFaceVertices0 = new List<Vector3>();
        //List<Vector3> stairTopFaceVertices1 = new List<Vector3>();

        sideVertices0.Add(v0);
        sideVertices0.Add(v2);
        sideVertices1.Add(v1);
        sideVertices1.Add(v3);

        // calc stair length/height
        float spanLength = Vector3.Distance(v0, v2);
        float stairLength = spanLength / stairCount;

        Vector3 refVector0 = v2;
        Vector3 refVector1 = v3;

        for (int i = 0; i < stairCount; i++)
        {

            Vector3 stairV0 = refVector0 + (stairLength * lt.Up);
            Vector3 stairV1 = stairV0 + (stairLength * back);

            Vector3 stairV2 = refVector1 + (stairLength * lt.Up);
            Vector3 stairV3 = stairV2 + (stairLength * back);

            //stairFrontFaceVertices0.Add(refVector0);
            //stairFrontFaceVertices0.Add(stairV0);

            //stairFrontFaceVertices1.Add(refVector1);
            //stairFrontFaceVertices1.Add(stairV2);

            //stairTopFaceVertices0.Add(stairV0);
            //stairTopFaceVertices0.Add(stairV1);

            //stairTopFaceVertices1.Add(stairV2);
            //stairTopFaceVertices1.Add(stairV3);

            List<Vector3> stairFrontFaceVertices = new List<Vector3>() { refVector0, stairV0, stairV2, refVector1};
            List<Vector3> stairTopFaceVertices = new List<Vector3>() { stairV0, stairV1, stairV3, stairV2 };

            //Mesh stairFrontFace = Triangulator.TriangulatePolygonNormal(stairFrontFaceVertices, true, -lt.Forward);
            Mesh stairFrontFace = Triangulator.TriangulatePolygonNormal(stairFrontFaceVertices, true, -direction);
            Mesh stairTopFace = Triangulator.TriangulatePolygonNormal(stairTopFaceVertices, true, lt.Up);

            faces.Add(stairFrontFace);
            faces.Add(stairTopFace);

            refVector0 = stairV1;
            refVector1 = stairV3;

            sideVertices0.Add(stairV0);
            sideVertices0.Add(stairV1);

            sideVertices1.Add(stairV2);
            sideVertices1.Add(stairV3);

            //GameObject a = UnityEngine.Object.Instantiate(Resources.Load("BlueCube"), stairV0, Quaternion.identity) as GameObject;
            //GameObject b = UnityEngine.Object.Instantiate(Resources.Load("PinkCube"), stairV1, Quaternion.identity) as GameObject;
            //GameObject c = UnityEngine.Object.Instantiate(Resources.Load("OrangeCube"), stairV2, Quaternion.identity) as GameObject;
            //GameObject d = UnityEngine.Object.Instantiate(Resources.Load("YellowCube"), stairV3, Quaternion.identity) as GameObject;

            //GameObject a = UnityEngine.Object.Instantiate(Resources.Load("BlueCube"), stairFrontFaceVertices0[i], Quaternion.identity) as GameObject;
            //GameObject b = UnityEngine.Object.Instantiate(Resources.Load("PinkCube"), stairFrontFaceVertices0[i + 1], Quaternion.identity) as GameObject;
            //GameObject c = UnityEngine.Object.Instantiate(Resources.Load("OrangeCube"), stairFrontFaceVertices1[i], Quaternion.identity) as GameObject;
            //GameObject d = UnityEngine.Object.Instantiate(Resources.Load("YellowCube"), stairFrontFaceVertices1[i + 1], Quaternion.identity) as GameObject;

            //GameObject a = UnityEngine.Object.Instantiate(Resources.Load("BlueCube"), stairTopFaceVertices0[i], Quaternion.identity) as GameObject;
            //GameObject b = UnityEngine.Object.Instantiate(Resources.Load("PinkCube"), stairTopFaceVertices0[i + 1], Quaternion.identity) as GameObject;
            //GameObject c = UnityEngine.Object.Instantiate(Resources.Load("OrangeCube"), stairTopFaceVertices1[i], Quaternion.identity) as GameObject;
            //GameObject d = UnityEngine.Object.Instantiate(Resources.Load("YellowCube"), stairTopFaceVertices1[i + 1], Quaternion.identity) as GameObject;


        }
        //v0/v2 right
        //Debug.DrawLine(lt.Origin, lt.Origin + (right * 25f), Color.red, 1000f);
        //Debug.DrawLine(lt.Origin, lt.Origin - (right * 25f), Color.blue, 1000f);


        //Debug.Log("side0: (red) " + BuildingUtility.isPolygonClockwise(sideVertices0));
        //Debug.Log("side1: " + BuildingUtility.isPolygonClockwise(sideVertices1));


        //for(int i = 0; i < sideVertices0.Count; i++)
        //{
        //    if (i == 0)
        //    {
        //        GameObject a = UnityEngine.Object.Instantiate(Resources.Load("BlueCube"), sideVertices0[i], Quaternion.identity) as GameObject;

        //    }
        //    else if(i == 1)
        //    {
        //        GameObject a = UnityEngine.Object.Instantiate(Resources.Load("YellowCube"), sideVertices0[i], Quaternion.identity) as GameObject;

        //    }
        //    else if (i == 2)
        //    {
        //        GameObject a = UnityEngine.Object.Instantiate(Resources.Load("PinkCube"), sideVertices0[i], Quaternion.identity) as GameObject;

        //    }
        //    else
        //    {
        //        GameObject a = UnityEngine.Object.Instantiate(Resources.Load("OrangeCube"), sideVertices0[i], Quaternion.identity) as GameObject;

        //    }
        //}

        if(BuildingUtility.isPolygonClockwise(sideVertices0))
        {
            sideVertices0.Reverse();
        }

        if (BuildingUtility.isPolygonClockwise(sideVertices1))
        {
            sideVertices1.Reverse();
        }

        Mesh sideFace0 = Triangulator.TriangulatePolygonNormal(sideVertices0, true, right);
        Mesh sideFace1 = Triangulator.TriangulatePolygonNormal(sideVertices1, true, -right);
        faces.Add(sideFace0);
        faces.Add(sideFace1);


        //Debug.DrawLine(lt.Origin, lt.Origin + direction, Color.green, 1000.0f);
        //Debug.DrawLine(lt.Origin, lt.Origin + (right * 25.0f), Color.yellow, 1000.0f);

        Mesh finalMesh = BuildingUtility.CombineMeshes(faces);

        finalMesh.RecalculateBounds();
        LocalTransform newLt = new LocalTransform(finalMesh.bounds.center, lt.Up, lt.Forward, lt.Right);

        //lt.Origin = finalMesh.bounds.center;

        return new Shape(finalMesh, newLt);
    }

    ShapeWrapper IShapeGrammarOperation.PerformOperation(List<Shape> input)
    {
        List<Shape> output = new List<Shape>();

        foreach (Shape shape in input)
        {
            Vector3 direction = shape.LocalTransform.DirectionToVector(this.direction);
            output.Add(Stair(shape, stairCount, direction));
        }

        return new ShapeWrapper(output);
    }
}
