using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Text;

public class TestManager : MonoBehaviour
{
    private GameManager manager = null;
    public GameManager Manager
    {
        set => manager = value;
        get => manager;
    }

    private ShapeGrammarDatabase sgDatabase;

    private List<KeyValuePair<string, Mesh>> controlBuildings = null;

    private ShapeGrammarParser sgParser;
    private ShapeGrammarProcessor sgProcessor;

    public void Init(GameManager manager)
    {
        this.manager = manager;
        sgDatabase = manager.SGDatabase;

        sgParser = manager.SGParser;
        sgProcessor = manager.SGProcessor;
    }

    // note: designed to be run on PC, not supported on mobile
    public IEnumerator RunTests()
    {
        // Outputs screenshots to file of buildings built with rulesets with square footprints
        // used for comparing to live buildings to help find visual errors
        //yield return CreateControlScreenshots();


        // outputs screenshots of random sample of generated buildings with live real-world GIS footprints
        // currently uses default location
        // parameter can set sample amount, default is 50 
        //yield return CreateRandomSampleScreenshots();

        //yield return CreateReportScreenshots();
        
        yield return null;
    }

    //public IEnumerator CreateTestScreenshots(int index)
    //{
    //    yield return CreateOperationDemoScreenshot("offset-1", true, true, index);
    //}

    //public IEnumerator CreateTestScreenshots(string ruleset, bool colorGeometry bool useLiveFootprint = false, int index = 0)
    //{
    //    if(useLiveFootprint)
    //    {
    //        yield return CreateOperationDemoScreenshot(ruleset, true, useLiveFootprint, index);
    //    }
    //    else
    //    {
    //        yield return CreateOperationDemoScreenshot(ruleset, true, useLiveFootprint, index);
    //    }


    //}

    public IEnumerator CreateReportScreenshots()
    {
        Debug.Log("CreateReportScreenshots()");

        //CreateOperationDemoScreenshot("lot-1");
        //yield return null;

        //CreateOperationDemoScreenshot("comp-1", true);
        //yield return null;

        //CreateOperationDemoScreenshot("split-1", true);
        //yield return null;

        //CreateOperationDemoScreenshot("split-2", true);
        //yield return null;

        //CreateOperationDemoScreenshot("split-3", true);
        //yield return null;

        //CreateOperationDemoScreenshot("split-4", true);
        //yield return null;

        //yield return CreateOperationDemoScreenshot("extrude-1");
        //yield return null;

        yield return CreateOperationDemoScreenshot("offset-1", false, true, 21);
        yield return null;

        //yield return CreateOperationDemoScreenshot("dup-1", false);
        //yield return null;

        //yield return GetLiveBuildingFootprint(24);

        yield return null;
    }

    //private IEnumerator GetLiveBuildingFootprint(int safeIndex)
    //{
    //    yield return this.manager.RetrieveAndProcessNewData(true, 0.5f, false);

    //    DataManager dm = this.manager.DataManager;
    //    List<Building> buildings = dm.LevelData.Buildings;

    //    // int randomIndex = (int) UnityEngine.Random.Range(0f, buildings.Count - 1);

    //    int index = safeIndex % (buildings.Count - 1);

    //    Building building = buildings[index];
    //    building.Materials = new Material[] {
    //            Resources.Load("Materials/TestMaterial_Blank") as Material
    //        };

    //    //List<Vector3> footprint = buildings[index].Footprint;

    //    ProcessingWrapper processedBuilding = ProcessRuleset(root, ruleset);

    //    buildings[i].Info.CGARuleset = candidate.name;
    //    buildings[i].UpdateProcessedBuilding(processedBuilding);

    //    manager.LevelManager.SetCurrentBuilding(building);

    //    yield return null;
    //}

