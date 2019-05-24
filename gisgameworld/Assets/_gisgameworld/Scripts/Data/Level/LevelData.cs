using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

public class LevelData
{
    private List<Building> buildings;
    public List<Building> Buildings
    {
        get => buildings;
        set => buildings = value;
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
