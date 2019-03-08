using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public DataManager dataManager;
    public ShapeGrammarManager shapeGrammarManager;

    void Awake()
    {
        DontDestroyOnLoad(this);
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(dataManager.GetDataWithCurrentLocation());

        shapeGrammarManager.ProcessData(dataManager.Data);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
