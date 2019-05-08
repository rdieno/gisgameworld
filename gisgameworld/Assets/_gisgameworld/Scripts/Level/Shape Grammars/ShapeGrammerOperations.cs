using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShapeGrammerOperations
{
    #region Extrusion

    // extrudes a mesh along the Y axis by the amount specified
    public static Mesh ExtrudeMeshY(Mesh mesh, Transform transform, float amount)
    {
        Edge2D[] edges = ExtrudeOperation.FindOuterEdges(mesh);

        Matrix4x4[] endPointTransforms = new Matrix4x4[2];
        Vector3 offset = new Vector3(0.0f, amount, 0.0f);
        endPointTransforms[0] = Matrix4x4.identity;
        endPointTransforms[1] = transform.localToWorldMatrix * Matrix4x4.Translate(offset);

        return ExtrudeOperation.Extrude(mesh, endPointTransforms, edges, true);
    }

    #endregion

    #region Split

    public static List<Mesh> SplitY(Mesh mesh, Transform transform, float ratio)
    {
        Vector3 pos = transform.position;
        Vector3 size = mesh.bounds.size;
        Plane cuttingPlane = new Plane(transform.up, new Vector3(pos.x, pos.y + (size.y * ratio), pos.z));
            
            
       //Plane cuttingPlane = new Plane(transform.right, new Vector3(pos.x, pos.y, pos.z));

        //SplitOperation.Split(mesh, transform, cuttingPlane);
        //return null;

        return SplitOperation.Split(mesh, transform, cuttingPlane);
    }

    public static List<Mesh> SplitX(Mesh mesh, Transform transform, float ratio)
    {
        Vector3 pos = transform.position;
        Vector3 size = mesh.bounds.size;
        //Plane cuttingPlane = new Plane(transform.up, new Vector3(pos.x, pos.y + (size.y * ratio), pos.z));

        float minX = pos.x - (size.x / 2.0f);
        float distX = size.x * ratio;

        //Vector3 pivot = new Vector3(pos.x - (size.x / 2.0f), pos.y, pos.z);

        Plane cuttingPlane = new Plane(transform.right, new Vector3(minX + distX, pos.y, pos.z));

        //SplitOperation.Split(mesh, transform, cuttingPlane);
        //return null;

        return SplitOperation.Split(mesh, transform, cuttingPlane);
    }

    //public static Mesh SliceIt(Mesh mesh, Transform transform, Plane plane)
    //{
    //    Mesh slicedMesh = new Mesh();

    //    Vector3[] vertices = mesh.vertices;
    //    Vector3[] verticesSlice = mesh.vertices;
    //   // List<Vector3> verticesSlice2 = new List<Vector3>();


    //    //Vector3[] cutplanevertices = plane.

    //    //p1 = cutplane.TransformPoint(cutplanevertices[40]);
    //    //p2 = cutplane.TransformPoint(cutplanevertices[20]);
    //    //p3 = cutplane.TransformPoint(cutplanevertices[0]);
    //    //var myplane = new Plane(p1, p2, p3);

    //    for (int i = 0; i < vertices.Length; i++)
    //    {
    //        Vector3 vertex = transform.TransformPoint(vertices[i]); // original object vertices

    //        if (plane.GetSide(vertex))
    //        {
    //            //vertices[i] = transform.InverseTransformPoint(new Vector3(vertex.x, vertex.y - (plane.GetDistanceToPoint(vertex)), vertex.z));

    //            verticesSlice[i] = transform.InverseTransformPoint(new Vector3(vertex.x, vertex.y, vertex.z));
    //           // var v = transform.InverseTransformPoint(new Vector3(vertex.x, vertex.y, vertex.z));
    //            //verticesSlice2.Add(v);
    //        }
    //        else
    //        {
    //            verticesSlice[i] = transform.InverseTransformPoint(new Vector3(vertex.x, vertex.y - (plane.GetDistanceToPoint(vertex)), vertex.z));
    //            //verticesSlice2.Add(v);
    //        }
    //    }

    //    //mesh.vertices = verticesSlice;
    //    //mesh.RecalculateBounds();

    //    slicedMesh.vertices = verticesSlice;
    //    slicedMesh.RecalculateBounds();

    //    return slicedMesh;
    //}

    #endregion
}