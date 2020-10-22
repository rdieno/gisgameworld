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
    }

    private LevelManager levelManager;
    public LevelManager LevelManager
    {
        get => levelManager;
    }

    private ShapeGrammarProcessor sgProcessor;

    private ShapeGrammarParser sgParser;
    public ShapeGrammarParser SGParser
    {
        get => sgParser;
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

    [SerializeField]
    private TestManager testManager;
    public TestManager TestManager
    {
        get => testManager;
    }

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
        testManager = new TestManager(this);
        sgParser = new ShapeGrammarParser();
        sgProcessor = new ShapeGrammarProcessor(this);
        
        //StartCoroutine(dataManager.GetData(true, 1.0f));
        //levelManager.ProcessData(dataManager.Data, dataManager.Info);
        //dataManager.HasLoadedData = true;
        //StartCoroutine(sgProcessor.RunOperationSandboxTest());

        isLowMemory = false;
        Application.lowMemory += () => { isLowMemory = true; };
    }

    private IEnumerator RetrieveAndProcessNewData(bool useSavedData = false, float boundsScale = 1.0f)
    {
        yield return StartCoroutine(dataManager.GetData(useSavedData, boundsScale));
        levelManager.ProcessData(dataManager.Data, dataManager.Info);
        dataManager.HasLoadedData = true;
    }

    private IEnumerator RetrieveAndProcessNewDataFromLocation(Location location, float boundsScale)
    {
        yield return StartCoroutine(dataManager.GetDataWithLocation(location, boundsScale));
        levelManager.ProcessData(dataManager.Data, dataManager.Info);
        levelManager.CurrentLocation = location;
        dataManager.HasLoadedData = true;
    }

    public IEnumerator RetrieveAndDisplayBuildingLots()
    {
        levelManager.ConstructLevelFromFile();
        yield return null;
    }

    public IEnumerator GenerateWithCurrentLocation(float boundsScale)
    {
        bool useSavedData = false;

        Debug.Log("retrieve and process new data");
        yield return StartCoroutine(RetrieveAndProcessNewData(useSavedData, boundsScale));

        Debug.Log("generate buildings");
        yield return StartCoroutine(GenerateBuildings());
    }

    public IEnumerator GenerateWithLocation(Location location, float boundsScale)
    {
        Debug.Log("retrieve and process new data");
        yield return StartCoroutine(RetrieveAndProcessNewDataFromLocation(location, boundsScale));

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
        yield return StartCoroutine(sgProcessor.ProcessBuildings());

        Debug.Log("add buildings to level");
        levelManager.AddBuildingsToLevel(true);

        yield return null;
    }

    public IEnumerator ClearCurrentLevel()
    {
        levelManager.ClearLevel();
        dataManager.ClearData();

        dataManager.HasLoadedData = false;

        System.GC.Collect();

        yield return null;
    }


    public void HideLevel()
    {
        level.SetActive(false);
    }

    public void ShowLevel()
    {
        level.SetActive(true);
    }

    void Update() { }
}
