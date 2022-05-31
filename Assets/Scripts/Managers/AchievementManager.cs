using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance;

    public List<Achievement> achievements = new List<Achievement>();

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
        if (LoadAchievementsPref())
        {
            // Ho gia' inizializzato la prima volta gli ach
        }
        else
        {
            InitializeAchievements();
        }
    }





    // TODO: DA TOGLIERE E METTERE DOVE SALVO/CARICO GLI ACHIEVEMENTS
    private bool LoadAchievementsPref()
    {
        throw new NotImplementedException();
    }

    private void InitializeAchievements()
    {
        throw new NotImplementedException();
    }
}
