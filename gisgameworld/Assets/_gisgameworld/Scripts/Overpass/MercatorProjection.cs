using System;
using UnityEngine;

public static class MercatorProjection
{
    //private static readonly double R_MAJOR = 6378137.0;
    //private static readonly double R_MINOR = 6356752.3142;
    //private static readonly double RATIO = R_MINOR / R_MAJOR;
    //private static readonly double ECCENT = Math.Sqrt(1.0 - (RATIO * RATIO));
    //private static readonly double COM = 0.5 * ECCENT;

    //private static readonly double DEG2RAD = Math.PI / 180.0;
    //private static readonly double RAD2Deg = 180.0 / Math.PI;
    //private static readonly double PI_2 = Math.PI / 2.0;
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


    //public static double lonToX(double lon)
    //{
    //    return R_MAJOR * Mathf.DegToRad(lon);
    //}


    //public static double latToY(double lat)
    //{
    //    lat = Math.Min(89.5, Math.Max(lat, -89.5));
    //    double phi = DegToRad(lat);
    //    double sinphi = Math.Sin(phi);
    //    double con = ECCENT * sinphi;
    //    con = Math.Pow(((1.0 - con) / (1.0 + con)), COM);
    //    double ts = Math.Tan(0.5 * ((Math.PI * 0.5) - phi)) / con;
    //    return 0 - R_MAJOR * Math.Log(ts);
    //}

    //public static double xToLon(double x)
    //{
    //    return RadToDeg(x) / R_MAJOR;
    //}

    //public static double yToLat(double y)
    //{
    //    double ts = Math.Exp(-y / R_MAJOR);
    //    double phi = PI_2 - 2 * Math.Atan(ts);
    //    double dphi = 1.0;
    //    int i = 0;
    //    while ((Math.Abs(dphi) > 0.000000001) && (i < 15))
    //    {
    //        double con = ECCENT * Math.Sin(phi);
    //        dphi = PI_2 - 2 * Math.Atan(ts * Math.Pow((1.0 - con) / (1.0 + con), COM)) - phi;
    //        phi += dphi;
    //        i++;
    //    }
    //    return RadToDeg(phi);
    //}

    //private static double RadToDeg(double rad)
    //{
    //    return rad * RAD2Deg;
    //}

    //private static double DegToRad(double deg)
    //{
    //    return deg * DEG2RAD;
    //}
}