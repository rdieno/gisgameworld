using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using g3;
using System;
using UnityEngine.ProBuilder;
using System.Linq;

//public static class MyExtensions
//{
//    public static T[] SubArray<T>(this T[] data, int index, int length)
//    {
//        T[] result = new T[length];
//        Array.Copy(data, index, result, 0, length);
//        return result;
//    }
//}
public class ShapeGrammarProcessor
{
    private GameManager manager;

    private DataManager dataManager;
    public DataManager DataManager
    {
        get { return dataManager; }
        //set { dataManager = value; }
    }

    private GameObject level;

    private MeshFilter levelMeshFilter;
    private MeshRenderer levelMeshRenderer;

    //public ExtrudedMeshTrail emt;

    private Building currentBuilding;
    private Mesh currentBuildingMesh;

    //private readonly Vector3[] referenceAngles = { Vector3.left, Vector3.right, Vector3.forward, Vector3.back};
    //private readonly Vector3[] referenceDiagonals = { new Vector3(0.7f, 0.0f, 0.7f), new Vector3(0.7f, 0.0f, -0.7f), new Vector3(-0.7f, 0.0f, -0.7f), new Vector3(-0.7f, 0.0f, 0.7f) };


    //private readonly Vector3[] referenceAngles = { Vector3.left, Vector3.right, Vector3.forward, Vector3.back, new Vector3(0.7f, 0.0f, 0.7f), new Vector3(0.7f, 0.0f, -0.7f), new Vector3(-0.7f, 0.0f, -0.7f), new Vector3(-0.7f, 0.0f, 0.7f) };

    public ShapeGrammarProcessor(GameManager manager)
    {
        this.manager = manager;
        this.dataManager = manager.DataManager;
        this.level = manager.Level;
        this.levelMeshFilter = level.GetComponent<MeshFilter>();
        this.levelMeshRenderer = level.GetComponent<MeshRenderer>();
        this.currentBuilding = null;
        this.currentBuildingMesh = null;
    }

    public void CreateTestSquare(float width = 10f, float depth = 10f)
    {
        Mesh plane = manager.LevelManager.CreatePlane(width, depth);
        LocalTransform lt = new LocalTransform(Vector3.zero, Vector3.up, Vector3.forward, Vector3.right);
        Shape s = new Shape(plane, lt);

        List<Vector3> footprint = plane.vertices.OfType<Vector3>().ToList();

        currentBuilding = new Building(footprint, -1, s);
        currentBuildingMesh = s.Mesh;

        currentBuildingMesh = manager.LevelManager.CreatePlane(10, 10);

        Material material = levelMeshRenderer.materials[0];
        material.mainTexture = manager.LevelManager.CreateTestTexture(10, 10);

        levelMeshFilter.mesh = currentBuildingMesh;
    }

    public void RetrieveBuilding(int index, bool moveToOrigin = false)
    {
        if (manager.DataManager.HasLoadedData)
        {
            Building building = this.DataManager.LevelData.Buildings[index];

            if (moveToOrigin)
            {
                Vector3[] vertices = building.Root.Vertices;

                Vector3 offset = building.Root.LocalTransform.Origin;

                for (int i = 0; i < vertices.Length; i++)
                {
                    vertices[i] = new Vector3(vertices[i].x - offset.x, vertices[i].y, vertices[i].z - offset.z);
                }

                building.Root.LocalTransform.Origin = Vector3.zero;

                building.Root.Vertices = vertices;
            }

            currentBuilding = building;

            // convert building to mesh
            //currentBuildingMesh = BuildingUtility.BuildingToMesh(currentBuilding, false);
            //levelMeshFilter.mesh = currentBuildingMesh;


            //currentBuildingMesh.RecalculateBounds();
            //currentBuildingMesh.RecalculateNormals();



        }
        else
        {
            Debug.Log("Shape Grammar Processor: could not find data in data manager");
            Debug.Log("Shape Grammar Processor: loading test plane");
            CreateTestSquare();
        }

        Material material = levelMeshRenderer.materials[0];
        material.mainTexture = manager.LevelManager.CreateTestTexture(10, 10);
    }

