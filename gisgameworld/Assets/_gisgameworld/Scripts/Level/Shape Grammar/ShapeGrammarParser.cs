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

    public SGOperationDictionary ParseRuleFile(string name)
    {
        string fullPathToFile = Application.streamingAssetsPath + "/ShapeGrammar/" + name + ".cga";

        Dictionary<string, string[]> rules = new Dictionary<string, string[]>();

        try
        {
            using (StreamReader sr = new StreamReader(fullPathToFile))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    // ignore commented and blank line
                    if(line.StartsWith("//") || String.IsNullOrEmpty(line))
                    {
                        continue;
                    }

                    // Careful: assumes rules are in correct syntax
                    // no error checking is done here

                    string[] splitObjectName = line.Split(new string[] { "-->" }, StringSplitOptions.None);

                    string objectName = splitObjectName[0].Replace(" ", string.Empty);
                    string wholeOperationsString = splitObjectName[1];

                    // splits individual operations while preserving parameters
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
            Debug.Log("ShapeGrammarParser: Parsing File: " + name + ". The file could not be read.");
            Debug.Log(e.Message);
        }

        return TokenizeRules(rules);
    }


    public SGOperationDictionary TokenizeRules(Dictionary<string, string[]> rules)
    {
        SGOperationDictionary TokenizedRules = new SGOperationDictionary();

        foreach (KeyValuePair<string, string[]> rule in rules)
        {
            foreach(string operation in rule.Value)
            {
                // splits individual operations while preserving paramters
                //string operationNameRegex = @"\b[^()]+";
                string operationNameRegex = @"[^()]+";
                //string operationNameRegex = @"[^()[0-9]\d*(\.\d+)?]+";

                string[] operations = Regex.Matches(operation, operationNameRegex)
                    .OfType<Match>()
                    .Select(m => m.Groups[0].Value)
                    .ToArray();

                switch (operations[0])
                {
                    case "translate":
                        string[] translateParameters = operations[1].Replace(" ", string.Empty).Split(',');
                        TokenizedRules.Add(rule.Key, new TranslateOperation(new Vector3(float.Parse(translateParameters[1]), float.Parse(translateParameters[2]), float.Parse(translateParameters[3])), ConvertStringToCoordSystem(translateParameters[0])));
                        break;

                    case "rotate":
                        string[] rotateParameters = operations[1].Replace(" ", string.Empty).Split(',');
                        TokenizedRules.Add(rule.Key, new RotateOperation(new Vector3(float.Parse(rotateParameters[1]), float.Parse(rotateParameters[2]), float.Parse(rotateParameters[3])), ConvertStringToCoordSystem(rotateParameters[0])));
                        break;

                    case "scale":
                        string[] scaleParameters = operations[1].Replace(" ", string.Empty).Split(',');
                        TokenizedRules.Add(rule.Key, new ScaleOperation(new Vector3(float.Parse(scaleParameters[0]), float.Parse(scaleParameters[1]), float.Parse(scaleParameters[2]))));
                        break;

                    case "offset":
                        string offsetExtraParametersRegex = @"\{.*\}";
                        string offsetExtraParameters = Regex.Match(operation, offsetExtraParametersRegex).Value;
                        TokenizedRules.Add(rule.Key, new OffsetOperation(float.Parse(operations[1]), ParseComponentOperationTerms(offsetExtraParameters)));
                        break;

                    case "extrude":
                        TokenizedRules.Add(rule.Key, new ExtrudeOperation(Axis.Up, float.Parse(operations[1])));
                        break;

                    case "taper":
                        string[] taperParameters = operations[1].Replace(" ", string.Empty).Split(',');
                        TokenizedRules.Add(rule.Key, new TaperOperation(float.Parse(taperParameters[0]), float.Parse(taperParameters[1])));
                        break;

                    case "split":
                        string extraParametersRegex = @"\{.*\}\*?";
                        string extraParameter = Regex.Match(operation, extraParametersRegex).Value;
                        TokenizedRules.Add(rule.Key, new SplitOperation(ConvertStringToAxis(operations[1]), ParseSplitOperationTerms(extraParameter)));
                        break;

                    case "roofshed":
                        string[] roofshedParameters = operations[1].Replace(" ", string.Empty).Split(',');
                        TokenizedRules.Add(rule.Key, new RoofShedOperation(float.Parse(roofshedParameters[0]), ConvertStringToDirection(roofshedParameters[1])));
                        break;

                    case "comp":
                        string compExtraParametersRegex = @"\{.*\}";
                        string compExtraParameters = Regex.Match(operation, compExtraParametersRegex).Value;
                        TokenizedRules.Add(rule.Key, new CompOperation(ParseComponentOperationTerms(compExtraParameters)));
                        break;

                    case "stair":
                        string[] stairParameters = operations[1].Replace(" ", string.Empty).Split(',');
                        TokenizedRules.Add(rule.Key, new StairOperation(ConvertStringToDirection(stairParameters[0]), int.Parse(stairParameters[1])));
                        break;

                    case "dup":
                        string dupExtraParametersRegex = @"\{.*\}";
                        string dupExtraParameters = Regex.Match(operation, dupExtraParametersRegex).Value;
                        TokenizedRules.Add(rule.Key, new DuplicateOperation(ParseComponentOperationTerms(dupExtraParameters)));
                        break;

                    default:
                        Debug.Log("Shape Grammar Parser: TokenizeRules(): invalid operation.\n " + string.Format("[{0}]", string.Join(", ", operations)));
                        break;
                }
            }
        }

        return TokenizedRules;
    }

    //public DictionaryList TokenizeRules(Dictionary<string, string[]> rules)
    //{
    //    DictionaryList TokenizedRules = new DictionaryList();

    //    foreach (KeyValuePair<string, string[]> rule in rules)
    //    {
    //        foreach(string operation in rule.Value)
    //        {
    //            // splits individual operations while preserving paramters
    //            string operationNameRegex = @"\b[^()]+";

    //            string[] operations = Regex.Matches(operation, operationNameRegex)
    //                .OfType<Match>()
    //                .Select(m => m.Groups[0].Value)
    //                .ToArray();

    //            switch (operations[0])
    //            {
    //                case "translate":
    //                    string[] translateParameters = operations[1].Replace(" ", string.Empty).Split(',');
    //                    TokenizedRules.Add<IShapeGrammarOperation>(rule.Key, new TranslateOperation(new Vector3(float.Parse(translateParameters[1]), float.Parse(translateParameters[2]), float.Parse(translateParameters[3])), ConvertStringToCoordSystem(translateParameters[0])));
    //                    break;

    //                case "rotate":
    //                    string[] rotateParameters = operations[1].Replace(" ", string.Empty).Split(',');
    //                    TokenizedRules.Add<IShapeGrammarOperation>(rule.Key, new RotateOperation(new Vector3(float.Parse(rotateParameters[1]), float.Parse(rotateParameters[2]), float.Parse(rotateParameters[3])), ConvertStringToCoordSystem(rotateParameters[0])));
    //                    break;

    //                case "scale":
    //                    string[] scaleParameters = operations[1].Replace(" ", string.Empty).Split(',');
    //                    TokenizedRules.Add<IShapeGrammarOperation>(rule.Key, new ScaleOperation(new Vector3(float.Parse(scaleParameters[0]), float.Parse(scaleParameters[1]), float.Parse(scaleParameters[2]))));
    //                    break;

    //                case "offset":
    //                    TokenizedRules.Add<IShapeGrammarOperation>(rule.Key, new OffsetOperation(float.Parse(operations[1])));
    //                    break;

    //                case "extrude":
    //                    TokenizedRules.Add<IShapeGrammarOperation>(rule.Key, new ExtrudeOperation(Axis.Up, float.Parse(operations[1])));
    //                    break;

    //                case "taper":
    //                    string[] taperParameters = operations[1].Replace(" ", string.Empty).Split(',');
    //                    TokenizedRules.Add<IShapeGrammarOperation>(rule.Key, new TaperOperation(float.Parse(taperParameters[0]), float.Parse(taperParameters[1])));
    //                    break;

    //                case "split":
    //                    string extraParametersRegex = @"\{.*\}\*?";
    //                    string extraParameter = Regex.Match(operation, extraParametersRegex).Value;
    //                    TokenizedRules.Add<IShapeGrammarOperation>(rule.Key, new SplitOperation(ConvertStringToAxis(operations[1]), ParseSplitOperationTerms(extraParameter)));
    //                    break;

    //                case "roofshed":
    //                    string[] roofshedParameters = operations[1].Replace(" ", string.Empty).Split(',');
    //                    TokenizedRules.Add<IShapeGrammarOperation>(rule.Key, new RoofShedOperation(float.Parse(roofshedParameters[0]), ConvertStringToDirection(roofshedParameters[0])));
    //                    break;

    //                case "comp":
    //                    string compExtraParametersRegex = @"\{.*\}";
    //                    string compExtraParameter = Regex.Match(operation, compExtraParametersRegex).Value;
    //                    TokenizedRules.Add<IShapeGrammarOperation>(rule.Key, new CompOperation(ParseCompOperationTerms(compExtraParameter)));
    //                    break;

    //                case "stair":
    //                    string[] stairParameters = operations[1].Replace(" ", string.Empty).Split(',');
    //                    TokenizedRules.Add<IShapeGrammarOperation>(rule.Key, new StairOperation(ConvertStringToDirection(stairParameters[0]), int.Parse(stairParameters[1])));
    //                    break;

    //                default:
    //                    Debug.Log("Shape Grammar Parser: TokenizeRules(): invalid operation");
    //                    break;

    //            }
    //        }
    //    }

    //    return TokenizedRules;
    //}

    private List<SplitTerm> ParseSplitOperationTerms(string inputString)
    {
        List<SplitTerm> splitTerms = new List<SplitTerm>();

        Regex splitTermRegex = new Regex(@"([^{}]+)* \} \*?", RegexOptions.IgnorePatternWhitespace);

        string[] terms = splitTermRegex.Matches(inputString)
            .OfType<Match>()
            .Select(m => m.Groups[0].Value)
            .ToArray();

        foreach(string term in terms)
        {
            bool isRepeat = false;
            if(term[term.Length - 1] == '*')
            {
                isRepeat = true;
            }

            string strippedTerm = term.Trim(new char[] { '}', '*' }).Replace(" ", string.Empty);
            string[] ratios = strippedTerm.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

            List<SplitRatio> splitRatios = new List<SplitRatio>();
            foreach(string ratio in ratios)
            {
                bool isFloating = false;
                if (ratio[0] == '~')
                {
                    isFloating = true;
                }

                string strippedRatio = ratio.Trim(new char[] { '~' });
                string[] ratioAndName = strippedRatio.Split(':');

                splitRatios.Add(new SplitRatio(isFloating, float.Parse(ratioAndName[0]), ratioAndName[1]));
            }

            splitTerms.Add(new SplitTerm(isRepeat, splitRatios));
        }

        return splitTerms;
    }

    private Dictionary<string, string> ParseComponentOperationTerms(string inputString)
    {
        string strippedBrackets = inputString.Trim(new char[] { '{', '}' }).Replace(" ", string.Empty);
        string[] separatedComponentNames = strippedBrackets.Split('|');

        Dictionary<string, string> componentNames = new Dictionary<string, string>();

        foreach(string componentName in separatedComponentNames)
        {
            string[] namePair = componentName.Split(':');

            componentNames.Add(namePair[0], namePair[1]);
        }

        return componentNames;
    }

    private Axis ConvertStringToAxis(string inputAxis)
    {
        Axis axis = Axis.Up;

        switch (inputAxis)
        {
            case "x":
                axis = Axis.Right;
                break;
            case "y":
                axis = Axis.Up;
                break;
            case "z":
                axis = Axis.Forward;
                break;
            default:
                Debug.Log("ShapeGrammarParser: ConvertAxisToVector(): input axis not found, must be x, y or z");
                break;
        }

        return axis;
    }

    private CoordSystem ConvertStringToCoordSystem(string inputCoordSystem)
    {
        CoordSystem coordSystem = CoordSystem.Local;

        if(inputCoordSystem == "world")
        {
            coordSystem = CoordSystem.World;
        }

        return coordSystem;
    }

    private Direction ConvertStringToDirection(string inputDirection)
    {
        Direction direction = Direction.Up;

        switch (inputDirection)
        {
            case "up":
                direction = Direction.Up;
                break;
            case "down":
                direction = Direction.Down;
                break;
            case "left":
                direction = Direction.Left;
                break;
            case "right":
                direction = Direction.Right;
                break;
            case "forward":
                direction = Direction.Forward;
                break;
            case "back":
                direction = Direction.Back;
                break;
            default:
                Debug.Log("ShapeGrammarParser: ConvertStringToDirection(): input direct not found, must be up, down, forward, back, left, right");
                break;
        }

        return direction;
    }

}

