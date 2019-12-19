using UnityEngine;
using System.Collections;

public class BuildingInfo
{
    private int sides;
    public int Sides { get => sides; }

    private Vector3 dimensions;
    public Vector3 Dimensions { get => dimensions; }

    private float area;
    public float Area { get => area; }

    private bool isConvex;
    public bool IsConvex { get => isConvex; }
    
    public BuildingInfo(int sides, Vector3 dimensions, float area, bool isConvex)
    {
        this.sides = sides;
        this.dimensions = dimensions;
        this.area = area;
        this.isConvex = isConvex;
    }
}
