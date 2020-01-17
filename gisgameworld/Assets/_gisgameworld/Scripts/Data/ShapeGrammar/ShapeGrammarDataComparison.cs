using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShapeGrammarDataComparison : IComparer<ShapeGrammarData>
{
    public int Compare(ShapeGrammarData x, ShapeGrammarData y)
    {
        if(x.score == y.score)
        {
            return 0;
        }

        //if(x.score > y.score)
        //{
        //    return -1;
        //}

        //return 1;

        return y.score.CompareTo(x.score);
    }
}