    public IEnumerator CreateOperationDemoScreenshot(string rulesetFilename, bool colorGeometry = false, bool useLiveRootShape = false, int rootShapeIndex = 99, bool takeScreenshot = false)
    {
        Building building = new Building();
        DataManager dm = null;
        List<Building> buildings = null;

        if (useLiveRootShape)
        {
            yield return this.manager.RetrieveAndProcessNewData(true, 0.5f, false);
            dm = this.manager.DataManager;
            buildings = dm.LevelData.Buildings;
            rootShapeIndex = rootShapeIndex % (buildings.Count - 1);
        }

        Material[] mats = null;

        if(colorGeometry)
        {
            mats = new Material[] {
                Resources.Load("Materials/TestMaterialBlue") as Material,
                Resources.Load("Materials/TestMaterialRed") as Material,
                Resources.Load("Materials/TestMaterialYellow") as Material,
                Resources.Load("Materials/TestMaterialPink") as Material,
                Resources.Load("Materials/TestMaterialOrange") as Material,
                Resources.Load("Materials/TestMaterialGreen") as Material,
                Resources.Load("Materials/TestMaterialPurple") as Material,
                Resources.Load("Materials/TestMaterialLightGreen") as Material,
                Resources.Load("Materials/TestMaterialLightBlue") as Material,
            };
        }
        else
        {
            mats = new Material[] {
                Resources.Load("Materials/TestMaterial_Blank") as Material
            };
        }

        SGOperationDictionary parsedRuleset = sgParser.ParseRuleFile("OperationDemos/" + rulesetFilename);

        float floorSize = 10.0f;

        LocalTransform lt = new LocalTransform(Vector3.zero, Vector3.up, Vector3.forward);
        Mesh planeMesh = manager.LevelManager.CreatePlane(floorSize, floorSize);

        Shape root = null;

        if(useLiveRootShape)
        {
            building = buildings[rootShapeIndex];

            Vector3[] vertices = building.Root.Vertices;

            Vector3 offset = building.Root.LocalTransform.Origin;

            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = new Vector3(vertices[i].x - offset.x, vertices[i].y, vertices[i].z - offset.z);
            }

            building.Root.LocalTransform.Origin = Vector3.zero;

            building.Root.Vertices = vertices;

            root = building.Root;
        }
        else
        {
            root = new Shape(planeMesh, lt);
        }
            
           
        ProcessingWrapper processedBuilding = sgProcessor.ProcessRuleset(root, parsedRuleset);

        //foreach (KeyValuePair<string, List<Shape>> shape in processedBuilding.processsedShapes)
        //{
        //    foreach (Shape s in shape.Value)
        //    {
        //        s.Debug_DrawOrientation(20f);
        //    }

        //}
        
        building.UpdateProcessedBuildingWithSubShapes(processedBuilding, colorGeometry, false);
        building.Materials = mats;

        manager.LevelManager.SetCurrentBuilding(building);

        if(takeScreenshot)
        {
            string id = DateTime.Now.ToString(@"MM-dd-yyyy_hh-mm-ss_tt");
            string filename = "OperationDemos/" + rulesetFilename + "_" + id + ".png";

            ScreenCapture.CaptureScreenshot(filename);
        }


