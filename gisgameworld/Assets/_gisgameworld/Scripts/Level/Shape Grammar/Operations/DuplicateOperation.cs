using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

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

        bool test = true;
        List<OperationTest> operationTests = new List<OperationTest>();
        List<bool> part1results = new List<bool>();
        List<bool> part2results = new List<bool>();
        List<bool> part3results = new List<bool>();
        List<bool> part4results = new List<bool>();
        int inputShapeCount = input.Count;

        
        foreach (KeyValuePair<string, string> component in componentNames)
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

        if(test)
        {
            foreach (KeyValuePair<string, List<Shape>> duplicate in output)
            {
                int duplicateShapeCount = duplicate.Value.Count;
                bool testResult1 = inputShapeCount == duplicateShapeCount;
                part1results.Add(testResult1);

                for(int i = 0; i < duplicateShapeCount; i++)
                {
                    bool testResult2 = CompareGeometry(input[i], duplicate.Value[i]);
                    part2results.Add(testResult2);

                    bool testResult3 = CompareTransform(input[i].LocalTransform, duplicate.Value[i].LocalTransform);
                    part3results.Add(testResult3);
                    
                    // check object reference, should be different to pass test so check is negated
                    bool testResult4 = !(System.Object.ReferenceEquals(input[i], duplicate.Value[i]));
                    part4results.Add(testResult4);
                }


            }
            
            operationTests.Add(new OperationTest("dup", "part 1", part1results));
            operationTests.Add(new OperationTest("dup", "part 2", part2results));
            operationTests.Add(new OperationTest("dup", "part 3", part3results));
            operationTests.Add(new OperationTest("dup", "part 4", part4results));
        }

        return new ShapeWrapper(output, operationTests, true);
    }


    private bool CompareGeometry(Shape original, Shape processed)
    {
        Vector3[] originalVertices = original.Vertices;
        Vector3[] processedVertices = original.Vertices;

        if(originalVertices.Length != processedVertices.Length)
        {
            return false;
        }

        Vector3[] originalNormals = original.Normals;
        Vector3[] processedNormals = original.Normals;

        if (originalVertices.Length != processedVertices.Length)
        {
            return false;
        }

        int[] originalTriangles = original.Triangles;
        int[] processedTriangles = original.Triangles;

        if (originalVertices.Length != processedVertices.Length)
        {
            return false;
        } 
        
        for(int i = 0; i < originalVertices.Length; i++)
        {
            Vector3 vA = originalVertices[i];
            Vector3 vB = processedVertices[i];

            if(vA != vB)
            {
                return false;
            }

            Vector3 nA = originalNormals[i];
            Vector3 nB = processedNormals[i];

            if (nA != nB)
            {
                return false;
            }
        }

        for (int i = 0; i < originalTriangles.Length; i++)
        {
            int iA = originalTriangles[i];
            int iB = processedTriangles[i];

            if(iA != iB)
            {
                return false;
            }
        }

        return true;
    }


    private bool CompareTransform(LocalTransform originalTransform, LocalTransform processedTransform)
    {
        if(originalTransform.Origin != processedTransform.Origin)
        {
            return false;
        }

        if (originalTransform.Up != processedTransform.Up)
        {
            return false;
        }

        if (originalTransform.Right != processedTransform.Right)
        {
            return false;
        }
        
        if (originalTransform.Forward != processedTransform.Forward)
        {
            return false;
        }
        
        return true;
    }
}
