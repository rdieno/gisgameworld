using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using g3;
using System;

public class BuildingUtility
{
    public static Mesh BuildingToMesh(Building building, bool moveToOrigin = false)
    {
        Shape root = building.Root;

        if (root.Mesh == null)
        {
            Mesh mesh = new Mesh();
            mesh.vertices = root.Vertices;
            mesh.triangles = root.Triangles;
            mesh.normals = root.Normals;

            mesh.RecalculateBounds();

            if (moveToOrigin)
            {
                Vector3[] vertices = root.Vertices;

                Vector3 offset = mesh.bounds.center;

                for (int i = 0; i < mesh.vertexCount; i++)
                {
                    vertices[i] = new Vector3(vertices[i].x - offset.x, vertices[i].y, vertices[i].z - offset.z);
                }

                mesh.vertices = vertices;
            }

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            return mesh;
        }
        else
        {
            return root.Mesh;
        }
    }

    // combines multiple individual triangles into a single polygonal mesh
    public static Mesh TrianglesToMesh(List<Triangle> geometry, bool weldVertices = false)
    {
        Mesh mesh = new Mesh();

        List<Mesh> triangleMeshes = new List<Mesh>();

        if (geometry == null || geometry.Count == 0)
        {
            System.Diagnostics.Debugger.Break();
        }

        for (int i = 0; i < geometry.Count; i++)
        {
            Mesh m = new Mesh();

            Triangle t = geometry[i];

            Vector3[] vertices = new Vector3[3];
            int[] triangles = new int[3];
            Vector2[] uv = new Vector2[3];
            Vector3[] normals = new Vector3[3];

            vertices[0] = t.v1.position;
            vertices[1] = t.v2.position;
            vertices[2] = t.v3.position;

            triangles[0] = 0;
            triangles[1] = 1;
            triangles[2] = 2;

            uv[0] = new Vector2(0, 0);
            uv[1] = new Vector2(0, 1);
            uv[2] = new Vector2(1, 0);

            if(t?.normal != null)
            {
                normals[0] = (Vector3) t?.normal;
                normals[1] = (Vector3) t?.normal;
                normals[2] = (Vector3) t?.normal;
            }
            else
            {
                normals[0] = Vector3.up;
                normals[1] = Vector3.up;
                normals[2] = Vector3.up;
            }

            m.vertices = vertices;
            m.triangles = triangles;
            m.uv = uv;
            m.normals = normals;

            triangleMeshes.Add(m);
        }

        mesh = BuildingUtility.CombineMeshes(triangleMeshes);

        if (weldVertices)
        {
            mesh.uv = null;

            MeshWelder mw = new MeshWelder(mesh);
            mesh = mw.Weld();
        }

        return mesh;
    }


    // combines multiple individual meshes into a single mesh object
    public static Mesh CombineMeshes(List<Mesh> meshes, bool submesh = false)
    {
        CombineInstance[] combine = new CombineInstance[meshes.Count];
        int i = 0;
        while (i < meshes.Count)
        {
            combine[i].mesh = meshes[i];
            combine[i].transform = Matrix4x4.zero;
            i++;
        }

        Mesh combinedMesh = new Mesh();

        if (submesh)
        {
            combinedMesh.CombineMeshes(combine, false, false);
        }
        else
        {
            combinedMesh.CombineMeshes(combine, true, false);
        }

        return combinedMesh;
    }
    
    
    // combines multiple individual shapes into a single mesh object
    public static Mesh CombineShapes(List<Shape> shapes, bool submesh = false)
    {
        List<Mesh> meshes = new List<Mesh>();

        for(int j = 0; j < shapes.Count; j++)
        {
            meshes.Add(shapes[j].Mesh);
        }

        CombineInstance[] combine = new CombineInstance[meshes.Count];
        int i = 0;
        while (i < meshes.Count)
        {
            combine[i].mesh = meshes[i];
            combine[i].transform = Matrix4x4.zero;

            //if(submesh)
            //{
            //    combine[i].
            //    combine[i].subMeshIndex = i;
            //}

            i++;
        }

        Mesh combinedMesh = new Mesh();

        if (submesh)
        {
            combinedMesh.CombineMeshes(combine, false, false);
        }
        else
        {
            combinedMesh.CombineMeshes(combine, true, false);
        }

        return combinedMesh;
    }