    public void RunSimpleRulesTest()
    {
        CreateTestSquare();

        Shape lot = currentBuilding.Root;

        Shape scaleX100 = ScaleOperation.Scale(lot, new Vector3(10f, 1f, 1f));
        //Shape extrudeY10 = ShapeGrammerOperations.ExtrudeNormal(scaleX100, level.transform, 10f, scaleX100.LocalTransform.Up);
        Shape extrudeY10 = ExtrudeOperation.ExtrudeNormal(scaleX100, 10f, scaleX100.LocalTransform.Up);


        int divisions = 3;

        List<string> shapeNames = new List<string>() { "A", "B", "C" };

        List<Shape> splitX = SplitOperation.SplitAxisDivisions(extrudeY10, extrudeY10.LocalTransform.Right, divisions);

        Dictionary<string, Shape> splitShapes = new Dictionary<string, Shape>();


        for(int i = 0; i < divisions; i++)
        {
            splitShapes.Add(shapeNames[i], splitX[i]);
        }

        List<Mesh> meshes = new List<Mesh>();


        foreach(KeyValuePair<string, Shape> splitShape in splitShapes)
        {
            meshes.Add(splitShape.Value.Mesh);
        }

        //meshes.Add(extrudeY10.Mesh);
        //meshes.Add(extrudeX100.Mesh);
      

        currentBuilding.Mesh = BuildingUtility.CombineMeshes(meshes, true);

        if (false)
        {
            Vector3[] verts = currentBuilding.Mesh.vertices;
            Vector3[] norms = currentBuilding.Mesh.normals;

            for (int i = 0; i < currentBuilding.Mesh.vertexCount; i++)
            {
                Debug.DrawLine(verts[i], verts[i] + norms[i], Color.green, 1000.0f);
            }
        }

        levelMeshFilter.mesh = currentBuilding.Mesh;

        Material[] mats = new Material[] {
            Resources.Load("Materials/TestMaterialBlue") as Material,
            Resources.Load("Materials/TestMaterialRed") as Material,
            Resources.Load("Materials/TestMaterialYellow") as Material,
            Resources.Load("Materials/TestMaterialPink") as Material,
            Resources.Load("Materials/TestMaterialOrange") as Material,
            Resources.Load("Materials/TestMaterialGreen") as Material,
            Resources.Load("Materials/TestMaterialPurple") as Material,
            Resources.Load("Materials/TestMaterialLightGreen") as Material,
            Resources.Load("Materials/TestMaterialLightBlue") as Material,
            Resources.Load("Materials/TestMaterialBlue") as Material,
            Resources.Load("Materials/TestMaterialRed") as Material,
            Resources.Load("Materials/TestMaterialYellow") as Material,
            Resources.Load("Materials/TestMaterialPink") as Material,
            Resources.Load("Materials/TestMaterialOrange") as Material,
            Resources.Load("Materials/TestMaterialGreen") as Material,
            Resources.Load("Materials/TestMaterialPurple") as Material,
            Resources.Load("Materials/TestMaterialLightGreen") as Material,
            Resources.Load("Materials/TestMaterialBlue") as Material,
            Resources.Load("Materials/TestMaterialRed") as Material,
            Resources.Load("Materials/TestMaterialYellow") as Material,
            Resources.Load("Materials/TestMaterialPink") as Material,
            Resources.Load("Materials/TestMaterialOrange") as Material,
            Resources.Load("Materials/TestMaterialGreen") as Material,
            Resources.Load("Materials/TestMaterialPurple") as Material,
            Resources.Load("Materials/TestMaterialLightGreen") as Material,
            Resources.Load("Materials/TestMaterialLightBlue") as Material,
            Resources.Load("Materials/TestMaterialBlue") as Material,
            Resources.Load("Materials/TestMaterialRed") as Material,
            Resources.Load("Materials/TestMaterialYellow") as Material,
            Resources.Load("Materials/TestMaterialPink") as Material,
            Resources.Load("Materials/TestMaterialOrange") as Material,
            Resources.Load("Materials/TestMaterialGreen") as Material,
            Resources.Load("Materials/TestMaterialPurple") as Material,
            Resources.Load("Materials/TestMaterialLightGreen") as Material,
        };
        levelMeshRenderer.materials = mats;
    }

