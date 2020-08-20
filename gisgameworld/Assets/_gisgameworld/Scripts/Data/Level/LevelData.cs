using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

public class LevelData
{
    private string id;
    public string ID
    {
        get => id;
        set => id = value;
    }

    private string name;
    public string Name
    {
        get => name;
        set
        {
            this.name = value;
            string id = value;
            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
            {
                id = id.Replace(c, '_');
            }
            this.id = id;
        }
    }

    private List<Building> buildings;
    public List<Building> Buildings
    {
        get => buildings;
        set => buildings = value;
    }

    private Location location;
    public Location Location
    {
        get => location;
        set => location = value;
    }

    public LevelData()
    {
        this.buildings = null;
    }

    public LevelData(List<Building> buildings)
    {
        this.buildings = buildings;
    }
}
