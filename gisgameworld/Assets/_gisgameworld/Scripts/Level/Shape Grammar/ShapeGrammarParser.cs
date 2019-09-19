using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class ShapeGrammarParser
{
    public ShapeGrammarParser()
    {

    }

    public Dictionary<string, string[]> ParseRuleFile(string filename)
    {
        string fullPathToFile = Application.streamingAssetsPath + "/ShapeGrammar/" + filename;

        Dictionary<string, string[]> rules = new Dictionary<string, string[]>();

        try
        {
            using (StreamReader sr = new StreamReader(fullPathToFile))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    // ignore commented lines
                    if(line.StartsWith("//"))
                    {
                        continue;
                    }

                    // Careful: assumes rules are in correct syntax
                    // no error checking is done here

                    string[] splitObjectName = line.Split(new string[] { "-->" }, StringSplitOptions.None);

                    string objectName = splitObjectName[0];
                    string wholeOperationsString = splitObjectName[1];

                    // splits individual operations while preserving paramters
                    string operationsRegex = @"\b[^()]+\((.*?)\)(\s\{.*\}\*?)?";

                    string[] operations = Regex.Matches(wholeOperationsString, operationsRegex)
                        .OfType<Match>()
                        .Select(m => m.Groups[0].Value)
                        .ToArray();

                    rules.Add(objectName, operations);
                }                
            }
        }
        catch (Exception e)
        {
            Debug.Log("ShapeGrammarParser: Parsing File: " + filename + ". The file could not be read.");
            Debug.Log(e.Message);
        }

        return rules;
    }
}

