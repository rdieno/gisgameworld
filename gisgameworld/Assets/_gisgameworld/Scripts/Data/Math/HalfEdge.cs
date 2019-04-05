using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HalfEdge
{
    // the vertex the edge points to
    public Vertex v;

    public Triangle t;

    public HalfEdge nextEdge;

    public HalfEdge prevEdge;

    public HalfEdge oppositeEdge;

    public HalfEdge(Vertex v)
    {
        this.v = v;
    }
}
