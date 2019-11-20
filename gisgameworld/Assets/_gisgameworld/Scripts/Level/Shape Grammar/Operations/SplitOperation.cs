using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using g3;
using System;

public struct SplitTerm
{
    public bool isRepeat;
    public List<SplitRatio> terms;

    public SplitTerm(bool isRepeat, List<SplitRatio> terms)
    {
        this.isRepeat = isRepeat;
        this.terms = terms;
    }
}

public struct SplitRatio
{
    public bool isFloating;
    public float ratio;
    public string shapeName;

    public SplitRatio(bool isFloating, float ratio, string shapeName)
    {
        this.isFloating = isFloating;
        this.ratio = ratio;
        this.shapeName = shapeName;
    }
}


public class SplitOperation : IShapeGrammarOperation
{
    private Axis axis;
    private List<SplitTerm> terms;

    public SplitOperation(Axis axis, List<SplitTerm> terms)
    {
        this.axis = axis;
        this.terms = terms;
    }


    // plane normal should be one of the shapes orientation vectors
    public Dictionary<string, List<Shape>> SplitAxisTerms(Shape shape, Vector3 planeNormal, List<SplitTerm> terms, bool debugDraw = false)
    {
        LocalTransform lt = shape.LocalTransform;
        List<Vector3> orientationVectors = new List<Vector3>() { lt.Up, lt.Right, lt.Forward };

        Vector3 refVector1 = Vector3.zero;
        Vector3 refVector2 = Vector3.zero;

        for (int i = 0; i < orientationVectors.Count; i++)
        {
            if (orientationVectors[i] == planeNormal)
            {
                refVector2 = orientationVectors[i];
                refVector1 = orientationVectors[MathUtility.ClampListIndex(i + 1, orientationVectors.Count)];
            }
        }

        Mesh shapeMesh = shape.Mesh;

        Quaternion upRotation = Quaternion.identity;
        Quaternion rightRotation = Quaternion.identity;
        Vector3[] vertices = shapeMesh.vertices;

        bool flattenedUp = false;
        bool flattenedRight = false;

        if (refVector1 != Vector3.up)
        {
            upRotation = Quaternion.FromToRotation(refVector1, Vector3.up);

            for (int k = 0; k < vertices.Length; k++)
            {
                vertices[k] = upRotation * vertices[k];
            }
            shapeMesh.vertices = vertices;
            flattenedUp = true;

            refVector2 = upRotation * refVector2;
        }

        //Vector3 testVector = upRotation * refVector1;
        //Debug.DrawLine(lt.Origin, lt.Origin + (refVector1 * 25.0f), Color.magenta, 1000.0f);
        //Debug.DrawLine(lt.Origin, lt.Origin + (testVector * 25.0f), Color.yellow, 1000.0f);

        if (refVector2 != Vector3.right)
        {
            rightRotation = Quaternion.FromToRotation(refVector2, Vector3.right);

            for (int k = 0; k < vertices.Length; k++)
            {
                vertices[k] = rightRotation * vertices[k];
            }
            shapeMesh.vertices = vertices;
            flattenedRight = true;
        }

        shapeMesh.RecalculateNormals();

        shapeMesh.RecalculateBounds();
        Vector3 newOrigin = shapeMesh.bounds.center;
        Vector3 min = shapeMesh.bounds.min;
        Vector3 max = shapeMesh.bounds.max;
        Vector3 size = shapeMesh.bounds.size;

        //float divisionSize = size.x / (float)divisions;

        List<Shape> allParts = new List<Shape>();

        Shape currentShape = new Shape(shapeMesh, lt);

        Vector3 cutNormal = Vector3.right;

        Tuple<List<float>, List<string>> sizesAndNames = DetermineTermSizesAndNames(terms, size.x);

        List<float> sizes = sizesAndNames.Item1;
        List<string> names = sizesAndNames.Item2;

        float currentCutPos = 0f;

        for (int i = 0; i < sizes.Count - 1; i++)
        {
            currentCutPos += sizes[i];

            Vector3 planePos = min + (currentCutPos * cutNormal);

            List<Shape> parts = new List<Shape>();
            parts.Add(SplitAxis(currentShape, planePos, cutNormal));
            parts.Add(SplitAxis(currentShape, planePos, -cutNormal));

            if (i == sizes.Count - 2)
            {
                allParts.Add(parts[0]);
                allParts.Add(parts[1]);

            }
            else
            {
                allParts.Add(parts[0]);
                currentShape = parts[1];
            }

        }

        for (int i = 0; i < allParts.Count; i++)
        {
            // Shape currentPart = allParts[i];

            Mesh currentMesh = allParts[i].Mesh;

            Vector3[] currentPartVertices = currentMesh.vertices;

            if (flattenedRight)
            {
                rightRotation = Quaternion.FromToRotation(Vector3.right, refVector2);

                for (int k = 0; k < currentPartVertices.Length; k++)
                {
                    currentPartVertices[k] = rightRotation * currentPartVertices[k];
                }

                currentMesh.vertices = currentPartVertices;
            }

            if (flattenedUp)
            {
                upRotation = Quaternion.FromToRotation(Vector3.up, refVector1);

                for (int k = 0; k < currentPartVertices.Length; k++)
                {
                    currentPartVertices[k] = upRotation * currentPartVertices[k];
                }

                currentMesh.vertices = currentPartVertices;
            }

            currentMesh.RecalculateNormals();
            currentMesh.RecalculateBounds();

            LocalTransform newTransform = new LocalTransform();

            DMesh3 dmesh = g3UnityUtils.UnityMeshToDMesh(currentMesh);
            MeshBoundaryLoops mbl = new MeshBoundaryLoops(dmesh);
            if (mbl.Loops.Count == 1)
            {
                //Debug.Log("single face");

                EdgeLoop el = mbl.Loops[0];
                int[] edgeLoopIndices = el.Vertices;

                List<Vector3> edgeLoopVertices = new List<Vector3>();
                for (int j = 0; j < edgeLoopIndices.Length; j++)
                {
                    edgeLoopVertices.Add((Vector3)dmesh.GetVertex(edgeLoopIndices[j]));
                }

                //return BuildingUtility.FindPolygonCenter(edgeLoopVertices, lt.Up);

                newTransform = new LocalTransform(BuildingUtility.FindPolygonCenter(edgeLoopVertices, lt.Up), lt.Up, lt.Forward, lt.Right);
            }
            else
            {
                //Debug.Log("multi face");

                newTransform = new LocalTransform(currentMesh.bounds.center, lt.Up, lt.Forward, lt.Right);
            }

            //GameObject a = UnityEngine.Object.Instantiate(Resources.Load("BlueCube"), currentMesh.bounds.center, Quaternion.LookRotation(lt.Forward, lt.Up)) as GameObject;

            //LocalTransform newTransform = new LocalTransform(currentMesh.bounds.center, lt.Up, lt.Forward, lt.Right);

            allParts[i] = new Shape(currentMesh, newTransform);
        }

        Dictionary<string, List<Shape>> output = new Dictionary<string, List<Shape>>();
        for(int i = 0; i < allParts.Count; i++)
        {
            if (output.ContainsKey(names[i]))
            {
                output[names[i]].Add(allParts[i]);
            }
            else
            {
                output.Add(names[i], new List<Shape>() { allParts[i] });
            }
        }

        return output;
    }

