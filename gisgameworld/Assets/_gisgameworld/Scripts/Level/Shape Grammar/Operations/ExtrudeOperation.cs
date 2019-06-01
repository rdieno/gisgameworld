using UnityEngine;
using System.Collections;

public class ExtrudeOperation
{
    // general function for extruding a mesh
    // can have multiple segments by adding more matrices to 'extrusion' parameter
    // only tested with flat polygons so far
    public static Mesh Extrude(Mesh mesh, Matrix4x4[] extrusion, Edge[] edges, bool invertFaces, bool handleUVs = false)
    {
        Mesh extrudedMesh = new Mesh();

        int extrudedVertexCount = edges.Length * 2 * extrusion.Length;
        int triIndicesPerStep = edges.Length * 6;
        int extrudedTriIndexCount = triIndicesPerStep * (extrusion.Length - 1);

        Vector3[] inputVertices = mesh.vertices;
        Vector2[] inputUV = mesh.uv;
        int[] inputTriangles = mesh.triangles;

        Vector3[] vertices = new Vector3[extrudedVertexCount + mesh.vertexCount * 2];
        Vector2[] uvs = new Vector2[vertices.Length];
        int[] triangles = new int[extrudedTriIndexCount + inputTriangles.Length * 2];

        // Build extruded vertices
        int v = 0;
        for (int i = 0; i < extrusion.Length; i++)
        {
            Matrix4x4 matrix = extrusion[i];
            float vcoord = (float)i / (extrusion.Length - 1);
            foreach (Edge e in edges)
            {
                vertices[v + 0] = matrix.MultiplyPoint(inputVertices[e.vertexIndex[0]]);
                vertices[v + 1] = matrix.MultiplyPoint(inputVertices[e.vertexIndex[1]]);
                if (handleUVs)
                {
                    uvs[v + 0] = new Vector2(inputUV[e.vertexIndex[0]].x, vcoord);
                    uvs[v + 1] = new Vector2(inputUV[e.vertexIndex[1]].x, vcoord);
                }

                v += 2;
            }
        }

        // Build cap vertices
        // * The bottom mesh we scale along it's negative extrusion direction. This way extruding a half sphere results in a capsule.
        for (int c = 0; c < 2; c++)
        {
            Matrix4x4 matrix = extrusion[c == 0 ? 0 : extrusion.Length - 1];
            int firstCapVertex = c == 0 ? extrudedVertexCount : extrudedVertexCount + inputVertices.Length;
            for (int i = 0; i < inputVertices.Length; i++)
            {
                vertices[firstCapVertex + i] = matrix.MultiplyPoint(inputVertices[i]);
                if (handleUVs)
                {
                    uvs[firstCapVertex + i] = inputUV[i];
                }
            }
        }

        // Build extruded triangles
        for (int i = 0; i < extrusion.Length - 1; i++)
        {
            int baseVertexIndex = (edges.Length * 2) * i;
            int nextVertexIndex = (edges.Length * 2) * (i + 1);
            for (int e = 0; e < edges.Length; e++)
            {
                int triIndex = i * triIndicesPerStep + e * 6;

                triangles[triIndex + 0] = baseVertexIndex + e * 2;
                triangles[triIndex + 1] = nextVertexIndex + e * 2;
                triangles[triIndex + 2] = baseVertexIndex + e * 2 + 1;
                triangles[triIndex + 3] = nextVertexIndex + e * 2;
                triangles[triIndex + 4] = nextVertexIndex + e * 2 + 1;
                triangles[triIndex + 5] = baseVertexIndex + e * 2 + 1;
            }
        }

        // build cap triangles
        int triCount = inputTriangles.Length / 3;
        // Top
        {
            int firstCapVertex = extrudedVertexCount;
            int firstCapTriIndex = extrudedTriIndexCount;
            for (int i = 0; i < triCount; i++)
            {
                triangles[i * 3 + firstCapTriIndex + 0] = inputTriangles[i * 3 + 1] + firstCapVertex;
                triangles[i * 3 + firstCapTriIndex + 1] = inputTriangles[i * 3 + 2] + firstCapVertex;
                triangles[i * 3 + firstCapTriIndex + 2] = inputTriangles[i * 3 + 0] + firstCapVertex;
            }
        }

        // Bottom
        {
            int firstCapVertex = extrudedVertexCount + inputVertices.Length;
            int firstCapTriIndex = extrudedTriIndexCount + inputTriangles.Length;
            for (int i = 0; i < triCount; i++)
            {
                triangles[i * 3 + firstCapTriIndex + 0] = inputTriangles[i * 3 + 0] + firstCapVertex;
                triangles[i * 3 + firstCapTriIndex + 1] = inputTriangles[i * 3 + 2] + firstCapVertex;
                triangles[i * 3 + firstCapTriIndex + 2] = inputTriangles[i * 3 + 1] + firstCapVertex;
            }
        }

        if (invertFaces)
        {
            for (int i = 0; i < triangles.Length / 3; i++)
            {
                int temp = triangles[i * 3 + 0];
                triangles[i * 3 + 0] = triangles[i * 3 + 1];
                triangles[i * 3 + 1] = temp;
            }
        }

        extrudedMesh.Clear();
        extrudedMesh.name = "extruded";
        extrudedMesh.vertices = vertices;
        if (handleUVs)
        {
            extrudedMesh.uv = uvs;
        }
        extrudedMesh.triangles = triangles;
        extrudedMesh.RecalculateNormals();

        return extrudedMesh;
    }


