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

        //dataManager.LoadData();
        //sgProcessor.RetrieveBuilding(1, true);
        //sgProcessor.RetrieveBuilding(17, true);
        //sgProcessor.RetrieveBuilding(23, true);


        //sgProcessor.CreateTestSquare(5);


        //////sgProcessor.RunSplitExample();
        //////sgProcessor.RunG3Example();
        //////sgProcessor.RunG3ExtrudeExample();
        //////sgProcessor.RunSplitExample(true);
        //sgProcessor.RunMultiSplitExample();
        ////sgProcessor.RunFaceSplitExample();

        ////sgProcessor.DrawVerts();
        ////sgProcessor.ClockwiseCheck();
        ////sgProcessor.OutwardNormals();
        ////sgProcessor.DrawNormals();

        //sgProcessor.RunCompExample();
        //sgProcessor.RunTaperExample();
        //sgProcessor.RunAdvancedTaperExample();
        //sgProcessor.RunAdvancedOperationExample();
        //sgProcessor.RoofShedOperationExample();
        //sgProcessor.RunAdvancedSplitExample();
        //sgProcessor.RunSuperAdvancedSplitExample();
        //sgProcessor.RunAdvancedOperationSplitAxisExample();

        //sgProcessor.RunAdvancedOffsetExample();
        //sgProcessor.RunStairExample();
        //sgProcessor.RunAdvancedStairExample();
        //sgProcessor.RunAdvancedPyramidTest();
        //sgProcessor.RunTriangulatePolygonWithHoles();

        //sgProcessor.RunTaperPyramidTest();

        //sgProcessor.RunFaceSelectionTest();
        //sgProcessor.RunTaperDoubleCheckTest();
        //sgProcessor.RunRoofShedDoubleCheckTest();
        //sgProcessor.RunOffsetDoubleCheckTest();

        //sgProcessor.RunSplitDoubleCheckTest();
        //sgProcessor.RunOffsetTaperTest();

        //sgProcessor.RunRotateTranslateScaleTest();
        //sgProcessor.RunAdvancedScaleTranslateTest();

        //Task fetchData = new Task(dataManager.GetData(true));
        //fetchData.Finished += delegate (bool manual)
        //{
        //    // process data
        //    levelManager.ProcessData(dataManager.Data, dataManager.Info);

        //    // save data to file
        //    dataManager.SaveData();
        //};

        //sgProcessor.RunSimpleRulesTest();
        //sgProcessor.SimpleTempleDesignTest();
        //sgProcessor.RunSplitRatioTest();
        //sgProcessor.DetermineSplitRatioSizesTest();

        // handling rule inputs

        //sgProcessor.RunSplitRatioTermsTest();
        //sgProcessor.RunCompMappingTest();
        //sgProcessor.RunExtrudeInputTest();
        //sgProcessor.RunOffsetInputTest();

        string ruleFilenameToTest = "split-test.cga";

        sgparser.ParseRuleFile(ruleFilenameToTest);
    }

    void Update()
    {
       
    }
}