    // plane normal should be one of the shapes orientation vectors
    public List<Shape> SplitAxisDivisions(Shape shape, Vector3 planeNormal, int divisions)
    {
        LocalTransform lt = shape.LocalTransform;
        List<Vector3> orientationVectors = new List<Vector3>() { lt.Up, lt.Right, lt.Forward };

        Vector3 refVector1 = Vector3.zero;
        Vector3 refVector2 = Vector3.zero;

        for (int i = 0; i < orientationVectors.Count; i++)
        {
            if (orientationVectors[i] == planeNormal)
            {
                refVector2 = orientationVectors[i];
                refVector1 = orientationVectors[MathUtility.ClampListIndex(i + 1, orientationVectors.Count)];
            }
        }

        Mesh shapeMesh = shape.Mesh;

        Quaternion upRotation = Quaternion.identity;
        Quaternion rightRotation = Quaternion.identity;
        Vector3[] vertices = shapeMesh.vertices;

        bool flattenedUp = false;
        bool flattenedRight = false;

        if (refVector1 != Vector3.up)
        {
            upRotation = Quaternion.FromToRotation(refVector1, Vector3.up);

            for (int k = 0; k < vertices.Length; k++)
            {
                vertices[k] = upRotation * vertices[k];
            }
            shapeMesh.vertices = vertices;
            flattenedUp = true;

            refVector2 = upRotation * refVector2;
        }

        //Vector3 testVector = upRotation * refVector1;
        //Debug.DrawLine(lt.Origin, lt.Origin + (refVector1 * 25.0f), Color.magenta, 1000.0f);
        //Debug.DrawLine(lt.Origin, lt.Origin + (testVector * 25.0f), Color.yellow, 1000.0f);

        if (refVector2 != Vector3.right)
        {
            rightRotation = Quaternion.FromToRotation(refVector2, Vector3.right);

            for (int k = 0; k < vertices.Length; k++)
            {
                vertices[k] = rightRotation * vertices[k];
            }
            shapeMesh.vertices = vertices;
            flattenedRight = true;
        }

        shapeMesh.RecalculateNormals();

        shapeMesh.RecalculateBounds();
        Vector3 newOrigin = shapeMesh.bounds.center;
        Vector3 min = shapeMesh.bounds.min;
        Vector3 max = shapeMesh.bounds.max;
        Vector3 size = shapeMesh.bounds.size;

        float divisionSize = size.x / (float)divisions;

        List<Shape> allParts = new List<Shape>();
        Shape currentShape = new Shape(shapeMesh, lt);

        Vector3 cutNormal = Vector3.right;

        for (int j = 0; j < divisions - 1; j++)
        {
            Vector3 planePos = min + ((divisionSize * (j + 1)) * cutNormal);

            List<Shape> parts = new List<Shape>();
            parts.Add(SplitAxis(currentShape, planePos, cutNormal));
            parts.Add(SplitAxis(currentShape, planePos, -cutNormal));

            if (j == divisions - 2)
            {
                allParts.Add(parts[0]);
                allParts.Add(parts[1]);
            }
            else
            {
                allParts.Add(parts[0]);
                currentShape = parts[1];
            }
        }

        for (int i = 0; i < allParts.Count; i++)
        {
            // Shape currentPart = allParts[i];

            Mesh currentMesh = allParts[i].Mesh;

            Vector3[] currentPartVertices = currentMesh.vertices;

            if (flattenedRight)
            {
                rightRotation = Quaternion.FromToRotation(Vector3.right, refVector2);

                for (int k = 0; k < currentPartVertices.Length; k++)
                {
                    currentPartVertices[k] = rightRotation * currentPartVertices[k];
                }

                currentMesh.vertices = currentPartVertices;
            }

            if (flattenedUp)
            {
                upRotation = Quaternion.FromToRotation(Vector3.up, refVector1);

                for (int k = 0; k < currentPartVertices.Length; k++)
                {
                    currentPartVertices[k] = upRotation * currentPartVertices[k];
                }

                currentMesh.vertices = currentPartVertices;
            }

            currentMesh.RecalculateNormals();
            currentMesh.RecalculateBounds();

            LocalTransform newTransform = new LocalTransform();

            DMesh3 dmesh = g3UnityUtils.UnityMeshToDMesh(currentMesh);
            MeshBoundaryLoops mbl = new MeshBoundaryLoops(dmesh);
            if (mbl.Loops.Count == 1)
            {
                //Debug.Log("single face");

                EdgeLoop el = mbl.Loops[0];
                int[] edgeLoopIndices = el.Vertices;

                List<Vector3> edgeLoopVertices = new List<Vector3>();
                for (int j = 0; j < edgeLoopIndices.Length; j++)
                {
                    edgeLoopVertices.Add((Vector3)dmesh.GetVertex(edgeLoopIndices[j]));
                }

                //return BuildingUtility.FindPolygonCenter(edgeLoopVertices, lt.Up);

                newTransform = new LocalTransform(BuildingUtility.FindPolygonCenter(edgeLoopVertices, lt.Up), lt.Up, lt.Forward, lt.Right);
            }
            else
            {
                //Debug.Log("multi face");

                newTransform = new LocalTransform(currentMesh.bounds.center, lt.Up, lt.Forward, lt.Right);
            }

            //GameObject a = UnityEngine.Object.Instantiate(Resources.Load("BlueCube"), currentMesh.bounds.center, Quaternion.LookRotation(lt.Forward, lt.Up)) as GameObject;

            //LocalTransform newTransform = new LocalTransform(currentMesh.bounds.center, lt.Up, lt.Forward, lt.Right);

            allParts[i] = new Shape(currentMesh, newTransform);
        }

        return allParts;
    }

