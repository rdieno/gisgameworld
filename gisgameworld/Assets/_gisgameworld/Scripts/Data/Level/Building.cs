using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Building
{
    private Mesh mesh;
    public Mesh Mesh
    {
        get => mesh;
        set => mesh = value;

    }

    //Vector3[] vertices;

    //private List<Triangle> geometry;
    //public List<Triangle> Geometry
    //{
    //    get => geometry;
    //    set => geometry = value;
    //}

    //[SerializeField]
    private List<Vector3> footprint;
    public List<Vector3> Footprint
    {
        get => footprint; 
        set => footprint = value; 
    }

    private int osmElementIndex;
    public int OSMElementIndex
    {
        get => osmElementIndex; 
        set => osmElementIndex = value; 
    }

    //private int buildingIndex;
    //public int BuildingIndex
    //{
    //    get => buildingIndex; 
    //    set => buildingIndex = value; 
    //}


    //private Vector3[] vertices;
    //public Vector3[] Vertices
    //{
    //    get => vertices;
    //    set => vertices = value;
    //}

    //private Vector3[] normals;
    //public Vector3[] Normals
    //{
    //    get => normals;
    //    set => normals = value;
    //}

    //private int[] triangles;
    //public int[] Triangles
    //{
    //    get => triangles;
    //    set => triangles = value;
    //}


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

    //private LocalTransform baseTransform;
    //public LocalTransform BaseTransform
    //{
    //    get => baseTransform;
    //    set => baseTransform = value;
    //}


    //public void SetTransform(Vector3 origin, Vector3 up, Vector3 forward, Vector3 right)
    //{
    //    baseTransform = new LocalTransform(origin, up, right, forward);
    //}

    //public void SetTransform(Vector3 origin, Vector3 up, Vector3 forward)
    //{
    //    baseTransform = new LocalTransform(origin, up, forward);
    //}

    //private List<Shape> current;
    //public List<Shape> Current
    //{
    //    get => current;
    //    set => current = value;
    //}

    private Shape root;
    public Shape Root
    {
        get => root;
        set => root = value;
    }
    

    //public Mesh Mesh
    //{
    //    // traverse shape tree
    //    get
    //    {
    //        if (root != null)
    //        {
    //            if (root.Children != null)
    //            {

    //                return BuildingUtility.CombineShapes(this.current);
    //            }
    //            else
    //            {
    //                return root.Mesh;
    //            }
    //        }
    //        else
    //        {
    //            return null;
    //        }
    //    }
    //}

    public Building()
    {
        this.footprint = null;
        this.osmElementIndex = -1;
        this.root = null;
    }

    public Building(List<Vector3> footprint, int osmElementIndex)
    {
        this.footprint = footprint;
        this.osmElementIndex = osmElementIndex;
        //this.vertices = null;
        //this.triangles = null;
        //this.normals = null;
        //this.baseTransform = null;
        this.root = null;
        //this.current = null;
    }

    public Building(List<Vector3> footprint, int osmElementIndex, Shape root)
    {
        this.footprint = footprint;
        this.osmElementIndex = osmElementIndex;
        this.root = root;
        //this.root.OwnerIndex = buildingIndex;
        this.mesh = root.Mesh;
    }

    //public Building(List<Vector3> footprint, int osmElementIndex, int buildingIndex, Shape root)
    //{
    //    this.footprint = footprint;
    //    this.osmElementIndex = osmElementIndex;
    //    this.root = root;
    //    //this.root.OwnerIndex = buildingIndex;
    //}

    //public Building(Mesh m, List<Triangle> g)
    //{
    //    mesh = m;
    //    geometry = g;
    //}

    public void UpdateMesh(Shape shape)
    {
        this.mesh = shape.Mesh;
    }

    public void UpdateMesh(List<Shape> shapes)
    {
        this.mesh = BuildingUtility.CombineShapes(shapes);
    }

}
