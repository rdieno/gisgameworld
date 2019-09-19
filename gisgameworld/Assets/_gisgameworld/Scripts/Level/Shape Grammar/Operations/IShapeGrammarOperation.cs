using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface IShapeGrammarOperation
{
    List<Shape> PerformOperation(List<Shape> shapes);
}