    // plane normal should be one of the shapes orientation vectors
    public List<Shape> SplitAxisRatio(Shape shape, Vector3 planeNormal, float ratio)
    {
        LocalTransform lt = shape.LocalTransform;
        List<Vector3> orientationVectors = new List<Vector3>() { lt.Up, lt.Right, lt.Forward };

        Vector3 refVector1 = Vector3.zero;
        Vector3 refVector2 = Vector3.zero;

        for (int i = 0; i < orientationVectors.Count; i++)
        {
            if (orientationVectors[i] == planeNormal)
            {
                refVector2 = orientationVectors[i];
                refVector1 = orientationVectors[MathUtility.ClampListIndex(i + 1, orientationVectors.Count)];
            }
        }

        Mesh shapeMesh = shape.Mesh;

        Quaternion upRotation = Quaternion.identity;
        Quaternion rightRotation = Quaternion.identity;
        Vector3[] vertices = shapeMesh.vertices;

        bool flattenedUp = false;
        bool flattenedRight = false;

        if (refVector1 != Vector3.up)
        {
            upRotation = Quaternion.FromToRotation(refVector1, Vector3.up);

            for (int k = 0; k < vertices.Length; k++)
            {
                vertices[k] = upRotation * vertices[k];
            }
            shapeMesh.vertices = vertices;
            flattenedUp = true;

            refVector2 = upRotation * refVector2;
        }

        //Vector3 testVector = upRotation * refVector1;
        //Debug.DrawLine(lt.Origin, lt.Origin + (refVector1 * 25.0f), Color.magenta, 1000.0f);
        //Debug.DrawLine(lt.Origin, lt.Origin + (testVector * 25.0f), Color.yellow, 1000.0f);

        if (refVector2 != Vector3.right)
        {
            rightRotation = Quaternion.FromToRotation(refVector2, Vector3.right);

            for (int k = 0; k < vertices.Length; k++)
            {
                vertices[k] = rightRotation * vertices[k];
            }
            shapeMesh.vertices = vertices;
            flattenedRight = true;
        }

        shapeMesh.RecalculateNormals();

        shapeMesh.RecalculateBounds();
        Vector3 newOrigin = shapeMesh.bounds.center;
        Vector3 min = shapeMesh.bounds.min;
        Vector3 max = shapeMesh.bounds.max;
        Vector3 size = shapeMesh.bounds.size;

        //float divisionSize = size.x / (float)divisions;

        List<Shape> allParts = new List<Shape>();
        Shape currentShape = new Shape(shapeMesh, lt);

        Vector3 cutNormal = Vector3.right;


        Vector3 planePos = min + ((size.x * ratio) * cutNormal);

        List<Shape> parts = new List<Shape>();
        parts.Add(SplitAxis(currentShape, planePos, cutNormal));
        parts.Add(SplitAxis(currentShape, planePos, -cutNormal));


        allParts.Add(parts[0]);
        allParts.Add(parts[1]);



        for (int i = 0; i < allParts.Count; i++)
        {
            // Shape currentPart = allParts[i];

            Mesh currentMesh = allParts[i].Mesh;

            Vector3[] currentPartVertices = currentMesh.vertices;

            if (flattenedRight)
            {
                rightRotation = Quaternion.FromToRotation(Vector3.right, refVector2);

                for (int k = 0; k < currentPartVertices.Length; k++)
                {
                    currentPartVertices[k] = rightRotation * currentPartVertices[k];
                }

                currentMesh.vertices = currentPartVertices;
            }

            if (flattenedUp)
            {
                upRotation = Quaternion.FromToRotation(Vector3.up, refVector1);

                for (int k = 0; k < currentPartVertices.Length; k++)
                {
                    currentPartVertices[k] = upRotation * currentPartVertices[k];
                }

                currentMesh.vertices = currentPartVertices;
            }

            currentMesh.RecalculateNormals();
            currentMesh.RecalculateBounds();

            LocalTransform newTransform = new LocalTransform();

            DMesh3 dmesh = g3UnityUtils.UnityMeshToDMesh(currentMesh);
            MeshBoundaryLoops mbl = new MeshBoundaryLoops(dmesh);
            if (mbl.Loops.Count == 1)
            {
                //Debug.Log("single face");

                EdgeLoop el = mbl.Loops[0];
                int[] edgeLoopIndices = el.Vertices;

                List<Vector3> edgeLoopVertices = new List<Vector3>();
                for (int j = 0; j < edgeLoopIndices.Length; j++)
                {
                    edgeLoopVertices.Add((Vector3)dmesh.GetVertex(edgeLoopIndices[j]));
                }

                //return BuildingUtility.FindPolygonCenter(edgeLoopVertices, lt.Up);

                newTransform = new LocalTransform(BuildingUtility.FindPolygonCenter(edgeLoopVertices, lt.Up), lt.Up, lt.Forward, lt.Right);
            }
            else
            {
                //Debug.Log("multi face");

                newTransform = new LocalTransform(currentMesh.bounds.center, lt.Up, lt.Forward, lt.Right);
            }

            //GameObject a = UnityEngine.Object.Instantiate(Resources.Load("BlueCube"), currentMesh.bounds.center, Quaternion.LookRotation(lt.Forward, lt.Up)) as GameObject;

            //LocalTransform newTransform = new LocalTransform(currentMesh.bounds.center, lt.Up, lt.Forward, lt.Right);

            allParts[i] = new Shape(currentMesh, newTransform);
        }

        return allParts;
    }


