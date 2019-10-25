using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DictionaryList
{
    public Dictionary<string, List<object>> _dict = new Dictionary<string, List<object>>();

    public DictionaryList()
    {

    }

    public void Add<T>(string key, T value) where T : class
    {
        if(_dict.ContainsKey(key))
        {
            _dict[key].Add(value);
        }
        else
        {
            _dict.Add(key, new List<object>() { value });
        }
    }

    public T GetValue<T>(string key) where T : class
    {
        return _dict[key] as T;
    }
}
