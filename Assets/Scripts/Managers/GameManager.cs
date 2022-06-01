using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Managers")]
    public PersistenceManager persistenceManager;
    public SolarSystemManager solarSystemManager;
    public AchievementManager achievementManager;

    public int money;
    public Planet currentPlanet;
    public PlayerController playerReference;

    private void SetupGame()
    {
        if (persistenceManager.LoadPlayerData(out PlayerData pd))
        {
            money = pd.moneyAmount;
        }
    }

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
        SetupGame();
    }

    private void Update()
    {
        // Sti if prima li tolgo da qui meglio eh
        if (Input.GetKeyDown(KeyCode.K))
        {
            Debug.Log("Salvo i dati di gioco");
            persistenceManager.SavePlayerData(playerReference);
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            persistenceManager.LoadPlayerData(out PlayerData pd);
        }
    }

    #endregion UNITY_METHODS
}