    public void SimpleTempleDesignTest()
    {
        //CreateTestSquare(20f, 30f);

        dataManager.LoadData();
        RetrieveBuilding(1, true);

        //RetrieveBuilding(17, true);

        List<Mesh> meshes = new List<Mesh>();

        Shape lot = currentBuilding.Root;

        //Shape extrudeY10 = ShapeGrammerOperations.ExtrudeNormal(lot, level.transform, 5f, lot.LocalTransform.Up);
        Shape extrudeY10 = ExtrudeOperation.ExtrudeNormal(lot, 5f, lot.LocalTransform.Up);

        Dictionary<string, List<Shape>> compBase = CompOperation.CompFaces(extrudeY10);
        foreach (KeyValuePair<string, List<Shape>> baseParts in compBase)
        {
            if (baseParts.Key == "Front")
            {
                List<Shape> fronts = baseParts.Value;

                for (int i = 0; i < fronts.Count; i++)
                {
                    Shape front = fronts[i];

                    Shape frontStairs = StairOperation.Stair(front, 5, front.LocalTransform.Forward);

                    meshes.Add(frontStairs.Mesh);
                }
            }
            if (baseParts.Key == "Top")
            {
                List<Shape> tops = baseParts.Value;

                for (int i = 0; i < tops.Count; i++)
                {
                    Shape top = tops[i];
                    Shape extrudeTopY10 = ExtrudeOperation.ExtrudeNormal(top, 10f, lot.LocalTransform.Up);
                    //Shape extrudeTopY10 = ShapeGrammerOperations.ExtrudeNormal(top, level.transform, 10f, lot.LocalTransform.Up);

                    List<Shape> xSplits = SplitOperation.SplitAxisDivisions(extrudeTopY10, extrudeTopY10.LocalTransform.Forward, 7);

                    List<Shape> xSplitColumns = new List<Shape>();

                    for(int j = 0; j < xSplits.Count; j++)
                    {
                        if(j != xSplits.Count - 1)
                        {
                            if (j % 2 == 0)
                            {
                                xSplitColumns.Add(xSplits[j]);
                            }
                        }
                        else
                        {
                            xSplitColumns.Add(xSplits[j]);
                        }
                    }

                    List<List<Shape>> ySplitColumns = new List<List<Shape>>();
                    List<Shape> finalColumns = new List<Shape>();

                    for (int j = 0; j < xSplitColumns.Count; j++)
                    {
                        List<Shape> ySplits = SplitOperation.SplitAxisDivisions(xSplitColumns[j], extrudeTopY10.LocalTransform.Right, 7);
                        ySplitColumns.Add(ySplits);
                    }

                    for (int j = 0; j < ySplitColumns.Count; j++)
                    {
                        if (j == 0 || j == ySplitColumns.Count - 1)
                        {

                            List<Shape> ySplit = ySplitColumns[j];

                            for (int k = 0; k < ySplit.Count; k++)
                            {
                                if (k != ySplit.Count - 1)
                                {
                                    if (k % 2 == 0)
                                    {
                                        meshes.Add(ySplit[k].Mesh);
                                    }
                                }
                                else
                                {
                                    meshes.Add(ySplit[k].Mesh);
                                }
                            }

                        }
                        else
                        {
                            List<Shape> ySplit = ySplitColumns[j];

                            for (int k = 0; k < ySplit.Count; k++)
                            {
                                if (k == 0 || k == ySplit.Count - 1)
                                {
                                    meshes.Add(ySplit[k].Mesh);
                                }
                            }
                        }
                    }

                    Dictionary<string, List<Shape>> compMid = CompOperation.CompFaces(extrudeTopY10);
                    foreach (KeyValuePair<string, List<Shape>> midParts in compMid)
                    {
                        if (midParts.Key == "Top")
                        {
                            List<Shape> midTops = midParts.Value;

                            for (int j = 0; j < midTops.Count; j++)
                            {
                                Shape taperTop = TaperOperation.Taper(midTops[j], 5.0f, 4.0f);

                                meshes.Add(taperTop.Mesh);
                            }

                        }
                        else
                        {
                            // meshes.Add(BuildingUtility.CombineShapes(midParts.Value));
                        }
                    }
                }

                meshes.Add(BuildingUtility.CombineShapes(baseParts.Value));
            }
            else
            {
                meshes.Add(BuildingUtility.CombineShapes(baseParts.Value));
            }
        }


        currentBuilding.Mesh = BuildingUtility.CombineMeshes(meshes, true);

        if (false)
        {
            Vector3[] verts = currentBuilding.Mesh.vertices;
            Vector3[] norms = currentBuilding.Mesh.normals;

            for (int i = 0; i < currentBuilding.Mesh.vertexCount; i++)
            {
                Debug.DrawLine(verts[i], verts[i] + norms[i], Color.green, 1000.0f);
            }
        }

        levelMeshFilter.mesh = currentBuilding.Mesh;

        Material[] mats = new Material[] {
            Resources.Load("Materials/TestMaterialBlue") as Material,
            Resources.Load("Materials/TestMaterialRed") as Material,
            Resources.Load("Materials/TestMaterialYellow") as Material,
            Resources.Load("Materials/TestMaterialPink") as Material,
            Resources.Load("Materials/TestMaterialOrange") as Material,
            Resources.Load("Materials/TestMaterialGreen") as Material,
            Resources.Load("Materials/TestMaterialPurple") as Material,
            Resources.Load("Materials/TestMaterialLightGreen") as Material,
            Resources.Load("Materials/TestMaterialLightBlue") as Material,
            Resources.Load("Materials/TestMaterialBlue") as Material,
            Resources.Load("Materials/TestMaterialRed") as Material,
            Resources.Load("Materials/TestMaterialYellow") as Material,
            Resources.Load("Materials/TestMaterialPink") as Material,
            Resources.Load("Materials/TestMaterialOrange") as Material,
            Resources.Load("Materials/TestMaterialGreen") as Material,
            Resources.Load("Materials/TestMaterialPurple") as Material,
            Resources.Load("Materials/TestMaterialLightGreen") as Material,
            Resources.Load("Materials/TestMaterialBlue") as Material,
            Resources.Load("Materials/TestMaterialRed") as Material,
            Resources.Load("Materials/TestMaterialYellow") as Material,
            Resources.Load("Materials/TestMaterialPink") as Material,
            Resources.Load("Materials/TestMaterialOrange") as Material,
            Resources.Load("Materials/TestMaterialGreen") as Material,
            Resources.Load("Materials/TestMaterialPurple") as Material,
            Resources.Load("Materials/TestMaterialLightGreen") as Material,
            Resources.Load("Materials/TestMaterialLightBlue") as Material,
            Resources.Load("Materials/TestMaterialBlue") as Material,
            Resources.Load("Materials/TestMaterialRed") as Material,
            Resources.Load("Materials/TestMaterialYellow") as Material,
            Resources.Load("Materials/TestMaterialPink") as Material,
            Resources.Load("Materials/TestMaterialOrange") as Material,
            Resources.Load("Materials/TestMaterialGreen") as Material,
            Resources.Load("Materials/TestMaterialPurple") as Material,
            Resources.Load("Materials/TestMaterialLightGreen") as Material,
        };
        levelMeshRenderer.materials = mats;
    }