    //public static Shape Split(Shape shape, Vector3 planePos, Vector3 planeNormal, AxisSelector axis, bool posSide, bool flatten = false, Vector3? flattenRotation = null)
    //{
    //    // get edge loops of cut so we can manually build meshes from them

    //    // create copy of original mesh
    //    Mesh originalMesh = shape.Mesh;
    //    Mesh meshCopy = new Mesh();
    //    meshCopy.vertices = originalMesh.vertices;
    //    meshCopy.normals = null;
    //    meshCopy.triangles = originalMesh.triangles;
    //    meshCopy.uv = null;

    //    // weld verts so we get closed edge loops instead of open edge spans
    //    MeshWelder mw = new MeshWelder(meshCopy);
    //    meshCopy = mw.Weld();

    //    // convert and cut the welded mesh
    //    DMesh3 dmesh = g3UnityUtils.UnityMeshToDMesh(meshCopy);
    //    MeshPlaneCut mpc = new MeshPlaneCut(dmesh, planePos, planeNormal);
    //    bool cutResult = mpc.Cut();

    //    if(!cutResult)
    //    {
    //        Debug.Log("SplitOperation: cut failed");
    //        return null;
    //    }

    //    // retreive the edge loops
    //    List<EdgeLoop> cutLoops = mpc.CutLoops;

