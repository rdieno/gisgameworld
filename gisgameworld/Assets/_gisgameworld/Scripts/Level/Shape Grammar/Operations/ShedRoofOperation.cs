using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using g3;


public class RoofShedOperation : IShapeGrammarOperation
{
    private float angle;
    private Direction direction;

    public RoofShedOperation(float angle, Direction direction)
    {
        this.angle = angle;
        this.direction = direction;
    }

    //static bool once = false;

    // direction should be forward, back, right or left unit vector
    public Shape RoofShed(Shape shape, float angle, Vector3 direction)
    {
        Mesh originalMesh = shape.Mesh;
        Mesh topFaceMesh = new Mesh();

        LocalTransform lt = shape.LocalTransform;
        List<Mesh> faces = new List<Mesh>();

        Vector3 right = Quaternion.AngleAxis(90f, lt.Up) * direction;

        // create top face from copy of bottom face vertices
        Vector3[] topFaceVertices = originalMesh.vertices;

        // find min value on local Z axis
        Vector3 minZ = MathUtility.FarthestPointInDirection(topFaceVertices, -direction);

        Vector3 rotationPoint = Vector3.zero;



        //bool intersect = Math3d.LineLineIntersection(out rotationPoint, originalMesh.bounds.center, -direction, minZ, right);
        bool intersect = Math3d.LineLineIntersection(out rotationPoint, lt.Origin, -direction, minZ, right);
        if(!intersect)
        {
            //if (!once)
            //{
            //    GameObject a = UnityEngine.Object.Instantiate(Resources.Load("BlueCube"), lt.Origin, Quaternion.identity) as GameObject;
            //    GameObject b = UnityEngine.Object.Instantiate(Resources.Load("YellowCube"), minZ, Quaternion.identity) as GameObject;

            //    Debug.DrawLine(originalMesh.bounds.center, originalMesh.bounds.center + (-direction * 25.0f), Color.green, 1000f);
            //    Debug.DrawLine(minZ, minZ + (-right * 25.0f), Color.green, 1000f);

            //    once = true;
            //}

            intersect = Math3d.LineLineIntersection(out rotationPoint, originalMesh.bounds.center, -direction, minZ, -right);

            if (!intersect)
            {
                Debug.Log("Shed Roof Operation: failed to find intersect for rotation point");
                return shape;
            }
        }

        Quaternion rotation = Quaternion.AngleAxis(-angle, right);

        Vector3[] topNormals = originalMesh.normals;
        Vector3 topNormal = rotation * topNormals[0];

        Plane topPlane = new Plane(topNormal, rotationPoint);

        // rotate all vertices around rotation point
        for (int i = 0; i < topFaceVertices.Length; i++)
        {
            Vector3 topFaceVertex = topFaceVertices[i];
            Ray ray = new Ray(topFaceVertex, lt.Up);

            //Initialise the enter variable
            float enter = 0.0f;

            if (topPlane.Raycast(ray, out enter))
            {
                //Get the point of intersection
                Vector3 hitPoint = ray.GetPoint(enter);

                topFaceVertices[i] = hitPoint;
            }
            //else
            //{
            //    //Debug.Log("Shed Roof Operation: ray/plane intersection failed or vertex is already on plane)");
            //}

            topNormals[i] = topNormal;
        }

        topFaceMesh.vertices = topFaceVertices;
        topFaceMesh.normals = topNormals;
        topFaceMesh.triangles = originalMesh.triangles;

        faces.Add(topFaceMesh);

        // get edge loop for top and bottom faces
        DMesh3 topFaceDMesh = g3UnityUtils.UnityMeshToDMesh(topFaceMesh);
        MeshBoundaryLoops topBoundaryLoop = new MeshBoundaryLoops(topFaceDMesh);
        List<EdgeLoop> topLoops = topBoundaryLoop.Loops;

        DMesh3 bottomFaceDMesh = g3UnityUtils.UnityMeshToDMesh(originalMesh);
        MeshBoundaryLoops bottomBoundaryLoop = new MeshBoundaryLoops(bottomFaceDMesh);
        List<EdgeLoop> bottomLoops = bottomBoundaryLoop.Loops;

        if (topLoops.Count != 1 || bottomLoops.Count != 1)
        {
            Debug.Log("Shed Roof Operation: Found hole in face");
        }

        // convert edge loop to Vector3 array

        Vector3[] topLoop = BuildingUtility.EdgeLoopToVertexArray(topLoops[0].Vertices, topFaceDMesh);
        Vector3[] bottomLoop = BuildingUtility.EdgeLoopToVertexArray(bottomLoops[0].Vertices, bottomFaceDMesh);

        // create missing faces by iterating over edges
        // if edge is along the "hinge", ignore it
        // if edge is adjacent to hinge create one triangle
        // otherwise create two triangles
        for (int i = 0; i < topLoop.Length; i++)
        {
            List<Triangle> tris = new List<Triangle>();

            Vector3 p0 = topLoop[i];
            Vector3 p1 = topLoop[MathUtility.ClampListIndex(i + 1, topLoop.Length)];
            Vector3 p2 = bottomLoop[i];
            Vector3 p3 = bottomLoop[MathUtility.ClampListIndex(i + 1, bottomLoop.Length)];

            Vector3 normal = Vector3.zero;

            if (p0 == p2)
            {
                normal = -Vector3.Cross(p1 - p0, p3 - p0).normalized;
                tris.Add(new Triangle(p0, p3, p1, normal));
            }
            else if(p1 == p3)
            {
                normal = -Vector3.Cross(p1 - p0, p2 - p0).normalized;
                tris.Add(new Triangle(p0, p2, p1, normal));
            }
            else
            {
                normal = -Vector3.Cross(p1 - p0, p2 - p0).normalized;
                tris.Add(new Triangle(p0, p3, p1, normal));
                tris.Add(new Triangle(p0, p2, p3, normal));
            }

            faces.Add(BuildingUtility.TrianglesToMesh(tris, true));
        }

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

        Mesh finalMesh = BuildingUtility.CombineMeshes(faces);

        finalMesh.RecalculateBounds();
        lt.Origin = finalMesh.bounds.center;

        return new Shape(finalMesh, lt);
    }

