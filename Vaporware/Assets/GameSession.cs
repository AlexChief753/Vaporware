using UnityEngine;

public enum StartMode { NewGame, LoadGame }

public static class GameSession
{
    // Default to NewGame so Play in Editor still works even if MainMenu is skipped
    public static StartMode startMode = StartMode.NewGame;

    public static SaveData pendingSaveData = null;

    public static void Clear()
    {
        startMode = StartMode.NewGame;
        pendingSaveData = null;
    }
}
