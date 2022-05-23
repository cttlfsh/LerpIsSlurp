using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Planet : MonoBehaviour
{
    public Transform planetToRevolveAround;
    public Vector3 groundNormal;
    public float planetGravity;
    public float rotationSpeed;
    public float revolutionSpeed;


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
        planetGravity = 100;
        groundNormal = transform.up;
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