    //    // retrieve the actual vertex vector3's from the edge loop indicies
    //    List<Vector3>[] cutLoopVertices = new List<Vector3>[cutLoops.Count];
    //    for (int i = 0; i < cutLoops.Count; i++)
    //    {
    //        EdgeLoop el = cutLoops[i];
    //        int[] verts = el.Vertices;

    //        cutLoopVertices[i] = new List<Vector3>();

    //        for (int j = 0; j < verts.Length; j++)
    //        {
    //            Vector3 vert = (Vector3)dmesh.GetVertex(verts[j]);
    //            cutLoopVertices[i].Add(vert);

    //            //if (j == verts.Length - 1)
    //            //{
    //            //    //cutLoopVertices[i].Add(cutLoopVertices[i][0]);
    //            //}
    //        }
    //    }

    //    // draw cut loops
    //    //List<Vector3> cutLoop0 = cutLoopVertices[0];

    //    //for (int i = 0; i < cutLoop0.Count - 1; i++)
    //    //{
    //    //    Debug.DrawLine(cutLoop0[i], cutLoop0[i+1], Color.yellow, 1000.0f);
    //    //}

    //    //List<Vector3> cutLoop1 = cutLoopVertices[1];

    //    //for (int i = 0; i < cutLoop1.Count - 1; i++)
    //    //{
    //    //    Debug.DrawLine(cutLoop1[i], cutLoop1[i+1], Color.yellow, 1000.0f);
    //    //}

