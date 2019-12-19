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
    private const float FACE_IGNORE_THRESHOLD = 6f; //4.5f; // 4f;

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

        //for(int i = 0; i < partsList.Count; i++)
        //{
        //    double diag = partsList[i].GetBounds().DiagonalLength;
        //    Debug.Log("face diag: " +"(" + i + "): " + diag);
        //}


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

        //Plane topPlane = new Plane();
        //Plane bottomPlane = new Plane();
        Plane upPlane = new Plane(transform.Up, transform.Origin);
        //Plane rightPlane = new Plane(transform.Right, transform.Origin);
        //Plane forwardPlane = new Plane(transform.Forward, transform.Origin);

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

                if (false)
                {
                    Vector3 a = MathUtility.ConvertToVector3(face.GetVertex(0));
                    Vector3 b = MathUtility.ConvertToVector3(face.GetVertex(1));
                    Vector3 c = MathUtility.ConvertToVector3(face.GetVertex(2));

                    Debug.DrawLine(newCenter, newCenter + ((newCenter - a).normalized * 50f), Color.magenta, 1000.0f);
                    Debug.DrawLine(newCenter, newCenter + ((newCenter - b).normalized * 50f), Color.green, 1000.0f);
                    Debug.DrawLine(newCenter, newCenter + ((newCenter - c).normalized * 50f), Color.yellow, 1000.0f);


                    Vector3 y = Quaternion.AngleAxis(90, transform.Right) * transform.Up;
                    Vector3 z = Quaternion.AngleAxis(90, transform.Right) * transform.Forward;
                    Vector3 x = transform.Right;

                    Debug.DrawLine(newCenter, newCenter + (y * 50f), Color.green, 1000.0f);
                    Debug.DrawLine(newCenter, newCenter + (z * 50f), Color.blue, 1000.0f);
                    Debug.DrawLine(newCenter, newCenter + (x * 50f), Color.red, 1000.0f);
                }


                Vector3 p0 = MathUtility.ConvertToVector3(face.GetVertex(0));
                Vector3 p1 = MathUtility.ConvertToVector3(face.GetVertex(1));
                Vector3 p2 = MathUtility.ConvertToVector3(face.GetVertex(2));

                List<Vector3> topVerts = new List<Vector3>();
                List<Vector3> bottomVerts = new List<Vector3>();

                List<Vector3> topRightVerts = new List<Vector3>();
                List<Vector3> bottomRightVerts = new List<Vector3>();

                List<Vector3> topLeftVerts = new List<Vector3>();
                List<Vector3> bottomLeftVerts = new List<Vector3>();
                //List<Vector3> refVerts = new List<Vector3>() { p0, p1, p2 };

                List<Vector3> refVerts = new List<Vector3>();
                for (int j = 0; j < face.VertexCount; j++)
                {
                    Vector3 refVert = MathUtility.ConvertToVector3(face.GetVertex(j));
                    refVerts.Add(refVert);
                }

                //Dictionary<Vector3, float> refVerts = new Dictionary<Vector3, float>();
                //refVerts.Add(p0, d0);
                //refVerts.Add(p1, d1);
                //refVerts.Add(p2, d2);

                //foreach(KeyValuePair<Vector3, float> vert in refVerts)
                //{
                //    if(vert.Value > 0f)
                //    {
                //        topVerts.Add(vert.Key);
                //    }
                //    else
                //    {
                //        bottomVerts.Add(vert.Key);
                //    }
                //}

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


                //foreach(Vector3 v in topVerts)
                //{
                //    GameObject a = UnityEngine.Object.Instantiate(Resources.Load("BlueCube"), v, Quaternion.identity) as GameObject;
                //}

                //foreach(Vector3 v in bottomVerts)
                //{
                //    GameObject a = UnityEngine.Object.Instantiate(Resources.Load("PinkCube"), v, Quaternion.identity) as GameObject;
                //}


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


                //Debug.DrawLine(newCenter, newCenter + (parallelToFace.Value * 50f), Color.yellow, 1000.0f);


                float parallelDirection = Vector3.Dot(parallelToFace.Value, transform.Right);

                // rotate up and forward vectors 90 degrees around right axis, keep right axis the same
                //Vector3 newUp = Quaternion.AngleAxis(90, transform.Right) * transform.Up;
                //Vector3 newForward = Quaternion.AngleAxis(90, transform.Right) * transform.Forward; 


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



                //Plane forwardPlane = new Plane(transform.Forward, transform.Origin);


                Vector3 newUp = normal;
                //Vector3 newForward = (p1 - p0).normalized;
                //Vector3 newRight = (p2 - p1).normalized;

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


                    

                    //foreach (Vector3 v in topLeftVerts)
                    //{
                    //    GameObject a = UnityEngine.Object.Instantiate(Resources.Load("BlueCube"), v, Quaternion.identity) as GameObject;
                    //}

                    //foreach (Vector3 v in topRightVerts)
                    //{
                    //    GameObject a = UnityEngine.Object.Instantiate(Resources.Load("PinkCube"), v, Quaternion.identity) as GameObject;
                    //}

                    //foreach (Vector3 v in bottomLeftVerts)
                    //{
                    //    GameObject a = UnityEngine.Object.Instantiate(Resources.Load("YellowCube"), v, Quaternion.identity) as GameObject;
                    //}

                    //foreach (Vector3 v in bottomRightVerts)
                    //{
                    //    GameObject a = UnityEngine.Object.Instantiate(Resources.Load("OrangeCube"), v, Quaternion.identity) as GameObject;
                    //}

                    //Debug.DrawLine(newCenter, newCenter + (parallelToFace.Value * 50f), Color.yellow, 1000.0f);
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



                //Debug.DrawLine(newCenter, newCenter + (newUp * 50f), Color.cyan, 1000.0f);
                //Debug.DrawLine(newCenter, newCenter + (oldForward * 50f), Color.yellow, 1000.0f);
                //Debug.DrawLine(newCenter, newCenter + (oldRight * 50f), Color.magenta, 1000.0f);

                //Vector3 p0 = (Vector3)face.GetVertex(0);
                //Vector3 p1 = (Vector3)face.GetVertex(1);
                //Vector3 p2 = (Vector3)face.GetVertex(2);

                List<Vector3> topVerts = new List<Vector3>();
                List<Vector3> bottomVerts = new List<Vector3>();

                List<Vector3> topRightVerts = new List<Vector3>();
                List<Vector3> bottomRightVerts = new List<Vector3>();

                List<Vector3> topLeftVerts = new List<Vector3>();
                List<Vector3> bottomLeftVerts = new List<Vector3>();
                //List<Vector3> refVerts = new List<Vector3>() { p0, p1, p2 };

                List<Vector3> refVerts = new List<Vector3>();
                for (int j = 0; j < face.VertexCount; j++)
                {
                    Vector3 refVert = MathUtility.ConvertToVector3(face.GetVertex(j));
                    refVerts.Add(refVert);
                }
                //Dictionary<Vector3, float> refVerts = new Dictionary<Vector3, float>();
                //refVerts.Add(p0, d0);
                //refVerts.Add(p1, d1);
                //refVerts.Add(p2, d2);

                //foreach(KeyValuePair<Vector3, float> vert in refVerts)
                //{
                //    if(vert.Value > 0f)
                //    {
                //        topVerts.Add(vert.Key);
                //    }
                //    else
                //    {
                //        bottomVerts.Add(vert.Key);
                //    }
                //}

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


                //foreach(Vector3 v in topVerts)
                //{
                //    GameObject a = UnityEngine.Object.Instantiate(Resources.Load("BlueCube"), v, Quaternion.identity) as GameObject;
                //}

                //foreach(Vector3 v in bottomVerts)
                //{
                //    GameObject a = UnityEngine.Object.Instantiate(Resources.Load("PinkCube"), v, Quaternion.identity) as GameObject;
                //}


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


                //Debug.DrawLine(newCenter, newCenter + (parallelToFace.Value * 50f), Color.yellow, 1000.0f);


                float parallelDirection = Vector3.Dot(parallelToFace.Value, transform.Forward);

                // rotate up and forward vectors 90 degrees around right axis, keep right axis the same
                //Vector3 newUp = Quaternion.AngleAxis(90, transform.Right) * transform.Up;
                //Vector3 newForward = Quaternion.AngleAxis(90, transform.Right) * transform.Forward; 

                //Debug.DrawLine(newCenter, newCenter + (parallelToFace.Value * 50f), Color.yellow, 1000.0f);

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



                //Plane forwardPlane = new Plane(transform.Forward, transform.Origin);


                //Vector3 newUp = normal;
                //Vector3 newForward = (p1 - p0).normalized;
                //Vector3 newRight = (p2 - p1).normalized;

                Vector3? newForward = null;
                Vector3? newRight = null;


                //foreach (Vector3 v in topLeftVerts)
                //{
                //    GameObject a = UnityEngine.Object.Instantiate(Resources.Load("BlueCube"), v, Quaternion.identity) as GameObject;
                //}

                //foreach (Vector3 v in topRightVerts)
                //{
                //    GameObject a = UnityEngine.Object.Instantiate(Resources.Load("PinkCube"), v, Quaternion.identity) as GameObject;
                //}

                //foreach (Vector3 v in bottomLeftVerts)
                //{
                //    GameObject a = UnityEngine.Object.Instantiate(Resources.Load("YellowCube"), v, Quaternion.identity) as GameObject;
                //}

                //foreach (Vector3 v in bottomRightVerts)
                //{
                //    GameObject a = UnityEngine.Object.Instantiate(Resources.Load("OrangeCube"), v, Quaternion.identity) as GameObject;
                //}



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
                    //newRight = oldRight;
                    //newForward = oldForward;
                    if (!newRight.HasValue || !newForward.HasValue)
                    {
                        Debug.Log("CompOperation: could not determine left face orientation vectors");
                    }

                }


                //LocalTransform newTransform = new LocalTransform(newCenter, newUp, newForward, newRight);
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

                //Vector3 newRight = (p1 - p0).normalized;
                //Vector3 newForward = (p1 - p2).normalized;

                //Debug.DrawLine(newCenter, newCenter + (newUp * 50f), Color.cyan, 1000.0f);
                //Debug.DrawLine(newCenter, newCenter + (oldForward * 50f), Color.yellow, 1000.0f);
                //Debug.DrawLine(newCenter, newCenter + (oldRight * 50f), Color.magenta, 1000.0f);

                //Vector3 p0 = (Vector3)face.GetVertex(0);
                //Vector3 p1 = (Vector3)face.GetVertex(1);
                //Vector3 p2 = (Vector3)face.GetVertex(2);

                List<Vector3> topVerts = new List<Vector3>();
                List<Vector3> bottomVerts = new List<Vector3>();

                List<Vector3> topRightVerts = new List<Vector3>();
                List<Vector3> bottomRightVerts = new List<Vector3>();

                List<Vector3> topLeftVerts = new List<Vector3>();
                List<Vector3> bottomLeftVerts = new List<Vector3>();
                //List<Vector3> refVerts = new List<Vector3>() { p0, p1, p2 };

                List<Vector3> refVerts = new List<Vector3>();

                for(int j = 0; j < face.VertexCount; j++)
                {
                    Vector3 refVert = MathUtility.ConvertToVector3(face.GetVertex(j));
                    refVerts.Add(refVert);
                }

                //refVerts.Add(p0);
                //refVerts.Add(p1);
                //refVerts.Add(p2);

                //Dictionary<Vector3, float> refVerts = new Dictionary<Vector3, float>();
                //refVerts.Add(p0, d0);
                //refVerts.Add(p1, d1);
                //refVerts.Add(p2, d2);

                //foreach(KeyValuePair<Vector3, float> vert in refVerts)
                //{
                //    if(vert.Value > 0f)
                //    {
                //        topVerts.Add(vert.Key);
                //    }
                //    else
                //    {
                //        bottomVerts.Add(vert.Key);
                //    }
                //}

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


                //foreach(Vector3 v in topVerts)
                //{
                //    GameObject a = UnityEngine.Object.Instantiate(Resources.Load("BlueCube"), v, Quaternion.identity) as GameObject;
                //}

                //foreach(Vector3 v in bottomVerts)
                //{
                //    GameObject a = UnityEngine.Object.Instantiate(Resources.Load("PinkCube"), v, Quaternion.identity) as GameObject;
                //}


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

                // rotate up and forward vectors 90 degrees around right axis, keep right axis the same
                //Vector3 newUp = Quaternion.AngleAxis(90, transform.Right) * transform.Up;
                //Vector3 newForward = Quaternion.AngleAxis(90, transform.Right) * transform.Forward; 


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



                //Plane forwardPlane = new Plane(transform.Forward, transform.Origin);


                //Vector3 newUp = normal;
                //Vector3 newForward = (p1 - p0).normalized;
                //Vector3 newRight = (p2 - p1).normalized;

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

                    //Debug.DrawLine(newCenter, newCenter + (parallelToFace.Value * 50f), Color.yellow, 1000.0f);

                    //foreach (Vector3 v in topLeftVerts)
                    //{
                    //    GameObject a = UnityEngine.Object.Instantiate(Resources.Load("BlueCube"), v, Quaternion.identity) as GameObject;
                    //}

                    //foreach (Vector3 v in topRightVerts)
                    //{
                    //    GameObject a = UnityEngine.Object.Instantiate(Resources.Load("PinkCube"), v, Quaternion.identity) as GameObject;
                    //}

                    //foreach (Vector3 v in bottomLeftVerts)
                    //{
                    //    GameObject a = UnityEngine.Object.Instantiate(Resources.Load("YellowCube"), v, Quaternion.identity) as GameObject;
                    //}

                    //foreach (Vector3 v in bottomRightVerts)
                    //{
                    //    GameObject a = UnityEngine.Object.Instantiate(Resources.Load("OrangeCube"), v, Quaternion.identity) as GameObject;
                    //}

                    //newRight = oldRight;
                    //newForward = oldForward;
                }


                //LocalTransform newTransform = new LocalTransform(newCenter, newUp, newForward, newRight);
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

                //Vector3 newRight = (p1 - p2).normalized;
                //Vector3 newForward = (p0 - p1).normalized;


                //Debug.DrawLine(newCenter, newCenter + (normal * 50f), Color.cyan, 1000.0f);
                //Debug.DrawLine(newCenter, newCenter + (oldForward * 50f), Color.yellow, 1000.0f);
                //Debug.DrawLine(newCenter, newCenter + (oldRight * 50f), Color.magenta, 1000.0f);

                List<Vector3> topVerts = new List<Vector3>();
                List<Vector3> bottomVerts = new List<Vector3>();

                List<Vector3> topRightVerts = new List<Vector3>();
                List<Vector3> bottomRightVerts = new List<Vector3>();

                List<Vector3> topLeftVerts = new List<Vector3>();
                List<Vector3> bottomLeftVerts = new List<Vector3>();
                //List<Vector3> refVerts = new List<Vector3>() { p0, p1, p2 };

                List<Vector3> refVerts = new List<Vector3>();
                for (int j = 0; j < face.VertexCount; j++)
                {
                    Vector3 refVert = MathUtility.ConvertToVector3(face.GetVertex(j));
                    refVerts.Add(refVert);
                }

                //Dictionary<Vector3, float> refVerts = new Dictionary<Vector3, float>();
                //refVerts.Add(p0, d0);
                //refVerts.Add(p1, d1);
                //refVerts.Add(p2, d2);

                //foreach(KeyValuePair<Vector3, float> vert in refVerts)
                //{
                //    if(vert.Value > 0f)
                //    {
                //        topVerts.Add(vert.Key);
                //    }
                //    else
                //    {
                //        bottomVerts.Add(vert.Key);
                //    }
                //}

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


                //foreach(Vector3 v in topVerts)
                //{
                //    GameObject a = UnityEngine.Object.Instantiate(Resources.Load("BlueCube"), v, Quaternion.identity) as GameObject;
                //}

                //foreach(Vector3 v in bottomVerts)
                //{
                //    GameObject a = UnityEngine.Object.Instantiate(Resources.Load("PinkCube"), v, Quaternion.identity) as GameObject;
                //}


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


                //Debug.DrawLine(newCenter, newCenter + (parallelToFace.Value * 50f), Color.yellow, 1000.0f);


                float parallelDirection = Vector3.Dot(parallelToFace.Value, transform.Right);

                // rotate up and forward vectors 90 degrees around right axis, keep right axis the same
                //Vector3 newUp = Quaternion.AngleAxis(90, transform.Right) * transform.Up;
                //Vector3 newForward = Quaternion.AngleAxis(90, transform.Right) * transform.Forward; 


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



                //Plane forwardPlane = new Plane(transform.Forward, transform.Origin);


                Vector3 newUp = normal;
                //Vector3 newForward = (p1 - p0).normalized;
                //Vector3 newRight = (p2 - p1).normalized;

                Vector3? newForward = null;
                Vector3? newRight = null;


                //foreach (Vector3 v in topLeftVerts)
                //{
                //    GameObject a = UnityEngine.Object.Instantiate(Resources.Load("BlueCube"), v, Quaternion.identity) as GameObject;
                //}

                //foreach (Vector3 v in topRightVerts)
                //{
                //    GameObject a = UnityEngine.Object.Instantiate(Resources.Load("PinkCube"), v, Quaternion.identity) as GameObject;
                //}

                //foreach (Vector3 v in bottomLeftVerts)
                //{
                //    GameObject a = UnityEngine.Object.Instantiate(Resources.Load("YellowCube"), v, Quaternion.identity) as GameObject;
                //}

                //foreach (Vector3 v in bottomRightVerts)
                //{
                //    GameObject a = UnityEngine.Object.Instantiate(Resources.Load("OrangeCube"), v, Quaternion.identity) as GameObject;
                //}

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
                //newMesh.RecalculateBounds();

                Vector3 newCenter = BuildingUtility.FindPolygonCenter(face, normal);



                double diag = face.GetBounds().DiagonalLength;

                double boundsWidth = face.GetBounds().Width;
                double boundsHeight = face.GetBounds().Height;
                double boundsDepth = face.GetBounds().Depth;




                //if (true)
                //{
                //    Vector3 a = (Vector3)face.GetVertex(0);
                //    Vector3 b = (Vector3)face.GetVertex(1);
                //    Vector3 c = (Vector3)face.GetVertex(2);

                //    Debug.DrawLine(newCenter, newCenter + ((newCenter - a).normalized * 50f), Color.magenta, 1000.0f);
                //    Debug.DrawLine(newCenter, newCenter + ((newCenter - b).normalized * 50f), Color.green, 1000.0f);
                //    Debug.DrawLine(newCenter, newCenter + ((newCenter - c).normalized * 50f), Color.yellow, 1000.0f);

                //    // rotate up and right vectors 90 degrees around forward axis, keep forward axis the same
                //    Vector3 y = Quaternion.AngleAxis(-90, transform.Right) * transform.Up;
                //    Vector3 x = Quaternion.AngleAxis(-90, transform.Right) * transform.Forward;
                //    Vector3 z = transform.Right;

                //    Debug.DrawLine(newCenter, newCenter + (y * 50f), Color.green, 1000.0f);
                //    Debug.DrawLine(newCenter, newCenter + (z * 50f), Color.blue, 1000.0f);
                //    Debug.DrawLine(newCenter, newCenter + (x * 50f), Color.red, 1000.0f);
                //}


                //topPlane = new Plane(transform.Up, newCenter);

                //LocalTransform newTransform = new LocalTransform(newCenter, normal, (p1 - p0).normalized);
                LocalTransform newTransform = new LocalTransform(newCenter, transform.Up, transform.Forward, transform.Right);

                Shape newShape = new Shape(newMesh, newTransform);

                if(diag <= FACE_IGNORE_THRESHOLD)
                {
                    extraFaces.Add(newShape);

                    //Debug.Log("comp, ignored face: w/h/d: " + boundsWidth +  "/ "+ boundsHeight + " / "+ boundsDepth + " | " + diag);

                }
                else
                {
                    topFaces.Add(newShape);

                    //Debug.Log("comp, added face: w/h/d: " + boundsWidth + "/ " + boundsHeight + " / " + boundsDepth + " | " + diag);
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

                //if (true)
                //{
                //    Vector3 a = (Vector3)face.GetVertex(0);
                //    Vector3 b = (Vector3)face.GetVertex(1);
                //    Vector3 c = (Vector3)face.GetVertex(2);

                //    Debug.DrawLine(newCenter, newCenter + ((newCenter - a).normalized * 50f), Color.magenta, 1000.0f);
                //    Debug.DrawLine(newCenter, newCenter + ((newCenter - b).normalized * 50f), Color.green, 1000.0f);
                //    Debug.DrawLine(newCenter, newCenter + ((newCenter - c).normalized * 50f), Color.yellow, 1000.0f);

                //    // rotate up and right vectors 90 degrees around forward axis, keep forward axis the same
                //    Vector3 y = Quaternion.AngleAxis(-90, transform.Right) * transform.Up;
                //    Vector3 x = Quaternion.AngleAxis(-90, transform.Right) * transform.Forward;
                //    Vector3 z = transform.Right;

                //    Debug.DrawLine(newCenter, newCenter + (y * 50f), Color.green, 1000.0f);
                //    Debug.DrawLine(newCenter, newCenter + (z * 50f), Color.blue, 1000.0f);
                //    Debug.DrawLine(newCenter, newCenter + (x * 50f), Color.red, 1000.0f);
                //}


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
