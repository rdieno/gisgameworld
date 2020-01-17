using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager
{
    private readonly Coordinate DEFAULT_LOCATION = new Coordinate(49.22552f, -123.0064f);
    //private readonly Coordinate DEFAULT_LOCATION = new Coordinate(49.21634f, -122.9632f);

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

    //private Box bounds;
    //public Box Bounds { get { return bounds; } }

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

    public IEnumerator GetData(bool useSavedData = true)
    {
        if (useSavedData)
        {
            bool useStreamingAssets = true;

#if UNITY_EDITOR
            useStreamingAssets = false;
#endif

            data = Serializer.DeserializeOSMData("default", useStreamingAssets);
            info = Serializer.DeserializeOSMInfo("default", useStreamingAssets);

            yield return null;
        }
        else
        {
            yield return GetDataWithCurrentLocation();
        }
    }

    public IEnumerator GetDataWithCurrentLocation(bool useDefaultLocation = false)
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
                //Debug.LogError("Could not get device location");
                //yield break;

                Debug.Log("Data Manager: Could not get device location, using saved location");

                //float testLatitude = 49.22552f;
                //float testLongitude = -123.0064f;
                location = DEFAULT_LOCATION;
            }
        }
        else
        {
            location = DEFAULT_LOCATION;
        }

        info = new OSMInfo(calculateOrigin(location), CreateBoundingBoxFromCoordinate(location));

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

    private Region CreateBoundingBoxFromCoordinate(Coordinate location)
    {
        float latitude = location.latitude;
        float longitude = location.longitude;

        float halfWidth = BOUNDS_HALF_WIDTH * BOUNDS_SCALE;
        float halfHeight = BOUNDS_HALF_HEIGHT * BOUNDS_SCALE;

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
        levelData = Serializer.DeserializeLevelData();

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

}
