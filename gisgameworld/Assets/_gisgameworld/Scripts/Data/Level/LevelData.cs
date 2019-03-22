using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelData
{
    private List<Building> buildings;

    public List<Building> Buildings { get { return buildings; } }

    public LevelData(List<Building> b)
    {
        buildings = b;
    }



}
