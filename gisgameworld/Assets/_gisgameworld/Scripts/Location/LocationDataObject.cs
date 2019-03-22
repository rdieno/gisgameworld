using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct BoundingBox
{
    public string name;
    public float minLongitude; // left
    public float minLatitude; // bottom
    public float maxLongitude; // right
    public float maxLatitude; // top
}

[CreateAssetMenu(fileName = "LocationData", menuName = "Custom/LocationData", order = 1)]
public class LocationDataObject : ScriptableObject
{
    [SerializeField]
    public BoundingBox[] areas;
}