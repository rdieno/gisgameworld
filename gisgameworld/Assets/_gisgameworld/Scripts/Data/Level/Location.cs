using UnityEngine;
using System.Collections;

public class Location
{
    public string id;
    public string name;
    public Coordinate coord;

    public Location(string name, Coordinate coord)
    {
        SetName(name);
        this.coord = coord;
    }

    public void SetName(string name)
    {
        string id = name;
        foreach (char c in System.IO.Path.GetInvalidFileNameChars())
        {
            id = id.Replace(c, '_');
        }

        this.id = id;
        this.name = name;
    }
}