    //    // ONLY X/Z AXES
    //    // rotate caps so they are flat, should enable for X and Z axes, disable for Y
    //    if ((axis == AxisSelector.X || axis == AxisSelector.Z) && flatten)
    //    {
    //        // reverse rotation for one side only
    //        if(posSide)
    //        {
    //            flattenRotation = new Vector3(-flattenRotation.Value.x, -flattenRotation.Value.y, -flattenRotation.Value.z);
    //        }

    //        for (int i = 0; i < cutLoopVertices.Length; i++)
    //        {
    //            for (int j = 0; j < cutLoopVertices[i].Count; j++)
    //            {
    //                cutLoopVertices[i][j] = Quaternion.Euler(flattenRotation.Value.x, flattenRotation.Value.y, flattenRotation.Value.z) * cutLoopVertices[i][j];
    //            }
    //        }
    //    }

    //    // create mesh from each edge loop and add to list
    //    List<Mesh> meshes = new List<Mesh>();

    //    for (int i = 0; i < cutLoopVertices.Length; i++)
    //    {
    //        // Only Y axis
    //        if (axis == AxisSelector.Y && !posSide)
    //        {
    //            cutLoopVertices[i].Reverse();
    //            //planeNormal = new Vector3(-planeNormal.x, -planeNormal.y, -planeNormal.z);
    //        }


    //        List<Triangle> capTriangles = Triangulator.TriangulatePolygon(cutLoopVertices[i], true);

    //        // Only Y axis on negative side
    //        // flip cap triangles so they face outwards
    //        if (axis == AxisSelector.Y && !posSide)
    //        {
    //            for (int j = 0; j < capTriangles.Count; j++)
    //            {
    //                capTriangles[j].ChangeOrientation();
    //            }
    //        }

    //        Mesh capMesh = BuildingUtility.TrianglesToMesh(capTriangles, true);

    //        // manually set normals in the same direction as the cut plane normal
    //        Vector3[] capNormals = new Vector3[capMesh.vertexCount];
    //        for (int j = 0; j < capMesh.vertexCount; j++)
    //        {
    //            capNormals[j] = planeNormal;
    //        }
    //        capMesh.normals = capNormals;

    //        meshes.Add(capMesh);
    //    }

    //    // ONLY X/Z AXES
    //    // reverse flatten
    //    if ((axis == AxisSelector.X || axis == AxisSelector.Z) && flatten)
    //    {
    //        for (int i = 0; i < meshes.Count; i++)
    //        {
    //            Vector3[] verts = meshes[i].vertices;

    //            for (int j = 0; j < verts.Length; j++)
    //            {
    //                verts[j] = Quaternion.Euler(-flattenRotation.Value.x, -flattenRotation.Value.y, -flattenRotation.Value.z) * verts[j];
    //            }

    //            meshes[i].vertices = verts;
    //        }
    //    }

    //    // cut the original non-welded mesh
    //    dmesh = g3UnityUtils.UnityMeshToDMesh(originalMesh);
    //    mpc = new MeshPlaneCut(dmesh, planePos, planeNormal);
    //    mpc.Cut();

    //    // separate to retrieve only the triangles on one side of the cut plane
    //    DMesh3[] parts = MeshConnectedComponents.Separate(dmesh);

    //    // add parts along with the caps we made earlier
    //    for (int i = 0; i < parts.Length; i++)
    //    {
    //        //meshes.Insert(0, g3UnityUtils.DMeshToUnityMesh(parts[i]));
    //        meshes.Add(g3UnityUtils.DMeshToUnityMesh(parts[i]));
    //    }

    //    // Combine the parts and recalculate transform origin
    //    Mesh finalMesh = BuildingUtility.SimplifyFaces(BuildingUtility.CombineMeshes(meshes));
    //    finalMesh.RecalculateBounds();
    //    LocalTransform newTransform = shape.LocalTransform;
    //    newTransform.Origin = finalMesh.bounds.center;

