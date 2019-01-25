using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Coordinate
{
    public float latitude;
    public float longitude;
}

[CreateAssetMenu(fileName = "LocationData", menuName = "Custom/LocationData", order = 1)]
public class LocationDataObject : ScriptableObject
{
    public Coordinate[] coords;
}