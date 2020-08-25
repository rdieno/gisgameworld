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

    private Shape root;
    public Shape Root
    {
        get => root;
        set => root = value;
    }

    private Material[] materials;
    public Material[] Materials
    {
        get => materials;
        set => materials = value;
    }

    private Dictionary<string, List<Shape>> shapes;
    public Dictionary<string, List<Shape>> Shapes
    {
        get => shapes;
        set => shapes = value;
    }

    private Vector3 originalPosition;
    public Vector3 OriginalPosition
    {
        get => originalPosition;
        set => originalPosition = value;
    }

    private BuildingInfo info;
    public BuildingInfo Info
    {
        get => info;
        set => info = value;
    }

    public Building()
    {
        this.footprint = null;
        this.osmElementIndex = -1;
        this.root = null;
        this.info = null;
    }

    public Building(List<Vector3> footprint, int osmElementIndex)
    {
        this.footprint = footprint;
        this.osmElementIndex = osmElementIndex;
        this.root = null;
        this.info = null;
    }

    public Building(List<Vector3> footprint, int osmElementIndex, Shape root)
    {
        this.footprint = footprint;
        this.osmElementIndex = osmElementIndex;
        this.root = root;
        this.mesh = root.Mesh;
        this.info = null;
    }

    public void UpdateMesh(Shape shape)
    {
        this.mesh = shape.Mesh;
    }

    public void UpdateMesh(List<Shape> shapes)
    {
        this.mesh = BuildingUtility.CombineShapes(shapes);
    }

    public void UpdateProcessedBuilding(Dictionary<string, List<Shape>> shapes, bool moveToOriginalLocation = false, bool keepShapeProcessingHistory = false)
    {
        List<Shape> allShapes = new List<Shape>();

        foreach(KeyValuePair<string, List<Shape>> currentRule in shapes)
        {
            if(currentRule.Key != "NIL")
            {
                allShapes.AddRange(currentRule.Value);
            }
        }

        Mesh mesh = BuildingUtility.CombineShapes(allShapes);

        if(moveToOriginalLocation)
        {
            Vector3[] vertices = mesh.vertices;

            for (int j = 0; j < vertices.Length; j++)
            {
                vertices[j] = new Vector3(vertices[j].x + originalPosition.x, vertices[j].y, vertices[j].z + originalPosition.z);

                root.LocalTransform.Origin = originalPosition;

                mesh.vertices = vertices;

            }

        }

        if (keepShapeProcessingHistory)
        {
            this.shapes = shapes;
        }

        this.mesh = mesh;
    }

    // remove data to save on disk space when saving
    public void PrepareForSerialization()
    {
        // shapes are not required for displaying the final mesh
        shapes = null;
    }
}
