using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class Triangulator
{
    // this function takes a polygon and turns it into triangles using the ear clipping algorithm
    // the points on the polygon should be ordered counter-clockwise and should have at least 3 points
    public static List<Triangle> TriangulatePolygon(List<Vector3> polygon)
    {
        List<Triangle> triangles = new List<Triangle>();

        // if we only have three points just return the triangle
        if (polygon.Count == 3)
        {
            triangles.Add(new Triangle(polygon[0], polygon[1], polygon[2]));

            return triangles;
        }

        // store the vertices in a list and determine their next and previous vertices
        List<Vertex> vertices = new List<Vertex>();

        for (int i = 0; i < polygon.Count; i++)
        {
            vertices.Add(new Vertex(polygon[i]));
        }

        for (int i = 0; i < vertices.Count; i++)
        {
            int nextPos = MathUtility.ClampListIndex(i + 1, vertices.Count);
            int prevPos = MathUtility.ClampListIndex(i - 1, vertices.Count);

            vertices[i].prevVertex = vertices[prevPos];
            vertices[i].nextVertex = vertices[nextPos];
        }

        // find the convex and concave vertices
        for (int i = 0; i < vertices.Count; i++)
        {
            CheckIfConvexOrConcave(vertices[i]);
        }

        // now that we know which vertices are convex and concave we can find the ears
        List<Vertex> earVertices = new List<Vertex>();

        for (int i = 0; i < vertices.Count; i++)
        {
            IsVertexEar(vertices[i], vertices, earVertices);
        }

        // begin ear clipping
        while (true)
        {
            if (earVertices.Count < 1)
            {
                Debug.Log("Triangulator: Bad Polygon");
                return null;
            }


            // only one triangle left so just add it to the list
            if (vertices.Count == 3)
            {
                //The final triangle
                triangles.Add(new Triangle(vertices[0], vertices[0].prevVertex, vertices[0].nextVertex));

                break;
            }

            // make a triangle from the first ear
            Vertex earVertex = earVertices[0];
   
            Vertex earVertexPrev = earVertex.prevVertex;
            Vertex earVertexNext = earVertex.nextVertex;

            Triangle newTriangle = new Triangle(earVertex, earVertexPrev, earVertexNext);

            triangles.Add(newTriangle);

            // remove the vertex from the lists
            earVertices.Remove(earVertex);
            vertices.Remove(earVertex);

            // update the next and previous vertices
            earVertexPrev.nextVertex = earVertexNext;
            earVertexNext.prevVertex = earVertexPrev;

            // check if the removal results in a new ear
            CheckIfConvexOrConcave(earVertexPrev);
            CheckIfConvexOrConcave(earVertexNext);

            earVertices.Remove(earVertexPrev);
            earVertices.Remove(earVertexNext);

            IsVertexEar(earVertexPrev, vertices, earVertices);
            IsVertexEar(earVertexNext, vertices, earVertices);
        }

        //Debug.Log(triangles.Count);

        return triangles;
    }

    // check if a vertex if either convex or concave
    private static void CheckIfConvexOrConcave(Vertex v)
    {
        v.isConcave = false;
        v.isConvex = false;

        // create triangle with the next and previous vertices
        Vector2 a = v.prevVertex.GetVec2XZ();
        Vector2 b = v.GetVec2XZ();
        Vector2 c = v.nextVertex.GetVec2XZ();

        if (MathUtility.IsTriangleOrientedClockwise(a, b, c))
        {
            v.isConcave = true;
        }
        else
        {
            v.isConvex = true;
        }
    }
    
    // check if a vertex is an ear
    private static void IsVertexEar(Vertex v, List<Vertex> vertices, List<Vertex> earVertices)
    {
        // concave vectors cannot be ears
        if (v.isConcave)
        {
            return;
        }

        Vector2 a = v.prevVertex.GetVec2XZ();
        Vector2 b = v.GetVec2XZ();
        Vector2 c = v.nextVertex.GetVec2XZ();

        bool hasPointInside = false;

        // check if there are any concave vertices inside the triangle
        for (int i = 0; i < vertices.Count; i++)
        {
            if (vertices[i].isConcave)
            {
                Vector2 p = vertices[i].GetVec2XZ();

                if (MathUtility.IsPointInTriangle(a, b, c, p))
                {
                    hasPointInside = true;
                    break;
                }
            }
        }

        if (!hasPointInside)
        {
            earVertices.Add(v);
        }
    }
}
