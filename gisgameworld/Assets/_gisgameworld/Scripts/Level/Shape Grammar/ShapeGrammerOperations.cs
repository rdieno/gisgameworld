using g3;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum AxisSelector
{
    X,
    Y,
    Z
}

public enum LocalAxisSelector
{
    Up,
    Right,
    Forward
}

public class ShapeGrammerOperations
{
    // extrudes a mesh along the Y axis by the amount specified
    //public static Mesh ExtrudeY(Mesh mesh, float amount)
    //{
    //    return Extrude(mesh, Vector3.up * amount);
    //}

    //public static Mesh Extrude(Mesh mesh, Vector3 normal)
    //{
    //    Mesh m = mesh;
    //    //m.vertices = mesh.vertices;
    //    //m.triangles = mesh.triangles;

    //    //MeshWelder mw = new MeshWelder(m);
    //    //m = mw.Weld();

    //    Vector3[] normals = new Vector3[m.vertexCount];
    //    for(int i = 0; i < normals.Length; i++)
    //    {
    //        normals[i] = normal;
    //    }

    //    m.normals = normals;

    //    DMesh3 dmesh = g3UnityUtils.UnityMeshToDMesh(m);
    //    MeshExtrudeMesh mem = new MeshExtrudeMesh(dmesh);
    //    mem.Extrude();

    //    MeshNormals.QuickCompute(dmesh);

    //    return g3UnityUtils.DMeshToUnityMesh(dmesh);
    //}


    // extrudes a mesh along the Y axis by the amount specified
    //public static Mesh ExtrudeY(Mesh mesh, Transform transform, float amount)
    //{
    //    Edge[] edges = ExtrudeOperation.FindOuterEdges(mesh);

    //    Matrix4x4[] endPointTransforms = new Matrix4x4[2];
    //    Vector3 offset = new Vector3(0.0f, amount, 0.0f);
    //    endPointTransforms[0] = Matrix4x4.identity;
    //    endPointTransforms[1] = transform.localToWorldMatrix * Matrix4x4.Translate(offset);

    //    return ExtrudeOperation.Extrude(mesh, endPointTransforms, edges, true);
    //}

    // extrudes a mesh along the normal by the amount specified
    //public static Mesh ExtrudeNormal(Mesh mesh, Transform transform, float amount, Vector3 normal)
    //{
    //    Edge[] edges = ExtrudeOperation.FindOuterEdges(mesh);

    //    Matrix4x4[] endPointTransforms = new Matrix4x4[2];
    //    Vector3 offset = normal * amount;// new Vector3(0.0f, amount, 0.0f);
    //    endPointTransforms[0] = Matrix4x4.identity;
    //    endPointTransforms[1] = transform.localToWorldMatrix * Matrix4x4.Translate(offset);

    //    return ExtrudeOperation.Extrude(mesh, endPointTransforms, edges, true);
    //}
    
    // extrudes a mesh along the normal by the amount specified
    public static Shape ExtrudeNormal(Shape shape, Transform transform, float amount, Vector3 normal)
    {
        Mesh mesh = shape.Mesh;

        Edge[] edges = ExtrudeOperation.FindOuterEdges(mesh);

        Matrix4x4[] endPointTransforms = new Matrix4x4[2];
        Vector3 offset = normal * amount;// new Vector3(0.0f, amount, 0.0f);
        endPointTransforms[0] = Matrix4x4.identity;
        endPointTransforms[1] = transform.localToWorldMatrix * Matrix4x4.Translate(offset);

        Mesh extrudedMesh = ExtrudeOperation.Extrude(mesh, endPointTransforms, edges, true);
        extrudedMesh.RecalculateBounds();

        LocalTransform localTransform = new LocalTransform(shape.LocalTransform.Origin, shape.LocalTransform.Up, shape.LocalTransform.Forward, shape.LocalTransform.Right);
        localTransform.Origin = extrudedMesh.bounds.center;

        return new Shape(extrudedMesh, localTransform);
    }

