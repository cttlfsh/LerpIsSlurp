using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetEarth : PlanetOld
{
    public override void Awake()
    {
        planetGravity = 100;
        groundNormal = transform.up;
        rotationSpeed = 0.1f;
        revolutionSpeed = 3f;
    }
}
