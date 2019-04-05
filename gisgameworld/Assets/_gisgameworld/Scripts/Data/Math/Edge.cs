using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// an edge between two vertices
public class Edge
{
    public Vertex v1;
    public Vertex v2;

    public bool isIntersecting = false;

    public Edge(Vertex v1, Vertex v2)
    {
        this.v1 = v1;
        this.v2 = v2;
    }

    public Edge(Vector3 v1, Vector3 v2)
    {
        this.v1 = new Vertex(v1);
        this.v2 = new Vertex(v2);
    }

    public Vector2 GetVertex2D(Vertex v)
    {
        return new Vector2(v.position.x, v.position.z);
    }

    public void FlipEdge()
    {
        Vertex temp = v1;

        v1 = v2;

        v2 = temp;
    }
}
