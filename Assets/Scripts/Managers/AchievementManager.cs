using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementManager : MonoBehaviour
{
    public enum AchievmentTypes
    {
        BOSSFIGHT,
        KILL,
        COLLECTION,
        DISCOVERY
    }

    public static AchievementManager Instance;

    public List<Achievement> achievements = new List<Achievement>();
    
    public AudioSource achievementAudio;

    /// <summary>
    /// Checks if an achievement has been reached and unlocks it.
    /// </summary>
    /// <param name="type">Type of the achievement to check</param>
    /// <param name="count"></param>
    public void CheckAchievement(AchievmentTypes type, int count = 0)
    {
        
    }


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
        GameManager.Instance.achievementManager = this;

    }
}
