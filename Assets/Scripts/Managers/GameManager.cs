using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Managers")]
    public SolarSystemManager solarSystemManager;
    public AchievementManager achievementManager;
    public MainMenuManager mainMenuManager;

    public int money;
    public int keys;
    public int keysGoal;
    public Planet currentPlanet;
    public PlayerController playerReference;

    private void SetPlayerData()
    {
        if (PersistenceHandler.LoadPlayerData(out PlayerData pd))
        {
            money = pd.moneyAmount;
        }
    }

    public void IncreaseCoinCount(int count)
    {
        money += count;
    }

    public void IncreaseKeyCount(int count)
    {
        keys += count;
    }

    public void CheckBossTrigger()
    {
        if (keys == keysGoal)
        {
            print("Bravo Coglione, ora vai al boss!");
        }
        else
        {
            print("Coglione! Ti mancano ancora: " + (keysGoal - keys) + " chiavi, vai a raccorglierle!");
        }
    }

    private void DebugChecks()
    {
        // Sti if prima li tolgo da qui meglio eh
        if (Input.GetKeyDown(KeyCode.K))
        {
            Debug.Log("Salvo i dati di gioco");
            PersistenceHandler.SavePlayerData(playerReference);
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            SetPlayerData();
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            CheckBossTrigger();
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
        money = 0;
        keys = 0;
        keysGoal = 3;
    }

    private void Start()
    {
        SetPlayerData();
    }

    private void Update()
    {
        DebugChecks();
    }

    #endregion UNITY_METHODS
}
