using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Shape
{
    //private int ownerIndex;
    //public int OwnerIndex
    //{
    //    get => ownerIndex;
    //    set => ownerIndex = value;
    //}


    public bool isChanged;

    private LocalTransform localTransform;
    public LocalTransform LocalTransform
    {
        get => localTransform;
        set => localTransform = value;
    }

    //public void SetTransform(Vector3 origin, Vector3 up, Vector3 forward, Vector3 right)
    //{ 
    //    localTransform = new LocalTransform(origin, up, right, forward);
    //}

    //public void SetTransform(Vector3 origin, Vector3 up, Vector3 forward)
    //{ 
    //    localTransform = new LocalTransform(origin, up, forward);
    //}


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
        get => triangles;
        set => triangles = value;
    }

    //private Mesh mesh;
    public Mesh Mesh
    {
        get
        {
            Mesh m = new Mesh();
            m.vertices = this.vertices;
            m.normals = this.normals;
            m.triangles = this.triangles;
            m.RecalculateBounds();
            return m;
        }
        
        //set => mesh = value;
    }



    private List<Shape> children;
    public List<Shape> Children
    {
        get => children;
        set => children = value;
    }

    public void AddChild(Shape shape)
    {
        //shape.ownerIndex = this.ownerIndex;
        this.children.Add(shape);
    }

    public Shape()
    {
        this.children = new List<Shape>();
        //this.mesh = null;
        this.vertices = null;
        this.normals = null;
        this.triangles = null;
        this.localTransform = null;
    }

    public Shape(Mesh mesh)
    {
        this.children = new List<Shape>();
        //this.mesh = mesh;
        this.vertices = mesh.vertices;
        this.normals = mesh.normals;
        this.triangles = mesh.triangles;
        this.localTransform = null;
    }

    public Shape(Mesh mesh, LocalTransform localTransform)
    {
        this.children = new List<Shape>();
        //this.mesh = mesh;
        this.vertices = mesh.vertices;
        this.normals = mesh.normals;
        this.triangles = mesh.triangles;
        this.localTransform = localTransform;
        isChanged = false;
    }

    public Shape(Vector3[] vertices, Vector3[] normals, int[] triangles)
    {
        this.children = new List<Shape>();
        //this.mesh = null;
        this.vertices = vertices;
        this.normals = normals;
        this.triangles = triangles;
        this.localTransform = null;
    }

    public Shape(Vector3[] vertices, Vector3[] normals, int[] triangles, LocalTransform localTransform)
    {
        this.children = new List<Shape>();
        //this.mesh = null;
        this.vertices = vertices;
        this.normals = normals;
        this.triangles = triangles;
        this.localTransform = localTransform;
    }

    public Shape(Shape s)
    {
        this.children = new List<Shape>(s.children);
        ////this.mesh = mesh;
        //this.vertices = s.vertices;
        //this.normals = s.normals;
        //this.triangles = s.triangles;
        //this.localTransform = s.localTransform;

        this.vertices = new Vector3[s.vertices.Length];
        this.normals = new Vector3[s.normals.Length];
        this.triangles = new int[s.triangles.Length];
        System.Array.Copy(s.vertices, this.vertices, s.vertices.Length);
        System.Array.Copy(s.normals, this.normals, s.normals.Length);
        System.Array.Copy(s.triangles, this.triangles, s.triangles.Length);

        this.localTransform = new LocalTransform(s.localTransform);
    }

    public void Debug_DrawOrientation(float distance = 5.0f)
    {
        Debug.DrawLine(localTransform.Origin, localTransform.Origin + (localTransform.Up * distance), Color.green, 1000.0f);
        Debug.DrawLine(localTransform.Origin, localTransform.Origin + (localTransform.Right * distance), Color.red, 1000.0f);
        Debug.DrawLine(localTransform.Origin, localTransform.Origin + (localTransform.Forward * distance), Color.blue, 1000.0f);
    }
}
