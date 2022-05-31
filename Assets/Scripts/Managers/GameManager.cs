using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int money;
    public Planet currentPlanet;
    public PlayerController playerReference;

    // TODO: Da sistemare, probabilmente andra' in un UIManager o simili
    private void LoadPlayerData()
    {
        if (SaveGameSystem.LoadPlayerData(out PlayerData pd))
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
        currentPlanet = SolarSystemManager.Instance.bodies[0];
        LoadPlayerData();
    }

    private void Update()
    {
        // Sti if prima li tolgo da qui meglio eh
        if (Input.GetKeyDown(KeyCode.K))
        {
            Debug.Log("Salvo i dati di gioco");
            SaveGameSystem.SavePlayerData(playerReference);
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            LoadPlayerData();
        }
        {

        }
    }

    #endregion UNITY_METHODS
}