    public void RunSplitRatioTest()
    {
        CreateTestSquare();

        Shape lot = currentBuilding.Root;

        Shape scaleX100 = ScaleOperation.Scale(lot, new Vector3(10f, 1f, 1f));

        //Shape extrudeY10 = ShapeGrammerOperations.ExtrudeNormal(scaleX100, level.transform, 10f, scaleX100.LocalTransform.Up);
        Shape extrudeY10 = ExtrudeOperation.ExtrudeNormal(scaleX100, 10f, scaleX100.LocalTransform.Up);

        List<Shape> splitShapes = SplitOperation.SplitAxisRatio(extrudeY10, extrudeY10.LocalTransform.Right, 0.25f);

        // split (x) { {0.1 : A | ~ 0.1 : B}* | 0.1 : A }


        Mesh current = extrudeY10.Mesh;

        float sizeX = current.bounds.size.x;

        float pointOneRatio = 0.1f * sizeX;
        float pointNineRatio = 0.1f * sizeX;


        List<float> cutPoints = new List<float>();

        float floatingTotal = 0f;

        float watsLeft = sizeX - pointOneRatio;

        int rhythmCount = 0;

        while (floatingTotal < watsLeft)
        {
            floatingTotal += pointOneRatio + pointOneRatio;
            if(floatingTotal < watsLeft)
            {
                rhythmCount++;
            }
        }

        float remainingFloatRatio = (watsLeft - (pointOneRatio * rhythmCount)) / rhythmCount;

        for(int i = 0; i < rhythmCount; i++)
        {
            cutPoints.Add(pointOneRatio);
            cutPoints.Add(remainingFloatRatio);
        }

        cutPoints.Add(pointOneRatio);

        currentBuilding.Mesh = splitShapes[0].Mesh;
        //currentBuilding.Mesh = BuildingUtility.CombineShapes(splitShapes, true);

        if (false)
        {
            Vector3[] verts = currentBuilding.Mesh.vertices;
            Vector3[] norms = currentBuilding.Mesh.normals;

            for (int i = 0; i < currentBuilding.Mesh.vertexCount; i++)
            {
                Debug.DrawLine(verts[i], verts[i] + norms[i], Color.green, 1000.0f);
            }
        }

        levelMeshFilter.mesh = currentBuilding.Mesh;

        Material[] mats = new Material[] {
            Resources.Load("Materials/TestMaterialBlue") as Material,
            Resources.Load("Materials/TestMaterialRed") as Material,
            Resources.Load("Materials/TestMaterialYellow") as Material,
            Resources.Load("Materials/TestMaterialPink") as Material,
            Resources.Load("Materials/TestMaterialOrange") as Material,
            Resources.Load("Materials/TestMaterialGreen") as Material,
            Resources.Load("Materials/TestMaterialPurple") as Material,
            Resources.Load("Materials/TestMaterialLightGreen") as Material,
            Resources.Load("Materials/TestMaterialLightBlue") as Material,
            Resources.Load("Materials/TestMaterialBlue") as Material,
            Resources.Load("Materials/TestMaterialRed") as Material,
            Resources.Load("Materials/TestMaterialYellow") as Material,
            Resources.Load("Materials/TestMaterialPink") as Material,
            Resources.Load("Materials/TestMaterialOrange") as Material,
            Resources.Load("Materials/TestMaterialGreen") as Material,
            Resources.Load("Materials/TestMaterialPurple") as Material,
            Resources.Load("Materials/TestMaterialLightGreen") as Material,
            Resources.Load("Materials/TestMaterialBlue") as Material,
            Resources.Load("Materials/TestMaterialRed") as Material,
            Resources.Load("Materials/TestMaterialYellow") as Material,
            Resources.Load("Materials/TestMaterialPink") as Material,
            Resources.Load("Materials/TestMaterialOrange") as Material,
            Resources.Load("Materials/TestMaterialGreen") as Material,
            Resources.Load("Materials/TestMaterialPurple") as Material,
            Resources.Load("Materials/TestMaterialLightGreen") as Material,
            Resources.Load("Materials/TestMaterialLightBlue") as Material,
            Resources.Load("Materials/TestMaterialBlue") as Material,
            Resources.Load("Materials/TestMaterialRed") as Material,
            Resources.Load("Materials/TestMaterialYellow") as Material,
            Resources.Load("Materials/TestMaterialPink") as Material,
            Resources.Load("Materials/TestMaterialOrange") as Material,
            Resources.Load("Materials/TestMaterialGreen") as Material,
            Resources.Load("Materials/TestMaterialPurple") as Material,
            Resources.Load("Materials/TestMaterialLightGreen") as Material,
        };
        levelMeshRenderer.materials = mats;
    }

