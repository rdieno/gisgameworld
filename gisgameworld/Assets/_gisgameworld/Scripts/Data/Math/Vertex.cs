using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Vertex
{
    public Vector3 position;

    // the outgoing halfedge 
   // public HalfEdge halfEdge;

    // the non serialized variables must be rebuilt 

    [NonSerialized]
    [JsonIgnore]
    public Triangle triangle;
    [NonSerialized]
    [JsonIgnore]
    public Vertex prevVertex;
    [NonSerialized]
    [JsonIgnore]
    public Vertex nextVertex;

    public bool isConvex;
    public bool isConcave;
    public bool isEar;

    public Vertex()
    {
        this.position = Vector3.zero;
    }

    public Vertex(Vector3 position)
    {
        this.position = position;
    }

    public Vector2 GetVec2XZ()
    {
        Vector2 vecXZ = new Vector2(position.x, position.z);
        return vecXZ;
    }
}
