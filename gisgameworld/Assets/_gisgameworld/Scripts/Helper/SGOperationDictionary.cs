using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SGOperationDictionary
{
    public Dictionary<string, List<IShapeGrammarOperation>> _dict = new Dictionary<string, List<IShapeGrammarOperation>>();

    public SGOperationDictionary()
    {

    }

    public void Add(string key, IShapeGrammarOperation value)
    {
        if (_dict.ContainsKey(key))
        {
            _dict[key].Add(value);
        }
        else
        {
            _dict.Add(key, new List<IShapeGrammarOperation>() { value });
        }
    }

    public List<IShapeGrammarOperation> GetValue(string key)
    {
        return _dict[key];
    }
}
