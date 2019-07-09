using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using g3;
using System;

public class SplitOperation : MonoBehaviour
{
    public static Shape Split(Shape shape, Vector3 planePos, Vector3 planeNormal, AxisSelector axis, bool posSide, bool flatten = false, Vector3? flattenRotation = null)
    {
        // get edge loops of cut so we can manually build meshes from them

        // create copy of original mesh
        Mesh originalMesh = shape.Mesh;
        Mesh meshCopy = new Mesh();
        meshCopy.vertices = originalMesh.vertices;
        meshCopy.normals = null;
        meshCopy.triangles = originalMesh.triangles;
        meshCopy.uv = null;

        // weld verts so we get closed edge loops instead of open edge spans
        MeshWelder mw = new MeshWelder(meshCopy);
        meshCopy = mw.Weld();

        // convert and cut the welded mesh
        DMesh3 dmesh = g3UnityUtils.UnityMeshToDMesh(meshCopy);
        MeshPlaneCut mpc = new MeshPlaneCut(dmesh, planePos, planeNormal);
        bool cutResult = mpc.Cut();

        if(!cutResult)
        {
            Debug.Log("SplitOperation: cut failed");
            return null;
        }

        // retreive the edge loops
        List<EdgeLoop> cutLoops = mpc.CutLoops;

        // retrieve the actual vertex vector3's from the edge loop indicies
        List<Vector3>[] cutLoopVertices = new List<Vector3>[cutLoops.Count];
        for (int i = 0; i < cutLoops.Count; i++)
        {
            EdgeLoop el = cutLoops[i];
            int[] verts = el.Vertices;

            cutLoopVertices[i] = new List<Vector3>();

            for (int j = 0; j < verts.Length; j++)
            {
                Vector3 vert = (Vector3)dmesh.GetVertex(verts[j]);
                cutLoopVertices[i].Add(vert);

                //if (j == verts.Length - 1)
                //{
                //    //cutLoopVertices[i].Add(cutLoopVertices[i][0]);
                //}
            }
        }

        // draw cut loops
        //List<Vector3> cutLoop0 = cutLoopVertices[0];

        //for (int i = 0; i < cutLoop0.Count - 1; i++)
        //{
        //    Debug.DrawLine(cutLoop0[i], cutLoop0[i+1], Color.yellow, 1000.0f);
        //}

        //List<Vector3> cutLoop1 = cutLoopVertices[1];

        //for (int i = 0; i < cutLoop1.Count - 1; i++)
        //{
        //    Debug.DrawLine(cutLoop1[i], cutLoop1[i+1], Color.yellow, 1000.0f);
        //}

        // ONLY X/Z AXES
        // rotate caps so they are flat, should enable for X and Z axes, disable for Y
        if ((axis == AxisSelector.X || axis == AxisSelector.Z) && flatten)
        {
            // reverse rotation for one side only
            if(posSide)
            {
                flattenRotation = new Vector3(-flattenRotation.Value.x, -flattenRotation.Value.y, -flattenRotation.Value.z);
            }

            for (int i = 0; i < cutLoopVertices.Length; i++)
            {
                for (int j = 0; j < cutLoopVertices[i].Count; j++)
                {
                    cutLoopVertices[i][j] = Quaternion.Euler(flattenRotation.Value.x, flattenRotation.Value.y, flattenRotation.Value.z) * cutLoopVertices[i][j];
                }
            }
        }

        // create mesh from each edge loop and add to list
        List<Mesh> meshes = new List<Mesh>();

        for (int i = 0; i < cutLoopVertices.Length; i++)
        {
            // Only Y axis
            if (axis == AxisSelector.Y && !posSide)
            {
                cutLoopVertices[i].Reverse();
                //planeNormal = new Vector3(-planeNormal.x, -planeNormal.y, -planeNormal.z);
            }


            List<Triangle> capTriangles = Triangulator.TriangulatePolygon(cutLoopVertices[i], true);

            // Only Y axis on negative side
            // flip cap triangles so they face outwards
            if (axis == AxisSelector.Y && !posSide)
            {
                for (int j = 0; j < capTriangles.Count; j++)
                {
                    capTriangles[j].ChangeOrientation();
                }
            }

            Mesh capMesh = BuildingUtility.TrianglesToMesh(capTriangles, true);

            // manually set normals in the same direction as the cut plane normal
            Vector3[] capNormals = new Vector3[capMesh.vertexCount];
            for (int j = 0; j < capMesh.vertexCount; j++)
            {
                capNormals[j] = planeNormal;
            }
            capMesh.normals = capNormals;

            meshes.Add(capMesh);
        }

        // ONLY X/Z AXES
        // reverse flatten
        if ((axis == AxisSelector.X || axis == AxisSelector.Z) && flatten)
        {
            for (int i = 0; i < meshes.Count; i++)
            {
                Vector3[] verts = meshes[i].vertices;

                for (int j = 0; j < verts.Length; j++)
                {
                    verts[j] = Quaternion.Euler(-flattenRotation.Value.x, -flattenRotation.Value.y, -flattenRotation.Value.z) * verts[j];
                }

                meshes[i].vertices = verts;
            }
        }

        // cut the original non-welded mesh
        dmesh = g3UnityUtils.UnityMeshToDMesh(originalMesh);
        mpc = new MeshPlaneCut(dmesh, planePos, planeNormal);
        mpc.Cut();

        // separate to retrieve only the triangles on one side of the cut plane
        DMesh3[] parts = MeshConnectedComponents.Separate(dmesh);

        // add parts along with the caps we made earlier
        for (int i = 0; i < parts.Length; i++)
        {
            //meshes.Insert(0, g3UnityUtils.DMeshToUnityMesh(parts[i]));
            meshes.Add(g3UnityUtils.DMeshToUnityMesh(parts[i]));
        }

        // Combine the parts and recalculate transform origin
        Mesh finalMesh = BuildingUtility.SimplifyFaces(BuildingUtility.CombineMeshes(meshes));
        finalMesh.RecalculateBounds();
        LocalTransform newTransform = shape.LocalTransform;
        newTransform.Origin = finalMesh.bounds.center;

        // combine all meshes into a single mesh
        return new Shape(finalMesh, newTransform);
    }

