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

    [SerializeField]
    private GameObject buildingPrefab = null;
    public GameObject BuildingPrefab
    {
        get => buildingPrefab;
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

    private ShapeGrammarParser sgParser;
    public ShapeGrammarParser SGParser
    {
        get => sgParser;
        //set => dataManager = value;
    }

    [SerializeField]
    private ShapeGrammarDatabase sgDatabase;
    public ShapeGrammarDatabase SGDatabase
    {
        get => sgDatabase;
    }

    void Awake()
    {
        DontDestroyOnLoad(this);
    }

    void Start()
    {
        dataManager = new DataManager(this);
        levelManager = new LevelManager(this);
        sgParser = new ShapeGrammarParser();
        sgProcessor = new ShapeGrammarProcessor(this);

        //levelManager.ConstructLevelFromFile();

        //dataManager.LoadData();
        //levelManager.ClassifyBuildings();

        //sgProcessor.ProcessBuildingsWithRuleset("simple-building-1", 100);
        //sgProcessor.ProcessBuildings();
        //sgProcessor.ProcessBuildingsRange(0, 25);
        //levelManager.AddBuildingsToLevel();

        //OSMData d = dataManager.Data;

        //List<OSMElement> eles = d.elements;

        //OSMElement e = null;

        //List<OSMElement> multis = new List<OSMElement>();

        //for (int j = 0; j < eles.Count; j++)
        //{
        //    Dictionary<string, string> tags = eles[j].tags;

        //    if(tags != null)
        //    {
        //        foreach (KeyValuePair<string, string> tag in tags)
        //        {
        //            if (tag.Key == "type" && tag.Value == "multipolygon")
        //            {
        //                multis.Add(eles[j]);
        //                //break;
        //                // int f = 0;
        //            }

        //        }
        //    }

        //}


        //int i = 0;

        //sgProcessor.StairFixTest();

        //sgProcessor.TaperFixTest();

        //sgProcessor.ProcessBuildings();
        //sgProcessor.ProcessBuildingsRange(200, 400);
        //levelManager.AddBuildingsToLevel();
        //levelManager.CombineBuildingMeshes();

        //levelManager.RetrieveBuilding(1, true);
        //levelManager.CreateTestSquare(20, 20);
        ////SGOperationDictionary simpleTestRuleset = sgparser.ParseRuleFile("operation-proc-test.cga");
        //SGOperationDictionary simpleTestRuleset = sgparser.ParseRuleFile("simple-building-1.cga");

        //Building currentBuilding = LevelManager.CurrentBuilding;

        //Dictionary<string, List<Shape>> processedRuleset = sgProcessor.ProcessRuleset(currentBuilding.Root, simpleTestRuleset);


        //currentBuilding.UpdateProcessedBuilding(processedRuleset);

        //sgProcessor.CompLocalTranformFixTest();
        //sgProcessor.SplitOffsetFixTest();
        //sgProcessor.RotateNormalsFixTest();
        //sgProcessor.TaperFixTest();
        //sgProcessor.StairFixTest();
        //sgProcessor.CompFixTest();

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



        //StartCoroutine(RetrieveAndProcessNewData());

    }

    private IEnumerator RetrieveAndProcessNewData()
    {
        yield return StartCoroutine(dataManager.GetData());
        levelManager.ProcessData(dataManager.Data, dataManager.Info);
        //dataManager.SaveData();
        dataManager.HasLoadedData = true;
    }


    public IEnumerator RetrieveAndDisplayBuildingLots()
    {
        levelManager.ConstructLevelFromFile();
        yield return null;
        //yield return StartCoroutine(dataManager.GetData());
        //levelManager.ProcessData(dataManager.Data, dataManager.Info);
        ////dataManager.SaveData();
        //dataManager.HasLoadedData = true;
    }

    public IEnumerator GenerateWithCurrentLocation()
    {
        Debug.Log("retrieve and process new data");
        yield return StartCoroutine(RetrieveAndProcessNewData());

        Debug.Log("generate buildings");
        yield return StartCoroutine(GenerateBuildings());
    }


    private IEnumerator GenerateBuildings()
    {
        if(!dataManager.HasLoadedData)
        {
            yield break;
        }

        Debug.Log("classify buildings");

        levelManager.ClassifyBuildings();


        Debug.Log("process buildings");

        sgProcessor.ProcessBuildings();

        Debug.Log("add buildings to level");

        levelManager.AddBuildingsToLevel();

        yield return null;
    }


    void Update()
    {

        

        //Debug.Log(Input.mousePosition + " | " + Camera.main.scaledPixelHeight + ", " + Camera.main.scaledPixelWidth);

        //movementController.HandleInput(Input.touches);

        //foreach (Touch touch in Input.touches)
        //{
        //    if (touch.phase == TouchPhase.Began)
        //    {
        //        // Construct a ray from the current touch coordinates
        //        Ray ray = __Camera__.main.ScreenPointToRay(touch.position);
        //        if (Physics.Raycast(ray))
        //        {
        //            // Create a particle if hit
        //            Instantiate(particle, transform.position, transform.rotation);
        //        }
        //    }
        //}
    }
}
