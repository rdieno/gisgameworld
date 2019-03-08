using UnityEngine;
using System.Collections;

public class BetterCoroutine
{
    public Coroutine coroutine { get; private set; }
    public object result;
    private IEnumerator target;
    public BetterCoroutine(MonoBehaviour owner, IEnumerator target)
    {
        this.target = target;
        this.coroutine = owner.StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        while (target.MoveNext())
        {
            result = target.Current;
            yield return result;
        }
    }
}