    public static Shape SplitAxis(Shape shape, Vector3 planePos, Vector3 planeNormal)
    {
        // get edge loops of cut so we can manually build meshes from them

        // create copy of original mesh
        Mesh originalMesh = shape.Mesh;
        Mesh meshCopy = new Mesh();
        meshCopy.vertices = originalMesh.vertices;
        meshCopy.normals = null;
        meshCopy.triangles = originalMesh.triangles;
        meshCopy.uv = null;

        // weld verts so we get closed edge loops instead of open edge spans
        MeshWelder mw = new MeshWelder(meshCopy);
        meshCopy = mw.Weld();

        // convert and cut the welded mesh
        DMesh3 dmesh = g3UnityUtils.UnityMeshToDMesh(meshCopy);
        MeshPlaneCut mpc = new MeshPlaneCut(dmesh, planePos, planeNormal);
        bool cutResult = mpc.Cut();

      
        if(!cutResult)
        {
            Debug.Log("SplitOperation: cut failed");
            return null;
        }

        // retreive the edge loops
        List<EdgeLoop> cutLoops = mpc.CutLoops;

        // retrieve the actual vertex vector3's from the edge loop indicies
        List<Vector3>[] cutLoopVertices = new List<Vector3>[cutLoops.Count];
        for (int i = 0; i < cutLoops.Count; i++)
        {
            EdgeLoop el = cutLoops[i];
            int[] verts = el.Vertices;

            cutLoopVertices[i] = new List<Vector3>();

            for (int j = 0; j < verts.Length; j++)
            {
                Vector3 vert = (Vector3)dmesh.GetVertex(verts[j]);
                cutLoopVertices[i].Add(vert);

                //if (j == verts.Length - 1)
                //{
                //    //cutLoopVertices[i].Add(cutLoopVertices[i][0]);
                //}
            }
        }

        // draw cut loops
        //List<Vector3> cutLoop0 = cutLoopVertices[0];

        //for (int i = 0; i < cutLoop0.Count - 1; i++)
        //{
        //    Debug.DrawLine(cutLoop0[i], cutLoop0[i+1], Color.yellow, 1000.0f);
        //}

        //List<Vector3> cutLoop1 = cutLoopVertices[1];

        //for (int i = 0; i < cutLoop1.Count - 1; i++)
        //{
        //    Debug.DrawLine(cutLoop1[i], cutLoop1[i+1], Color.yellow, 1000.0f);
        //}

        // ONLY X/Z AXES
        // rotate caps so they are flat, should enable for X and Z axes, disable for Y

        bool flattened = false;
        Quaternion rotation = Quaternion.identity;

        if (planeNormal != Vector3.up)
        {
            rotation = Quaternion.FromToRotation(planeNormal, Vector3.up);

            for (int i = 0; i < cutLoopVertices.Length; i++)
            {
                for (int j = 0; j < cutLoopVertices[i].Count; j++)
                {
                    cutLoopVertices[i][j] = rotation * cutLoopVertices[i][j];
                }
            }

            flattened = true;
        }

        // create mesh from each edge loop and add to list
        List<Mesh> meshes = new List<Mesh>();

        for (int i = 0; i < cutLoopVertices.Length; i++)
        {
            //// Only Y axis
            //if (axis == AxisSelector.Y && !posSide)
            //{
            //    cutLoopVertices[i].Reverse();
            //    //planeNormal = new Vector3(-planeNormal.x, -planeNormal.y, -planeNormal.z);
            //}


            List<Triangle> capTriangles = Triangulator.TriangulatePolygon(cutLoopVertices[i], true);

            // Only Y axis on negative side
            // flip cap triangles so they face outwards
            //if (axis == AxisSelector.Y && !posSide)
            //{
            //    for (int j = 0; j < capTriangles.Count; j++)
            //    {
            //        capTriangles[j].ChangeOrientation();
            //    }
            //}

            Mesh capMesh = BuildingUtility.TrianglesToMesh(capTriangles, true);

            // manually set normals in the same direction as the cut plane normal
            Vector3[] capNormals = new Vector3[capMesh.vertexCount];
            for (int j = 0; j < capMesh.vertexCount; j++)
            {
                capNormals[j] = planeNormal;
            }
            capMesh.normals = capNormals;

            meshes.Add(capMesh);
        }

        // reverse flatten
        if (flattened)
        {
            for (int i = 0; i < meshes.Count; i++)
            {
                Vector3[] verts = meshes[i].vertices;

                for (int j = 0; j < verts.Length; j++)
                {
                    verts[j] = Quaternion.Inverse(rotation) * verts[j];
                }

                meshes[i].vertices = verts;
            }
        }

        // cut the original non-welded mesh
        dmesh = g3UnityUtils.UnityMeshToDMesh(originalMesh);
        mpc = new MeshPlaneCut(dmesh, planePos, planeNormal);
        mpc.Cut();

        // separate to retrieve only the triangles on one side of the cut plane
        DMesh3[] parts = MeshConnectedComponents.Separate(dmesh);

        // add parts along with the caps we made earlier
        for (int i = 0; i < parts.Length; i++)
        {
            //meshes.Insert(0, g3UnityUtils.DMeshToUnityMesh(parts[i]));
            meshes.Add(g3UnityUtils.DMeshToUnityMesh(parts[i]));
        }

        // Combine the parts and recalculate transform origin
        Mesh finalMesh = BuildingUtility.SimplifyFaces(BuildingUtility.CombineMeshes(meshes));
        finalMesh.RecalculateBounds();
        LocalTransform newTransform = shape.LocalTransform;
        newTransform.Origin = finalMesh.bounds.center;

        // combine all meshes into a single mesh
        return new Shape(finalMesh, newTransform);
    }
  
}
