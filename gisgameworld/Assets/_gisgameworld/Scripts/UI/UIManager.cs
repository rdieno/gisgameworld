using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour
{
    [Header("General")]
    [SerializeField]
    private GameManager manager = null;

    [Header("Menu Scene")]
    [SerializeField]
    private GameObject menuSceneObject = null;
    [SerializeField]
    private Button useCurrentLocationButton = null;
    [SerializeField]
    private Button buildingLotsButton = null;
    //[SerializeField]
    //private Button useSavedLocation;

    [Header("Loading Scene")]
    [SerializeField]
    private GameObject loadingSceneObject = null;

    [Header("Game Scene")]
    [SerializeField]
    private GameObject touchManager;

    void Start()
    {
        if(!menuSceneObject.activeInHierarchy)
            menuSceneObject.SetActive(true);

        //if(touchManager.activeInHierarchy)
        //{
        //    touchManager.SetActive(false);
        //}

        useCurrentLocationButton.onClick.AddListener(() => { manager.StartCoroutine(OnClickCallback(manager.GenerateWithCurrentLocation())); });
        buildingLotsButton.onClick.AddListener(() => { manager.StartCoroutine(OnClickCallback(manager.RetrieveAndDisplayBuildingLots())); });
    }
    
    private IEnumerator OnClickCallback(IEnumerator routine)
    {
        menuSceneObject.SetActive(false);
        loadingSceneObject.SetActive(true);
        yield return manager.StartCoroutine(routine);

        Debug.Log("loading scene disabled");
        loadingSceneObject.SetActive(false);
       // touchManager.SetActive(true);
    }
}