    public void RunSplitRatioTermsTest()
    {
        List<Mesh> meshes = new List<Mesh>();

        dataManager.LoadData();
        RetrieveBuilding(1, true);
        //CreateTestSquare();

        Shape lot = currentBuilding.Root;

        //Shape scaleX100 = ScaleOperation.Scale(lot, new Vector3(10f, 1f, 1f));

        //Shape extrudeY10 = ShapeGrammerOperations.ExtrudeNormal(lot, level.transform, 10f, lot.LocalTransform.Up);
        Shape extrudeY10 = ExtrudeOperation.ExtrudeNormal(lot, 10f, lot.LocalTransform.Up);
       // Shape translatedOriginal = TranslateOperation.Translate(extrudeY10, new Vector3(0f, 0f, 15f), CoordSystem.World);


        //SplitRatio ratio1 = new SplitRatio(false, 0.1f);
        //SplitRatio ratio2 = new SplitRatio(true, 0.1f);
        //SplitRatio ratio3 = new SplitRatio(false, 0.1f);

        //SplitTerm term1 = new SplitTerm(true, new List<SplitRatio>() { ratio1, ratio2 });
        //SplitTerm term2 = new SplitTerm(false, new List<SplitRatio>() { ratio3 });

        //List<SplitTerm> terms = new List<SplitTerm>() { term1, term2 };



        //SplitRatio ratio1 = new SplitRatio(false, 0.1f);
        //SplitRatio ratio2 = new SplitRatio(false, 0.1f);
        //SplitRatio ratio3 = new SplitRatio(false, 0.1f);

        SplitTerm term1 = new SplitTerm(false, new List<SplitRatio>() { new SplitRatio(false, 0.2f) });
        SplitTerm term2 = new SplitTerm(false, new List<SplitRatio>() { new SplitRatio(false, 0.1f) });
        SplitTerm term3 = new SplitTerm(false, new List<SplitRatio>() { new SplitRatio(false, 0.1f) });
        SplitTerm term4 = new SplitTerm(false, new List<SplitRatio>() { new SplitRatio(false, 0.25f) });
        SplitTerm term5 = new SplitTerm(false, new List<SplitRatio>() { new SplitRatio(false, 0.25f) });
        SplitTerm term6 = new SplitTerm(false, new List<SplitRatio>() { new SplitRatio(false, 0.1f) });

        List<SplitTerm> terms = new List<SplitTerm>() { term1, term2, term3, term4, term5, term6 };



        List<Shape> splitShapes = SplitOperation.SplitAxisTerms(extrudeY10, extrudeY10.LocalTransform.Forward, terms);


        meshes.Add(BuildingUtility.CombineShapes(splitShapes));
        //meshes.Add(translatedOriginal.Mesh);

        //currentBuilding.Mesh = splitShapes[0].Mesh;
        //currentBuilding.Mesh = BuildingUtility.CombineShapes(splitShapes, true);
        currentBuilding.Mesh = BuildingUtility.CombineMeshes(meshes, true);

        if (false)
        {
            Vector3[] verts = currentBuilding.Mesh.vertices;
            Vector3[] norms = currentBuilding.Mesh.normals;

            for (int i = 0; i < currentBuilding.Mesh.vertexCount; i++)
            {
                Debug.DrawLine(verts[i], verts[i] + norms[i], Color.green, 1000.0f);
            }
        }

        levelMeshFilter.mesh = currentBuilding.Mesh;

        Material[] mats = new Material[] {
            Resources.Load("Materials/TestMaterialBlue") as Material,
            Resources.Load("Materials/TestMaterialRed") as Material,
            Resources.Load("Materials/TestMaterialYellow") as Material,
            Resources.Load("Materials/TestMaterialPink") as Material,
            Resources.Load("Materials/TestMaterialOrange") as Material,
            Resources.Load("Materials/TestMaterialGreen") as Material,
            Resources.Load("Materials/TestMaterialPurple") as Material,
            Resources.Load("Materials/TestMaterialLightGreen") as Material,
            Resources.Load("Materials/TestMaterialLightBlue") as Material,
            Resources.Load("Materials/TestMaterialBlue") as Material,
            Resources.Load("Materials/TestMaterialRed") as Material,
            Resources.Load("Materials/TestMaterialYellow") as Material,
            Resources.Load("Materials/TestMaterialPink") as Material,
            Resources.Load("Materials/TestMaterialOrange") as Material,
            Resources.Load("Materials/TestMaterialGreen") as Material,
            Resources.Load("Materials/TestMaterialPurple") as Material,
            Resources.Load("Materials/TestMaterialLightGreen") as Material,
            Resources.Load("Materials/TestMaterialBlue") as Material,
            Resources.Load("Materials/TestMaterialRed") as Material,
            Resources.Load("Materials/TestMaterialYellow") as Material,
            Resources.Load("Materials/TestMaterialPink") as Material,
            Resources.Load("Materials/TestMaterialOrange") as Material,
            Resources.Load("Materials/TestMaterialGreen") as Material,
            Resources.Load("Materials/TestMaterialPurple") as Material,
            Resources.Load("Materials/TestMaterialLightGreen") as Material,
            Resources.Load("Materials/TestMaterialLightBlue") as Material,
            Resources.Load("Materials/TestMaterialBlue") as Material,
            Resources.Load("Materials/TestMaterialRed") as Material,
            Resources.Load("Materials/TestMaterialYellow") as Material,
            Resources.Load("Materials/TestMaterialPink") as Material,
            Resources.Load("Materials/TestMaterialOrange") as Material,
            Resources.Load("Materials/TestMaterialGreen") as Material,
            Resources.Load("Materials/TestMaterialPurple") as Material,
            Resources.Load("Materials/TestMaterialLightGreen") as Material,
        };
        levelMeshRenderer.materials = mats;
    }