    // attempts to correct angles that are near 180, 90, 45 and 135 degrees
    // should input a polygon that is approximately orthogonal for best results
    public static List<Vector3> Rectify(List<Vector3> polygon, float margin = 10.0f, bool isXOriented = true)
    {
        List<Vector3> edgeVertices = polygon;

        // pass for 180 degree angles
        for (int i = 1; i < edgeVertices.Count + 1; i++)
        {
            // save the indices of our chosen vertices so they can be overridden later
            // we use the modulo operater because for the last two angles we need to wrap around
            int index1 = (i - 1) % edgeVertices.Count;
            int index2 = i % edgeVertices.Count;
            int index3 = (i + 1) % edgeVertices.Count;

            // get the first three vertices in the polygon
            Vector3 vert1 = edgeVertices[index1];
            Vector3 vert2 = edgeVertices[index2];
            Vector3 vert3 = edgeVertices[index3];

            // calculate the normals pointing away from the middle vertex
            Vector3 norm1 = (vert1 - vert2).normalized;
            Vector3 norm2 = (vert3 - vert2).normalized;

            // calculate the angle between the two normals in degrees
            float angle = Mathf.Acos(Vector3.Dot(norm1, norm2)) * Mathf.Rad2Deg;

            // check for angles near 180 degrees
            if (angle < 180.0f + margin && angle > 180.0f - margin)
            {
                // remove the middle vertex, essentially joining the outer two with a straight line
                edgeVertices.RemoveAt(index2);
            }
        }

        // pass for 90 degree angles

        // set the appropriate reference angle
        Vector3 orientationAxis = Vector3.forward;

        if (isXOriented)
        {
            orientationAxis = Vector3.right;
        }

        int startIndex = 0;

        // check for reference angle
        int? referenceVertexIndex = CheckForReferenceAngle(edgeVertices, orientationAxis);
        if (referenceVertexIndex.HasValue)
        {
            // if an edge that matches the reference angle is found, start at that index
            startIndex = referenceVertexIndex.Value;
        }

        int edgeVerticesSize = edgeVertices.Count;

        for (int i = 0; i < edgeVerticesSize - 1; i++)
        {
            int index0 = GetCircularIndex(startIndex, edgeVerticesSize);
            int index1 = GetCircularIndex(startIndex + 1, edgeVerticesSize);
            int index2 = GetCircularIndex(startIndex + 2, edgeVerticesSize);

            // get the first three vertices in the polygon
            Vector3 vert0 = edgeVertices[index0];
            Vector3 vert1 = edgeVertices[index1];
            Vector3 vert2 = edgeVertices[index2];

            // calculate the normals pointing away from the middle vertex
            Vector3 norm1 = (vert0 - vert1).normalized;
            Vector3 norm2 = (vert2 - vert1).normalized;

            // calculate the angle between the two normals in degrees
            float angle = Mathf.Acos(Vector3.Dot(norm1, norm2)) * Mathf.Rad2Deg;

            float targetAngle = 0.0f;

            //ignore angles that are already squared
            if (angle != 90.0f && angle != 270.0f)
            {
                if (angle > 90.0f - margin && angle < 90.0f + margin)
                {
                    targetAngle = 90.0f;

                    float offsetAngle = Mathf.Abs(angle - targetAngle);

                    if (angle < targetAngle)
                    {
                        offsetAngle *= -1.0f;
                    }

                    Vector3 oldEdge = vert2 - vert1;

                    Vector3 newDirection = Vector3.RotateTowards(norm2, norm1, offsetAngle * Mathf.Deg2Rad, 0.0f);

                    Vector3 projectedVector = Vector3.Project(oldEdge, newDirection);

                    Vector3 newPoint = vert1 + projectedVector;

                    edgeVertices[index2] = newPoint;
                }
                else if(angle > 270.0f - margin && angle < 270.0f + margin)
                {
                    targetAngle = 270.0f;

                    float offsetAngle = Mathf.Abs(angle - targetAngle);

                    if (angle < targetAngle)
                    {
                        offsetAngle *= -1.0f;
                    }

                    Vector3 oldEdge = vert2 - vert1;

                    Vector3 newDirection = Vector3.RotateTowards(norm2, norm1, offsetAngle * Mathf.Deg2Rad, 0.0f);

                    Vector3 projectedVector = Vector3.Project(oldEdge, newDirection);

                    Vector3 newPoint = vert1 + projectedVector;

                    edgeVertices[index2] = newPoint;
                }
            }

            // increment the index
            startIndex = GetCircularIndex(startIndex + 1, edgeVerticesSize);
        }

        // pass for 45 and  degree angles

        startIndex = 0;

        for (int i = 0; i < edgeVerticesSize - 1; i++)
        {
            int index0 = GetCircularIndex(startIndex, edgeVerticesSize);
            int index1 = GetCircularIndex(startIndex + 1, edgeVerticesSize);
            int index2 = GetCircularIndex(startIndex + 2, edgeVerticesSize);

            // get the first three vertices in the polygon
            Vector3 vert0 = edgeVertices[index0];
            Vector3 vert1 = edgeVertices[index1];
            Vector3 vert2 = edgeVertices[index2];

            // calculate the normals pointing away from the middle vertex
            Vector3 norm1 = (vert0 - vert1).normalized;
            Vector3 norm2 = (vert2 - vert1).normalized;

            // calculate the angle between the two normals in degrees
            float angle = Mathf.Acos(Vector3.Dot(norm1, norm2)) * Mathf.Rad2Deg;

            float targetAngle = 0.0f;

            //ignore angles that are already squared or at a 45 degree angle
            if (angle != 45.0f && angle != 135.0f)
            {
                if (angle > 45.0f - margin && angle < 45.0f + margin)
                {
                    targetAngle = 45.0f;

                    float offsetAngle = Mathf.Abs(angle - targetAngle);

                    if (angle < targetAngle)
                    {
                        offsetAngle *= -1.0f;
                    }

                    Vector3 oldEdge = vert2 - vert1;

                    Vector3 newDirection = Vector3.RotateTowards(norm2, norm1, offsetAngle * Mathf.Deg2Rad, 0.0f);

                    Vector3 projectedVector = Vector3.Project(oldEdge, newDirection);

                    Vector3 newPoint = vert1 + projectedVector;

                    edgeVertices[index2] = newPoint;

                }
                else if (angle > 135.0f - margin && angle < 135.0f + margin)
                {
                    targetAngle = 135.0f;

                    float offsetAngle = Mathf.Abs(angle - targetAngle);

                    if (angle < targetAngle)
                    {
                        offsetAngle *= -1.0f;
                    }

                    Vector3 oldEdge = vert2 - vert1;

                    Vector3 newDirection = Vector3.RotateTowards(norm2, norm1, offsetAngle * Mathf.Deg2Rad, 0.0f);

                    Vector3 projectedVector = Vector3.Project(oldEdge, newDirection);

                    Vector3 newPoint = vert1 + projectedVector;

                    edgeVertices[index2] = newPoint;
                }
            }

            // increment the index
            startIndex = GetCircularIndex(startIndex + 1, edgeVerticesSize);
        }

        return polygon;
    }

