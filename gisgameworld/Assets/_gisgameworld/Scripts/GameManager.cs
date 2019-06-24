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

        //sgProcessor.RetrieveBuilding(1, true);
        sgProcessor.CreateTestSquare();
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
        sgProcessor.RunAdvancedOperationExample();

        //Task fetchData = new Task(dataManager.GetData(true));
        //fetchData.Finished += delegate (bool manual)
        //{
        //    // process data
        //    levelManager.ProcessData(dataManager.Data, dataManager.Info);

        //    // save data to file
        //    dataManager.SaveData();
        //};


    }

    void Update()
    {
       
    }
}
