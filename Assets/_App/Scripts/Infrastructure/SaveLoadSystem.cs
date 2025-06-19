using UnityEngine;
using System.IO;

public static class SaveLoadSystem
{
    private static string SavePath => Path.Combine(Application.persistentDataPath, "savefile.json");

    public static void SaveGame(Farm farm)
    {
        try
        {
            string json = JsonUtility.ToJson(farm, true);
            File.WriteAllText(SavePath, json);
            Debug.Log("Game saved successfully!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save game: {e.Message}");
        }
    }

    public static Farm LoadGame()
    {
        try
        {
            if (File.Exists(SavePath))
            {
                string json = File.ReadAllText(SavePath);
                var farm = JsonUtility.FromJson<Farm>(json);
                Debug.Log("Game loaded successfully!");
                return farm;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load game: {e.Message}");
        }
        
        return null;
    }

    public static bool HasSaveFile()
    {
        return File.Exists(SavePath);
    }

    public static void DeleteSave()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
            Debug.Log("Save file deleted!");
        }
    }
}