using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SplitOperation : MonoBehaviour
{
    public static List<Mesh> Split(Mesh mesh, Transform transform, Plane plane)
    {

        Vector3[] inputVertices = mesh.vertices;
        Vector2[] inputUV = mesh.uv;
        int[] inputTriangles = mesh.triangles;
        Vector3[] inputNormals = mesh.normals;

        int vertexCount = mesh.vertexCount;

        //Vector3[] vertices = new Vector3[vertexCount + 8];
        //Vector2[] uvs = new Vector2[vertices.Length];
        //int[] triangles = new int[inputTriangles.Length + 16];

        List<Triangle> positive = new List<Triangle>();
        List<Triangle> negative = new List<Triangle>();
        //List<Mesh> positive = new List<Mesh>();
        //List<Mesh> negative = new List<Mesh>();

        //int triangleCount = triangles.Length;

        //Mesh positive = mesh;
        //Mesh negative = mesh;

        //Mesh positive = mesh;
        //Mesh negative = mesh;
        bool[] sides = new bool[vertexCount];

        List<Vector3> gizmos = new List<Vector3>();

        // organize the mesh into two sides using the plane as a divider
        for(int i = 0; i < mesh.vertexCount; i++)
        {
            inputVertices[i] = transform.TransformPoint(inputVertices[i]);
            sides[i] = plane.GetSide(inputVertices[i]);
        }

        // 

        for (int i = 0; i < inputTriangles.Length - 2; i += 3)
        {
            if(i > 12)
            {
                //continue;
            }

            int lonePoint = -1;

            // detect if a triangle intersects the dividing plane
            if(sides[inputTriangles[i + 1]] != sides[inputTriangles[i + 2]] ||
                sides[inputTriangles[i + 1]] != sides[inputTriangles[i]] ||
                sides[inputTriangles[i + 2]] != sides[inputTriangles[i]])
            {
                if (sides[inputTriangles[i]] == sides[inputTriangles[i + 1]])
                {
                    lonePoint = 2;
                }
                else if(sides[inputTriangles[i + 1]] == sides[inputTriangles[i + 2]])
                {
                    lonePoint = 0;
                }
                else
                {
                    lonePoint = 1;
                }
            }

            if (lonePoint >= 0)
            {



                ////draw gizmo on triangle verts
                //for (int j = 0; j < 3; j++)
                //{
                //    Instantiate(Resources.Load("Cube", typeof(GameObject)), vertices[triangles[i + j]], Quaternion.identity);
                //    Debug.Log(i + j);
                //}


                //if(i > 0)
                //{
                //   // continue;
                //}

                // find the extra vertices required to split

                // split vertices by side
                // should be one on one side and two on the other
                List<int> aSide = new List<int>();
                List<int> bSide = new List<int>();

                for (int j = 0; j < 3; j++)
                {
                    if (sides[inputTriangles[i + j]])
                    {
                        aSide.Add(inputTriangles[i + j]);
                    }
                    else
                    {
                        bSide.Add(inputTriangles[i + j]);
                    }

                    //Instantiate(Resources.Load("Cube", typeof(GameObject)), vertices[triangles[i + j]], Quaternion.identity);
                    //Debug.Log(i + j);
                }

                bool test = true;

                int oneVert;
                int[] twoVerts = new int[2];

                if (aSide.Count == 1)
                {
                    oneVert = aSide[0];
                    twoVerts[0] = bSide[0];
                    twoVerts[1] = bSide[1];

                    test = true;
                }
                else // a side has two
                {
                    oneVert = bSide[0];
                    twoVerts[0] = aSide[0];
                    twoVerts[1] = aSide[1];

                    test = false;
                }


                Vector3 a = inputVertices[oneVert];
                Vector3 b = inputVertices[twoVerts[0]];
                Vector3 c = inputVertices[twoVerts[1]];

                Vector3 direction1 = a - b;
                Vector3 direction2 = a - c;

                direction1.Normalize();
                direction2.Normalize();

                Ray ray1 = new Ray(b, direction1);
                Ray ray2 = new Ray(c, direction2);

                //Initialise the enter variable
                float enter1 = 0.0f;
                float enter2 = 0.0f;

                Vector3 extraVert1 = Vector3.zero;
                Vector3 extraVert2 = Vector3.zero;

                if (plane.Raycast(ray1, out enter1))
                {
                    extraVert1 = ray1.GetPoint(enter1);
                    //Instantiate(Resources.Load("PinkCube", typeof(GameObject)), extraVert1, Quaternion.identity);
                }
                else
                {
                    Debug.Log("BAD BAD");
                }

                if (plane.Raycast(ray2, out enter2))
                {
                    extraVert2 = ray2.GetPoint(enter2);
                    //Instantiate(Resources.Load("YellowCube", typeof(GameObject)), extraVert2, Quaternion.identity);
                }
                else
                {
                    Debug.Log("BAD BAD");
                }

                //Triangle aSideTriangle = new Triangle(a, extraVert1, extraVert2, inputNormals[oneVert]);
                //Triangle bSideTriangle1 = new Triangle(b, c, extraVert2, inputNormals[twoVerts[0]]);
                //Triangle bSideTriangle2 = new Triangle(b, extraVert2, inputNormals[twoVerts[0]]);

                Vector3 normal = inputNormals[oneVert];


                //if (test)
                //{
                if (i == 18)
                {
                    GameObject go = (GameObject)Instantiate(Resources.Load("YellowCube", typeof(GameObject)), extraVert1, Quaternion.identity);
                    go.transform.localScale = new Vector3(go.transform.localScale.x * 5.0f, go.transform.localScale.y * 5.0f, go.transform.localScale.z * 5.0f);

                    go = (GameObject)Instantiate(Resources.Load("YellowCube", typeof(GameObject)), extraVert2, Quaternion.identity);
                    go.transform.localScale = new Vector3(go.transform.localScale.x * 5.0f, go.transform.localScale.y * 5.0f, go.transform.localScale.z * 5.0f);
                    //Debug.Log(i);
                }



                //}
                //else
                //{
                //    GameObject go = (GameObject)Instantiate(Resources.Load("YellowCube", typeof(GameObject)), c, Quaternion.identity);
                //    go.transform.localScale = new Vector3(go.transform.localScale.x * 5.0f, go.transform.localScale.y * 5.0f, go.transform.localScale.z * 5.0f);

                //}


                Triangle aSideTriangle = new Triangle(a, extraVert1, extraVert2, normal);
                Triangle bSideTriangle1 = new Triangle(b, c, extraVert2, normal);
                Triangle bSideTriangle2 = new Triangle(b, extraVert2, extraVert1, normal);

                Vector3 surfaceNormalA = Vector3.Cross(aSideTriangle.v2.position - aSideTriangle.v1.position, aSideTriangle.v3.position - aSideTriangle.v1.position).normalized;
                Vector3 surfaceNormalb1 = Vector3.Cross(bSideTriangle1.v2.position - bSideTriangle1.v1.position, bSideTriangle1.v3.position - bSideTriangle1.v1.position).normalized;
                Vector3 surfaceNormalb2 = Vector3.Cross(bSideTriangle2.v2.position - bSideTriangle2.v1.position, bSideTriangle2.v3.position - bSideTriangle2.v1.position).normalized;


                if (surfaceNormalA != normal)
                {
                    aSideTriangle.ChangeOrientation();
                }

                if (surfaceNormalb1 != normal)
                {
                    bSideTriangle1.ChangeOrientation();
                }

                if (surfaceNormalb2 != normal)
                {
                    bSideTriangle2.ChangeOrientation();
                }



                //Debug.DrawLine(a, a + normalA, Color.yellow, 1000.0f, false);
                //Debug.DrawLine(b, b + normalB, Color.yellow, 1000.0f, false);
                //Debug.DrawLine(originalVert, originalVert + (originalNormal * 1.5f), Color.red, 1000.0f, false);


                //List<Vector3> bSideVertices = new List<Vector3>() { b, c, extraVert2, extraVert1 };

                //Instantiate(Resources.Load("YellowCube", typeof(GameObject)), b, Quaternion.identity);
                //Instantiate(Resources.Load("PinkCube", typeof(GameObject)), c, Quaternion.identity);
                //Instantiate(Resources.Load("OrangeCube", typeof(GameObject)), extraVert1, Quaternion.identity);





                // List<Triangle> bSideTriangles = Triangulator.TriangulatePolygon(bSideVertices);

                //int f = 0;

                if (sides[oneVert])
                {
                    //aSideTriangle.ChangeOrientation();

                    //Plane p = new Plane(aSideTriangle.v1.position, aSideTriangle.v2.position, aSideTriangle.v3.position);

                    //float dist = plane.GetDistanceToPoint(aSideTriangle.v1.position);
                    //Vector3 originalNormal = inputNormals[oneVert];

                    //p.Translate(originalNormal * dist);


                    //Vector3 planePoint = p.ClosestPointOnPlane(aSideTriangle.v1.position);

                    //Vector3 direction = aSideTriangle.v1.position - planePoint;
                    //direction.Normalize();

                    //Vector3 originalVert = inputVertices[oneVert];


                    //Debug.DrawLine(planePoint, planePoint + (direction * 1.5f), Color.yellow, 1000.0f, false);
                    //Debug.DrawLine(originalVert, originalVert + (originalNormal * 1.5f), Color.red, 1000.0f, false);
                    ////Debug.DrawLine(Vector3.zero, (originalNormal * 5.0f), Color.red, 1000.0f, false);

                    //Debug.Log("pos: " + direction + " | " + originalNormal);

                    //float dot = Vector3.Dot(planePoint.normalized, aSideTriangle.v1.position.normalized);

                    //Vector3 planePoint2 = plane.ClosestPointOnPlane(bSideTriangle1.v1.position.normalized);
                    //float dot2 = Vector3.Dot(planePoint2.normalized, bSideTriangle1.v1.position.normalized);

                    //Vector3 planePoint3 = plane.ClosestPointOnPlane(bSideTriangle2.v1.position.normalized);
                    //float dot3 = Vector3.Dot(planePoint3.normalized, bSideTriangle2.v1.position.normalized);



                    //float angle = Mathf.Acos(dot / (planePoint.normalized.magnitude * aSideTriangle.v1.position.normalized.magnitude)); // arccos(dot(A, B) / (| A | * | B |)
                    //float angle2 = Mathf.Acos(dot2 / (planePoint2.normalized.magnitude * bSideTriangle1.v1.position.normalized.magnitude)); // arccos(dot(A, B) / (| A | * | B |)
                    //float angle3 = Mathf.Acos(dot3 / (planePoint3.normalized.magnitude * bSideTriangle2.v1.position.normalized.magnitude)); // arccos(dot(A, B) / (| A | * | B |)

                    // Debug.Log("pos: " + angle + ", " + angle2 + ", " + angle3);

                    //Debug.Log(MathUtility.IsTriangleOrientedClockwise3(aSideTriangle.v1.position, aSideTriangle.v2.position, aSideTriangle.v3.position));
                    //Debug.Log(MathUtility.IsTriangleOrientedClockwise3(aSideTriangle));





                    positive.Add(aSideTriangle);
                    negative.Add(bSideTriangle1);
                    negative.Add(bSideTriangle2);





                }
                else
                {

                    //Vector3 planePoint = plane.ClosestPointOnPlane(aSideTriangle.v1.position);

                    //Vector3 direction = planePoint - aSideTriangle.v1.position;
                    //direction.Normalize();

                   // Debug.Log("neg: " + direction);

                    positive.Add(bSideTriangle1);
                    positive.Add(bSideTriangle2);
                    //aSideTriangle.ChangeOrientation();
                    negative.Add(aSideTriangle);
                }

            }
            else
            {
                //Debug.Log(i);


                Vector3 uneditedV1 = inputVertices[inputTriangles[i]];
                Vector3 uneditedV2 = inputVertices[inputTriangles[i + 1]];
                Vector3 uneditedV3 = inputVertices[inputTriangles[i + 2]];

                Vector3 uneditedNormal = inputNormals[inputTriangles[i]];
                Triangle uneditedTriangle = new Triangle(uneditedV1, uneditedV2, uneditedV3, uneditedNormal);

                //Debug.DrawLine(uneditedV1, uneditedV1 + (uneditedNormal * 1.5f), Color.yellow, 1000.0f, false);

                Vector3 fillV1 = plane.ClosestPointOnPlane(uneditedV1);
                Vector3 fillV2 = plane.ClosestPointOnPlane(uneditedV2);
                Vector3 fillV3 = plane.ClosestPointOnPlane(uneditedV3);

                Triangle fillTriangle = new Triangle(fillV1, fillV2, fillV3, uneditedNormal);
                Vector3 fillSurfaceNormal = Vector3.Cross(fillV2 - fillV1, fillV3 - fillV1).normalized;

                if (fillSurfaceNormal != uneditedNormal)
                {
                    fillTriangle.ChangeOrientation();
                }


                //Debug.DrawLine(fillV1, fillV1 + (fillSurfaceNormal * 1.5f), Color.yellow, 1000.0f, false);



                if (sides[inputTriangles[i]])
                {
                    //positive.Add(new Triangle(inputVertices[inputTriangles[i]], inputVertices[inputTriangles[i + 1]], inputVertices[inputTriangles[i + 2]], inputNormals[inputTriangles[i]]));
                    positive.Add(uneditedTriangle);


                    // create a copy of these and move them to the 'cut point', add to negative




                    //Triangle t = new Triangle(v1pre, v2pre, v3pre);
                    //negative.Add(fillTriangle);

                }
                else
                {
                    //negative.Add(new Triangle(inputVertices[inputTriangles[i]], inputVertices[inputTriangles[i + 1]], inputVertices[inputTriangles[i + 2]]));
                    negative.Add(uneditedTriangle);


                    // move triangle to cut point

                    //Vector3 v1pre = inputVertices[inputTriangles[i]];
                    //Vector3 v2pre = inputVertices[inputTriangles[i + 1]];
                    //Vector3 v3pre = inputVertices[inputTriangles[i + 2]];

                    //Vector3 v1 = plane.ClosestPointOnPlane(inputVertices[inputTriangles[i]]);
                    //Vector3 v2 = plane.ClosestPointOnPlane(inputVertices[inputTriangles[i + 1]]);
                    //Vector3 v3 = plane.ClosestPointOnPlane(inputVertices[inputTriangles[i + 2]]);
                    //Triangle t = new Triangle(v1, v2, v3, inputNormals[inputTriangles[i]]);
                    //Triangle t = new Triangle(v1pre, v2pre, v3pre);
                    //positive.Add(fillTriangle);


                    //Instantiate(Resources.Load("YellowCube", typeof(GameObject)), v1, Quaternion.identity);
                    //Instantiate(Resources.Load("YellowCube", typeof(GameObject)), v2, Quaternion.identity);
                    //Instantiate(Resources.Load("YellowCube", typeof(GameObject)), v3, Quaternion.identity);
                    //Instantiate(Resources.Load("PinkCube", typeof(GameObject)), v1pre, Quaternion.identity);
                    //Instantiate(Resources.Load("PinkCube", typeof(GameObject)), v2pre, Quaternion.identity);
                    //Instantiate(Resources.Load("PinkCube", typeof(GameObject)), v3pre, Quaternion.identity);


                    //Debug.Log("neg");
                }
            }
        }


        //int p = 0;

        //return new Mesh[] { positive, negative };

        Mesh positiveMesh = BuildingUtility.TrianglesToMesh(positive);
        Mesh negativeMesh = BuildingUtility.TrianglesToMesh(negative);

        //positiveMesh.RecalculateNormals();
       // negativeMesh.RecalculateNormals();

        return new List<Mesh> { positiveMesh, negativeMesh };
        //return new List<Mesh> { negativeMesh };
        //return new List<Mesh> { positiveMesh };
    }
}
