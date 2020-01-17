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

    private string cgaRuleset;
    public string CGARuleset { get => cgaRuleset; set => cgaRuleset = value; }

    private int index;
    public int Index { get => index; }

    public BuildingInfo(int sides, Vector3 dimensions, float area, bool isConvex, int index)
    {
        this.sides = sides;
        this.dimensions = dimensions;
        this.area = area;
        this.isConvex = isConvex;
        this.index = index;
    }
}
