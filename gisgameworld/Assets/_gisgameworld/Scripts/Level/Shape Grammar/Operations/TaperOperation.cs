using g3;
using System.Collections.Generic;
using UnityEngine;

public class TaperOperation
{
    public static Shape Taper(Shape shape, float yAmount, float xzAmount = 0f)
    {
        // get the original mesh
        Mesh originalMesh = shape.Mesh;
        originalMesh.RecalculateBounds();

        // find edge loop
        DMesh3 dmesh = g3UnityUtils.UnityMeshToDMesh(originalMesh);
        MeshBoundaryLoops mbl = new MeshBoundaryLoops(dmesh);
        List<EdgeLoop> loops = mbl.Loops;
        if(loops.Count != 1)
        {
            Debug.Log("Taper: found zero or > 1 loops: " + loops.Count);
            return null;
        }

        // get edge loop vertices 
        EdgeLoop loop = loops[0];
        int[] loopVertexIndicies = loop.Vertices;
        List<Vector3> loopVertices = new List<Vector3>();
        List<Vector3> taperedVertices = new List<Vector3>();

        for (int i = 0; i < loopVertexIndicies.Length; i++)
        {
            loopVertices.Add((Vector3) dmesh.GetVertex(loopVertexIndicies[i]));
        }

        // find center
        LocalTransform lt = shape.LocalTransform;
        Vector3 yOffset = lt.Up * yAmount;
        Vector3 topCenter = originalMesh.bounds.center + yOffset;

        // move new loop vertices up and inward
        for(int i = 0; i < loopVertices.Count; i++)
        {
            Vector3 towardCenter = (topCenter - loopVertices[i]).normalized;

            Vector3 tapered = loopVertices[i];
            tapered += (towardCenter * xzAmount);
            tapered += yOffset;

            taperedVertices.Add(tapered);
        }

        List<Mesh> faces = new List<Mesh>();

        // flip orientation and normal of bottom face so it points outwards
        Vector3[] originalVertices = originalMesh.vertices;
        Vector3[] originalNormals = originalMesh.normals;
        int[] originalTris = originalMesh.triangles;

        List<Triangle> bottomFaceTriangles = new List<Triangle>();

        // convert to triangles and reverse orientation
        for(int i = 0; i < originalTris.Length - 2; i += 3)
        {
            Vector3 p0 = originalVertices[originalTris[i]];
            Vector3 p1 = originalVertices[originalTris[i + 1]];
            Vector3 p2 = originalVertices[originalTris[i + 2]];
            Triangle t = new Triangle(p0, p1, p2);
            t.ChangeOrientation();
            bottomFaceTriangles.Add(t);
        }

        // flip the normals
        for(int i = 0; i < originalNormals.Length; i++)
        {
            originalNormals[i] = -lt.Up;
        }

        Mesh bottomFace = BuildingUtility.TrianglesToMesh(bottomFaceTriangles, true);
        bottomFace.normals = originalNormals;

        faces.Add(bottomFace);

        // for each edge pair, triangulate a new face
        for (int i = 0; i < loopVertices.Count; i++)
        {
            Vector3 p0 = loopVertices[i];
            Vector3 p1 = loopVertices[MathUtility.ClampListIndex(i + 1, loopVertices.Count)];

            Vector3 p2 = taperedVertices[i];
            Vector3 p3 = taperedVertices[MathUtility.ClampListIndex(i + 1, loopVertices.Count)];

            Triangle t0 = new Triangle(p0, p1, p2);
            Triangle t1 = new Triangle(p2, p1, p3);

            List<Triangle> newFaceTriangles = new List<Triangle>() { t0, t1 };
            Mesh newFace = BuildingUtility.TrianglesToMesh(newFaceTriangles, true);

            // calculate new normals for face
            Vector3 normal = Vector3.Cross((p1 - p0).normalized, (p2 - p0).normalized);

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
        List<Triangle> topFaceTriangles = Triangulator.TriangulatePolygonN(taperedVertices, true, lt.Up);
        Mesh topFace = BuildingUtility.TrianglesToMesh(topFaceTriangles, true);
        faces.Add(topFace);

        Mesh finalMesh = BuildingUtility.CombineMeshes(faces);
        finalMesh.RecalculateBounds();
        lt.Origin = finalMesh.bounds.center;

        Shape finalShape = new Shape(BuildingUtility.CombineMeshes(faces), lt);

        return finalShape;
    }
}
