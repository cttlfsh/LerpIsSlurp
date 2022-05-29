using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetSun : PlanetOld
{

    #region UNITY_METHODS
    public override void Awake()
    {
        planetGravity = 100;
        groundNormal = transform.up;
        rotationSpeed = 0.04f;
        planetToRevolveAround = null;
    }

    #endregion UNITY_METHODS
}
