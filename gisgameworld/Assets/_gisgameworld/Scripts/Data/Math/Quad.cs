using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quad
{
    public Triangle t1;
    public Triangle t2;

    public Vertex[] vertices;

    public Quad(Triangle t1, Triangle t2)
    {
        this.t1 = t1;
        this.t2 = t2;
    }
}