    //public static List<Mesh> Split(Mesh mesh, AxisSelector axis, int divisions)
    public static List<Shape> Split(Shape shape, Vector3 pos, Vector3 size, AxisSelector axis, int divisions)
    {
        float minAxisPos = 0f;
        float divisionSize = 0f;

        switch (axis)
        {
            case AxisSelector.X:
                minAxisPos = pos.x - (size.x / 2.0f);
                divisionSize = size.x / (float)divisions;
                return MultiSplit(shape, pos, minAxisPos, divisionSize, divisions, SplitX);
            case AxisSelector.Y:
                minAxisPos = pos.y - (size.y / 2.0f);
                divisionSize = size.y / (float)divisions;
                return MultiSplit(shape, pos, minAxisPos, divisionSize, divisions, SplitY);
            case AxisSelector.Z:
                minAxisPos = pos.z - (size.z / 2.0f);
                divisionSize = size.z / (float)divisions;
                return MultiSplit(shape, pos, minAxisPos, divisionSize, divisions, SplitZ);
            default:
                return null;
        }
    }

    //public static Mesh MultiSplit(Mesh mesh, float size, int divisions, Func<Mesh, float, List<Mesh>> splitFunction)
    //{


    //    float offset = size / (float) divisions;

    //    List<Mesh> allParts = new List<Mesh>();
    //    Mesh currentPart = mesh;

    //    for (int i = 0; i < divisions - 1; i++)
    //    {
    //        List<Mesh> parts = splitFunction(currentPart, offset);

    //        if(i == 1)
    //        {
    //            return parts[1];
    //        }

    //        //allParts.Add(parts[0]);
    //        //currentPart = parts[1];



    //        //if (i == 1)
    //        //{
    //        //    return new List<Mesh>() { allParts[i - 1], allParts[i] };
    //        //}

    //        if(i == divisions - 2)
    //        {
    //            allParts.Add(parts[0]);
    //            allParts.Add(parts[1]);
    //        }
    //        else
    //        {

    //            allParts.Add(parts[0]);
    //            currentPart = parts[1];
    //        }
    //    }

    //    //return allParts;
    //    return null;
    //    //return new List<Mesh>() { allParts[0], allParts[1] };
    //}