    ShapeWrapper IShapeGrammarOperation.PerformOperation(List<Shape> input)
    {
        List<Shape> output = new List<Shape>();

        foreach (Shape shape in input)
        {
            Vector3 direction = shape.LocalTransform.DirectionToVector(this.direction);
            output.Add(RoofShed(shape, angle, direction));
        }

        return new ShapeWrapper(output);
    }

    //public static Shape RoofShed(Shape shape, float angle, Vector3 direction)
    //{
    //    Mesh originalMesh = shape.Mesh;
    //    Mesh topFaceMesh = new Mesh();

    //    LocalTransform lt = shape.LocalTransform;
    //    List<Mesh> faces = new List<Mesh>();

    //    // create top face from copy of bottom face vertices
    //    Vector3[] topFaceVertices = originalMesh.vertices;

    //    // find min value on local Z axis
    //    Vector3 minZ = MathUtility.FarthestPointInDirection(topFaceVertices, -lt.Forward);

    //    Vector3 rotationPoint = Vector3.zero;

    //    bool intersect = Math3d.LineLineIntersection(out rotationPoint, originalMesh.bounds.center, -lt.Forward, minZ, lt.Right);
    //    if(!intersect)
    //    {
    //        Debug.Log("Shed Roof Operation: failed to find intersect for rotation point");
    //    }

    //    Quaternion rotation = Quaternion.AngleAxis(-angle, lt.Right);

    //    Vector3[] topNormals = originalMesh.normals;
    //    Vector3 topNormal = rotation * topNormals[0];

    //    Plane topPlane = new Plane(topNormal, rotationPoint);

