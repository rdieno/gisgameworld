using UnityEngine;
using System.Collections;

public class UnityBuilding : MonoBehaviour
{
    public int sides;
    
    public Vector3 dimensions;
    
    public float area;
    
    public bool isConvex;

    public string cgaRuleset;

    public int index;

    public void SetValues(BuildingInfo info)
    {
        this.sides = info.Sides;
        this.dimensions = info.Dimensions;
        this.area = info.Area;
        this.isConvex = info.IsConvex;
        this.cgaRuleset = info.CGARuleset;
        this.index = info.Index;
    }
}
