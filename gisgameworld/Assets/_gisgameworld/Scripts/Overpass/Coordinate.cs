using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coordinate
{
    public float latitude;
    public float longitude;

    public Coordinate(float lat, float lon)
    {
        latitude = lat;
        longitude = lon;
    }

    public override string ToString() 
    {
        return "(" + latitude + ", " + longitude + ")";
    }
}