    //    // rotate all vertices around rotation point
    //    for (int i = 0; i < topFaceVertices.Length; i++)
    //    {
    //        Vector3 topFaceVertex = topFaceVertices[i];
    //        Ray ray = new Ray(topFaceVertex, lt.Up);

    //        //Initialise the enter variable
    //        float enter = 0.0f;

    //        if (topPlane.Raycast(ray, out enter))
    //        {
    //            //Get the point of intersection
    //            Vector3 hitPoint = ray.GetPoint(enter);

    //            topFaceVertices[i] = hitPoint;
    //        }
    //        //else
    //        //{
    //        //    //Debug.Log("Shed Roof Operation: ray/plane intersection failed or vertex is already on plane)");
    //        //}

    //        topNormals[i] = topNormal;
    //    }

    //    topFaceMesh.vertices = topFaceVertices;
    //    topFaceMesh.normals = topNormals;
    //    topFaceMesh.triangles = originalMesh.triangles;

    //    faces.Add(topFaceMesh);

    //    // get edge loop for top and bottom faces
    //    DMesh3 topFaceDMesh = g3UnityUtils.UnityMeshToDMesh(topFaceMesh);
    //    MeshBoundaryLoops topBoundaryLoop = new MeshBoundaryLoops(topFaceDMesh);
    //    List<EdgeLoop> topLoops = topBoundaryLoop.Loops;

    //    DMesh3 bottomFaceDMesh = g3UnityUtils.UnityMeshToDMesh(originalMesh);
    //    MeshBoundaryLoops bottomBoundaryLoop = new MeshBoundaryLoops(bottomFaceDMesh);
    //    List<EdgeLoop> bottomLoops = bottomBoundaryLoop.Loops;

    //    if (topLoops.Count != 1 || bottomLoops.Count != 1)
    //    {
    //        Debug.Log("Shed Roof Operation: Found hole in face");
    //    }

    //    // convert edge loop to Vector3 array

    //    Vector3[] topLoop = BuildingUtility.EdgeLoopToVertexArray(topLoops[0].Vertices, topFaceDMesh);
    //    Vector3[] bottomLoop = BuildingUtility.EdgeLoopToVertexArray(bottomLoops[0].Vertices, bottomFaceDMesh);

    //    // create missing faces by iterating over edges
    //    // if edge is along the "hinge", ignore it
    //    // if edge is adjacent to hinge create one triangle
    //    // otherwise create two triangles
    //    for (int i = 0; i < topLoop.Length; i++)
    //    {
    //        List<Triangle> tris = new List<Triangle>();

    //        Vector3 p0 = topLoop[i];
    //        Vector3 p1 = topLoop[MathUtility.ClampListIndex(i + 1, topLoop.Length)];
    //        Vector3 p2 = bottomLoop[i];
    //        Vector3 p3 = bottomLoop[MathUtility.ClampListIndex(i + 1, bottomLoop.Length)];

    //        Vector3 normal = Vector3.zero;

    //        if (p0 == p2)
    //        {
    //            normal = -Vector3.Cross(p1 - p0, p3 - p0).normalized;
    //            tris.Add(new Triangle(p0, p3, p1, normal));
    //        }
    //        else if(p1 == p3)
    //        {
    //            normal = -Vector3.Cross(p1 - p0, p2 - p0).normalized;
    //            tris.Add(new Triangle(p0, p2, p1, normal));
    //        }
    //        else
    //        {
    //            normal = -Vector3.Cross(p1 - p0, p2 - p0).normalized;
    //            tris.Add(new Triangle(p0, p3, p1, normal));
    //            tris.Add(new Triangle(p0, p2, p3, normal));
    //        }

    //        faces.Add(BuildingUtility.TrianglesToMesh(tris, true));
    //    }

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

    //    Mesh finalMesh = BuildingUtility.CombineMeshes(faces);

    //    finalMesh.RecalculateBounds();
    //    lt.Origin = finalMesh.bounds.center;

    //    return new Shape(finalMesh, lt);
    //}
}
