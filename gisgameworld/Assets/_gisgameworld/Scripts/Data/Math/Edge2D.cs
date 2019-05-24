using UnityEngine;
using System.Collections;

public class Edge
{
    // The index to each vertex
    public int[] vertexIndex = new int[2];

    // The index into the face.
    // (faceindex[0] == faceindex[1] means the edge connects to only one triangle)
    public int[] faceIndex = new int[2];
}
