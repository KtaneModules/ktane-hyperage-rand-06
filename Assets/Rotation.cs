using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class Rotation
{
    public static Vector3 getCoordinate(float rotation, float magnitude, float y)
    {
        return new Vector3(
            (float)Math.Sin(rotation*Math.PI*2) * magnitude, y, (float)Math.Cos(rotation*Math.PI*2) * magnitude
        );
    }
}