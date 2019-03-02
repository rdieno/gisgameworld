using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

public class OSMData
{
    public double version;
    public string generator;
    public Osm3s osm3s;

    [JsonProperty("elements")]
    public List<OSMElement> elements;
}
