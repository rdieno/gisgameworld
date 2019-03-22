using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private DataManager dataManager = null;
    [SerializeField]
    private LevelManager levelManager = null;

    void Awake()
    {
        DontDestroyOnLoad(this);
    }

    void Start()
    {
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
        Task fetchData = new Task(dataManager.GetDataWithCurrentLocation());
        fetchData.Finished += delegate (bool manual)
        {
            levelManager.ProcessData(dataManager.Data, dataManager.Bounds);
            //Test();
        };
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