    public void RunCompMappingTest()
    {
        List<Mesh> meshes = new List<Mesh>();

        dataManager.LoadData();
        //RetrieveBuilding(1, true);
        RetrieveBuilding(1);
        //CreateTestSquare();

        Shape lot = currentBuilding.Root;
        //Shape extrudeY10 = ShapeGrammerOperations.ExtrudeNormal(lot, level.transform, 10f, lot.LocalTransform.Up);
        Shape extrudeY10 = ExtrudeOperation.ExtrudeNormal(lot, 10f, lot.LocalTransform.Up);

        meshes.Add(extrudeY10.Mesh);

        Dictionary<string, string> compOperationInput = new Dictionary<string, string>();
        compOperationInput.Add("Front", "A");
        compOperationInput.Add("Back", "C");
        compOperationInput.Add("Left", "C");
        compOperationInput.Add("Right", "C");
        compOperationInput.Add("Top", "B");
        compOperationInput.Add("Bottom", "C");

        Dictionary<string, List<Shape>> compSorted = new Dictionary<string, List<Shape>>();

        Dictionary<string, List<Shape>> compBase = CompOperation.CompFaces(extrudeY10);
        foreach (KeyValuePair<string, List<Shape>> baseParts in compBase)
        {
            string key = baseParts.Key;
            List<Shape> shapes = baseParts.Value;

            string inputKey = compOperationInput[key];

            if(inputKey == null)
            {
                Debug.Log("Shape Grammar Processor: Comp Operation sorting error");
            }

            if(compSorted.ContainsKey(inputKey))
            {
                compSorted[inputKey].AddRange(shapes);

            }
            else
            {
                compSorted.Add(inputKey, shapes);
            }
        }


        //currentBuilding.Mesh = splitShapes[0].Mesh;
        //currentBuilding.Mesh = BuildingUtility.CombineShapes(splitShapes, true);
        currentBuilding.Mesh = BuildingUtility.CombineMeshes(meshes, true);

        if (false)
        {
            Vector3[] verts = currentBuilding.Mesh.vertices;
            Vector3[] norms = currentBuilding.Mesh.normals;

            for (int i = 0; i < currentBuilding.Mesh.vertexCount; i++)
            {
                Debug.DrawLine(verts[i], verts[i] + norms[i], Color.green, 1000.0f);
            }
        }

        levelMeshFilter.mesh = currentBuilding.Mesh;

        Material[] mats = new Material[] {
            Resources.Load("Materials/TestMaterialBlue") as Material,
            Resources.Load("Materials/TestMaterialRed") as Material,
            Resources.Load("Materials/TestMaterialYellow") as Material,
            Resources.Load("Materials/TestMaterialPink") as Material,
            Resources.Load("Materials/TestMaterialOrange") as Material,
            Resources.Load("Materials/TestMaterialGreen") as Material,
            Resources.Load("Materials/TestMaterialPurple") as Material,
            Resources.Load("Materials/TestMaterialLightGreen") as Material,
            Resources.Load("Materials/TestMaterialLightBlue") as Material,
            Resources.Load("Materials/TestMaterialBlue") as Material,
            Resources.Load("Materials/TestMaterialRed") as Material,
            Resources.Load("Materials/TestMaterialYellow") as Material,
            Resources.Load("Materials/TestMaterialPink") as Material,
            Resources.Load("Materials/TestMaterialOrange") as Material,
            Resources.Load("Materials/TestMaterialGreen") as Material,
            Resources.Load("Materials/TestMaterialPurple") as Material,
            Resources.Load("Materials/TestMaterialLightGreen") as Material,
            Resources.Load("Materials/TestMaterialBlue") as Material,
            Resources.Load("Materials/TestMaterialRed") as Material,
            Resources.Load("Materials/TestMaterialYellow") as Material,
            Resources.Load("Materials/TestMaterialPink") as Material,
            Resources.Load("Materials/TestMaterialOrange") as Material,
            Resources.Load("Materials/TestMaterialGreen") as Material,
            Resources.Load("Materials/TestMaterialPurple") as Material,
            Resources.Load("Materials/TestMaterialLightGreen") as Material,
            Resources.Load("Materials/TestMaterialLightBlue") as Material,
            Resources.Load("Materials/TestMaterialBlue") as Material,
            Resources.Load("Materials/TestMaterialRed") as Material,
            Resources.Load("Materials/TestMaterialYellow") as Material,
            Resources.Load("Materials/TestMaterialPink") as Material,
            Resources.Load("Materials/TestMaterialOrange") as Material,
            Resources.Load("Materials/TestMaterialGreen") as Material,
            Resources.Load("Materials/TestMaterialPurple") as Material,
            Resources.Load("Materials/TestMaterialLightGreen") as Material,
        };
        levelMeshRenderer.materials = mats;
    }


