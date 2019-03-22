using UnityEngine;
using System.Collections;
using System;

public class InspectorReferenceMissingException : Exception
{

    public InspectorReferenceMissingException()
    {
    }

    public InspectorReferenceMissingException(string message)
        : base(message)
    {
    }

    public InspectorReferenceMissingException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