    // checks if there is an edge that lines up with the reference angle, otherwise returns null
    // returns index of first vertex of matching edge
    private static int? CheckForReferenceAngle(List<Vector3> edgeVertices, Vector3 referenceNormal, float tolerance = 0.1f)
    {
        float minDot = Mathf.Infinity;
        int returnIndex = 0;

        for (int i = 0; i < edgeVertices.Count; i++)
        {
            // save the indices of our chosen vertices so they can be overridden later
            // we use the modulo operater because for the last two angles we need to wrap around
            int edgeVerticesSize = edgeVertices.Count;

            int index0 = GetCircularIndex(i, edgeVerticesSize);
            int index1 = GetCircularIndex(i + 1, edgeVerticesSize);

            // get the first three vertices in the polygon
            Vector3 vert0 = edgeVertices[index0];
            Vector3 vert1 = edgeVertices[index1];

            // calculate the normals pointing away from the middle vertex
            Vector3 edgeNormal = (vert1 - vert0).normalized;

            // check the dot product between the edge and the reference edge
            float dot = Mathf.Abs(Vector3.Dot(edgeNormal, referenceNormal));

            if (dot < minDot)
            {
                minDot = dot;
                returnIndex = i;
            }
        }

        // if the edge is almost perpendicular return the index
        if (minDot <= tolerance)
        {
            return returnIndex;
        }
        else
        {
            return null;
        }
    }

