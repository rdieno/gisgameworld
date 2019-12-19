using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour
{
    [Header("General")]
    [SerializeField]
    private GameManager manager;

    [Header("Menu Scene")]
    [SerializeField]
    private GameObject menuSceneObject;
    [SerializeField]
    private Button useCurrentLocationButton;
    //[SerializeField]
    //private Button useSavedLocation;

    [Header("Loading Scene")]
    [SerializeField]
    private GameObject loadingSceneObject;

    

    void Start()
    {
        if(!menuSceneObject.activeInHierarchy)
            menuSceneObject.SetActive(true);

        useCurrentLocationButton.onClick.AddListener(() => { manager.StartCoroutine(OnClickCallback(manager.GenerateWithCurrentLocation())); });
    }
    
    private IEnumerator OnClickCallback(IEnumerator routine)
    {
        menuSceneObject.SetActive(false);
        loadingSceneObject.SetActive(true);
        yield return manager.StartCoroutine(routine);

        Debug.Log("loading scene disabled");
        loadingSceneObject.SetActive(false);
    }
}
