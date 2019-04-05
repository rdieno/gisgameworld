using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vertex
{
    public Vector3 position;

    // the outgoing halfedge 
    public HalfEdge halfEdge;

    public Triangle triangle;

    public Vertex prevVertex;
    public Vertex nextVertex;

    public bool isConvex;
    public bool isConcave;
    public bool isEar;

    public Vertex(Vector3 p)
    {
        position = p;
    }

    public Vector2 GetVec2XZ()
    {
        Vector2 vecXZ = new Vector2(position.x, position.z);
        return vecXZ;
    }
}
