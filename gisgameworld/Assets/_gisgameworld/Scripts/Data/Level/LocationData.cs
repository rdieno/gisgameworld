using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LocationData
{
    public List<Location> locations;

    //public LocationData()
    //{

    //}

    public LocationData(Location location)
    {
        if(locations == null)
        {
            locations = new List<Location>() { location };
        }
        else
        {
            locations.Add(location);
        }
    }
}
