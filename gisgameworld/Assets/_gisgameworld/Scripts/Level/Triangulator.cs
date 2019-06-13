﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class Triangulator
{
    // this function takes a polygon and turns it into triangles using the ear clipping algorithm
    // the points on the polygon should be ordered counter-clockwise and should have at least 3 points
    public static List<Triangle> TriangulatePolygonN(List<Vector3> polygon, bool flatten = false, Vector3? normal = null)
    {
        bool flattened = false;
        Quaternion rotation = Quaternion.identity;

        if (flatten && normal != Vector3.up)
        {
            rotation = Quaternion.FromToRotation(normal.Value, Vector3.up);

            for (int i = 0; i < polygon.Count; i++)
            {
                polygon[i] = rotation * polygon[i];
            }

            flattened = true;
        }

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

        List<Vertex> earVertices = new List<Vertex>();

        int attempt = 1;

        //while(!foundEar)
        //{

        CheckForEarVertices(vertices, earVertices);

        //for (int i = 0; i < vertices.Count; i++)
        //{
        //    int nextPos = MathUtility.ClampListIndex(i + 1, vertices.Count);
        //    int prevPos = MathUtility.ClampListIndex(i - 1, vertices.Count);

        //    vertices[i].prevVertex = vertices[prevPos];
        //    vertices[i].nextVertex = vertices[nextPos];
        //}

        //// find the convex and concave vertices
        //for (int i = 0; i < vertices.Count; i++)
        //{
        //    CheckIfConvexOrConcave(vertices[i]);
        //}

        //// now that we know which vertices are convex and concave we can find the ears
        //for (int i = 0; i < vertices.Count; i++)
        //{
        //    IsVertexEar(vertices[i], vertices, earVertices);
        //}

        //    if(earVertices.Count > 0)
        //    {
        //        foundEar = true;
        //    }
        //    else
        //    {
        //        if (attempt == 2)
        //        {
        //            Debug.Log("Triangulator: Bad Polygon");
        //            return null;
        //        }
        //        else
        //        {
        //            vertices.Reverse();
        //        }


        //    }

        //    attempt++;
        //}

        // begin ear clipping
        while (true)
        {
            if (earVertices.Count == 0)
            {
                if(attempt < 2)
                {
                    vertices.Clear();

                    for (int i = 0; i < polygon.Count; i++)
                    {
                        vertices.Add(new Vertex(polygon[i]));
                    }

                    vertices.Reverse();
                    earVertices.Clear();
                    triangles.Clear();

                    CheckForEarVertices(vertices, earVertices);

                    if (earVertices.Count == 0)
                    {
                        Debug.Log("Triangulator: Bad Polygon");

                        System.Diagnostics.Debugger.Break();

                        break;
                    }

                    attempt++;
                }
                else
                {
                    Debug.Log("Triangulator: Bad Polygon");

                    System.Diagnostics.Debugger.Break();

                    break;
                }
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

        if (flattened)
        {

            Quaternion inverseRotation = Quaternion.FromToRotation(Vector3.up, normal.Value);

            ///Quaternion inverseRotation = Quaternion.Inverse(rotation);

            for (int i = 0; i < triangles.Count; i++)
            {
                triangles[i].v1.position = inverseRotation * triangles[i].v1.position;
                triangles[i].v2.position = inverseRotation * triangles[i].v2.position;
                triangles[i].v3.position = inverseRotation * triangles[i].v3.position;
            }
        }

        return triangles;
    }

    // this function takes a polygon and turns it into triangles using the ear clipping algorithm
    // the points on the polygon should be ordered counter-clockwise and should have at least 3 points
    public static List<Triangle> TriangulatePolygon(List<Vector3> polygon, bool isSplit = false)
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

        List<Vertex> earVertices = new List<Vertex>();

        int attempt = 1;

        //while(!foundEar)
        //{

        CheckForEarVertices(vertices, earVertices);

        //for (int i = 0; i < vertices.Count; i++)
        //{
        //    int nextPos = MathUtility.ClampListIndex(i + 1, vertices.Count);
        //    int prevPos = MathUtility.ClampListIndex(i - 1, vertices.Count);

        //    vertices[i].prevVertex = vertices[prevPos];
        //    vertices[i].nextVertex = vertices[nextPos];
        //}

        //// find the convex and concave vertices
        //for (int i = 0; i < vertices.Count; i++)
        //{
        //    CheckIfConvexOrConcave(vertices[i]);
        //}

        //// now that we know which vertices are convex and concave we can find the ears
        //for (int i = 0; i < vertices.Count; i++)
        //{
        //    IsVertexEar(vertices[i], vertices, earVertices);
        //}

        //    if(earVertices.Count > 0)
        //    {
        //        foundEar = true;
        //    }
        //    else
        //    {
        //        if (attempt == 2)
        //        {
        //            Debug.Log("Triangulator: Bad Polygon");
        //            return null;
        //        }
        //        else
        //        {
        //            vertices.Reverse();
        //        }


        //    }

        //    attempt++;
        //}

        // begin ear clipping
        while (true)
        {
            if (isSplit)
            {
                if (earVertices.Count < 1)
                {
                    if (triangles.Count > 0)
                    {
                        return triangles;
                    }
                    else if (attempt == 2)
                    {
                        Debug.Log("Triangulator: Bad Polygon");

                        System.Diagnostics.Debugger.Break();

                        return null;
                    }
                    else
                    {
                        //return null;

                        vertices.Clear();

                        for (int i = 0; i < polygon.Count; i++)
                        {
                            vertices.Add(new Vertex(polygon[i]));
                        }

                        vertices.Reverse();
                        earVertices.Clear();
                        triangles.Clear();

                        CheckForEarVertices(vertices, earVertices);

                        if (earVertices.Count < 1)
                        {
                            Debug.Log("Triangulator: Bad Polygon");

                            System.Diagnostics.Debugger.Break();

                            return null;

                        }

                        attempt++;
                    }
                }
            }
            else
            {
                if (earVertices.Count < 1)
                {
                    if (attempt == 2)
                    {
                        Debug.Log("Triangulator: Bad Polygon");

                        return triangles;
                    }

                    vertices.Clear();

                    for (int i = 0; i < polygon.Count; i++)
                    {
                        vertices.Add(new Vertex(polygon[i]));
                    }

                    vertices.Reverse();
                    earVertices.Clear();
                    triangles.Clear();

                    CheckForEarVertices(vertices, earVertices);

                    if (earVertices.Count < 1)
                    {
                        Debug.Log("Triangulator: Bad Polygon");

                        return null;
                    }

                    attempt++;
                }
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

    public static void CheckForEarVertices(List<Vertex> vertices, List<Vertex> earVertices)
    {
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
        for (int i = 0; i < vertices.Count; i++)
        {
            IsVertexEar(vertices[i], vertices, earVertices);
        }
    }

    public static List<Vector3> RemoveUnecessaryVertices(List<Vector3> polygon, Vector3 normal)
    {
        bool flattened = false;
        Quaternion rotation = Quaternion.identity;
        List<int> toRemove = new List<int>();

        if (polygon.Count > 2)
        {
            if(normal != Vector3.up)
            {
                rotation = Quaternion.FromToRotation(normal, Vector3.up);

                for (int i = 0; i < polygon.Count; i++)
                {
                    polygon[i] = rotation * polygon[i];
                }

                flattened = true;
            }

            for (int i = 0; i < polygon.Count; i++)
            {
                Vector3 vert1 = polygon[MathUtility.ClampListIndex(i, polygon.Count)];
                Vector3 vert2 = polygon[MathUtility.ClampListIndex(i + 1, polygon.Count)];
                Vector3 vert3 = polygon[MathUtility.ClampListIndex(i + 2, polygon.Count)];


                // calculate the normals pointing away from the middle vertex
                Vector3 norm1 = (vert1 - vert2).normalized;
                Vector3 norm2 = (vert3 - vert2).normalized;

                // calculate the angle between the two normals in degrees
                float angle = Mathf.Acos(Vector3.Dot(norm1, norm2)) * Mathf.Rad2Deg;


                float errorMargin = 0.00001f;

                if (Mathf.Abs(angle - 180f) < errorMargin)
                {
                    toRemove.Add(MathUtility.ClampListIndex(i + 1, polygon.Count));
                }

            }

            toRemove.Sort();
            
            for(int i = toRemove.Count - 1; i >= 0; i--)
            {
                polygon.RemoveAt(toRemove[i]);
            }

            if(flattened)
            {
                for (int i = 0; i < polygon.Count; i++)
                {
                    polygon[i] = Quaternion.Inverse(rotation) * polygon[i];
                }
                
            }

        }

        return polygon;
    }

}