    // finds outer edges, outer edges are those that connect to only one triangle
    public static Edge[] FindOuterEdges(Mesh mesh)
    {
        // find all edges
        Edge[] edges = FindAllEdges(mesh.vertexCount, mesh.triangles);

        // only keep the ones that only point to a single face
        ArrayList outerEdges = new ArrayList();
        foreach (Edge edge in edges)
        {
            if (edge.faceIndex[0] == edge.faceIndex[1])
            {
                outerEdges.Add(edge);
            }
        }

        return outerEdges.ToArray(typeof(Edge)) as Edge[];
    }



    // finds all uniques edges in a mesh
    // algorithm retrived from: https://github.com/knapeczadam/Unity-Procedural-Examples-Updated
    public static Edge[] FindAllEdges(int vertexCount, int[] triangleArray)
    {
        int maxEdgeCount = triangleArray.Length;
        int[] firstEdge = new int[vertexCount + maxEdgeCount];
        int nextEdge = vertexCount;
        int triangleCount = triangleArray.Length / 3;

        for (int a = 0; a < vertexCount; a++)
            firstEdge[a] = -1;

        // First pass over all triangles. This finds all the edges satisfying the
        // condition that the first vertex index is less than the second vertex index
        // when the direction from the first vertex to the second vertex represents
        // a counterclockwise winding around the triangle to which the edge belongs.
        // For each edge found, the edge index is stored in a linked list of edges
        // belonging to the lower-numbered vertex index i. This allows us to quickly
        // find an edge in the second pass whose higher-numbered vertex index is i.
        Edge[] edgeArray = new Edge[maxEdgeCount];

        int edgeCount = 0;
        for (int a = 0; a < triangleCount; a++)
        {
            int i1 = triangleArray[a * 3 + 2];
            for (int b = 0; b < 3; b++)
            {
                int i2 = triangleArray[a * 3 + b];
                if (i1 < i2)
                {
                    Edge newEdge = new Edge();
                    newEdge.vertexIndex[0] = i1;
                    newEdge.vertexIndex[1] = i2;
                    newEdge.faceIndex[0] = a;
                    newEdge.faceIndex[1] = a;
                    edgeArray[edgeCount] = newEdge;

                    int edgeIndex = firstEdge[i1];
                    if (edgeIndex == -1)
                    {
                        firstEdge[i1] = edgeCount;
                    }
                    else
                    {
                        while (true)
                        {
                            int index = firstEdge[nextEdge + edgeIndex];
                            if (index == -1)
                            {
                                firstEdge[nextEdge + edgeIndex] = edgeCount;
                                break;
                            }

                            edgeIndex = index;
                        }
                    }

                    firstEdge[nextEdge + edgeCount] = -1;
                    edgeCount++;
                }

                i1 = i2;
            }
        }

        // Second pass over all triangles. This finds all the edges satisfying the
        // condition that the first vertex index is greater than the second vertex index
        // when the direction from the first vertex to the second vertex represents
        // a counterclockwise winding around the triangle to which the edge belongs.
        // For each of these edges, the same edge should have already been found in
        // the first pass for a different triangle. Of course we might have edges with only one triangle
        // in that case we just add the edge here
        // So we search the list of edges
        // for the higher-numbered vertex index for the matching edge and fill in the
        // second triangle index. The maximum number of comparisons in this search for
        // any vertex is the number of edges having that vertex as an endpoint.

        for (int a = 0; a < triangleCount; a++)
        {
            int i1 = triangleArray[a * 3 + 2];
            for (int b = 0; b < 3; b++)
            {
                int i2 = triangleArray[a * 3 + b];
                if (i1 > i2)
                {
                    bool foundEdge = false;
                    for (int edgeIndex = firstEdge[i2]; edgeIndex != -1; edgeIndex = firstEdge[nextEdge + edgeIndex])
                    {
                        Edge edge = edgeArray[edgeIndex];
                        if ((edge.vertexIndex[1] == i1) && (edge.faceIndex[0] == edge.faceIndex[1]))
                        {
                            edgeArray[edgeIndex].faceIndex[1] = a;
                            foundEdge = true;
                            break;
                        }
                    }

                    if (!foundEdge)
                    {
                        Edge newEdge = new Edge();
                        newEdge.vertexIndex[0] = i1;
                        newEdge.vertexIndex[1] = i2;
                        newEdge.faceIndex[0] = a;
                        newEdge.faceIndex[1] = a;
                        edgeArray[edgeCount] = newEdge;
                        edgeCount++;
                    }
                }

                i1 = i2;
            }
        }

        Edge[] compactedEdges = new Edge[edgeCount];
        for (int e = 0; e < edgeCount; e++)
            compactedEdges[e] = edgeArray[e];

        return compactedEdges;
    }
}
