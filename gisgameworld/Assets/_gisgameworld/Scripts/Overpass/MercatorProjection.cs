using System;
using UnityEngine;

public static class MercatorProjection
{
    private static readonly float EARTH_CIRCUMFERENCE = 40075016.686f;

    // convert osm coords to vector
    public static float[] toPixel(float lon, float lat)
    {
        return new float[] { lonToX(lon), latToZ(lat) };
    }

    // converts vector data to osm coord
    public static float[] toGeoCoord(float x, float z)
    {
        return new float[] { xToLon(x), zToLat(z) };
    }

    // calculates the earth's circumference at a given latitude
    public static float earthCircumferece(float lat)
    {
        return EARTH_CIRCUMFERENCE * Mathf.Cos(lat * Mathf.Deg2Rad);
    }

    public static float lonToX(float lon)
    {
        return (lon + 180.0f) / 360.0f;
    }

    public static float latToZ(float lat)
    {
        float sinLat = Mathf.Sin(lat * Mathf.Deg2Rad);
		return Mathf.Log((1.0f + sinLat) / (1.0f - sinLat)) / (4.0f * Mathf.PI) + 0.5f;
    }

    public static float xToLon(float x)
    {
        return 360.0f * (x - 0.5f);
    }

    public static float zToLat(float z)
    {
        return 360.0f * Mathf.Atan(Mathf.Exp((z - 0.5f) * (2.0f * Mathf.PI))) / Mathf.PI - 90.0f;
    }
}
