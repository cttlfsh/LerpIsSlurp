using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class PersistenceManager : MonoBehaviour
{
    public static PersistenceManager Instance;

    private string savePlayerPath = Application.persistentDataPath + "/playerData.pd";

    /// <summary>
    /// Handles the save of the PlayerController data through a PlayerData class type
    /// </summary>
    /// <param name="player">Reference to the PlayerController</param>
    public void SavePlayerData(PlayerController player)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(savePlayerPath, FileMode.Create);
        PlayerData data = new PlayerData(player);

        formatter.Serialize(stream, data);
        stream.Close();

    }

    /// <summary>
    /// Handles the load of a saving data file from the system.
    /// </summary>
    /// <param name="playerData">Reference to the PlayerData object where the file are loaded</param>
    /// <param name="saveSlot">The number of the saving slot. Default 0</param>
    /// <returns></returns>
    public bool LoadPlayerData(out PlayerData playerData, int saveSlot = 0)
    {
        Debug.Log(savePlayerPath);
        if (File.Exists(savePlayerPath))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(savePlayerPath, FileMode.Open);

            playerData = formatter.Deserialize(stream) as PlayerData;
            stream.Close();

            return true;
        }
        Debug.Log("Couldn't find the player save");
        playerData = null;
        return false;
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
        GameManager.Instance.persistenceManager = this;
    }

    #endregion


}
