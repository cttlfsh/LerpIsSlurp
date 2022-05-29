using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlanetOld : MonoBehaviour
{
    public Transform planetToRevolveAround;
    
    protected Vector3 groundNormal;
    protected float planetGravity;
    protected float rotationSpeed;
    protected float revolutionSpeed;

    public float PlanetGravity { get => planetGravity; }
    public Vector3 GroundNormal { get => groundNormal; set => groundNormal = value; }


    #region PRIVATE_METHODS
    private void RotatePlanet()
    {
        transform.Rotate(new Vector3(0f, rotationSpeed, 0f));
    }

    private void RevolvePlanet()
    {
        transform.RotateAround(planetToRevolveAround.position, Vector3.up, revolutionSpeed * Time.deltaTime);
    }

    #endregion PRIVATE_METHODS

    #region UNITY_METHODS
    public virtual void Awake()
    {
        planetGravity = 0;
        GroundNormal = transform.up;
        rotationSpeed = 0f;
        revolutionSpeed = 0f;
    }

    public void Update()
    {
        RotatePlanet();
        if (planetToRevolveAround != null)
        {
            RevolvePlanet();
        }
    }
    #endregion UNITY_METHODS
}
