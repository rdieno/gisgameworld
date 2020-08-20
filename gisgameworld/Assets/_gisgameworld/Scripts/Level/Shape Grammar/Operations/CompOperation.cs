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
    private const float FACE_IGNORE_THRESHOLD = 6f;

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
        
        Dictionary<string, Vector3> directionNormals = new Dictionary<string, Vector3>();
        directionNormals.Add("Front", transform.Forward);
        directionNormals.Add("Back", -transform.Forward);
        directionNormals.Add("Left", -transform.Right);
        directionNormals.Add("Right", transform.Right);
        directionNormals.Add("Top", transform.Up);
        directionNormals.Add("Bottom", -transform.Up);

        // sort faces by direction
        Plane upPlane = new Plane(transform.Up, transform.Origin);

        // find front faces
        List<Shape> frontfaces = new List<Shape>();
        for(int i = partsList.Count - 1; i >= 0 ; i--)
        {
            DMesh3 face = partsList[i];
            Vector3 normal = MathUtility.ConvertToVector3(face.GetVertexNormal(0));

            float dot = Vector3.Dot(directionNormals["Front"], normal);

            if (dot > 0.0001f && dot <= 1.0001f)
            {
                Mesh newMesh = g3UnityUtils.DMeshToUnityMesh(face);
                
                Vector3 newCenter = BuildingUtility.FindPolygonCenter(face, normal);

                Vector3 faceCenter = MathUtility.ConvertToVector3(face.GetBounds().Center);
                upPlane = new Plane(transform.Up, faceCenter);

                Vector3 p0 = MathUtility.ConvertToVector3(face.GetVertex(0));
                Vector3 p1 = MathUtility.ConvertToVector3(face.GetVertex(1));
                Vector3 p2 = MathUtility.ConvertToVector3(face.GetVertex(2));

                List<Vector3> topVerts = new List<Vector3>();
                List<Vector3> bottomVerts = new List<Vector3>();

                List<Vector3> topRightVerts = new List<Vector3>();
                List<Vector3> bottomRightVerts = new List<Vector3>();

                List<Vector3> topLeftVerts = new List<Vector3>();
                List<Vector3> bottomLeftVerts = new List<Vector3>();

                List<Vector3> refVerts = new List<Vector3>();
                for (int j = 0; j < face.VertexCount; j++)
                {
                    Vector3 refVert = MathUtility.ConvertToVector3(face.GetVertex(j));
                    refVerts.Add(refVert);
                }

                foreach (Vector3 v in refVerts)
                {
                    if(upPlane.GetDistanceToPoint(v) > 0f)
                    {
                        topVerts.Add(v);
                    }
                    else
                    {
                        bottomVerts.Add(v);
                    }
                }

                Vector3? parallelToFace = null;

                if(topVerts.Count > 1)
                {
                    Vector3 vertA = Vector3.zero;
                    Vector3 vertB = Vector3.zero;

                    for(int j = 0; j < topVerts.Count - 1; j++)
                    {
                        vertA = topVerts[j];
                        vertB = topVerts[j + 1];

                        if(Vector3.SqrMagnitude(vertA - vertB) > 0.01f)
                        {
                            break;
                        }
                    }

                    parallelToFace = (vertB - vertA).normalized;
                }
                else if(bottomVerts.Count > 1)
                {
                    Vector3 vertA = Vector3.zero;
                    Vector3 vertB = Vector3.zero;

                    for (int j = 0; j < bottomVerts.Count - 1; j++)
                    {
                        vertA = bottomVerts[j];
                        vertB = bottomVerts[j + 1];

                        if (Vector3.SqrMagnitude(vertA - vertB) > 0.01f)
                        {
                            break;
                        }
                    }

                    parallelToFace = (vertB - vertA).normalized;
                }

                float parallelDirection = Vector3.Dot(parallelToFace.Value, transform.Right);

                List<Vector3> leftVerts = new List<Vector3>();
                List<Vector3> rightVerts = new List<Vector3>();

                bool isPointingRight = true;

                if(parallelDirection < 0)
                {
                    isPointingRight = false;
                }
                
                Plane rightPlane = new Plane(parallelToFace.Value, newCenter);

                foreach(Vector3 v in topVerts)
                {
                    if(rightPlane.GetDistanceToPoint(v) >= 0)
                    {
                        if (isPointingRight)
                        {
                            topRightVerts.Add(v);
                        }
                        else
                        {
                            topLeftVerts.Add(v);
                        }
                    }
                    else
                    {
                        if (isPointingRight)
                        {
                            topLeftVerts.Add(v);
                        }
                        else
                        {
                            topRightVerts.Add(v);
                        }
                    }
                }

                foreach(Vector3 v in bottomVerts)
                {
                    if(rightPlane.GetDistanceToPoint(v) > 0)
                    {
                        if (isPointingRight)
                        {
                            bottomRightVerts.Add(v);
                        }
                        else
                        {
                            bottomLeftVerts.Add(v);
                        }
                    }
                    else
                    {
                        if (isPointingRight)
                        {
                            bottomLeftVerts.Add(v);
                        }
                        else
                        {
                            bottomRightVerts.Add(v);
                        }
                    }
                }

                Vector3 newUp = normal;

                Vector3? newForward = null;
                Vector3? newRight = null;

                if (topLeftVerts.Count > 0 && topRightVerts.Count > 0)
                {
                    newRight = (topRightVerts[0] - topLeftVerts[0]).normalized;
                }
                else if (bottomLeftVerts.Count > 0 && bottomRightVerts.Count > 0)
                {
                    newRight = (bottomRightVerts[0] - bottomLeftVerts[0]).normalized;
                }
                else if (topLeftVerts.Count > 1)
                {
                    newRight = (topLeftVerts[0] - topLeftVerts[1]).normalized;
                }
                else if (topRightVerts.Count > 1)
                {
                    newRight = (topRightVerts[0] - topRightVerts[1]).normalized;
                }

                if (topLeftVerts.Count > 0 && bottomLeftVerts.Count > 0)
                {
                    newForward = (bottomLeftVerts[0] - topLeftVerts[0]).normalized;
                }
                else if (topRightVerts.Count > 0 && bottomRightVerts.Count > 0)
                {
                    newForward = (bottomRightVerts[0] - topRightVerts[0]).normalized;
                }
                else if (bottomLeftVerts.Count > 1)
                {
                    newForward = (bottomLeftVerts[0] - bottomLeftVerts[1]).normalized;
                }
                else if (bottomRightVerts.Count > 1)
                {
                    newForward = (bottomRightVerts[0] - bottomRightVerts[1]).normalized;
                }

                if (!newRight.HasValue || !newForward.HasValue)
                {
                    if(!newRight.HasValue)
                    {
                        if (topVerts.Count > 1)
                        {
                            newRight = (topVerts[0] - topVerts[1]).normalized;
                        }
                        else if(bottomVerts.Count > 1)
                        {
                            newRight = (bottomVerts[0] - bottomVerts[1]).normalized;
                        }
                    }

                    if (!newForward.HasValue)
                    {
                        if (topLeftVerts.Count > 0 && bottomRightVerts.Count > 0)
                        {
                            newForward = (topLeftVerts[0] - bottomRightVerts[0]).normalized;
                        }
                        else if(bottomLeftVerts.Count > 0 && topRightVerts.Count > 0)
                        {
                            newForward = (bottomLeftVerts[0] - topRightVerts[0]).normalized;
                        }
                    }

                    if(!newRight.HasValue || !newForward.HasValue)
                    {
                        Debug.Log("CompOperation: could not determine front face orientation vectors");
                    }
                }

                LocalTransform newTransform = new LocalTransform(newCenter, newUp, newForward.Value, newRight.Value);

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
            Vector3 normal = MathUtility.ConvertToVector3(face.GetVertexNormal(0));

            float dot = Vector3.Dot(directionNormals["Left"], normal);

            if (dot > 0.0001f && dot <= 1.0001f)
            {
                Mesh newMesh = g3UnityUtils.DMeshToUnityMesh(face);

                Vector3 newCenter = BuildingUtility.FindPolygonCenter(face, normal);

                Vector3 faceCenter = MathUtility.ConvertToVector3(face.GetBounds().Center);
                upPlane = new Plane(transform.Up, faceCenter);

                Vector3 p0 = MathUtility.ConvertToVector3(face.GetVertex(0));
                Vector3 p1 = MathUtility.ConvertToVector3(face.GetVertex(1));
                Vector3 p2 = MathUtility.ConvertToVector3(face.GetVertex(2));
                
                Vector3 newUp = normal;
                Vector3 oldRight = (p0 - p1).normalized;
                Vector3 oldForward = (p2 - p1).normalized;
                
                List<Vector3> topVerts = new List<Vector3>();
                List<Vector3> bottomVerts = new List<Vector3>();

                List<Vector3> topRightVerts = new List<Vector3>();
                List<Vector3> bottomRightVerts = new List<Vector3>();

                List<Vector3> topLeftVerts = new List<Vector3>();
                List<Vector3> bottomLeftVerts = new List<Vector3>();

                List<Vector3> refVerts = new List<Vector3>();
                for (int j = 0; j < face.VertexCount; j++)
                {
                    Vector3 refVert = MathUtility.ConvertToVector3(face.GetVertex(j));
                    refVerts.Add(refVert);
                }

                foreach (Vector3 v in refVerts)
                {
                    if (upPlane.GetDistanceToPoint(v) > 0f)
                    {
                        topVerts.Add(v);
                    }
                    else
                    {
                        bottomVerts.Add(v);
                    }
                }

                Vector3? parallelToFace = null;

                if (topVerts.Count > 1)
                {
                    Vector3 vertA = Vector3.zero;
                    Vector3 vertB = Vector3.zero;

                    for (int j = 0; j < topVerts.Count - 1; j++)
                    {
                        vertA = topVerts[j];
                        vertB = topVerts[j + 1];

                        if (Vector3.SqrMagnitude(vertA - vertB) > 0.01f)
                        {
                            break;
                        }
                    }

                    parallelToFace = (vertB - vertA).normalized;
                }
                else if (bottomVerts.Count > 1)
                {
                    Vector3 vertA = Vector3.zero;
                    Vector3 vertB = Vector3.zero;

                    for (int j = 0; j < bottomVerts.Count - 1; j++)
                    {
                        vertA = bottomVerts[j];
                        vertB = bottomVerts[j + 1];

                        if (Vector3.SqrMagnitude(vertA - vertB) > 0.01f)
                        {
                            break;
                        }
                    }

                    parallelToFace = (vertB - vertA).normalized;
                }

                float parallelDirection = Vector3.Dot(parallelToFace.Value, transform.Forward);

                List<Vector3> leftVerts = new List<Vector3>();
                List<Vector3> rightVerts = new List<Vector3>();

                bool isPointingRight = false;

                if (parallelDirection < 0)
                {
                    isPointingRight = true;
                }

                Plane rightPlane = new Plane(parallelToFace.Value, newCenter);

                foreach (Vector3 v in topVerts)
                {
                    if (rightPlane.GetDistanceToPoint(v) > 0)
                    {
                        if (isPointingRight)
                        {
                            topRightVerts.Add(v);
                        }
                        else
                        {
                            topLeftVerts.Add(v);
                        }
                    }
                    else
                    {
                        if (isPointingRight)
                        {
                            topLeftVerts.Add(v);
                        }
                        else
                        {
                            topRightVerts.Add(v);
                        }
                    }
                }

                foreach (Vector3 v in bottomVerts)
                {
                    if (rightPlane.GetDistanceToPoint(v) > 0)
                    {
                        if (isPointingRight)
                        {
                            bottomRightVerts.Add(v);
                        }
                        else
                        {
                            bottomLeftVerts.Add(v);
                        }
                    }
                    else
                    {
                        if (isPointingRight)
                        {
                            bottomLeftVerts.Add(v);
                        }
                        else
                        {
                            bottomRightVerts.Add(v);
                        }
                    }
                }

                Vector3? newForward = null;
                Vector3? newRight = null;

                if (topLeftVerts.Count > 0 && topRightVerts.Count > 0)
                {
                    newForward = (topLeftVerts[0] - topRightVerts[0]).normalized;
                }
                else if (bottomLeftVerts.Count > 0 && bottomRightVerts.Count > 0)
                {
                    newForward = (bottomLeftVerts[0] - bottomRightVerts[0]).normalized;
                }
                else if (bottomLeftVerts.Count > 1)
                {
                    newForward = (bottomLeftVerts[1] - bottomLeftVerts[0]).normalized;
                }
                else if (bottomRightVerts.Count > 1)
                {
                    newForward = (bottomRightVerts[1] - bottomRightVerts[0]).normalized;
                }

                if (topLeftVerts.Count > 0 && bottomLeftVerts.Count > 0)
                {
                    newRight = (topLeftVerts[0] - bottomLeftVerts[0]).normalized;
                }
                else if (topRightVerts.Count > 0 && bottomRightVerts.Count > 0)
                {
                    newRight = (topRightVerts[0] - bottomRightVerts[0]).normalized;
                }
                else if (topLeftVerts.Count > 1)
                {
                    newRight = (topLeftVerts[1] - topLeftVerts[0]).normalized;
                }
                else if (topRightVerts.Count > 1)
                {
                    newRight = (topRightVerts[1] - topRightVerts[0]).normalized;
                }

                if (!newRight.HasValue || !newForward.HasValue)
                {
                    if (!newRight.HasValue)
                    {
                        if (topVerts.Count > 1)
                        {
                            newRight = (topVerts[0] - topVerts[1]).normalized;
                        }
                        else if (bottomVerts.Count > 1)
                        {
                            newRight = (bottomVerts[0] - bottomVerts[1]).normalized;
                        }
                    }

                    if (!newForward.HasValue)
                    {
                        if (topLeftVerts.Count > 0 && bottomRightVerts.Count > 0)
                        {
                            newForward = (topLeftVerts[0] - bottomRightVerts[0]).normalized;
                        }
                        else if (bottomLeftVerts.Count > 0 && topRightVerts.Count > 0)
                        {
                            newForward = (bottomLeftVerts[0] - topRightVerts[0]).normalized;
                        }
                    }

                    if (!newRight.HasValue || !newForward.HasValue)
                    {
                        Debug.Log("CompOperation: could not determine left face orientation vectors");
                    }
                }

                LocalTransform newTransform = new LocalTransform(newCenter, newUp, newForward.Value, newRight.Value);
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
            Vector3 normal = MathUtility.ConvertToVector3((face.GetVertexNormal(0)));

            float dot = Vector3.Dot(directionNormals["Right"], normal);

            if (dot > 0.0001f && dot <= 1.0001f)
            {
                Mesh newMesh = g3UnityUtils.DMeshToUnityMesh(face);

                Vector3 newCenter = BuildingUtility.FindPolygonCenter(face, normal);

                Vector3 faceCenter = MathUtility.ConvertToVector3(face.GetBounds().Center);
                upPlane = new Plane(transform.Up, faceCenter);


                Vector3 p0 = MathUtility.ConvertToVector3(face.GetVertex(0));
                Vector3 p1 = MathUtility.ConvertToVector3(face.GetVertex(1));
                Vector3 p2 = MathUtility.ConvertToVector3(face.GetVertex(2));

                Vector3 newUp = normal;
                Vector3 oldRight = (p1 - p0).normalized;
                Vector3 oldForward = (p1 - p2).normalized;

                List<Vector3> topVerts = new List<Vector3>();
                List<Vector3> bottomVerts = new List<Vector3>();

                List<Vector3> topRightVerts = new List<Vector3>();
                List<Vector3> bottomRightVerts = new List<Vector3>();

                List<Vector3> topLeftVerts = new List<Vector3>();
                List<Vector3> bottomLeftVerts = new List<Vector3>();

                List<Vector3> refVerts = new List<Vector3>();

                for(int j = 0; j < face.VertexCount; j++)
                {
                    Vector3 refVert = MathUtility.ConvertToVector3(face.GetVertex(j));
                    refVerts.Add(refVert);
                }

                foreach (Vector3 v in refVerts)
                {
                    if (upPlane.GetDistanceToPoint(v) > 0f)
                    {
                        topVerts.Add(v);
                    }
                    else
                    {
                        bottomVerts.Add(v);
                    }
                }

                Vector3? parallelToFace = null;

                if (topVerts.Count > 1)
                {
                    Vector3 vertA = Vector3.zero;
                    Vector3 vertB = Vector3.zero;

                    for (int j = 0; j < topVerts.Count - 1; j++)
                    {
                        vertA = topVerts[j];
                        vertB = topVerts[j + 1];

                        if (Vector3.SqrMagnitude(vertA - vertB) > 0.01f)
                        {
                            break;
                        }
                    }

                    parallelToFace = (vertB - vertA).normalized;
                }
                else if (bottomVerts.Count > 1)
                {
                    Vector3 vertA = Vector3.zero;
                    Vector3 vertB = Vector3.zero;

                    for (int j = 0; j < bottomVerts.Count - 1; j++)
                    {
                        vertA = bottomVerts[j];
                        vertB = bottomVerts[j + 1];

                        if (Vector3.SqrMagnitude(vertA - vertB) > 0.01f)
                        {
                            break;
                        }
                    }

                    parallelToFace = (vertB - vertA).normalized;
                }

                float parallelDirection = Vector3.Dot(parallelToFace.Value, transform.Forward);

                List<Vector3> leftVerts = new List<Vector3>();
                List<Vector3> rightVerts = new List<Vector3>();

                bool isPointingRight = false;

                if (parallelDirection >= 0)
                {
                    isPointingRight = true;
                }

                Plane rightPlane = new Plane(parallelToFace.Value, newCenter);

                foreach (Vector3 v in topVerts)
                {
                    if (rightPlane.GetDistanceToPoint(v) > 0)
                    {
                        if (isPointingRight)
                        {
                            topRightVerts.Add(v);
                        }
                        else
                        {
                            topLeftVerts.Add(v);
                        }
                    }
                    else
                    {
                        if (isPointingRight)
                        {
                            topLeftVerts.Add(v);
                        }
                        else
                        {
                            topRightVerts.Add(v);
                        }
                    }
                }

                foreach (Vector3 v in bottomVerts)
                {
                    if (rightPlane.GetDistanceToPoint(v) > 0)
                    {
                        if (isPointingRight)
                        {
                            bottomRightVerts.Add(v);
                        }
                        else
                        {
                            bottomLeftVerts.Add(v);
                        }
                    }
                    else
                    {
                        if (isPointingRight)
                        {
                            bottomLeftVerts.Add(v);
                        }
                        else
                        {
                            bottomRightVerts.Add(v);
                        }
                    }
                }

                Vector3? newForward = null;
                Vector3? newRight = null;

                if (topLeftVerts.Count > 0 && topRightVerts.Count > 0)
                {
                    newForward = (topRightVerts[0] - topLeftVerts[0]).normalized;
                }
                else if (bottomLeftVerts.Count > 0 && bottomRightVerts.Count > 0)
                {
                    newForward = (bottomRightVerts[0] - bottomLeftVerts[0]).normalized;
                }
                else if (bottomLeftVerts.Count > 1)
                {
                    newForward = (bottomLeftVerts[0] - bottomLeftVerts[1]).normalized;
                }
                else if (bottomRightVerts.Count > 1)
                {
                    newForward = (bottomRightVerts[0] - bottomRightVerts[1]).normalized;
                }

                if (topLeftVerts.Count > 0 && bottomLeftVerts.Count > 0)
                {
                    newRight = (bottomLeftVerts[0] - topLeftVerts[0]).normalized;
                }
                else if (topRightVerts.Count > 0 && bottomRightVerts.Count > 0)
                {
                    newRight = (bottomRightVerts[0] - topRightVerts[0]).normalized;
                }
                else if (topLeftVerts.Count > 1)
                {
                    newRight = (topLeftVerts[0] - topLeftVerts[1]).normalized;
                }
                else if (topRightVerts.Count > 1)
                {
                    newRight = (topRightVerts[0] - topRightVerts[1]).normalized;
                }

                if (!newRight.HasValue || !newForward.HasValue)
                {
                    if (!newRight.HasValue)
                    {
                        if (topVerts.Count > 1)
                        {
                            newRight = (topVerts[0] - topVerts[1]).normalized;
                        }
                        else if (bottomVerts.Count > 1)
                        {
                            newRight = (bottomVerts[0] - bottomVerts[1]).normalized;
                        }
                    }

                    if (!newForward.HasValue)
                    {
                        if (topLeftVerts.Count > 0 && bottomRightVerts.Count > 0)
                        {
                            newForward = (topLeftVerts[0] - bottomRightVerts[0]).normalized;
                        }
                        else if (bottomLeftVerts.Count > 0 && topRightVerts.Count > 0)
                        {
                            newForward = (bottomLeftVerts[0] - topRightVerts[0]).normalized;
                        }
                    }

                    if (!newRight.HasValue || !newForward.HasValue)
                        Debug.Log("CompOperation: could not determine right face orientation vectors");
                }

                LocalTransform newTransform = new LocalTransform(newCenter, newUp, newForward.Value, newRight.Value);

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
            Vector3 normal = MathUtility.ConvertToVector3(face.GetVertexNormal(0));

            float dot = Vector3.Dot(directionNormals["Back"], normal);

            if (dot > 0.0001f && dot <= 1.0001f)
            {
                Mesh newMesh = g3UnityUtils.DMeshToUnityMesh(face);

                Vector3 newCenter = BuildingUtility.FindPolygonCenter(face, normal);

                Vector3 faceCenter = MathUtility.ConvertToVector3(face.GetBounds().Center);
                upPlane = new Plane(transform.Up, faceCenter);

                Vector3 p0 = MathUtility.ConvertToVector3(face.GetVertex(0));
                Vector3 p1 = MathUtility.ConvertToVector3(face.GetVertex(1));
                Vector3 p2 = MathUtility.ConvertToVector3(face.GetVertex(2));

                Vector3 oldForward = (p0 - p1).normalized;
                Vector3 oldRight = (p1 - p2).normalized;

                List<Vector3> topVerts = new List<Vector3>();
                List<Vector3> bottomVerts = new List<Vector3>();

                List<Vector3> topRightVerts = new List<Vector3>();
                List<Vector3> bottomRightVerts = new List<Vector3>();

                List<Vector3> topLeftVerts = new List<Vector3>();
                List<Vector3> bottomLeftVerts = new List<Vector3>();

                List<Vector3> refVerts = new List<Vector3>();
                for (int j = 0; j < face.VertexCount; j++)
                {
                    Vector3 refVert = MathUtility.ConvertToVector3(face.GetVertex(j));
                    refVerts.Add(refVert);
                }

                foreach (Vector3 v in refVerts)
                {
                    if (upPlane.GetDistanceToPoint(v) > 0f)
                    {
                        topVerts.Add(v);
                    }
                    else
                    {
                        bottomVerts.Add(v);
                    }
                }

                Vector3? parallelToFace = null;

                if (topVerts.Count > 1)
                {
                    Vector3 vertA = Vector3.zero;
                    Vector3 vertB = Vector3.zero;

                    for (int j = 0; j < topVerts.Count - 1; j++)
                    {
                        vertA = topVerts[j];
                        vertB = topVerts[j + 1];

                        if (Vector3.SqrMagnitude(vertA - vertB) > 0.01f)
                        {
                            break;
                        }
                    }

                    parallelToFace = (vertB - vertA).normalized;
                }
                else if (bottomVerts.Count > 1)
                {
                    Vector3 vertA = Vector3.zero;
                    Vector3 vertB = Vector3.zero;

                    for (int j = 0; j < bottomVerts.Count - 1; j++)
                    {
                        vertA = bottomVerts[j];
                        vertB = bottomVerts[j + 1];

                        if (Vector3.SqrMagnitude(vertA - vertB) > 0.01f)
                        {
                            break;
                        }
                    }

                    parallelToFace = (vertB - vertA).normalized;
                }

                float parallelDirection = Vector3.Dot(parallelToFace.Value, transform.Right);

                List<Vector3> leftVerts = new List<Vector3>();
                List<Vector3> rightVerts = new List<Vector3>();

                bool isPointingRight = false;

                if (parallelDirection < 0)
                {
                    isPointingRight = true;
                }

                Plane rightPlane = new Plane(parallelToFace.Value, newCenter);

                foreach (Vector3 v in topVerts)
                {
                    if (rightPlane.GetDistanceToPoint(v) > 0)
                    {
                        if (isPointingRight)
                        {
                            topRightVerts.Add(v);
                        }
                        else
                        {
                            topLeftVerts.Add(v);
                        }
                    }
                    else
                    {
                        if (isPointingRight)
                        {
                            topLeftVerts.Add(v);
                        }
                        else
                        {
                            topRightVerts.Add(v);
                        }
                    }
                }

                foreach (Vector3 v in bottomVerts)
                {
                    if (rightPlane.GetDistanceToPoint(v) > 0)
                    {
                        if (isPointingRight)
                        {
                            bottomRightVerts.Add(v);
                        }
                        else
                        {
                            bottomLeftVerts.Add(v);
                        }
                    }
                    else
                    {
                        if (isPointingRight)
                        {
                            bottomLeftVerts.Add(v);
                        }
                        else
                        {
                            bottomRightVerts.Add(v);
                        }
                    }
                }

                Vector3 newUp = normal;

                Vector3? newForward = null;
                Vector3? newRight = null;

                if (topLeftVerts.Count > 0 && topRightVerts.Count > 0)
                {
                    newRight = (topLeftVerts[0] - topRightVerts[0]).normalized;
                }
                else if (bottomLeftVerts.Count > 0 && bottomRightVerts.Count > 0)
                {
                    newRight = (bottomLeftVerts[0] - bottomRightVerts[0]).normalized;
                }
                else if (topLeftVerts.Count > 1)
                {
                    newRight = (topLeftVerts[1] - topLeftVerts[0]).normalized;
                }
                else if (topRightVerts.Count > 1)
                {
                    newRight = (topRightVerts[1] - topRightVerts[0]).normalized;
                }

                if (topLeftVerts.Count > 0 && bottomLeftVerts.Count > 0)
                {
                    newForward = (topLeftVerts[0] - bottomLeftVerts[0]).normalized;
                }
                else if (topRightVerts.Count > 0 && bottomRightVerts.Count > 0)
                {
                    newForward = (topRightVerts[0] - bottomRightVerts[0]).normalized;
                }
                else if (bottomLeftVerts.Count > 1)
                {
                    newForward = (bottomLeftVerts[1] - bottomLeftVerts[0]).normalized;
                }
                else if (bottomRightVerts.Count > 1)
                {
                    newForward = (bottomRightVerts[1] - bottomRightVerts[0]).normalized;
                }

                if (!newRight.HasValue || !newForward.HasValue)
                {
                    if (!newRight.HasValue)
                    {
                        if (topVerts.Count > 1)
                        {
                            newRight = (topVerts[0] - topVerts[1]).normalized;
                        }
                        else if (bottomVerts.Count > 1)
                        {
                            newRight = (bottomVerts[0] - bottomVerts[1]).normalized;
                        }
                    }

                    if (!newForward.HasValue)
                    {
                        if (topLeftVerts.Count > 0 && bottomRightVerts.Count > 0)
                        {
                            newForward = (topLeftVerts[0] - bottomRightVerts[0]).normalized;
                        }
                        else if (bottomLeftVerts.Count > 0 && topRightVerts.Count > 0)
                        {
                            newForward = (bottomLeftVerts[0] - topRightVerts[0]).normalized;
                        }
                    }
                    if (!newRight.HasValue || !newForward.HasValue)
                        Debug.Log("CompOperation: could not determine back face orientation vectors");
                }

                LocalTransform newTransform = new LocalTransform(newCenter, newUp, newForward.Value, newRight.Value);

                Shape newShape = new Shape(newMesh, newTransform);
                backFaces.Add(newShape);
                
                partsList.RemoveAt(i);
            }
        }

        List<Shape> extraFaces = new List<Shape>();

        // find top faces
        List<Shape> topFaces = new List<Shape>();
        for (int i = partsList.Count - 1; i >= 0; i--)
        {
            DMesh3 face = partsList[i];
            Vector3 normal = MathUtility.ConvertToVector3(face.GetVertexNormal(0));

            float dot = Vector3.Dot(directionNormals["Top"], normal);

            if (dot > 0.0001f && dot <= 1.0001f)
            {
                Vector3 p0 = MathUtility.ConvertToVector3(face.GetVertex(0));
                Vector3 p1 = MathUtility.ConvertToVector3(face.GetVertex(1));

                Mesh newMesh = g3UnityUtils.DMeshToUnityMesh(face);

                Vector3 newCenter = BuildingUtility.FindPolygonCenter(face, normal);

                double diag = face.GetBounds().DiagonalLength;

                double boundsWidth = face.GetBounds().Width;
                double boundsHeight = face.GetBounds().Height;
                double boundsDepth = face.GetBounds().Depth;
                
                LocalTransform newTransform = new LocalTransform(newCenter, transform.Up, transform.Forward, transform.Right);

                Shape newShape = new Shape(newMesh, newTransform);

                if(diag <= FACE_IGNORE_THRESHOLD)
                {
                    extraFaces.Add(newShape);
                }
                else
                {
                    topFaces.Add(newShape);
                }

                partsList.RemoveAt(i);
            }
        }

        // find bottom faces
        List<Shape> bottomFaces = new List<Shape>();
        for (int i = partsList.Count - 1; i >= 0; i--)
        {
            DMesh3 face = partsList[i];
            Vector3 normal = MathUtility.ConvertToVector3(face.GetVertexNormal(0));

            float dot = Vector3.Dot(directionNormals["Bottom"], normal);

            if (dot > 0.0001f && dot <= 1.0001f)
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

                //bottomPlane = new Plane(newUp, newCenter);
            }
        }

        faces.Add("Front", frontfaces);
        faces.Add("Back", backFaces);
        faces.Add("Left", leftFaces);
        faces.Add("Right", rightFaces);
        faces.Add("Top", topFaces);
        faces.Add("Bottom", bottomFaces);
        faces.Add("Extra", extraFaces);

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
}
