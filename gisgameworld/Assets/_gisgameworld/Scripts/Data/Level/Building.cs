using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building
{
    private Mesh mesh;
    public Mesh Mesh { get { return mesh; } }
    //Vector3[] vertices;

    public Building(Mesh m)
    {
        mesh = m;
    }
}