    private static int GetCircularIndex(int index, int arraySize)
    {
        if (index < 0)
        {
            return arraySize + index;
        }
        else
        {
            return index % arraySize;
        }
    }

    // determines if a 2D polygon is longer in the x or z dimension
    public static bool isPolygonXOriented(List<Vector3> polygon)
    {
        float maxX = float.MinValue;
        float maxZ = float.MinValue;
        float minX = float.MaxValue;
        float minZ = float.MaxValue;

        foreach(Vector3 vertex in polygon)
        {
            if(vertex.x > maxX)
            {
                maxX = vertex.x;
            }

            if (vertex.z > maxZ)
            {
                maxZ = vertex.z;
            }

            if (vertex.x < minX)
            {
                minX = vertex.x;
            }

            if (vertex.z < minZ)
            {
                minZ = vertex.z;
            }
        }

        return (maxX - minX) > (maxZ - minZ) ? true : false;
    }

    // simplifies faces getting rid of unnecessary
    public static Mesh SimplifyFaces(Mesh mesh)
    {
        List<Mesh> finalMeshes = new List<Mesh>();

        //MeshWelder welder = new MeshWelder(mesh);
        //mesh = welder.Weld();

        DMesh3 dmesh = g3UnityUtils.UnityMeshToDMesh(mesh);

        DMesh3[] parts = MeshConnectedComponents.Separate(dmesh);

        //MeshEdgeSelection edgeSelector = new MeshEdgeSelection(parts[i]);
        MeshBoundaryLoops loopSelector = new MeshBoundaryLoops(dmesh);
        List<EdgeLoop> edgeLoops = loopSelector.Loops;

        for (int i = 0; i < edgeLoops.Count; i++)
        {
            EdgeLoop el = edgeLoops[i];
            List<Vector3> loopVertices = new List<Vector3>();

            int[] verts = el.Vertices;
            Vector3 loopNormal = (Vector3)dmesh.GetVertexNormal(verts[0]);


            for (int k = 0; k < verts.Length; k++)
            {
                loopVertices.Add((Vector3)dmesh.GetVertex(verts[k]));
            }


            loopVertices = Triangulator.RemoveUnecessaryVertices(loopVertices, loopNormal);

            bool flattened = false;
            Quaternion rotation = Quaternion.identity;
            if (loopNormal != Vector3.up)
            {
                rotation = Quaternion.FromToRotation(loopNormal, Vector3.up);

                for (int k = 0; k < loopVertices.Count; k++)
                {
                    loopVertices[k] = rotation * loopVertices[k];
                }

                flattened = true;
            }


            //loopVertices.Reverse();

            //for (int j = 0; j < loopVertices.Count; j++)
            //{
            //    Vector3 p0 = loopVertices[MathUtility.ClampListIndex(j, loopVertices.Count)];
            //    Vector3 p1 = loopVertices[MathUtility.ClampListIndex(j + 1, loopVertices.Count)];

            //    Debug.DrawLine(p0, p1, Color.yellow, 1000.0f);
            //}

            //Debug.Log("Before:");
            //for (int j = 0; j < loopVertices.Count; j++)
            //{
            //    Debug.Log(loopVertices[j]);
            //}


            //List<Triangle> face = Triangulator.TriangulatePolygonN(loopVertices);//, true, loopNormal);
            List<Triangle> face = Triangulator.TriangulatePolygon(loopVertices);//, true, loopNormal);


            //Debug.Log("After:");
            //for (int j = 0; j < vts.Length; j++)
            //{
            //    Debug.Log(vts[j]);
            //}

            if(flattened)
            {
                Mesh m = BuildingUtility.TrianglesToMesh(face, true);
                Vector3[] faceVertices = m.vertices;
                Vector3[] faceNormals = m.normals;

                Quaternion invRotation = Quaternion.Inverse(rotation);

                // rotation = Quaternion.FromToRotation(loopNormal, Vector3.up);

                for (int j = 0; j < faceVertices.Length; j++)
                {
                    faceVertices[j] = invRotation * faceVertices[j];
                    faceNormals[j] = loopNormal;
                }

                m.vertices = faceVertices;
                m.normals = faceNormals;

                finalMeshes.Add(m);
            }
            else
            {
                for (int j = 0; j < face.Count; j++)
                {
                    face[j].normal = loopNormal;

                    Vector3 v1 = face[j].v1.position;
                    Vector3 v2 = face[j].v2.position;
                    Vector3 v3 = face[j].v3.position;
                    Vector3 e1 = v2 - v1;
                    Vector3 e2 = v3 - v1;

                    Vector3 calculatedNormal = Vector3.Cross(e1, e2).normalized;

                    if (calculatedNormal != loopNormal)
                    {
                        face[j].ChangeOrientation();
                    }
                }

                finalMeshes.Add(BuildingUtility.TrianglesToMesh(face, true));
            }


        }

        //    for (int i = 0; i < parts.Length; i++)
        //{


        //    if (edgeLoops == null || edgeLoops.Count == 0)
        //    {
        //        Debug.Log("SimplifyFaces: No edge loops detected");
        //        return null;
        //    }

        //    if (edgeLoops.Count > 1)
        //    {
        //        Debug.Log("SimplifyFaces: More than one edge loop detected");
        //        return null;
        //    }

        //    EdgeLoop el = edgeLoops[0];

        //    // retrieve the actual vertex vector3's from the edge loop indicies
        //    Vector3[] cutLoopVertices = new Vector3[el.Vertices.Length];

        //    List<Vector3> loopVertices = new List<Vector3>();

        //    int[] verts = el.Vertices;

        //    for (int k = 0; k < verts.Length; k++)
        //    {
        //        loopVertices.Add((Vector3)dmesh.GetVertex(verts[k]));
        //    }

        //    List<Triangle> face = Triangulator.TriangulatePolygon(loopVertices);
        //    finalMeshes.Add(BuildingUtility.TrianglesToMesh(face, true ));
        //}

        // combine all meshes into a single mesh
        return BuildingUtility.CombineMeshes(finalMeshes);

        //return null;
    }