    public void RunExtrudeInputTest()
    {
        List<Mesh> meshes = new List<Mesh>();

        dataManager.LoadData();
        //RetrieveBuilding(1, true);
        RetrieveBuilding(1);
        //CreateTestSquare();

        Shape lot = currentBuilding.Root;

        float extrudeInput = 10f;

        Shape extrudeY10 = ExtrudeOperation.ExtrudeNormal(lot, extrudeInput, lot.LocalTransform.Up);

        meshes.Add(extrudeY10.Mesh);

        currentBuilding.Mesh = BuildingUtility.CombineMeshes(meshes, true);

        if (false)
        {
            Vector3[] verts = currentBuilding.Mesh.vertices;
            Vector3[] norms = currentBuilding.Mesh.normals;

            for (int i = 0; i < currentBuilding.Mesh.vertexCount; i++)
            {
                Debug.DrawLine(verts[i], verts[i] + norms[i], Color.green, 1000.0f);
            }
        }

        levelMeshFilter.mesh = currentBuilding.Mesh;

        Material[] mats = new Material[] {
            Resources.Load("Materials/TestMaterialBlue") as Material,
            Resources.Load("Materials/TestMaterialRed") as Material,
            Resources.Load("Materials/TestMaterialYellow") as Material,
            Resources.Load("Materials/TestMaterialPink") as Material,
            Resources.Load("Materials/TestMaterialOrange") as Material,
            Resources.Load("Materials/TestMaterialGreen") as Material,
            Resources.Load("Materials/TestMaterialPurple") as Material,
            Resources.Load("Materials/TestMaterialLightGreen") as Material,
            Resources.Load("Materials/TestMaterialLightBlue") as Material,
            Resources.Load("Materials/TestMaterialBlue") as Material,
            Resources.Load("Materials/TestMaterialRed") as Material,
            Resources.Load("Materials/TestMaterialYellow") as Material,
            Resources.Load("Materials/TestMaterialPink") as Material,
            Resources.Load("Materials/TestMaterialOrange") as Material,
            Resources.Load("Materials/TestMaterialGreen") as Material,
            Resources.Load("Materials/TestMaterialPurple") as Material,
            Resources.Load("Materials/TestMaterialLightGreen") as Material,
            Resources.Load("Materials/TestMaterialBlue") as Material,
            Resources.Load("Materials/TestMaterialRed") as Material,
            Resources.Load("Materials/TestMaterialYellow") as Material,
            Resources.Load("Materials/TestMaterialPink") as Material,
            Resources.Load("Materials/TestMaterialOrange") as Material,
            Resources.Load("Materials/TestMaterialGreen") as Material,
            Resources.Load("Materials/TestMaterialPurple") as Material,
            Resources.Load("Materials/TestMaterialLightGreen") as Material,
            Resources.Load("Materials/TestMaterialLightBlue") as Material,
            Resources.Load("Materials/TestMaterialBlue") as Material,
            Resources.Load("Materials/TestMaterialRed") as Material,
            Resources.Load("Materials/TestMaterialYellow") as Material,
            Resources.Load("Materials/TestMaterialPink") as Material,
            Resources.Load("Materials/TestMaterialOrange") as Material,
            Resources.Load("Materials/TestMaterialGreen") as Material,
            Resources.Load("Materials/TestMaterialPurple") as Material,
            Resources.Load("Materials/TestMaterialLightGreen") as Material,
        };
        levelMeshRenderer.materials = mats;
    }


