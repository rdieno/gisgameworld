using UnityEngine;
using System.Collections;
using Newtonsoft.Json;

public class OSMInfo
{
    public Vector3 origin;

    public Region bounds;

    public OSMInfo(Vector3 origin, Region bounds)
    {
        this.origin = origin;
        this.bounds = bounds;
    }
}
