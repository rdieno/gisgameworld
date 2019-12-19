using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SimplePolygon
{
    private List<Vector3> edgeLoop;
    public List<Vector3> EdgeLoop { get { return edgeLoop; } }

    private Vector3 normal;
    public Vector3 Normal { get { return normal; } }

    private List<List<Vector3>> holes;
    public List<List<Vector3>> Holes { get { return holes; } }

    public bool toRemove;

    private float yPos;

    private bool flattened;



    public SimplePolygon(List<Vector3> edgeLoop, Vector3 normal, bool flatten = true)
    {
        this.edgeLoop = edgeLoop;
        this.normal = normal;
        holes = new List<List<Vector3>>();
        //this.yPos = yPos;
        toRemove = false;
        flattened = false;

        if(flatten)
            Flatten();
    }

    public void AddHole(List<Vector3> hole)
    {
        holes.Add(hole);
    }

    public void Flatten()
    {
        if (normal != Vector3.up)
        {
            Quaternion rotation = Quaternion.FromToRotation(normal, Vector3.up);

            for (int i = 0; i < edgeLoop.Count; i++)
            {
                edgeLoop[i] = rotation * edgeLoop[i];
            }

            yPos = edgeLoop[0].y;

            flattened = true;
        }
        else
        {
            yPos = edgeLoop[0].y;
        }
    }

    public void Unflatten()
    {
        if (flattened)
        {
            Quaternion rotation = Quaternion.FromToRotation(Vector3.up, normal);

            for (int k = 0; k < edgeLoop.Count; k++)
            {
                edgeLoop[k] = rotation * edgeLoop[k];
            }

            flattened = false;
        }
    }

    public Mesh ToMesh(bool unflatten = true)
    {
        List<int> indices = null;
        List<Vector3> vertices = null;

        if(edgeLoop.Count < 3)
        {
            edgeLoop.Add(edgeLoop[0]);
        }

        bool success = Triangulator.Triangulate(edgeLoop, holes, normal, out indices, out vertices, yPos);

        //bool success = false;

        //try
        //{
        //    success = Triangulator.Triangulate(edgeLoop, holes, normal, out indices, out vertices, yPos);
        //}
        //catch (System.Exception e)
        //{
        //    for (int i = 0; i < edgeLoop.Count - 1; i++)
        //    {
        //        Vector3 p0 = edgeLoop[i];
        //        Vector3 p1 = edgeLoop[MathUtility.ClampListIndex(i + 1, edgeLoop.Count)];

        //        //GameObject a = UnityEngine.Object.Instantiate(Resources.Load("BlueCube"), edgeLoop[i], Quaternion.identity) as GameObject;
        //        Debug.DrawLine(p0, p1, Color.yellow, 1000f);
        //    }

        //    for (int j = 0; j < holes.Count; j++)
        //    {
        //        List<Vector3> hole = holes[j];
        //        for (int i = 0; i < hole.Count - 1; i++)
        //        {
        //            Vector3 p0 = hole[i];
        //            Vector3 p1 = hole[MathUtility.ClampListIndex(i + 1, hole.Count)];

        //            //GameObject a = UnityEngine.Object.Instantiate(Resources.Load("BlueCube"), edgeLoop[i], Quaternion.identity) as GameObject;
        //            Debug.DrawLine(p0, p1, Color.red, 1000f);
        //        }
        //    }



        //    Debug.Log("SimplePolygon ToMesh(): Error: (" + "): " + e.Message);
        //}

        if (success)
        {
            Mesh result = new Mesh();

            if(unflatten)
            {
                Quaternion rotation = Quaternion.FromToRotation(Vector3.up, normal);

                for (int i = 0; i < vertices.Count; i++)
                {
                    vertices[i] = rotation * new Vector3(vertices[i].x, yPos, vertices[i].z);
                    //vertices[i] = rotation * vertices[i];
                }
            }

            result.vertices = vertices.ToArray();

            List<Vector3> normals = new List<Vector3>(vertices.Count);
            for(int i = 0; i < vertices.Count; i++)
            {
                normals.Add(normal);
            }

            result.normals = normals.ToArray();

            result.triangles = indices.ToArray();

            return result;
        }
        else
        {
            return null;
        }
    }

    public void DebugDraw(Color edgeColor, Color holeColor, bool drawPoints = false)
    {
        for (int j = 0; j < edgeLoop.Count; j++)
        {
            Vector3 p0 = edgeLoop[MathUtility.ClampListIndex(j, edgeLoop.Count)];
            Vector3 p1 = edgeLoop[MathUtility.ClampListIndex(j + 1, edgeLoop.Count)];

            if (holes.Count != 0)
            {
                edgeColor = Color.magenta;
            }

            if(drawPoints)
            {
                GameObject a = UnityEngine.Object.Instantiate(Resources.Load("BlueCube"), p0, Quaternion.identity) as GameObject;
            }

            Debug.DrawLine(p0, p1, edgeColor, 1000.0f);
        }

        for (int i = 0; i < holes.Count; i++)
        {
            List<Vector3> hole = holes[i];

            for (int j = 0; j < hole.Count; j++)
            {
                Vector3 p0 = hole[MathUtility.ClampListIndex(j, hole.Count)];
                Vector3 p1 = hole[MathUtility.ClampListIndex(j + 1, hole.Count)];

                Debug.DrawLine(p0, p1, holeColor, 1000.0f);
            }
        }
    }
}
