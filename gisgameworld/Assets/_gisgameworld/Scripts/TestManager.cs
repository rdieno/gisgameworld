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
            MeshRenderer renderer = levelObject.GetComponent<MeshRenderer>();
            MeshFilter filter = levelObject.GetComponent<MeshFilter>();

            UIManager uiManager = manager.UIManager;
            
            int index = 0;
            foreach(KeyValuePair<string, Mesh> building in controlBuildings)
            {
                filter.mesh = building.Value;
                uiManager.SetTestcaseInfoText(building.Key);
                
                string name = building.Key;
                string filename = "Tests/" + index + "_Control-Building_" + name + ".png";

                ScreenCapture.CaptureScreenshot(filename);
                yield return null;

                index++;
            }
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
