using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface IShapeGrammarOperation
{
    ShapeWrapper PerformOperation(List<Shape> input);
}