    //// attempts to correct angles of 180, 90, 45 degrees
    //// should input a polygon that is approximately orthogonal for best results
    //List<Vector3> CorrectAnglesOfPolygon(List<Vector3> polygon, bool isXOriented)
    //{
    //    List<Vector3> correctedVertices = polygon;

    //    for (int i = 1; i < polygon.Count - 1; i++)
    //    {
    //        int index1 = i - 1;
    //        int index2 = i;
    //        int index3 = i + 1;

    //        //Edge3D edge = new Edge3D();
    //        //edge.vertex[0] = edgeVertices[i - 1];
    //        //edge.vertex[1] = edgeVertices[i];

    //        //Edge3D edge1 = edges[i - 1];
    //        //Edge3D edge2 = edges[i];

    //        Vector3 vert1 = polygon[index1];
    //        Vector3 vert2 = polygon[index2];
    //        Vector3 vert3 = polygon[index3];

    //        Vector3 norm1 = (vert1 - vert2).normalized;
    //        Vector3 norm2 = (vert3 - vert2).normalized;

    //        //a.Normalize();
    //        //b.Normalize();

    //        //Debug.DrawLine(vert2, vert2 + (norm1 * 1.5f), Color.green, 1000.0f, false);
    //        //Debug.DrawLine(vert2, vert2 + (norm2 * 1.5f), Color.green, 1000.0f, false);

