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

    // direction should be forward, back, right or left unit vector
    public Shape RoofShed(Shape shape, float angle, Vector3 direction)
    {
        Mesh originalMesh = shape.Mesh;
        Mesh topFaceMesh = new Mesh();

        LocalTransform lt = shape.LocalTransform;
        List<Mesh> faces = new List<Mesh>();

        // rotate the ramp input direction by 90 degrees around the up vector to find the new right vector
        Vector3 right = Quaternion.AngleAxis(90f, lt.Up) * direction;

        // create top face from copy of bottom face vertices
        Vector3[] topFaceVertices = originalMesh.vertices;

        // find min value on local Z axis
        Vector3 minZ = MathUtility.FarthestPointInDirection(topFaceVertices, -direction);

        Vector3 rotationPoint = Vector3.zero;

        bool intersect = Math3d.LineLineIntersection(out rotationPoint, lt.Origin, -direction, minZ, right);
        if(!intersect)
        {
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

        // perform a raycast from input face's vertices upwards to the rotated top plane
        // where it intersects will become that vertex's new position
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

        bool test = true;
        List<bool> part1results = new List<bool>();
        List<bool> part2results = new List<bool>();
        LocalTransform originalTransform = null;

        foreach (Shape shape in input)
        {
            if(test)
            {
                originalTransform = new LocalTransform(shape.LocalTransform);
            }
            
            Vector3 direction = shape.LocalTransform.DirectionToVector(this.direction);
            Shape result = RoofShed(shape, angle, direction);

            if (test)
            {
                Vector3 bottomFaceNormal = -1 * originalTransform.Up;
                Vector3 right = Quaternion.AngleAxis(90f, originalTransform.Up) * direction;
                Quaternion rotation = Quaternion.AngleAxis(-angle, right);
                Vector3 topFaceNormal = rotation * originalTransform.Up;

                bool topFaceTestResult = CheckIfVerticesOnPlaneFromNormal(originalTransform, result, topFaceNormal);
                part1results.Add(topFaceTestResult);

                bool bottomFaceTestResult = CheckIfVerticesOnPlaneFromNormal(originalTransform, result, bottomFaceNormal);
                part2results.Add(bottomFaceTestResult);
            }

            output.Add(result);
        }
        
        if (test)
        {
            List<OperationTest> operationTests = new List<OperationTest>();
            operationTests.Add(new OperationTest("roofshed", "part 1", part1results));
            operationTests.Add(new OperationTest("roofshed", "part 2", part2results));
            return new ShapeWrapper(output, operationTests);
        }
        
        return new ShapeWrapper(output);
    }


    bool CheckIfVerticesOnPlaneFromNormal(LocalTransform originalTransform, Shape processedShape, Vector3 normal)
    {
        DMesh3 dmesh = g3UnityUtils.UnityMeshToDMesh(processedShape.Mesh);

        DMesh3[] parts = MeshConnectedComponents.Separate(dmesh);

        List<Vector3> faceVertices = new List<Vector3>();

        foreach (DMesh3 dm in parts)
        {
            Mesh unitymesh = g3UnityUtils.DMeshToUnityMesh(dm);

            Vector3[] norms = unitymesh.normals;
            if (norms[0] == normal)
            {
                MeshBoundaryLoops mbl = new MeshBoundaryLoops(dm);
                if (mbl.Loops.Count < 1)
                {
                    Debug.Log("Roof Shed Operation: Test: found zero loops: bottom face" + mbl.Loops.Count);
                    return false;
                }

                EdgeLoop loop = mbl.Loops[0];
                int[] loopVertexIndicies = loop.Vertices;

                for (int i = 0; i < loopVertexIndicies.Length; i++)
                {
                    Vector3 vertex = MathUtility.ConvertToVector3(dm.GetVertex(loopVertexIndicies[i]));
                    faceVertices.Add(vertex);
                }

                break;
            }
        }

        Plane plane = new Plane(normal, faceVertices[0]);

        foreach (Vector3 point in faceVertices)
        {
            Vector3 pointOnPlane = plane.ClosestPointOnPlane(point);
            float length = (pointOnPlane - point).magnitude;

            bool testResult = length == 0f;
        }

        return true;
    }

    bool CheckIfPointLiesOnPlane(Vector3 point, Plane plane)
    {

        return true;
    }
}
