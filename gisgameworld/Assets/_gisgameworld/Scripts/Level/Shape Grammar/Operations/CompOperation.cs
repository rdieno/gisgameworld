using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using g3;

public enum GeometrySelector
{
    Face,
    Edge, 
    Vertex
}


public class CompOperation
{
    //public static Dictionary<string, List<Mesh>> Comp(Mesh mesh, GeometrySelector selection)
    //{


    //}

    public static Dictionary<string, List<Shape>> CompFaces(Shape shape)
    {
        Dictionary<string, List<Shape>> faces = new Dictionary<string, List<Shape>>();

        DMesh3 dmesh = g3UnityUtils.UnityMeshToDMesh(shape.Mesh);

        // separate faces
        DMesh3[] parts = MeshConnectedComponents.Separate(dmesh);
        List<DMesh3> partsList = new List<DMesh3>(parts);


        LocalTransform transform = shape.LocalTransform;

        //shape.Debug_DrawOrientation();

        Dictionary<string, Vector3> directionNormals = new Dictionary<string, Vector3>();
        directionNormals.Add("Front", transform.Forward);
        directionNormals.Add("Back", -transform.Forward);
        directionNormals.Add("Left", -transform.Right);
        directionNormals.Add("Right", transform.Right);
        directionNormals.Add("Top", transform.Up);
        directionNormals.Add("Bottom", -transform.Up);

        // sort faces by direction


        // find front faces
        List<Shape> frontfaces = new List<Shape>();
        for(int i = partsList.Count - 1; i >= 0 ; i--)
        {
            DMesh3 face = partsList[i];
            Vector3 normal = face.GetVertexNormal(0);

            float dot = Vector3.Dot(directionNormals["Front"], normal);

            if (dot > 0.0001f && dot <= 1.0001f)
            {
                Vector3 p0 = (Vector3) face.GetVertex(0);
                Vector3 p1 = (Vector3) face.GetVertex(1);

                Mesh newMesh = g3UnityUtils.DMeshToUnityMesh(face);
                //newMesh.RecalculateBounds();

                Vector3 newCenter = BuildingUtility.FindPolygonCenter(face, normal);



                //newMesh.

                //Debug.DrawLine(newMesh.bounds.center, newMesh.bounds.center + normal, Color.green, 1000.0f);
                //Debug.DrawLine(newMesh.bounds.center, newMesh.bounds.center + transform.Right, Color.red, 1000.0f);
                //Debug.DrawLine(newMesh.bounds.center, newMesh.bounds.center + Vector3.Cross(transform.Right, normal).normalized, Color.blue, 1000.0f);

                //GameObject a = UnityEngine.Object.Instantiate(Resources.Load("BlueCube"), newCenter, Quaternion.identity) as GameObject;
                //GameObject b = UnityEngine.Object.Instantiate(Resources.Load("PinkCube"), newMesh.bounds.center, Quaternion.identity) as GameObject;

                //GameObject a = UnityEngine.Object.Instantiate(Resources.Load("BlueCube"), p0, Quaternion.identity) as GameObject;
                //GameObject b = UnityEngine.Object.Instantiate(Resources.Load("PinkCube"), p1, Quaternion.identity) as GameObject;

                LocalTransform newTransform = new LocalTransform(newCenter, normal, (p1 - p0).normalized);
                //LocalTransform newTransform = new LocalTransform(newMesh.bounds.center, (p1 - p0).normalized, normal);
                //LocalTransform newTransform = new LocalTransform(newMesh.bounds.center, normal, Vector3.Cross(transform.Right, normal).normalized, transform.Right);

                Shape newShape = new Shape(newMesh, newTransform);
                frontfaces.Add(newShape);

                partsList.RemoveAt(i);
            }
        }

        // find left faces
        List<Shape> leftFaces = new List<Shape>();
        for (int i = partsList.Count - 1; i >= 0; i--)
        {
            DMesh3 face = partsList[i];
            Vector3 normal = face.GetVertexNormal(0);

            float dot = Vector3.Dot(directionNormals["Left"], normal);

            if (dot > 0.0001f && dot <= 1.0001f)
            {
                Vector3 p0 = (Vector3)face.GetVertex(0);
                Vector3 p1 = (Vector3)face.GetVertex(1);

                Mesh newMesh = g3UnityUtils.DMeshToUnityMesh(face);
                //newMesh.RecalculateBounds();

                Vector3 newCenter = BuildingUtility.FindPolygonCenter(face, normal);

                //LocalTransform newTransform = new LocalTransform(newMesh.bounds.center, normal, (p1 - p0).normalized);
                //LocalTransform newTransform = new LocalTransform(newMesh.bounds.center, normal, Vector3.Cross(transform.Forward, normal).normalized, transform.Forward);
                LocalTransform newTransform = new LocalTransform(newCenter, normal, (p1 - p0).normalized);

                Shape newShape = new Shape(newMesh, newTransform);
                leftFaces.Add(newShape);


                partsList.RemoveAt(i);
            }
        }

        // find right faces
        List<Shape> rightFaces = new List<Shape>();
        for (int i = partsList.Count - 1; i >= 0; i--)
        {
            DMesh3 face = partsList[i];
            Vector3 normal = face.GetVertexNormal(0);

            float dot = Vector3.Dot(directionNormals["Right"], normal);

            if (dot > 0.0001f && dot <= 1.0001f)
            {
                Vector3 p0 = (Vector3)face.GetVertex(0);
                Vector3 p1 = (Vector3)face.GetVertex(1);

                Mesh newMesh = g3UnityUtils.DMeshToUnityMesh(face);
                //newMesh.RecalculateBounds();

                Vector3 newCenter = BuildingUtility.FindPolygonCenter(face, normal);

                //LocalTransform newTransform = new LocalTransform(newMesh.bounds.center, normal, (p1 - p0).normalized);
                //LocalTransform newTransform = new LocalTransform(newMesh.bounds.center, normal, Vector3.Cross(transform.Forward, normal).normalized, transform.Forward);
                LocalTransform newTransform = new LocalTransform(newCenter, normal, (p1 - p0).normalized);

                Shape newShape = new Shape(newMesh, newTransform);
                rightFaces.Add(newShape);


                partsList.RemoveAt(i);
            }

        }

        // find back faces
        List<Shape> backFaces = new List<Shape>();
        for (int i = partsList.Count - 1; i >= 0; i--)
        {
            DMesh3 face = partsList[i];
            Vector3 normal = face.GetVertexNormal(0);

            float dot = Vector3.Dot(directionNormals["Back"], normal);

            if (dot > 0.0001f && dot <= 1.0001f)
            {
                Vector3 p0 = (Vector3)face.GetVertex(0);
                Vector3 p1 = (Vector3)face.GetVertex(1);

                Mesh newMesh = g3UnityUtils.DMeshToUnityMesh(face);
                //newMesh.RecalculateBounds();

                Vector3 newCenter = BuildingUtility.FindPolygonCenter(face, normal);

                //LocalTransform newTransform = new LocalTransform(newMesh.bounds.center, normal, (p1 - p0).normalized);
                //LocalTransform newTransform = new LocalTransform(newMesh.bounds.center, normal, Vector3.Cross(transform.Forward, normal).normalized, transform.Forward);
                LocalTransform newTransform = new LocalTransform(newCenter, normal, (p1 - p0).normalized);

                Shape newShape = new Shape(newMesh, newTransform);
                backFaces.Add(newShape);


                partsList.RemoveAt(i);
            }

        }

        // find top faces
        List<Shape> topFaces = new List<Shape>();
        for (int i = partsList.Count - 1; i >= 0; i--)
        {
            DMesh3 face = partsList[i];
            Vector3 normal = face.GetVertexNormal(0);

            float dot = Vector3.Dot(directionNormals["Top"], normal);

            if(dot > 0.0001f && dot <= 1.0001f)
            {
                Vector3 p0 = (Vector3)face.GetVertex(0);
                Vector3 p1 = (Vector3)face.GetVertex(1);

                Mesh newMesh = g3UnityUtils.DMeshToUnityMesh(face);
                //newMesh.RecalculateBounds();

                Vector3 newCenter = BuildingUtility.FindPolygonCenter(face, normal);

                //LocalTransform newTransform = new LocalTransform(newMesh.bounds.center, normal, (p1 - p0).normalized);
                //LocalTransform newTransform = new LocalTransform(newMesh.bounds.center, normal, Vector3.Cross(transform.Forward, normal).normalized, transform.Forward);
                LocalTransform newTransform = new LocalTransform(newCenter, normal, (p1 - p0).normalized);

                Shape newShape = new Shape(newMesh, newTransform);
                topFaces.Add(newShape);


                partsList.RemoveAt(i);
            }
        }

        // find bottom faces
        List<Shape> bottomFaces = new List<Shape>();
        for (int i = partsList.Count - 1; i >= 0; i--)
        {
            DMesh3 face = partsList[i];
            Vector3 normal = face.GetVertexNormal(0);

            float dot = Vector3.Dot(directionNormals["Bottom"], normal);

            if(dot > 0.0001f && dot <= 1.0001f)
            {
                Vector3 p0 = (Vector3)face.GetVertex(0);
                Vector3 p1 = (Vector3)face.GetVertex(1);

                Mesh newMesh = g3UnityUtils.DMeshToUnityMesh(face);
                //newMesh.RecalculateBounds();

                Vector3 newCenter = BuildingUtility.FindPolygonCenter(face, normal);

                //LocalTransform newTransform = new LocalTransform(newMesh.bounds.center, normal, (p1 - p0).normalized);
                //LocalTransform newTransform = new LocalTransform(newMesh.bounds.center, normal, Vector3.Cross(transform.Forward, normal).normalized, transform.Forward);
                LocalTransform newTransform = new LocalTransform(newCenter, normal, (p1 - p0).normalized);

                Shape newShape = new Shape(newMesh, newTransform);
                bottomFaces.Add(newShape);

                partsList.RemoveAt(i);
            }
        }



        faces.Add("Front", frontfaces);
        faces.Add("Back", backFaces);
        faces.Add("Left", leftFaces);
        faces.Add("Right", rightFaces);
        faces.Add("Top", topFaces);
        faces.Add("Bottom", bottomFaces);

        return faces;
    }

