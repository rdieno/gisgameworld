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
    [SerializeField]
    private Button testingSceneButton = null;

    [Header("Generate Scene")]
    [SerializeField]
    private GameObject generateSceneObject = null;
    [SerializeField]
    private GenerateSelectionViewController generateSelectionView = null;
    [SerializeField]
    private Button generateSceneBackButton = null;
    [SerializeField]
    private ToggleGroup boundsScaleSelector = null;
    [SerializeField]
    private Slider boundsScaleSlider = null;


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
    [SerializeField]
    private Button gameSceneSceneBackButton = null;

    [Header("Testing Scene")]
    [SerializeField]
    private GameObject testingSceneObject = null;
    [SerializeField]
    private TextMeshProUGUI testcaseInfoText = null;

    void Start()
    {
        isSettingsOpen = false;

        if (!menuSceneObject.activeInHierarchy)
            menuSceneObject.SetActive(true);

        generateSceneButton.onClick.AddListener(() => { manager.StartCoroutine(SwitchScenes(menuSceneObject, generateSceneObject, LoadGenerateLocationDataFromFile())); });

        viewSceneButton.onClick.AddListener(() => { manager.StartCoroutine(SwitchScenes(menuSceneObject, viewSceneObject, LoadViewLocationDataFromFile())); });

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

        gameSceneSceneBackButton.onClick.AddListener(() =>
        {
            pausedObject.SetActive(false);
            playingObject.SetActive(true);

            manager.StartCoroutine(SwitchScenes(gameSceneObject, menuSceneObject, manager.ClearCurrentLevel()));
        });


        testingSceneButton.onClick.AddListener(() =>
        {
            manager.StartCoroutine(SwitchScenes(menuSceneObject , testingSceneObject, manager.TestManager.RunTests()));
        });

        
        UpdateExtraText("");
        saveStatusText.text = "";
        testcaseInfoText.text = "";
        
    }

    private IEnumerator OnClickCallback(IEnumerator routine)
    {
        menuSceneObject.SetActive(false);
        loadingSceneObject.SetActive(true);
        yield return manager.StartCoroutine(routine);

        Debug.Log("loading scene disabled");
        loadingSceneObject.SetActive(false);
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

    public IEnumerator GenerateBuildings(bool useCurrentLocation, Location location = null, float boundsScale = 1.0f)
    {
        manager.ClearCurrentLevel();
        manager.HideLevel();

        manager.UIManager.UpdateExtraText("");

        if (!useCurrentLocation)
        {
            yield return manager.StartCoroutine(SwitchScenes(generateSceneObject, loadingSceneObject, manager.GenerateWithLocation(location, boundsScale)));
        }
        else
        {
            yield return manager.StartCoroutine(SwitchScenes(generateSceneObject, loadingSceneObject, manager.GenerateWithCurrentLocation(boundsScale)));
        }

        manager.ShowLevel();
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
    }

    public void ToggleSettingsPanel()
    {
        if(pausedObject.activeInHierarchy)
        {
            manager.ShowLevel();
            pausedObject.SetActive(false);
        }
        else
        {
            manager.HideLevel();
            saveButton.interactable = true;
            nameInputField.text = "";
            saveStatusText.text = "";
            pausedObject.SetActive(true);
        }
    }
    
    public float GetSelectedBoundsScale()
    {
        float boundsScale = 1.0f;

        foreach (Toggle t in boundsScaleSelector.ActiveToggles())
        {
            if (t != null)
            {
                BoundsScale b = t.gameObject.GetComponent<BoundsScale>();
                if (b != null)
                {
                    boundsScale = b.scale;
                }
            }
        }

        return boundsScale;
    }

    public void SetTestcaseInfoText(string text)
    {
        this.testcaseInfoText.text = text;
    }
}
