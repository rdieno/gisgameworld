using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShapeGrammarData", menuName = "Custom/ShapeGrammarData", order = 1)]
public class ShapeGrammarData : ScriptableObject
{
    public new string name;

    public int minSides;

    public int maxSides;

    public float minArea;

    public float maxArea;

    public bool canBeConcave;

    public int score;

    public float controlSize;
}
