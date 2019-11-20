using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MathUtility
{
    // clamp list indices
    // will even work if index is larger/smaller than listSize, so can loop multiple times
    public static int ClampListIndex(int index, int listSize)
    {
        index = ((index % listSize) + listSize) % listSize;

        return index;
    }

    // checks if a triangle in 2d space is oriented clockwise or counter-clockwise
    // https://math.stackexchange.com/questions/1324179/how-to-tell-if-3-connected-points-are-connected-clockwise-or-counter-clockwise
    // https://en.wikipedia.org/wiki/Curve_orientation
    public static bool IsTriangleOrientedClockwise(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        bool isClockWise = true;

        float determinant = p1.x * p2.y + p3.x * p1.y + p2.x * p3.y - p1.x * p3.y - p3.x * p2.y - p2.x * p1.y;

        if (determinant > 0f)
        {
            isClockWise = false;
        }

        return isClockWise;
    }


    public static bool IsTriangleOrientedClockwise3(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        bool isClockWise = true;

        float determinant = p1.x * p2.z + p3.x * p1.z + p2.x * p3.z - p1.x * p3.z - p3.x * p2.z - p2.x * p1.z;

        if (determinant > 0f)
        {
            isClockWise = false;
        }

        return isClockWise;
    }

    public static bool IsTriangleOrientedClockwise3(Triangle t)
    {
        Vector3 p1 = t.v1.position;
        Vector3 p2 = t.v2.position;
        Vector3 p3 = t.v3.position;


        Vector3 N = Vector3.Cross(p2 - p1, p3 - p2);
        float S = Vector3.Dot(N, p1);

        bool isClockWise = true;

        //float determinant = p1.x * p2.z + p3.x * p1.z + p2.x * p3.z - p1.x * p3.z - p3.x * p2.z - p2.x * p1.z;

        if (S > 0f)
        {
            isClockWise = false;
        }

        return isClockWise;
    }


    // from http://totologic.blogspot.se/2014/01/accurate-point-in-triangle-test.html
    // p is the testpoint, and the other points are corners in the triangle
    public static bool IsPointInTriangle(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p)
    {
        bool isWithinTriangle = false;

        //Based on Barycentric coordinates
        float denominator = ((p2.y - p3.y) * (p1.x - p3.x) + (p3.x - p2.x) * (p1.y - p3.y));

        float a = ((p2.y - p3.y) * (p.x - p3.x) + (p3.x - p2.x) * (p.y - p3.y)) / denominator;
        float b = ((p3.y - p1.y) * (p.x - p3.x) + (p1.x - p3.x) * (p.y - p3.y)) / denominator;
        float c = 1 - a - b;

        //The point is within the triangle or on the border if 0 <= a <= 1 and 0 <= b <= 1 and 0 <= c <= 1
        //if (a >= 0f && a <= 1f && b >= 0f && b <= 1f && c >= 0f && c <= 1f)
        //{
        //    isWithinTriangle = true;
        //}

        //The point is within the triangle
        if (a > 0f && a < 1f && b > 0f && b < 1f && c > 0f && c < 1f)
        {
            isWithinTriangle = true;
        }

        return isWithinTriangle;
    }

    public static Vector3 FarthestPointInDirection(Vector3[] vertices, Vector3 direction)
    {
        int index = 0;
        float farthestDistance = float.MinValue;

        for(int i = 0; i < vertices.Length; i++)
        {
            float distance = Vector3.Dot(vertices[i], direction);

            if(distance > farthestDistance)
            {
                farthestDistance = distance;
                index = i;
            }
        }

        return vertices[index];
    }

    public static bool IsPointInPolygonZ(Vector3 point, List<Vector3> polygon)
    {
        int polygonLength = polygon.Count, i = 0;
        bool inside = false;
        // x, y for tested point.
        float pointX = point.x, pointY = point.z;
        // start / end point for the current polygon segment.
        float startX, startY, endX, endY;
        Vector3 endPoint = polygon[polygonLength - 1];
        endX = endPoint.x;
        endY = endPoint.z;
        while (i < polygonLength)
        {
            startX = endX; startY = endY;
            endPoint = polygon[i++];
            endX = endPoint.x; endY = endPoint.z;
            //
            inside ^= (endY > pointY ^ startY > pointY) /* ? pointY inside [startY;endY] segment ? */
                      && /* if so, test if it is under the segment */
                      ((pointX - endX) < (pointY - endY) * (startX - endX) / (startY - endY));
        }
        return inside;
    }

}