    //public static Dictionary<string, List<Mesh>> CompFaces(Shape shape)
    //{
    //    Dictionary<string, List<Mesh>> faces = new Dictionary<string, List<Mesh>>();

    //    DMesh3 dmesh = g3UnityUtils.UnityMeshToDMesh(shape.Mesh);


    //    // separate faces
    //    DMesh3[] parts = MeshConnectedComponents.Separate(dmesh);


    //    LocalTransform transform = shape.LocalTransform;

    //    Dictionary<string, Vector3> directionNormals = new Dictionary<string, Vector3>();
    //    directionNormals.Add("Front", transform.Forward);
    //    directionNormals.Add("Back", -transform.Forward);
    //    directionNormals.Add("Left", -transform.Right);
    //    directionNormals.Add("Right", transform.Right);
    //    directionNormals.Add("Top", transform.Up);
    //    directionNormals.Add("Bottom", -transform.Up);

    //    // sort faces by direction


    //    // find front faces
    //    List<Mesh> frontfaces = new List<Mesh>();
    //    for(int i = 0; i < parts.Length; i++)
    //    {
    //        DMesh3 face = parts[i];
    //        Vector3 normal = face.GetVertexNormal(0);

    //        if(normal == directionNormals["Front"])
    //        {
    //            frontfaces.Add(g3UnityUtils.DMeshToUnityMesh(face));
    //        }

