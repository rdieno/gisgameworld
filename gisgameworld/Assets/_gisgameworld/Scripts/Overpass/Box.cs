using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Region
{
    public Coordinate topLeft;
    public Coordinate topright;
    public Coordinate bottomRight;
    public Coordinate bottomLeft;

    public Region(Coordinate tl, Coordinate tr, Coordinate br, Coordinate bl)
    {
        topLeft = tl;
        topright = tr;
        bottomRight = br;
        bottomLeft = bl;
    }

    public override string ToString()
    {
        return topLeft.latitude + " " + 
            topLeft.longitude + " " +
            topright.latitude + " " +
            topright.longitude + " " +
            bottomRight.latitude + " " +
            bottomRight.longitude + " " +
            bottomLeft.latitude + " " +
            bottomLeft.longitude;
    }
}
