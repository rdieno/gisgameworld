using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager
{
    private readonly Coordinate DEFAULT_LOCATION = new Coordinate(49.22552f, -123.0064f);

    private const float BOUNDS_HALF_HEIGHT = 0.0075f;
    private const float BOUNDS_HALF_WIDTH = 16.0f * BOUNDS_HALF_HEIGHT / 9.0f;
    private const float BOUNDS_SCALE = 1.0f;

    private GameManager manager;

    private OverpassManager overpassManager;
    private LocationService locationService;

    private bool hasLoadedData;
    public bool HasLoadedData
    {
        get => hasLoadedData;
        set => hasLoadedData = value;
    }

    private OSMData data;
    public OSMData Data
    {
        get => data;
    }

    private OSMInfo info;
    public OSMInfo Info
    {
        get => info;
    }

    private LevelData levelData;
    public LevelData LevelData
    {
        get => levelData;
        set => levelData = value;
    }


    public DataManager(GameManager manager)
    {
        this.manager = manager;
        this.data = null;
        this.info = null;
        this.levelData = null;
        

        this.locationService = new LocationService();
        this.overpassManager = new OverpassManager();

        this.hasLoadedData = false;
    }

    public IEnumerator GetData(bool useSavedData = true, float boundsScale = 1.0f)
    {
        if (useSavedData)
        {
            data = Serializer.DeserializeOSMData("default");
            info = Serializer.DeserializeOSMInfo("default");

            yield return null;
        }
        else
        {
            yield return GetDataWithCurrentLocation(false, boundsScale);
        }
    }

    public IEnumerator GetDataWithCurrentLocation(bool useDefaultLocation = false, float boundsScale = 1.0f)
    {
        Coordinate location = null;

        if (!useDefaultLocation)
        {
            BetterCoroutine getLocationCoroutine = new BetterCoroutine(manager, locationService.GetLocation());
            yield return getLocationCoroutine.result;

            location = (Coordinate)getLocationCoroutine.result;
            if (location != null)
            {
                location = (Coordinate)getLocationCoroutine.result;

                Debug.Log("Data Manager: using location: " + location.ToString());
            }
            else
            {
                Debug.Log("Data Manager: Could not get device location, using saved location");
                location = DEFAULT_LOCATION;
            }
        }
        else
        {
            location = DEFAULT_LOCATION;
        }

        manager.LevelManager.CurrentLocation = new Location(@"default", location);

        info = new OSMInfo(calculateOrigin(location), CreateBoundingBoxFromCoordinate(location, boundsScale));

        BetterCoroutine fetchDataCoroutine = new BetterCoroutine(manager, overpassManager.RunQuery(info));
        yield return fetchDataCoroutine.result;
        OSMData osmData = (OSMData)fetchDataCoroutine.result;
        if (osmData != null)
        {
            data = (OSMData)fetchDataCoroutine.result;
        }
        else
        {
            Debug.LogError("Data Manager: Could not fetch OSM data");
            yield break;
        }
    }

    public IEnumerator GetDataWithLocation(Location location, float boundsScale)
    {
        Coordinate locationCoords = location.coord;

        info = new OSMInfo(calculateOrigin(locationCoords), CreateBoundingBoxFromCoordinate(locationCoords, boundsScale));

        BetterCoroutine fetchDataCoroutine = new BetterCoroutine(manager, overpassManager.RunQuery(info));
        yield return fetchDataCoroutine.result;
        OSMData osmData = (OSMData)fetchDataCoroutine.result;
        if (osmData != null)
        {
            data = (OSMData)fetchDataCoroutine.result;
        }
        else
        {
            Debug.LogError("Data Manager: Could not fetch OSM data");
            yield break;
        }
    }

    private Region CreateBoundingBoxFromCoordinate(Coordinate location, float boundsScale)
    {
        float latitude = location.latitude;
        float longitude = location.longitude;

        float halfWidth = BOUNDS_HALF_WIDTH * boundsScale;
        float halfHeight = BOUNDS_HALF_HEIGHT * boundsScale;

        Coordinate topLeft = new Coordinate(latitude + halfHeight, longitude + halfWidth);
        Coordinate topRight = new Coordinate(latitude + halfHeight, longitude - halfWidth);
        Coordinate bottomRight = new Coordinate(latitude - halfHeight, longitude - halfWidth);
        Coordinate bottomLeft = new Coordinate(latitude - halfHeight, longitude + halfWidth);

        return new Region(topLeft, topRight, bottomRight, bottomLeft);
    }

    private Vector3 calculateOrigin(Coordinate location)
    {
        float conversionFactor = MercatorProjection.earthCircumferece(location.latitude);
        return new Vector3(MercatorProjection.lonToX(location.longitude) * conversionFactor, 0, MercatorProjection.latToZ(location.latitude) * conversionFactor);
    }

    public void SaveData()
    {
        if (data != null && info != null && levelData.Buildings != null)
        {
            // write raw osm string and info to file
            Serializer.SerializeOSMData(data);
            Serializer.SerializeOSMInfo(info);

            levelData.Name = @"default";

            // write building data to file
            Serializer.SerializeLevelData(levelData);

            Debug.Log("Data Manager: Saved data");
        }
        else
        {
            Debug.Log("Data Manager: There was a problem saving data to file");
        }
    }

    public void LoadData()
    {
        data = Serializer.DeserializeOSMData();
        info = Serializer.DeserializeOSMInfo();
        levelData = Serializer.DeserializeLevelData(@"default");

        if (data == null || info == null || levelData == null)
        {
            Debug.Log("Data Manager: There was a problem loading building data from file");
        }
        else
        {
            hasLoadedData = true;
            Debug.Log("Data Manager: Loaded data");
        }
    }

    public void SaveLevelData()
    {
        if (levelData.Buildings != null)
        {
            // write building data to file
            Serializer.SerializeLevelData(levelData);

            Debug.Log("Data Manager: Saved data");
        }
        else
        {
            Debug.Log("Data Manager: There was a problem saving data to file");
        }
    }

    public void LoadLevelData(string id)
    {
        levelData = Serializer.DeserializeLevelData(id);

        if (levelData == null)
        {
            Debug.Log("Data Manager: There was a problem loading building data from file");
        }
        else
        {
            hasLoadedData = true;
            Debug.Log("Data Manager: Loaded data");
        }
    }

    public void ClearData()
    {
        levelData = null;
        data = null;
        info = null;
        hasLoadedData = false;
    }

}