    //    }
        
    //    // find back faces
    //    List<Mesh> backFaces = new List<Mesh>();
    //    for(int i = 0; i < parts.Length; i++)
    //    {
    //        DMesh3 face = parts[i];
    //        Vector3 normal = face.GetVertexNormal(0);

    //        if(normal == directionNormals["Back"])
    //        {
    //            backFaces.Add(g3UnityUtils.DMeshToUnityMesh(face));
    //        }

    //    }

    //    // find top faces
    //    List<Mesh> topFaces = new List<Mesh>();
    //    for (int i = 0; i < parts.Length; i++)
    //    {
    //        DMesh3 face = parts[i];
    //        Vector3 normal = face.GetVertexNormal(0);

    //        float dot = Vector3.Dot(directionNormals["Top"], normal);

    //        if(dot > 0f && dot <= 1f)
    //        {
    //            topFaces.Add(g3UnityUtils.DMeshToUnityMesh(face));
    //        }

    //    }

    //    // find bottom faces
    //    List<Mesh> bottomFaces = new List<Mesh>();
    //    for (int i = 0; i < parts.Length; i++)
    //    {
    //        DMesh3 face = parts[i];
    //        Vector3 normal = face.GetVertexNormal(0);

    //        float dot = Vector3.Dot(directionNormals["Bottom"], normal);

    //        if(dot > 0f && dot <= 1f)
    //        {
    //            bottomFaces.Add(g3UnityUtils.DMeshToUnityMesh(face));
    //        }

    //    }

    //    // find left faces
    //    List<Mesh> leftFaces = new List<Mesh>();
    //    for (int i = 0; i < parts.Length; i++)
    //    {
    //        DMesh3 face = parts[i];
    //        Vector3 normal = face.GetVertexNormal(0);

    //        float dot = Vector3.Dot(directionNormals["Left"], normal);

    //        if (dot > 0f && dot <= 1f)
    //        {
    //            leftFaces.Add(g3UnityUtils.DMeshToUnityMesh(face));
    //        }
    //    }

    //    // find right faces
    //    List<Mesh> rightFaces = new List<Mesh>();
    //    for (int i = 0; i < parts.Length; i++)
    //    {
    //        DMesh3 face = parts[i];
    //        Vector3 normal = face.GetVertexNormal(0);

    //        float dot = Vector3.Dot(directionNormals["Right"], normal);

    //        if (dot > 0f && dot <= 1f)
    //        {
    //            rightFaces.Add(g3UnityUtils.DMeshToUnityMesh(face));
    //        }

    //    }


    //    faces.Add("Front", frontfaces);
    //    faces.Add("Back", backFaces);
    //    faces.Add("Left", leftFaces);
    //    faces.Add("Right", rightFaces);
    //    faces.Add("Top", topFaces);
    //    faces.Add("Bottom", bottomFaces);

    //    return faces;


    //    //front | back | left | right | top | bottom

    //    // front faces

    //}


    //SortFaces(Dictionary<string, Vector3> DirectionNormals)
}
