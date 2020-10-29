using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class TestManager
{
    private GameManager manager = null;
    private ShapeGrammarDatabase sgDatabase;

    private List<KeyValuePair<string, Mesh>> controlBuildings = null;

    public TestManager(GameManager manager)
    {
        this.manager = manager;
        sgDatabase = manager.SGDatabase;


    }

    // note: designed to be run on PC, not supported on mobile
    public IEnumerator RunTests()
    {
        // Outputs screenshots to file of buildings built with rulesets with square footprints
        // used for comparing to live buildings to help find visual errors
        yield return CreateControlScreenshots();


        // outputs screenshots of random sample of generated buildings with live real-world GIS footprints
        // currently uses default location
        // parameter can set sample amount, default is 50 
        yield return CreateRandomSampleScreenshots();
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

                ScreenCapture.CaptureScreenshot(filename);
                yield return null;

                index++;
            }
        }

        yield return null;
    }

    public IEnumerator CreateRandomSampleScreenshots(int amount = 50)
    {
        // perform building generation with default location
        yield return manager.GenerateWithCurrentLocation(1f, false);

        //find 50 random unique numbers between 0 and number of buildings -1 inclusive

        List<Building> buildings = manager.DataManager.LevelData.Buildings;

        int[] randomSample = new int[amount];

        // The Knuth algorithm to find random sample of indices between 0 and building count -1
        // retrieved from: https://stackoverflow.com/questions/1608181/unique-random-numbers-in-an-integer-array-in-the-c-programming-language
        int buildingCount = buildings.Count - 1; 
        int sampleCount = amount;
        int x, y;

        y = 0;

        for (x = 0; x < buildingCount && y < sampleCount; ++x) {
            int indicesRemaining = buildingCount - x;
            int indicesToFind = sampleCount - y;

            int random = UnityEngine.Random.Range(0, 400);

            int prob = random % indicesRemaining;

            if (prob < indicesToFind)
                randomSample[y++] = x;
        }

        // iterate over buildings using 50 random indices

        GameObject levelObject = manager.Level;
        MeshFilter meshFilter = levelObject.GetComponent<MeshFilter>();
        UIManager uiManager = manager.UIManager;

        for (int i = 0; i < amount; i++)
        {
            int currentRandomIndex = randomSample[i];
            Building currentBuilding = buildings[currentRandomIndex];

            string rulesetName = currentBuilding.Info.CGARuleset;
            Mesh currentBuildingMesh = currentBuilding.Mesh;

            Shape root = currentBuilding.Root;
            Mesh FootprintMesh = new Mesh();
            FootprintMesh.vertices = root.Vertices;
            FootprintMesh.triangles = root.Triangles;
            FootprintMesh.normals = root.Normals;
            
            CombineInstance[] combine = new CombineInstance[buildings.Count];
            combine[0].mesh = FootprintMesh;
            combine[0].transform = Matrix4x4.zero;
            combine[1].mesh = currentBuildingMesh;
            combine[1].transform = Matrix4x4.zero;
            
            meshFilter.mesh = new Mesh();
            meshFilter.mesh.CombineMeshes(combine, true, false);

            uiManager.SetTestcaseInfoText(rulesetName);

            string filename = "Tests/Random/" + i + "-Random-Building_" + currentRandomIndex + ".png"; 

            ScreenCapture.CaptureScreenshot(filename);

            yield return null;
        }

        yield return null;
    }


    public IEnumerator TakeTestScreenshot()
    {
        string id = DateTime.Now.ToString(@"MM-dd-yyyy_hh-mm-ss_tt");

        string filename = "Tests/TestScreenshot_" + id + ".png";

        ScreenCapture.CaptureScreenshot(filename);

        yield return null;
    }
}
