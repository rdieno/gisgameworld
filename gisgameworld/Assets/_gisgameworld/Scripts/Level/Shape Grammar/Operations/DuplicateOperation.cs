﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DuplicateOperation : IShapeGrammarOperation
{
    Dictionary<string, string> componentNames; 

    public DuplicateOperation(Dictionary<string, string> componentNames)
    {
        this.componentNames = componentNames;
    }


    ShapeWrapper IShapeGrammarOperation.PerformOperation(List<Shape> input)
    {
        Dictionary<string, List<Shape>> output = new Dictionary<string, List<Shape>>();
        
        foreach(KeyValuePair<string, string> component in componentNames)
        {
            output.Add(component.Value, input);
        }

        return new ShapeWrapper(output, true);
    }
}
