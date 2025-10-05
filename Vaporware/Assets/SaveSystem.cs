using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private static string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

    public static void Save(SaveData data)
    {
        try
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SavePath, json);
#if UNITY_EDITOR
            Debug.Log("Saved to: " + SavePath);
#endif
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Save failed: " + ex);
        }
    }

    public static SaveData Load()
    {
        try
        {
            if (!File.Exists(SavePath)) return null;
            string json = File.ReadAllText(SavePath);
            return JsonUtility.FromJson<SaveData>(json);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Load failed: " + ex);
            return null;
        }
    }

    public static bool HasSave() => File.Exists(SavePath);

    public static void Delete()
    {
        try { if (File.Exists(SavePath)) File.Delete(SavePath); }
        catch (System.Exception ex) { Debug.LogError("Delete save failed: " + ex); }
    }
}
