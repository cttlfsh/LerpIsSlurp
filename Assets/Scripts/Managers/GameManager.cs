using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public PlayerController playerReference;
    public Planet currentPlanet;

    #region UNITY_METHODS

    private void Awake()
    {
        if (Instance != null)
        {
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        currentPlanet = SolarSystemManager.Instance.bodies[0];
        //playerReference.ChangePlanet(currentPlanet);
    }

    #endregion UNITY_METHODS
}
