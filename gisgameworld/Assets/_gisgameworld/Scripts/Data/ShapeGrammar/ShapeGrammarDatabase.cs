using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShapeGrammarDatabase", menuName = "Custom/ShapeGrammarDatabase", order = 1)]
public class ShapeGrammarDatabase : ScriptableObject
{
    public ShapeGrammarData[] shapeGrammarData;
}
