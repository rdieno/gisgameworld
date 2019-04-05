using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building
{
    private Mesh mesh;
    public Mesh Mesh { get { return mesh; } }
    //Vector3[] vertices;

    private List<Triangle> geometry;
    public List<Triangle> Geometry { get { return geometry; } }

    public Building(Mesh m)
    {
        mesh = m;
    }

    public Building(Mesh m, List<Triangle> g)
    {
        mesh = m;
        geometry = g;
    }
}
