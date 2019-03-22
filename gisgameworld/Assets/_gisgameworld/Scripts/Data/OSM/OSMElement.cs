using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

public class OSMElement
{
    public string type;

    public long id;

    [JsonProperty("bounds")]
    public OSMBounds bounds;

    [JsonProperty("nodes")]
    public List<long> nodes;

    [JsonProperty("geometry")]
    public List<OSMCoordinate> geometry;

    [JsonProperty("tags")]
    public Dictionary<string, string> tags;

    [JsonProperty("element")]
    public List<OSMMember> members;
}
