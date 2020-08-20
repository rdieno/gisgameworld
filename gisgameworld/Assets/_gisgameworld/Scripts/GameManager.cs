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

    [SerializeField]
    private UIManager uiManager;
    public UIManager UIManager
    {
        get => uiManager;
    }


    //public SelectionViewController svc;

    private bool isLowMemory;
    public bool IsLowMemory
    {
        get => isLowMemory;
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

        isLowMemory = false;
        Application.lowMemory += () => { isLowMemory = true; };

        //Coordinate test = new Coordinate(49.22552f, -123.0064f);
        //string name = "test\"\"<>";

        //Location loc = new Location(name, test);
        //Serializer.SerializeLocation(loc);

        //LocationData ld = Serializer.DeserializeLocations();

        ////int f = 0;

        //svc.PopulateView(ld);
    }

    private IEnumerator RetrieveAndProcessNewData(bool useSavedData = false)
    {
        yield return StartCoroutine(dataManager.GetData(useSavedData));
        levelManager.ProcessData(dataManager.Data, dataManager.Info);
        //dataManager.SaveData();
        dataManager.HasLoadedData = true;
    }

    private IEnumerator RetrieveAndProcessNewDataFromLocation(Location location)
    {
        yield return StartCoroutine(dataManager.GetDataWithLocation(location));
        levelManager.ProcessData(dataManager.Data, dataManager.Info);
        //dataManager.SaveData();
        levelManager.CurrentLocation = location;
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
        bool useSavedData = false;

//#if UNITY_EDITOR
//        useSavedData = true;
//        //useSavedData = false;
//#endif

        Debug.Log("retrieve and process new data");
        yield return StartCoroutine(RetrieveAndProcessNewData(useSavedData));

        Debug.Log("generate buildings");
        yield return StartCoroutine(GenerateBuildings());
    }

    public IEnumerator GenerateWithLocation(Location location)
    {
        Debug.Log("retrieve and process new data");
        yield return StartCoroutine(RetrieveAndProcessNewDataFromLocation(location));

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
        //sgProcessor.ProcessBuildings(398, true);

        yield return StartCoroutine(sgProcessor.ProcessBuildings());
        
        //sgProcessor.ProcessBuildingsRange(0, 100);
        //sgProcessor.ProcessBuildingsRange(101, 200);
        //sgProcessor.ProcessBuildingsRange(201, 300);
        //sgProcessor.ProcessBuildingsRange(301, 399);

        //sgProcessor.ProcessBuildingsWithRuleset("simple-temple-2-lg", 250);

        Debug.Log("add buildings to level");

        levelManager.AddBuildingsToLevel(true);

        yield return null;
    }

    //private IEnumerator SaveCurrentBuildings()
    //{
    //    if (!dataManager.HasLoadedData)
    //    {
    //        yield break;
    //    }
        
    //    if(dataManager.LevelData.Buildings == null)
    //    {
    //        yield break;
    //    }


    //    Serializer.SerializeLevelData(dataManager.LevelData);

    //    yield return null;
    //}


    //public IEnumerator SaveBuildings(string name)
    //{



    //    yield return null;
    //}

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
