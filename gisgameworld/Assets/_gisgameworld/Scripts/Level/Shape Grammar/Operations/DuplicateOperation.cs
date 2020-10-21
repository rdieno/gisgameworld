using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// creates a deep copy of the input shape
// adds to a dictionary where the keys are the shape grammar names used to refer to the shapes
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
                // deep copy the shape using the copy constructor
                copy.Add(new Shape(shape));
            }

            // add to output using the input name as the new dictionary key
            output.Add(component.Value, copy);
        }

        return new ShapeWrapper(output, true);
    }
}
