using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData 
{
    public int moneyAmount;
    public PlayerData(PlayerController player)
    {
        moneyAmount = GameManager.Instance.money;
    }
}