    //        float angle = Mathf.Acos(Vector3.Dot(norm1, norm2)) * Mathf.Rad2Deg;


    //        //Debug.Log(angle);

    //        if (angle != 0.0f && angle != 90.0f)
    //        {
    //            Vector3 referenceAngle1 = Vector3.right;
    //            Vector3 referenceAngle2 = Vector3.forward;

    //            if (isXOriented)
    //            {
    //                referenceAngle1 = Vector3.right;
    //                referenceAngle2 = Vector3.forward;
    //            }
    //            else
    //            {
    //                referenceAngle1 = Vector3.forward;
    //                referenceAngle2 = Vector3.right;
    //            }


    //            //float angle = Mathf.Acos(Vector3.Dot(norm1, norm2)) * Mathf.Rad2Deg;

    //            float angle1 = Mathf.Acos(Vector3.Dot(norm1, referenceAngle1)) * Mathf.Rad2Deg;
    //            float angle2 = Mathf.Acos(Vector3.Dot(norm2, referenceAngle1)) * Mathf.Rad2Deg;

    //            if (angle1 != 0.0f)
    //            {


    //            }
    //            else
    //            {
    //                float offsetAngle = 0.0f;

    //                if (angle2 < 90.0f)
    //                {
    //                    offsetAngle = 90.0f - angle2;
    //                }
    //                else
    //                {
    //                    offsetAngle = angle2 - 90.0f;
    //                }

    //                Vector3 newDirection = Quaternion.Euler(0, offsetAngle, 0) * norm2;

    //                Vector3 oldEdge = (vert3 - vert2);

    //                Debug.DrawLine(vert2, vert2 + oldEdge, Color.cyan, 1000.0f, false);

    //                Vector3 projectedVector = Vector3.Project(oldEdge, newDirection);
    //                Vector3 newPoint = vert2 + projectedVector;

    //                //Debug.DrawLine(vert2, newPoint, Color.yellow, 1000.0f, false);

    //                correctedVertices[index3] = newPoint;

    //                //vert3 = edgeVertices[index3];
    //                //norm2 = (vert3 - vert2).normalized;

    //                //angle = Mathf.Acos(Vector3.Dot(norm1, norm2)) * Mathf.Rad2Deg;

    //            }

    //            //Debug.DrawLine(edge.vertex[0], edge.vertex[0] + (edge.vertex[1] - edge.vertex[0]), Color.red, 1000.0f, false);

    //            //edges.Add(edge);

    //        }


    //    }

    //    return correctedVertices;
    //}

    //public static LocalTransform DetermineOrientation(Vector3 vertices)
    //{
    //    // 
    //}

    public static bool isPolygonClockwise(List<Vector3> polygon, Vector3? normal = null)
    {

       // bool flattened = false;
        //Quaternion rotation = Quaternion.identity;
        if (normal.HasValue && normal != Vector3.up)
        {
            Quaternion rotation = Quaternion.FromToRotation(normal.Value, Vector3.up);

            for (int k = 0; k < polygon.Count; k++)
            {
                polygon[k] = rotation * polygon[k];
            }

            //flattened = true;
        }

        float total = 0f;

        for(int i = 0; i < polygon.Count; i++)
        {
            Vector3 p0 = polygon[i];
            Vector3 p1 = polygon[MathUtility.ClampListIndex(i + 1, polygon.Count)];

            total += (p1.x - p0.x) * (p1.y + p0.y);
        }

        return total > 0 ? true : false;
    }
}