        yield return null;
    }

    public IEnumerator TakeScreenshotOfCurrentView(string rulesetFilename)
    {
        string id = DateTime.Now.ToString(@"MM-dd-yyyy_hh-mm-ss_tt");
        string filename = "OperationDemos/" + rulesetFilename + "_" + id + ".png";

        ScreenCapture.CaptureScreenshot(filename);

        yield return null;
    }

    public IEnumerator CreateControlScreenshots()
    {
        ShapeGrammarParser sgParser = manager.SGParser;
        ShapeGrammarProcessor sgProcessor = manager.SGProcessor;

        // create control building meshes and assosiate them with their respective name
        controlBuildings = new List<KeyValuePair<string, Mesh>>();

        foreach (ShapeGrammarData data in sgDatabase.shapeGrammarData)
        {
            SGOperationDictionary ruleset = sgParser.ParseRuleFile(data.name);

            if (ruleset != null)
            {
                try
                {

                    KeyValuePair<string, Mesh> building = new KeyValuePair<string, Mesh>(data.name, sgProcessor.CreateControlBuilding(ruleset, data.controlSize));
                    controlBuildings.Add(building);
                }
                catch (Exception e)
                {
                    Debug.Log("Error: TestManager: CreateControlScreenshots(): " + e);
                }
            }
        }


        // Setup and take screenshots of control buildings
        if(controlBuildings.Count > 0)
        {
            GameObject levelObject = manager.Level;
            //MeshRenderer renderer = levelObject.GetComponent<MeshRenderer>();
            MeshFilter filter = levelObject.GetComponent<MeshFilter>();

            UIManager uiManager = manager.UIManager;
            
            int index = 0;
            foreach(KeyValuePair<string, Mesh> building in controlBuildings)
            {
                filter.mesh = building.Value;
                uiManager.SetTestcaseInfoText(building.Key);
                
                string name = building.Key;
                string filename = "Tests/Control/" + index + "_Control-Building_" + name + ".png";

                //ScreenCapture.CaptureScreenshot(filename);
                yield return null;

                index++;
            }
        }

        yield return null;
    }

    public IEnumerator CreateRandomSampleScreenshots(int amount = 50)
    {
        // perform building generation with default location
        yield return manager.GenerateWithCurrentLocation(.1f, false);

        // collect test results
        List<Building> buildings = manager.DataManager.LevelData.Buildings;

        List<BuildingTest> testResults = new List<BuildingTest>();
        
        foreach(Building b in buildings)
        {
            testResults.Add(b.TestResult);
        }

        // print test results
        string formattedTestResults = FormatTestResults(testResults);
        int f = 0;

        ////find 50 random unique numbers between 0 and number of buildings -1 inclusive

        //List<Building> buildings = manager.DataManager.LevelData.Buildings;

        //int[] randomSample = new int[amount];

        //// The Knuth algorithm to find random sample of indices between 0 and building count -1
        //// retrieved from: https://stackoverflow.com/questions/1608181/unique-random-numbers-in-an-integer-array-in-the-c-programming-language
        //int buildingCount = buildings.Count - 1; 
        //int sampleCount = amount;
        //int x, y;

        //y = 0;

        //for (x = 0; x < buildingCount && y < sampleCount; ++x) {
        //    int indicesRemaining = buildingCount - x;
        //    int indicesToFind = sampleCount - y;

        //    int random = UnityEngine.Random.Range(0, 400);

        //    int prob = random % indicesRemaining;

        //    if (prob < indicesToFind)
        //        randomSample[y++] = x;
        //}

        //// iterate over buildings using 50 random indices
        //// capture and output screenshots
        //GameObject levelObject = manager.Level;
        //MeshFilter meshFilter = levelObject.GetComponent<MeshFilter>();
        //UIManager uiManager = manager.UIManager;

        //for (int i = 0; i < amount; i++)
        //{
        //    int currentRandomIndex = randomSample[i];
        //    Building currentBuilding = buildings[currentRandomIndex];

        //    string rulesetName = currentBuilding.Info.CGARuleset;
        //    Mesh currentBuildingMesh = currentBuilding.Mesh;

        //    Shape root = currentBuilding.Root;
        //    Mesh FootprintMesh = new Mesh();
        //    FootprintMesh.vertices = root.Vertices;
        //    FootprintMesh.triangles = root.Triangles;
        //    FootprintMesh.normals = root.Normals;

        //    CombineInstance[] combine = new CombineInstance[buildings.Count];
        //    combine[0].mesh = FootprintMesh;
        //    combine[0].transform = Matrix4x4.zero;
        //    combine[1].mesh = currentBuildingMesh;
        //    combine[1].transform = Matrix4x4.zero;

        //    meshFilter.mesh = new Mesh();
        //    meshFilter.mesh.CombineMeshes(combine, true, false);

        //    uiManager.SetTestcaseInfoText(rulesetName);

        //    string filename = "Tests/Random/" + i + "-Random-Building_" + currentRandomIndex + ".png"; 

        //   // ScreenCapture.CaptureScreenshot(filename);

        //    yield return null;
        //}

        yield return null;
    }


    public IEnumerator TakeTestScreenshot()
    {
        string id = DateTime.Now.ToString(@"MM-dd-yyyy_hh-mm-ss_tt");

        string filename = "Tests/TestScreenshot_" + id + ".png";

        ScreenCapture.CaptureScreenshot(filename);

        yield return null;
    }

    //public static void SaveTestResult(TestResult result)
    //{
    //
    //}


    //public List<TestResult> TestRuleset(Shape lot, SGOperationDictionary ruleset)
    //{
    //    Dictionary<string, List<Shape>> shapes = new Dictionary<string, List<Shape>>();
    //    Dictionary<string, List<IShapeGrammarOperation>> currentRuleset = ruleset._dict;
    //    List<TestResult> results = new List<TestResult>();

    //    shapes.Add("Lot", new List<Shape>() { lot });

    //    List<string> shapesToRemove = new List<string>();

    //    foreach (KeyValuePair<string, List<IShapeGrammarOperation>> operation in ruleset._dict)
    //    {
    //        List<IShapeGrammarOperation> currentOperationList = operation.Value;
    //        List<Shape> currentShapes = null;

    //        bool foundKey = shapes.TryGetValue(operation.Key, out currentShapes);
    //        if (foundKey)
    //        {

    //            for (int i = 0; i < currentOperationList.Count; i++)
    //            {
    //                IShapeGrammarOperation currentOperation = currentOperationList[i];

    //                ShapeWrapper operationResult = currentOperation.PerformOperation(currentShapes);
    //                bool testResult = currentOperation.TestOperation(currentShapes);
                    
    //                switch (operationResult.type)
    //                {
    //                    default:
    //                    case ShapeContainerType.List:
    //                        currentShapes = operationResult.shapeList;
    //                        break;
    //                    case ShapeContainerType.Dictionary:
    //                        Dictionary<string, List<Shape>> resultShapeDictionary = operationResult.shapeDictionary;
    //                        shapes = CombineShapeDictionary(resultShapeDictionary, shapes);
    //                        break;
    //                }

    //                if (operationResult.removeParentShape)
    //                {
    //                    shapesToRemove.Add(operation.Key);
    //                }
    //            }

    //            shapes[operation.Key] = currentShapes;
    //        }
    //    }

    //    foreach (string name in shapesToRemove)
    //    {
    //        shapes.Remove(name);
    //    }

    //    return results;
    //}

    private string FormatTestResults(List<BuildingTest> testResults)
    {
        string line = "";

        StringBuilder sb = new StringBuilder();
        //sb.AppendLine();

        foreach (BuildingTest test in testResults)
        {
            int buildingIndex = test.buildingIndex;
            string ruleset = test.ruleset;

            List<ShapeTest> shapeTests = test.shapeTests;

            string buildingString = buildingIndex.ToString() + " " + ruleset;

            sb.AppendLine(buildingString);
            sb.AppendLine("-----");

            foreach (ShapeTest shapeTest in shapeTests)
            {
                List<OperationTest> operationTests = shapeTest.operationTests;

                string shapeName = shapeTest.shapeName;

                sb.AppendLine(shapeName);
                sb.AppendLine("-----");

                foreach (OperationTest operationTest in operationTests)
                {
                    string operation = operationTest.operation;
                    string part = operationTest.part;
                    
                    List<bool> operationTestResults = operationTest.result;

                    string s = operation + " " + part + ": ";

                    foreach(bool individualTest in operationTestResults)
                    {
                        string ss = s + individualTest;
                        sb.AppendLine(ss);
                    }
                }

                sb.AppendLine("-----");
            }

            sb.AppendLine("-----");

        }


        string final = sb.ToString();
        Debug.Log(final);

        //allLines.ForEach(line =>
        //{
        //    sb.AppendLine(string.Join(",", line));
        //});


        return "";
    }
}
