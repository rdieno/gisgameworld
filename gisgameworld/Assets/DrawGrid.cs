﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Author: Hayden Scott-Baron (@docky) - http://twitter.com/docky
// Retrieved from: http://wiki.unity3d.com/index.php/DrawGizmoGrid
public class DrawGrid : MonoBehaviour
{
    // universal grid scale
    float gridScale = 1f;

    // extents of the grid
    public int minX = -15;
    public int minY = -15;
    public int maxX = 15;
    public int maxY = 15;

    // nudges the whole grid rel
    public Vector3 gridOffset = Vector3.zero;

    // is this an XY or an XZ grid?
    public bool topDownGrid = true;

    // choose a colour for the gizmos
    public int gizmoMajorLines = 5;
    public Color gizmoLineColor = new Color(0.4f, 0.4f, 0.3f, 1f);

    // Start is called before the first frame update
    private void Start()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        // orient to the gameobject, so you can rotate the grid independently if desired
        Gizmos.matrix = transform.localToWorldMatrix;

        // set colours
        Color dimColor = new Color(gizmoLineColor.r, gizmoLineColor.g, gizmoLineColor.b, 0.25f * gizmoLineColor.a);
        Color brightColor = Color.Lerp(Color.white, gizmoLineColor, 0.75f);

        // draw the horizontal lines
        for (int x = minX; x < maxX + 1; x++)
        {
            // find major lines
            Gizmos.color = (x % gizmoMajorLines == 0 ? gizmoLineColor : dimColor);
            if (x == 0)
                Gizmos.color = brightColor;

            Vector3 pos1 = new Vector3(x, minY, 0) * gridScale;
            Vector3 pos2 = new Vector3(x, maxY, 0) * gridScale;

            // convert to topdown/overhead units if necessary
            if (topDownGrid)
            {
                pos1 = new Vector3(pos1.x, 0, pos1.y);
                pos2 = new Vector3(pos2.x, 0, pos2.y);
            }

            Gizmos.DrawLine((gridOffset + pos1), (gridOffset + pos2));
        }

        // draw the vertical lines
        for (int y = minY; y < maxY + 1; y++)
        {
            // find major lines
            Gizmos.color = (y % gizmoMajorLines == 0 ? gizmoLineColor : dimColor);
            if (y == 0)
                Gizmos.color = brightColor;

            Vector3 pos1 = new Vector3(minX, y, 0) * gridScale;
            Vector3 pos2 = new Vector3(maxX, y, 0) * gridScale;

            // convert to topdown/overhead units if necessary
            if (topDownGrid)
            {
                pos1 = new Vector3(pos1.x, 0, pos1.y);
                pos2 = new Vector3(pos2.x, 0, pos2.y);
            }

            Gizmos.DrawLine((gridOffset + pos1), (gridOffset + pos2));
        }
    }
}