    public void RunOffsetInputTest()
    {
        List<Mesh> meshes = new List<Mesh>();

        //dataManager.LoadData();
        //RetrieveBuilding(1, true);
        //RetrieveBuilding(1);
        CreateTestSquare();

        Shape lot = currentBuilding.Root;

        float offsetAmount = -2f;

//        Shape extrudeY10 = ExtrudeOperation.ExtrudeNormal(lot, extrudeInput, lot.LocalTransform.Up);
        Dictionary<string, Shape> offset = OffsetOperation.Offset(lot, offsetAmount);


        //meshes.Add(offset["Inside"].Mesh);
        meshes.Add(offset["Border"].Mesh);


        Shape taper = TaperOperation.Taper(offset["Inside"], 10f, 5f);

        meshes.Add(taper.Mesh);

        currentBuilding.Mesh = BuildingUtility.CombineMeshes(meshes, true);

        if (false)
        {
            Vector3[] verts = currentBuilding.Mesh.vertices;
            Vector3[] norms = currentBuilding.Mesh.normals;

            for (int i = 0; i < currentBuilding.Mesh.vertexCount; i++)
            {
                Debug.DrawLine(verts[i], verts[i] + norms[i], Color.green, 1000.0f);
            }
        }

        levelMeshFilter.mesh = currentBuilding.Mesh;

        Material[] mats = new Material[] {
            Resources.Load("Materials/TestMaterialBlue") as Material,
            Resources.Load("Materials/TestMaterialRed") as Material,
            Resources.Load("Materials/TestMaterialYellow") as Material,
            Resources.Load("Materials/TestMaterialPink") as Material,
            Resources.Load("Materials/TestMaterialOrange") as Material,
            Resources.Load("Materials/TestMaterialGreen") as Material,
            Resources.Load("Materials/TestMaterialPurple") as Material,
            Resources.Load("Materials/TestMaterialLightGreen") as Material,
            Resources.Load("Materials/TestMaterialLightBlue") as Material,
            Resources.Load("Materials/TestMaterialBlue") as Material,
            Resources.Load("Materials/TestMaterialRed") as Material,
            Resources.Load("Materials/TestMaterialYellow") as Material,
            Resources.Load("Materials/TestMaterialPink") as Material,
            Resources.Load("Materials/TestMaterialOrange") as Material,
            Resources.Load("Materials/TestMaterialGreen") as Material,
            Resources.Load("Materials/TestMaterialPurple") as Material,
            Resources.Load("Materials/TestMaterialLightGreen") as Material,
            Resources.Load("Materials/TestMaterialBlue") as Material,
            Resources.Load("Materials/TestMaterialRed") as Material,
            Resources.Load("Materials/TestMaterialYellow") as Material,
            Resources.Load("Materials/TestMaterialPink") as Material,
            Resources.Load("Materials/TestMaterialOrange") as Material,
            Resources.Load("Materials/TestMaterialGreen") as Material,
            Resources.Load("Materials/TestMaterialPurple") as Material,
            Resources.Load("Materials/TestMaterialLightGreen") as Material,
            Resources.Load("Materials/TestMaterialLightBlue") as Material,
            Resources.Load("Materials/TestMaterialBlue") as Material,
            Resources.Load("Materials/TestMaterialRed") as Material,
            Resources.Load("Materials/TestMaterialYellow") as Material,
            Resources.Load("Materials/TestMaterialPink") as Material,
            Resources.Load("Materials/TestMaterialOrange") as Material,
            Resources.Load("Materials/TestMaterialGreen") as Material,
            Resources.Load("Materials/TestMaterialPurple") as Material,
            Resources.Load("Materials/TestMaterialLightGreen") as Material,
        };
        levelMeshRenderer.materials = mats;
    }

    //public void DetermineSplitRatioSizesTest()
    //{
    //    //SplitRatio ratio1 = new SplitRatio(false, 0.1f);
    //    //SplitRatio ratio2 = new SplitRatio(true, 0.1f);
    //    //SplitRatio ratio3 = new SplitRatio(false, 0.1f);

    //    //SplitTerm term1 = new SplitTerm(true, new List<SplitRatio>() { ratio1, ratio2 });
    //    //SplitTerm term2 = new SplitTerm(false, new List<SplitRatio>() { ratio3 });

    //    //List<SplitTerm> terms = new List<SplitTerm>() { term1, term2 };

    //    //List<float> sizes = SplitOperation.DetermineTermSizes(terms, 100f);




    //    SplitTerm term1 = new SplitTerm(false, new List<SplitRatio>() { new SplitRatio(false, 0.2f) });
    //    SplitTerm term2 = new SplitTerm(false, new List<SplitRatio>() { new SplitRatio(false, 0.2f) });
    //    SplitTerm term3 = new SplitTerm(false, new List<SplitRatio>() { new SplitRatio(false, 0.2f) });
    //    SplitTerm term4 = new SplitTerm(false, new List<SplitRatio>() { new SplitRatio(false, 0.2f) });
    //    SplitTerm term5 = new SplitTerm(false, new List<SplitRatio>() { new SplitRatio(false, 0.2f) });

    //    List<SplitTerm> terms2 = new List<SplitTerm>() { term1, term2, term3, term4, term5 };

    //    List<float> sizes2 = SplitOperation.DetermineTermSizes(terms2, 100f);

    //    System.Diagnostics.Debugger.Break();
    //}
}
