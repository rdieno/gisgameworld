using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("General")]
    [SerializeField]
    private GameManager manager = null;

    [Header("Menu Scene")]
    [SerializeField]
    private GameObject menuSceneObject = null;
    [SerializeField]
    private Button generateSceneButton = null;
    [SerializeField]
    private Button viewSceneButton = null;
    //[SerializeField]
    //private Button useSavedLocation;

    [Header("Generate Scene")]
    [SerializeField]
    private GameObject generateSceneObject = null;
    [SerializeField]
    private GenerateSelectionViewController generateSelectionView = null;
    [SerializeField]
    private Button generateSceneBackButton = null;

    [Header("View Scene")]
    [SerializeField]
    private GameObject viewSceneObject = null;
    [SerializeField]
    private ViewSelectionViewController viewSelectionView = null;
    [SerializeField]
    private Button viewSceneBackButton = null;

    [Header("Loading Scene")]
    [SerializeField]
    private GameObject loadingSceneObject = null;
    [SerializeField]
    private TextMeshProUGUI extraInfoText = null;

    [Header("Game Scene")]
    [SerializeField]
    private GameObject touchManager;
    [SerializeField]
    private GameObject gameSceneObject = null;
    [SerializeField]
    private Button settingsButton = null;
    [SerializeField]
    private GameObject playingObject = null;
    [SerializeField]
    private GameObject pausedObject = null;
    [SerializeField]
    private InputField nameInputField = null;
    [SerializeField]
    private Button saveButton = null;
    [SerializeField]
    private TextMeshProUGUI saveStatusText = null;
    private bool isSettingsOpen;

    void Start()
    {
        isSettingsOpen = false;

        if (!menuSceneObject.activeInHierarchy)
            menuSceneObject.SetActive(true);


        generateSceneButton.onClick.AddListener(() => { manager.StartCoroutine(SwitchScenes(menuSceneObject, generateSceneObject, LoadGenerateLocationDataFromFile())); });
      viewSceneButton.onClick.AddListener(() => { manager.StartCoroutine(SwitchScenes(menuSceneObject, viewSceneObject, LoadViewLocationDataFromFile())); });
        //viewSceneButton.onClick.AddListener(() => { manager.StartCoroutine(OnClickCallback(manager.RetrieveAndDisplayBuildingLots())); });

        generateSceneBackButton.onClick.AddListener(() =>
        {
            generateSelectionView.ClearView();
            manager.StartCoroutine(SwitchScenes(generateSceneObject, menuSceneObject, null));
        });

        viewSceneBackButton.onClick.AddListener(() =>
        {
            viewSelectionView.ClearView();
            manager.StartCoroutine(SwitchScenes(viewSceneObject, menuSceneObject, null));
        });


        settingsButton.onClick.AddListener(() => { ToggleSettingsPanel(); });


        saveButton.onClick.AddListener(() =>
        {
            string levelID = string.Empty;

            if (nameInputField.text == "")
            {
                levelID = DateTime.Now.ToString(@"MM\/dd\/yyyy_hh\:mm\:ss_tt");
            }
            else
            {
                levelID = nameInputField.text;
            }

            manager.StartCoroutine(SaveBuildings(levelID));
        });

        UpdateExtraText("");
        saveStatusText.text = "";
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

    private IEnumerator SwitchScenes(GameObject sourceScene, GameObject destScene, IEnumerator routine = null)
    {
        if(sourceScene != null)
        {
            sourceScene.SetActive(false);
        }

        if(destScene != null)
        {
            destScene.SetActive(true);
        }

        if(routine != null)
        {
            yield return manager.StartCoroutine(routine);
        }
    }

    public void UpdateExtraText(string text)
    {
        extraInfoText.text = text;
    }

    private IEnumerator LoadGenerateLocationDataFromFile()
    {
        LocationData locationData = Serializer.DeserializeLocations();
        generateSelectionView.PopulateView(locationData, this, manager);

        yield return null;
    }

    private IEnumerator LoadViewLocationDataFromFile()
    {
        LocationData locationData = Serializer.DeserializeLocations();
        viewSelectionView.PopulateView(locationData, this, manager);

        yield return null;
    }

    public IEnumerator GenerateBuildings(bool useCurrentLocation, Location location = null)
    {
        if(!useCurrentLocation)
        {
            yield return manager.StartCoroutine(SwitchScenes(generateSceneObject, loadingSceneObject, manager.GenerateWithLocation(location)));
        }
        else
        {
            yield return manager.StartCoroutine(SwitchScenes(generateSceneObject, loadingSceneObject, manager.GenerateWithCurrentLocation()));
        }

        yield return manager.StartCoroutine(SwitchScenes(loadingSceneObject, gameSceneObject, null));
    }

    public IEnumerator SaveBuildings(string name)
    {
        LevelData levelData = manager.DataManager.LevelData;
        levelData.Name = name;

        Location currentLocation = manager.LevelManager.CurrentLocation;
        currentLocation.SetName(name);
        levelData.Location = currentLocation;

        saveButton.interactable = false;
        saveStatusText.text = "Saving level data...";
        yield return null;

        Serializer.SerializeLevelData(levelData);
        Serializer.SerializeLocation(levelData.Location);
        saveStatusText.text = "Completed.";
        yield return null;
    }

    public IEnumerator LoadBuildings(string id)
    {
        UpdateExtraText("Loading buildings from file...");
        yield return manager.StartCoroutine(SwitchScenes(viewSceneObject, loadingSceneObject, null));

        LevelData levelData = Serializer.DeserializeLevelData(id);

        if(levelData == null)
        {
            yield break;
        }

        manager.DataManager.LevelData = levelData;
        manager.LevelManager.CurrentLocation = levelData.Location;

        manager.LevelManager.AddBuildingsToLevel(true);


        yield return manager.StartCoroutine(SwitchScenes(loadingSceneObject, gameSceneObject, null));
        //Serializer.SerializeLevelData(levelData);
        //Serializer.SerializeLocation(levelData.Location);
        //saveStatusText.text = "Completed.";
        //saveButton.interactable = false;

        //yield return null;
    }


    public void ToggleSettingsPanel()
    {
        if(isSettingsOpen)
        {
            //playingObject.SetActive(false);
            pausedObject.SetActive(false);
            isSettingsOpen = false;
        }
        else
        {
            saveButton.interactable = true;
            nameInputField.text = "";
            saveStatusText.text = "";
            pausedObject.SetActive(true);
            isSettingsOpen = true;
        }
    }
}