    //    // combine all meshes into a single mesh
    //    return new Shape(finalMesh, newTransform);
    //}

    public Shape SplitAxis(Shape shape, Vector3 planePos, Vector3 planeNormal)
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

        if (!cutResult)
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

        //for(int j = 0; j < cutLoopVertices.Length; j++)
        //{
        //    List<Vector3> cutLoop0 = cutLoopVertices[j];

        //    for (int i = 0; i < cutLoop0.Count - 1; i++)
        //    {
        //        Debug.DrawLine(cutLoop0[i], cutLoop0[i + 1], Color.yellow, 1000.0f);
        //    }
        //}

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


        //for (int j = 0; j < cutLoopVertices.Length; j++)
        //{
        //    List<Vector3> cutLoop0 = cutLoopVertices[j];

        //    for (int i = 0; i < cutLoop0.Count - 1; i++)
        //    {
        //        Debug.DrawLine(cutLoop0[i], cutLoop0[i + 1], Color.red, 1000.0f);
        //    }
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


            //for (int j = 0; j < cutLoopVertices.Length; j++)
            //{
            //    List<Vector3> cutLoop0 = cutLoopVertices[i];

            //    for (int j = 0; j < cutLoop0.Count - 1; j++)
            //    {
            //        Debug.DrawLine(cutLoop0[j], cutLoop0[j + 1], Color.yellow, 1000.0f);
            //    }
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
            rotation = Quaternion.FromToRotation(Vector3.up, planeNormal);