    public static List<Shape> MultiSplit(Shape shape, Vector3 pos, float minPos, float divisionSize, int divisions, Func<Shape, Vector3, float, List<Shape>> splitFunction)
    {
        float offset = divisionSize / (float)divisions;

        List<Shape> allParts = new List<Shape>();
        Shape currentShape = shape;

        for (int i = 0; i < divisions - 1; i++)
        {
            float cutPos = minPos + (divisionSize * (i + 1));

            List<Shape> parts = splitFunction(currentShape, pos, cutPos);

            if (i == divisions - 2)
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

        return allParts;

        //return new List<Mesh>() { allParts[0], allParts[1] };
    }

    // plane normal should be one of the shapes orientation vectors
    public static List<Shape> SplitAxis(Shape shape, Vector3 planeNormal, int divisions)
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
            parts.Add(SplitOperation.SplitAxis(currentShape, planePos, cutNormal));
            parts.Add(SplitOperation.SplitAxis(currentShape, planePos, -cutNormal));

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

        for(int i = 0; i < allParts.Count; i++)
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
            if(mbl.Loops.Count == 1)
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
    public static List<Shape> SplitAxisRatio(Shape shape, Vector3 planeNormal, float ratio)
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
        parts.Add(SplitOperation.SplitAxis(currentShape, planePos, cutNormal));
        parts.Add(SplitOperation.SplitAxis(currentShape, planePos, -cutNormal));


        allParts.Add(parts[0]);
        allParts.Add(parts[1]);



        for(int i = 0; i < allParts.Count; i++)
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
            if(mbl.Loops.Count == 1)
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

    public static List<Shape> SplitY(Shape shape, Vector3 pos, float cutPos)
    {
        //// determine location of cut
        //Vector3 pos = mesh.bounds.center;
        //Vector3 size = mesh.bounds.size;

        //float minY = pos.y - (size.y / 2.0f);

        // create cut plane
        Vector3 planePos = new Vector3(pos.x, cutPos, pos.z);
        Vector3 planeNormal = Vector3.up;

        //Vector3 flattenRotation = new Vector3(0.0f, 0.0f, 90.0f);

        // call Split once for each side by reversing plane normal
        List<Shape> meshes = new List<Shape>();
        Shape sideA = SplitOperation.Split(shape, planePos, planeNormal, AxisSelector.Y, true);
        Shape sideB = SplitOperation.Split(shape, planePos, -planeNormal, AxisSelector.Y, false);

        return new List<Shape>() { sideA, sideB };
    }

    //public static List<Mesh> SplitY2(Mesh mesh, float ratio)
    //{
    //    // determine location of cut
    //    Vector3 pos = mesh.bounds.center;
    //    Vector3 size = mesh.bounds.size;

    //    float minY = pos.y - (size.y / 2.0f);
    //    float distY = size.y * ratio;
    //    //    float minX = pos.x - (size.x / 2.0f);
    //    //    float distX = size.x * ratio;
    //    // create cut plane
    //    Vector3 planePos = new Vector3(pos.x, minY + distY, pos.z);
    //    Vector3 planeNormal = Vector3.up;

    //    //Vector3 flattenRotation = new Vector3(0.0f, 0.0f, 90.0f);

    //    // call Split once for each side by reversing plane normal
    //    List<Mesh> meshes = new List<Mesh>();
    //    Mesh sideA = SplitOperation.Split(mesh, planePos, planeNormal, AxisSelector.Y, false);
    //    Mesh sideB = SplitOperation.Split(mesh, planePos, -planeNormal, AxisSelector.Y, true);

    //    return new List<Mesh>() { sideA, sideB };
    //}

    //// splits the x axis
    //public static List<Mesh> SplitX(Mesh mesh, float ratio)
    //{
    //    // determine location of cut
    //    Vector3 pos = mesh.bounds.center;
    //    Vector3 size = mesh.bounds.size;

    //    float minX = pos.x - (size.x / 2.0f);
    //    float distX = size.x * ratio;

    //    // create cut plane
    //    Vector3 planePos = new Vector3(minX + distX, pos.y, pos.z);
    //    Vector3 planeNormal = Vector3.right;

    //    // determine which way to rotate edge loops so they are flat
    //    Vector3 flattenRotation = new Vector3(0.0f, 0.0f, 90.0f);

    //    // call Split once for each side by reversing plane normal
    //    List<Mesh> meshes = new List<Mesh>();
    //    Mesh sideA = SplitOperation.Split(mesh, planePos, planeNormal, AxisSelector.X, false, true, flattenRotation);
    //    Mesh sideB = SplitOperation.Split(mesh, planePos, -planeNormal, AxisSelector.X, true, true, flattenRotation);

    //    return new List<Mesh>() { sideA, sideB };
    //}

    public static List<Shape> SplitX(Shape shape, Vector3 pos, float cutPos)
    {
        //// determine location of cut
        //Vector3 pos = mesh.bounds.center;
        //Vector3 size = mesh.bounds.size;

        //float minX = pos.x - (size.x / 2.0f);

        // create cut plane
        Vector3 planePos = new Vector3(cutPos, pos.y, pos.z);
        Vector3 planeNormal = Vector3.right;

        // determine which way to rotate edge loops so they are flat
        Vector3 flattenRotation = new Vector3(0.0f, 0.0f, 90.0f);

        // call Split once for each side by reversing plane normal
        List<Shape> meshes = new List<Shape>();
        Shape sideA = SplitOperation.Split(shape, planePos, planeNormal, AxisSelector.X, false, true, flattenRotation);
        Shape sideB = SplitOperation.Split(shape, planePos, -planeNormal, AxisSelector.X, true, true, flattenRotation);

        return new List<Shape>() { sideA, sideB };
    }

    //public static List<Shape> SplitXAxis(Shape shape, Vector3 pos, float cutPos)
    //{
    //    //// determine location of cut
    //    //Vector3 pos = mesh.bounds.center;
    //    //Vector3 size = mesh.bounds.size;

    //    //float minX = pos.x - (size.x / 2.0f);

    //    // create cut plane
    //    Vector3 planePos = new Vector3(cutPos, pos.y, pos.z);
    //    Vector3 planeNormal = Vector3.right;

    //    // determine which way to rotate edge loops so they are flat
    //    Vector3 flattenRotation = new Vector3(0.0f, 0.0f, 90.0f);

    //    // call Split once for each side by reversing plane normal
    //    List<Shape> meshes = new List<Shape>();
    //    Shape sideA = SplitOperation.Split(shape, planePos, planeNormal, AxisSelector.X, false, true, flattenRotation);
    //    Shape sideB = SplitOperation.Split(shape, planePos, -planeNormal, AxisSelector.X, true, true, flattenRotation);

    //    return new List<Shape>() { sideA, sideB };
    //}

    //public static List<Mesh> SplitX2(Mesh mesh, Vector3 pos, float sizeX, int divisions, int currentDiv)
    //{
    //    // determine location of cut
    //    //Vector3 pos = mesh.bounds.center;
    //    //Vector3 size = mesh.bounds.size;

    //    float minX = pos.x - (sizeX / 2.0f);
    //    //float xPos = 

    //    float divisionSize = sizeX / (float)divisions;
    //    float xOffset = divisionSize * currentDiv;

    //    // create cut plane
    //    Vector3 planePos = new Vector3(minX + xOffset, pos.y, pos.z);
    //    Vector3 planeNormal = Vector3.right;

    //    // determine which way to rotate edge loops so they are flat
    //    Vector3 flattenRotation = new Vector3(0.0f, 0.0f, 90.0f);

    //    // call Split once for each side by reversing plane normal
    //    List<Mesh> meshes = new List<Mesh>();
    //    Mesh sideA = SplitOperation.Split(mesh, planePos, planeNormal, AxisSelector.X, false, true, flattenRotation);
    //    Mesh sideB = SplitOperation.Split(mesh, planePos, -planeNormal, AxisSelector.X, true, true, flattenRotation);

    //    return new List<Mesh>() { sideA, sideB };
    //}

    public static List<Shape> SplitZ(Shape shape, Vector3 pos, float cutPos)
    {
        //// determine location of cut
        //Vector3 pos = mesh.bounds.center;
        //Vector3 size = mesh.bounds.size;

        //float minZ = pos.z - (size.z / 2.0f);
        
        // create cut plane
        Vector3 planePos = new Vector3(pos.x, pos.y, cutPos);
        Vector3 planeNormal = Vector3.forward;
        
        // determine which way to rotate edge loops so they are flat
        Vector3 flattenRotation = new Vector3(90.0f, 0.0f, 0.0f);

        // call Split once for each side by reversing plane normal
        List<Shape> meshes = new List<Shape>();
        Shape sideA = SplitOperation.Split(shape, planePos, planeNormal, AxisSelector.Z, true, true, flattenRotation);
        Shape sideB = SplitOperation.Split(shape, planePos, -planeNormal, AxisSelector.Z, false, true, flattenRotation);

        return new List<Shape>() { sideA, sideB };
    }

    //public static List<Shape> SplitAxis(Shape shape, Vector3 pos, float cutPos)
    //{
    //    //// determine location of cut
    //    //Vector3 pos = mesh.bounds.center;
    //    //Vector3 size = mesh.bounds.size;

    //    //float minZ = pos.z - (size.z / 2.0f);

    //    // create cut plane
    //    Vector3 planePos = new Vector3(pos.x, pos.y, cutPos);
    //    Vector3 planeNormal = Vector3.forward;

    //    // determine which way to rotate edge loops so they are flat
    //    Vector3 flattenRotation = new Vector3(90.0f, 0.0f, 0.0f);

    //    // call Split once for each side by reversing plane normal
    //    List<Shape> meshes = new List<Shape>();
    //    Shape sideA = SplitOperation.Split(shape, planePos, planeNormal, AxisSelector.Z, true, true, flattenRotation);
    //    Shape sideB = SplitOperation.Split(shape, planePos, -planeNormal, AxisSelector.Z, false, true, flattenRotation);

    //    return new List<Shape>() { sideA, sideB };
    //}


}