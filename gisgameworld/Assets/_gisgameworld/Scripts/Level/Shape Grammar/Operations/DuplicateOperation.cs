using UnityEngine;
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
            List<Shape> copy = new List<Shape>();

            foreach (Shape shape in input)
            {
                copy.Add(new Shape(shape));
            }


            //output.Add(component.Value, input);
            output.Add(component.Value, copy);
        }

        return new ShapeWrapper(output, true);
    }
}
