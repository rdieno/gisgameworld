using UnityEngine;
using System.Collections;
using System;

public class LevelManagerNotFoundException : Exception
{

    public LevelManagerNotFoundException()
    {
    }

    public LevelManagerNotFoundException(string message)
        : base(message)
    {
    }

    public LevelManagerNotFoundException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