            for (int i = 0; i < meshes.Count; i++)
            {
                Vector3[] verts = meshes[i].vertices;

                for (int j = 0; j < verts.Length; j++)
                {
                    verts[j] = rotation * verts[j];
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
        //for (int i = 0; i < 2; i++)
        {
            //meshes.Insert(0, g3UnityUtils.DMeshToUnityMesh(parts[i]));
            meshes.Add(g3UnityUtils.DMeshToUnityMesh(parts[i]));
        }

        // Combine the parts and recalculate transform origin
        Mesh finalMesh = BuildingUtility.SimplifyFaces2(BuildingUtility.CombineMeshes(meshes));
        //Mesh finalMesh = BuildingUtility.SimplifyFaces(BuildingUtility.CombineMeshes(meshes));
        //Mesh finalMesh = BuildingUtility.CombineMeshes(meshes);
        finalMesh.RecalculateBounds();
        LocalTransform newTransform = shape.LocalTransform;
        newTransform.Origin = finalMesh.bounds.center;

        // combine all meshes into a single mesh
        return new Shape(finalMesh, newTransform);
    }

    public Tuple<List<float>, List<string>> DetermineTermSizesAndNames(List<SplitTerm> terms, float size)
    {
        List<float> sizes = new List<float>();
        List<string> names = new List<string>();

        float currentSize = size;
        //float termSize = size;

        Dictionary<int, SplitTerm> repeats = new Dictionary<int, SplitTerm>();

        for (int i = 0; i < terms.Count; i++)
        {
            SplitTerm splitTerm = terms[i];

            if (splitTerm.isRepeat)
            {
                repeats.Add(i, splitTerm);
            }
            else
            {
                foreach (SplitRatio ratio in splitTerm.terms)
                {
                    float sizeToAdd = ratio.ratio * size;
                    currentSize -= sizeToAdd;

                    sizes.Add(sizeToAdd);
                    names.Add(ratio.shapeName);
                }

            }

        }

        foreach (KeyValuePair<int, SplitTerm> repeatingTerms in repeats)
        {
            List<float> sizesToInsert = new List<float>();
            List<string> namesToInsert = new List<string>();

            SplitTerm repeatingTerm = repeatingTerms.Value;

            int rhythmCount = 0;
            float currentTermTotal = 0f;

            while (currentTermTotal < currentSize)
            {
                foreach (SplitRatio term in repeatingTerm.terms)
                {
                    currentTermTotal += term.ratio * size;
                }

                if (currentTermTotal <= currentSize)
                {
                    rhythmCount++;
                }
            }

            float currentSizeOfAllReltiveRatios = 0f;

            foreach (SplitRatio term in repeatingTerm.terms)
            {
                if (!term.isFloating)
                {
                    currentSizeOfAllReltiveRatios += term.ratio * size;
                }
            }


            float remainingFloatSize = (currentSize - (currentSizeOfAllReltiveRatios * rhythmCount)) / rhythmCount;

            for (int j = 0; j < rhythmCount; j++)
            {
                for (int k = 0; k < repeatingTerm.terms.Count; k++)
                {
                    if (repeatingTerm.terms[k].isFloating)
                    {
                        sizesToInsert.Add(remainingFloatSize);
                        namesToInsert.Add(repeatingTerm.terms[k].shapeName);
                    }
                    else
                    {
                        sizesToInsert.Add(repeatingTerm.terms[k].ratio * size);
                        namesToInsert.Add(repeatingTerm.terms[k].shapeName);
                    }

                }
            }

            sizes.InsertRange(repeatingTerms.Key, sizesToInsert);
            names.InsertRange(repeatingTerms.Key, namesToInsert);
        }

        return Tuple.Create<List<float>, List<string>>(sizes, names);
    }
    //} public static List<float> DetermineTermSizes(List<SplitTerm> terms, float size)
    //{
    //    List<float> sizes = new List<float>();

    //    float currentSize = size;
    //    //float termSize = size;

    //    for (int i = 0; i < terms.Count; i++)
    //    {
    //        SplitTerm splitTerm = terms[i];

    //        if(splitTerm.isRepeat)
    //        {
    //            int rhythmCount = 0;
    //            float currentTermTotal = 0f;

    //            while (currentTermTotal < currentSize)
    //            {
    //                foreach(SplitRatio term in splitTerm.terms)
    //                {
    //                    currentTermTotal += term.ratio * size;
    //                }

    //                if (currentTermTotal <= currentSize)
    //                {
    //                    rhythmCount++;
    //                }
    //            }

    //            float currentSizeOfAllReltiveRatios = 0f;

    //            //int floatingRatioIndex = -1;

    //            //for(int j = 0; j < splitTerm.terms.Count; j++)
    //            //{
    //            //    if (!splitTerm.terms[j].isFloating)
    //            //    {
    //            //        currentSizeOfAllReltiveRatios += splitTerm.terms[j].ratio;
    //            //    }
    //            //    else
    //            //    {
    //            //        floatingRatioIndex = j;
    //            //    }
    //            //}

    //            foreach (SplitRatio term in splitTerm.terms)
    //            {
    //                if(!term.isFloating)
    //                {
    //                    currentSizeOfAllReltiveRatios += term.ratio * currentSize;
    //                }
    //            }

    //            float remainingFloatSize = (currentSize - (currentSizeOfAllReltiveRatios * rhythmCount)) / rhythmCount;

    //            for (int j = 0; j < rhythmCount; j++)
    //            {
    //                for(int k = 0; k < splitTerm.terms.Count; k++)
    //                {
    //                    if(splitTerm.terms[k].isFloating)
    //                    {
    //                        sizes.Add(remainingFloatSize);
    //                    }
    //                    else
    //                    {
    //                        sizes.Add(splitTerm.terms[k].ratio * currentSize);
    //                    }

    //                }
    //            }
    //        }
    //        else
    //        {

    //        }

    //    }



    //    return sizes;
    //}

    ShapeWrapper IShapeGrammarOperation.PerformOperation(List<Shape> input)
    {
        Dictionary<string, List<Shape>> output = new Dictionary<string, List<Shape>>();

        foreach (Shape shape in input)
        {
            Vector3 cutPlaneNormal = shape.LocalTransform.AxisToVector(axis);
            Dictionary<string, List<Shape>> current = SplitAxisTerms(shape, cutPlaneNormal, terms);

            foreach(KeyValuePair<string, List<Shape>> shapes in current)
            {
                if (output.ContainsKey(shapes.Key))
                {
                    output[shapes.Key].AddRange(shapes.Value);
                }
                else
                {
                    output.Add(shapes.Key, shapes.Value);
                }
            }
        }

        return new ShapeWrapper(output, true);
    }
}
