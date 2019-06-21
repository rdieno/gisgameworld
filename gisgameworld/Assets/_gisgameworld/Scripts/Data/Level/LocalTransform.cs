using UnityEngine;
using System.Collections;

public class LocalTransform
{
    private Vector3 origin;
    public Vector3 Origin
    {
        get => origin;
        set => origin = value;
    }

    private Vector3 up;
    public Vector3 Up
    {
        get => up;
        set => up = value;
    }

    private Vector3 right;
    public Vector3 Right
    {
        get => right;
        set => right = value;
    }

    private Vector3 forward;
    public Vector3 Forward
    {
        get => forward;
        set => forward = value;
    }

    public LocalTransform()
    {
        this.origin = this.up = this.forward = this.right = Vector3.zero;
    }

    public LocalTransform(Vector3 origin, Vector3 up, Vector3 forward)
    {
        this.origin = origin;
        this.up = up;
        this.forward = forward;
        this.right = Vector3.Cross(up, forward).normalized;
    }

    public LocalTransform(Vector3 origin, Vector3 up, Vector3 forward, Vector3 right)
    {
        this.origin = origin;
        this.up = up;
        this.forward = forward;
        this.right = right;
    }


    //public Orientation(Vector3 up, Vector3 forward, Vector3 right)
    //{
    //    Vector2 origin = new Vector2(0.5f 0.9f);
    //    Vector3 pos = Camera.main.ViewportToWorldPoint(origin);
    //    float length = 0.1f

    //    GL.PushMatrix();
    //    GL.Begin(GL.LINES);

    //    GL.Color(Color.red);
    //    GL.Vertex(pos);
    //    GL.Vertex(pos + Vector3.right * length);

    //    GL.Color(Color.yellow);
    //    GL.Vertex(pos);
    //    GL.Vertex(pos + Vector3.up * length);

    //    GL.Color(Color.blue);
    //    GL.Vertex(pos);
    //    GL.Vertex(pos + Vector3.forward * length);

    //    GL.End();
    //    GL.PopMatrix();
    //}

    //public DrawArrows
}
