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


public class CompOperation : IShapeGrammarOperation
{
    private Dictionary<string, string> componentNames;

    public CompOperation(Dictionary<string, string> componentNames)
    {
        this.componentNames = componentNames;
    }

    public Dictionary<string, List<Shape>> CompFaces(Shape shape)
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
                Mesh newMesh = g3UnityUtils.DMeshToUnityMesh(face);
                
                Vector3 newCenter = BuildingUtility.FindPolygonCenter(face, normal);

                // rotate up and forward vectors 90 degrees around right axis, keep right axis the same
                Vector3 newUp = Quaternion.AngleAxis(90, transform.Right) * transform.Up;
                Vector3 newForward = Quaternion.AngleAxis(90, transform.Right) * transform.Forward; 

                LocalTransform newTransform = new LocalTransform(newCenter, newUp, newForward, transform.Right);

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
                Mesh newMesh = g3UnityUtils.DMeshToUnityMesh(face);

                Vector3 newCenter = BuildingUtility.FindPolygonCenter(face, normal);

                // rotate up and right vectors 90 degrees around forward axis, keep forward axis the same
                Vector3 newUp = Quaternion.AngleAxis(90, transform.Forward) * transform.Up;
                Vector3 newRight = Quaternion.AngleAxis(90, transform.Forward) * transform.Right;

                LocalTransform newTransform = new LocalTransform(newCenter, newUp, transform.Forward, newRight);

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
                Mesh newMesh = g3UnityUtils.DMeshToUnityMesh(face);

                Vector3 newCenter = BuildingUtility.FindPolygonCenter(face, normal);

                // rotate up and right vectors -90 degrees around forward axis, keep forward axis the same
                Vector3 newUp = Quaternion.AngleAxis(-90, transform.Forward) * transform.Up;
                Vector3 newRight = Quaternion.AngleAxis(-90, transform.Forward) * transform.Right;

                LocalTransform newTransform = new LocalTransform(newCenter, newUp, transform.Forward, newRight);

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
                Mesh newMesh = g3UnityUtils.DMeshToUnityMesh(face);

                Vector3 newCenter = BuildingUtility.FindPolygonCenter(face, normal);

                // rotate up and forward vectors -90 degrees around right axis, keep right axis the same
                Vector3 newUp = Quaternion.AngleAxis(-90, transform.Right) * transform.Up;
                Vector3 newForward = Quaternion.AngleAxis(-90, transform.Right) * transform.Forward;

                LocalTransform newTransform = new LocalTransform(newCenter, newUp, newForward, transform.Right);

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


                //LocalTransform newTransform = new LocalTransform(newCenter, normal, (p1 - p0).normalized);
                LocalTransform newTransform = new LocalTransform(newCenter, transform.Up, transform.Forward, transform.Right);

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
                Mesh newMesh = g3UnityUtils.DMeshToUnityMesh(face);

                Vector3 newCenter = BuildingUtility.FindPolygonCenter(face, normal);

                // rotate up and right vectors 180 degrees around forward axis
                Vector3 newUp = Quaternion.AngleAxis(180, transform.Forward) * transform.Up;
                Vector3 newRight = Quaternion.AngleAxis(180, transform.Forward) * transform.Right;

                // flip forward axis
                Vector3 newForward = -transform.Forward;

                LocalTransform newTransform = new LocalTransform(newCenter, newUp, newForward, newRight);

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

    ShapeWrapper IShapeGrammarOperation.PerformOperation(List<Shape> input)
    {
        Dictionary<string, List<Shape>> output = new Dictionary<string, List<Shape>>();

        foreach (Shape shape in input)
        {
            Dictionary<string, List<Shape>> currentResult = CompFaces(shape);

            foreach(KeyValuePair<string, List<Shape>> component in currentResult)
            {
                if (output.ContainsKey(componentNames[component.Key]))
                {
                    output[componentNames[component.Key]].AddRange(component.Value);
                }
                else
                {
                    output.Add(componentNames[component.Key], component.Value);
                }
            }
        }

        return new ShapeWrapper(output, true);
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
