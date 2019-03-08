using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public OverpassManager overpassManager;
    public LocationService locationService;

    private OSMData data;
    public OSMData Data { get { return data; } }

    void Awake()
    {
        DontDestroyOnLoad(this);
    }

    void Start()
    {

    }

    void Update()
    {

    }

    public IEnumerator GetDataWithCurrentLocation()
    {
        BetterCoroutine getLocationCoroutine = new BetterCoroutine(this, locationService.GetLocation());
        yield return getLocationCoroutine.result;
        Coordinate location = (Coordinate)getLocationCoroutine.result;
        if (location != null)
        {
            location = (Coordinate)getLocationCoroutine.result;
        }
        else
        {
            //Debug.LogError("Could not get device location");
            //yield break;

            Debug.Log("Could not get device location, using saved location");

            //float testLatitude = 49.22552f;
            //float testLongitude = -123.0064f;
            location = new Coordinate(49.22552f, -123.0064f);
        }

        BetterCoroutine fetchDataCoroutine = new BetterCoroutine(this, overpassManager.RunQuery(location));
        yield return fetchDataCoroutine.result;
        OSMData osmData = (OSMData)fetchDataCoroutine.result;
        if (osmData != null)
        {
            data = (OSMData)fetchDataCoroutine.result;
        }
        else
        {
            Debug.LogError("Could not fetch OSM data");
            yield break;
        }
    }
}
