using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Triangle
{
    public Vertex v1;
    public Vertex v2;
    public Vertex v3;

    public Vector3? normal;

    // because of the half edge data structure we just need one half edge
   // public HalfEdge halfEdge;

    public Triangle(Vertex v1, Vertex v2, Vertex v3)
    {
        this.v1 = v1;
        this.v2 = v2;
        this.v3 = v3;
        this.normal = null;
    }

    public Triangle(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        this.v1 = new Vertex(v1);
        this.v2 = new Vertex(v2);
        this.v3 = new Vertex(v3);
        this.normal = null;
    }

    public Triangle(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 normal)
    {
        this.v1 = new Vertex(v1);
        this.v2 = new Vertex(v2);
        this.v3 = new Vertex(v3);
        this.normal = normal;
    }

    //public Triangle(HalfEdge he)
    //{
    //    halfEdge = he;
    //}

    // changes triangle orientation from counterclockwise to clockwise and vice-versa
    public void ChangeOrientation()
    {
        Vertex temp = this.v1;

        this.v1 = this.v2;

        this.v2 = temp;
    }
}