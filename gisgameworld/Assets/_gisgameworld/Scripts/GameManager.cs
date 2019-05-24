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

    void Awake()
    {
        DontDestroyOnLoad(this);
    }

    void Start()
    {
        dataManager = new DataManager(this);
        levelManager = new LevelManager(this);
        sgProcessor = new ShapeGrammarProcessor(this);



        //levelManager.ConstructLevelFromFile();

        dataManager.LoadData();

        sgProcessor.RetrieveBuilding(1);
        sgProcessor.RunSplitExample();

        //// check if level manager was set
        //if (levelManager == null)
        //{
        //    throw new InspectorReferenceMissingException("Level Manager not set");
        //}

        // check if mesh object was set
        //if (meshObject == null)
        //{
        //    throw new InspectorReferenceMissingException("Mesh object not set");
        //}

        // initialize the data manager and level manager
        //dataManager = new DataManager();
        //levelManager = new LevelManager(meshObject);

        // fetch osm data based on current location
        //Task fetchData = new Task(dataManager.GetDataWithCurrentLocation());

        //levelManager.DataManager = DataManager; // TODO: replace by passing through constructor

        //Task fetchData = new Task(dataManager.GetData(false));
        //fetchData.Finished += delegate (bool manual)
        //{
        //    // process data
        //    levelManager.ProcessData(dataManager.Data, dataManager.Info);

        //    //Test();


        //    dataManager.SaveData();
        //};


    }

    void Update()
    {
       
    }

    void Test()
    {

        //LevelManager levelManager = new LevelManager();
        //int i = 0;
    }
}
