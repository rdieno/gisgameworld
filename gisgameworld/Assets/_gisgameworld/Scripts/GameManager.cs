using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private GameObject level = null;
    public GameObject Level
    {
        get => level;
    }


    private DataManager dataManager;
    public DataManager DataManager
    {
        get => dataManager;
        //set => dataManager = value;
    }

    private LevelManager levelManager;
    public LevelManager LevelManager
    {
        get => levelManager;
        //set => dataManager = value;
    }

    private ShapeGrammarProcessor sgProcessor;

    private ShapeGrammarParser sgparser;
    public ShapeGrammarParser SGParser
    {
        get => sgparser;
        //set => dataManager = value;
    }

    void Awake()
    {
        DontDestroyOnLoad(this);
    }

    void Start()
    {
        dataManager = new DataManager(this);
        levelManager = new LevelManager(this);
        sgProcessor = new ShapeGrammarProcessor(this);
        sgparser = new ShapeGrammarParser();

        //levelManager.ConstructLevelFromFile();

        dataManager.LoadData();
        //levelManager.RetrieveBuilding(1, true);
        //levelManager.CreateTestSquare(20, 20);
        ////SGOperationDictionary simpleTestRuleset = sgparser.ParseRuleFile("operation-proc-test.cga");
        //SGOperationDictionary simpleTestRuleset = sgparser.ParseRuleFile("simple-building-1.cga");

        //Building currentBuilding = LevelManager.CurrentBuilding;

        //Dictionary<string, List<Shape>> processedRuleset = sgProcessor.ProcessRuleset(currentBuilding.Root, simpleTestRuleset);


        //currentBuilding.UpdateProcessedBuilding(processedRuleset);

        //sgProcessor.CompLocalTranformFixTest();


        //Material[] mats = new Material[] {
        //    //Resources.Load("Materials/TestMaterialBlue") as Material,
        //    //Resources.Load("Materials/TestMaterialRed") as Material,
        //    //Resources.Load("Materials/TestMaterialYellow") as Material,
        //    //Resources.Load("Materials/TestMaterialPink") as Material,
        //    //Resources.Load("Materials/TestMaterialOrange") as Material,
        //    //Resources.Load("Materials/TestMaterialGreen") as Material,
        //    Resources.Load("Materials/TestMaterialPurple") as Material,
        //    //Resources.Load("Materials/TestMaterialLightGreen") as Material,
        //    //Resources.Load("Materials/TestMaterialLightBlue") as Material,
        //};

        //currentBuilding.Materials = mats;

        //levelManager.SetCurrentBuilding(currentBuilding);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //levelManager.CreateTestSquare(20, 20);
            levelManager.RetrieveBuilding(4, true);
            //levelManager.RetrieveBuilding(4, true);

            //SGOperationDictionary simpleTestRuleset = sgparser.ParseRuleFile("operation-proc-test.cga");
            SGOperationDictionary simpleTestRuleset = sgparser.ParseRuleFile("WorkingClass/simple-building-1.cga");

            Building currentBuilding = LevelManager.CurrentBuilding;

            Dictionary<string, List<Shape>> processedRuleset = sgProcessor.ProcessRuleset(currentBuilding.Root, simpleTestRuleset);

            //List<Shape> a = processedRuleset["FirstFloor"];
            //List<Shape> b = processedRuleset["SecondFloorBaseA"];
            ////List<Shape> c = processedRuleset["HouseAA"];

            //foreach (Shape s in a)
            //{
            //    s.Debug_DrawOrientation(25.0f);
            //}

            //foreach (Shape s in b)
            //{
            //    s.Debug_DrawOrientation();
            //}

            //foreach (Shape s in c)
            //{
            //    s.Debug_DrawOrientation();
            //}

            currentBuilding.UpdateProcessedBuilding(processedRuleset);


            Material[] mats = new Material[] {
                Resources.Load("Materials/TestMaterial_Blank") as Material,
                //Resources.Load("Materials/TestMaterialBlue") as Material,
                //Resources.Load("Materials/TestMaterialRed") as Material,
                //Resources.Load("Materials/TestMaterialYellow") as Material,
                //Resources.Load("Materials/TestMaterialPink") as Material,
                //Resources.Load("Materials/TestMaterialOrange") as Material,
                //Resources.Load("Materials/TestMaterialGreen") as Material,
                //Resources.Load("Materials/TestMaterialPurple") as Material,
                //Resources.Load("Materials/TestMaterialLightGreen") as Material,
                //Resources.Load("Materials/TestMaterialLightBlue") as Material,
            };

            currentBuilding.Materials = mats;

            levelManager.SetCurrentBuilding(currentBuilding);

        }
    }
}
