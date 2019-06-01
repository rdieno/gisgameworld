using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Building
{
    //private Mesh mesh;
    //public Mesh Mesh { get { return mesh; } }
    //Vector3[] vertices;

    //private List<Triangle> geometry;
    //public List<Triangle> Geometry
    //{
    //    get => geometry;
    //    set => geometry = value;
    //}

    //[SerializeField]
    private List<Vector3> polygon;
    public List<Vector3> Polygon
    {
        get => polygon; 
        set => polygon = value; 
    }

    private int osmElementIndex;
    public int OSMElementIndex
    {
        get => osmElementIndex; 
        set => osmElementIndex = value; 
    }
    

    private Vector3[] vertices;
    public Vector3[] Vertices
    {
        get => vertices;
        set => vertices = value;
    }

    private Vector3[] normals;
    public Vector3[] Normals
    {
        get => normals;
        set => normals = value;
    }

    private int[] triangles;
    public int[] Triangles
    {
        get => triangles; set => triangles = value;
    }

    //public Building(Mesh m)
    //{
    //    mesh = m;
    //}

    //public Building(List<Triangle> geometry)
    //{
    //    this.geometry = geometry;
    //}


    //public Building(List<Triangle> geometry, List<Vector3> polygon)
    //{
    //    this.geometry = geometry;
    //    this.polygon = polygon;
    //}

    //public Building(List<Triangle> geometry, List<Vector3> polygon, int osmElementIndex)
    //{
    //    this.geometry = geometry;
    //    this.polygon = polygon;
    //    this.osmElementIndex = osmElementIndex;
    //}

    public Building(List<Vector3> polygon, int osmElementIndex)
    {
        this.polygon = polygon;
        this.osmElementIndex = osmElementIndex;
        this.vertices = null;
        this.triangles = null;
        this.normals = null;
    }

    //public Building(Mesh m, List<Triangle> g)
    //{
    //    mesh = m;
    //    geometry = g;
    //}
}
