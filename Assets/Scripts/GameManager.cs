using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public PlayerBehaviour playerReference;
    public Planet currentPlanet;
    
    public List<Planet> planets = new List<Planet>();


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
        currentPlanet = planets[0];
        playerReference.ChangePlanet(currentPlanet);
    }

    #endregion UNITY_METHODS
}
