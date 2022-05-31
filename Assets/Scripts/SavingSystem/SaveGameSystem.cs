using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveGameSystem 
{
    private static string savePlayerPath = Application.persistentDataPath + "/playerData.pd";

    public static void SavePlayerData(PlayerController player)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(savePlayerPath, FileMode.Create);
        PlayerData data = new PlayerData(player);

        formatter.Serialize(stream, data);
        stream.Close();

    }

    public static bool LoadPlayerData(out PlayerData playerData)
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
     
}
