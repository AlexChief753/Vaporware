using UnityEngine;
using UnityEngine.SceneManagement;

public class VideoSettingsBootstrap : MonoBehaviour
{
    private static VideoSettingsBootstrap instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateBootstrap()
    {
        if (instance != null) return;

        GameObject go = new GameObject("VideoSettingsBootstrap");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<VideoSettingsBootstrap>();
    }

    private void Awake()
    {
        // Apply immediately for current scene
        ApplySavedVideoSettings();

        // Keep settings synced on every scene change
        SceneManager.activeSceneChanged += OnSceneChanged;
    }

    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnSceneChanged;
    }

    private void OnSceneChanged(Scene oldScene, Scene newScene)
    {
        ApplySavedVideoSettings();
    }

    private void ApplySavedVideoSettings()
    {
        // Always re-read latest saved prefs so new changes persist across scenes
        int modeInt = PlayerPrefs.GetInt("DisplayMode", 0); // 0 = Borderless
        FullScreenMode mode = FullScreenMode.FullScreenWindow;
        if (modeInt == 1) mode = FullScreenMode.ExclusiveFullScreen;
        else if (modeInt == 2) mode = FullScreenMode.Windowed;

        int width = PlayerPrefs.GetInt("ResWidth", Screen.currentResolution.width);
        int height = PlayerPrefs.GetInt("ResHeight", Screen.currentResolution.height);

        Screen.SetResolution(width, height, mode);
        Debug.Log($"[VideoSettingsBootstrap] Applied {width}x{height} {mode}");
    }
}






