using UnityEngine;

public enum StartMode { NewGame, LoadGame }

public static class GameSession
{
    // Default to NewGame so Play-in-Editor still works even if MainMenu is skipped
    public static StartMode startMode = StartMode.NewGame;

    // Placeholder for when you wire a save system later
    public static SaveData pendingSaveData = null;

    public static void Clear()
    {
        startMode = StartMode.NewGame;
        pendingSaveData = null;
    }
}
