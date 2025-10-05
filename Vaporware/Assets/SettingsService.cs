using UnityEngine;

public class SettingsService : MonoBehaviour
{
    public static SettingsService Instance { get; private set; }

    [System.Serializable]
    public class Data
    {
        // Audio placeholders
        public float musicVolume = 1f;
        public float sfxVolume = 1f;

        // Video & misc
        public bool fullscreen = true;
        public int controlsSheetIndex = 0;
    }
    public Data Current = new Data();

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
        ApplyAll();
    }

    public void Load()
    {
        var json = PlayerPrefs.GetString("settings", "");
        if (!string.IsNullOrEmpty(json))
            Current = JsonUtility.FromJson<Data>(json);
    }

    public void Save()
    {
        var json = JsonUtility.ToJson(Current);
        PlayerPrefs.SetString("settings", json);
        PlayerPrefs.Save();
    }

    public void ApplyAll()
    {
        // AUDIO PLACEHOLDER

        // Video
        ApplyDisplay();
    }

    private void ApplyDisplay()
    {
        bool wantFullscreen = Current.fullscreen;

        // Use borderless fullscreen on desktop; switch to windowed when off.
        var mode = wantFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        Screen.fullScreenMode = mode;        // set the mode first

        if (wantFullscreen)
        {
            // Snap to native desktop resolution when fullscreen
            int w = Display.main.systemWidth;
            int h = Display.main.systemHeight;
            Screen.SetResolution(w, h, mode);
        }
        else
        {
            // Make a clearly-windowed size (80% of desktop) so you can see the change
            int w = Mathf.RoundToInt(Display.main.systemWidth * 0.8f);
            int h = Mathf.RoundToInt(Display.main.systemHeight * 0.8f);
            Screen.SetResolution(w, h, mode);
        }

        Debug.Log($"[Settings] Fullscreen={wantFullscreen} mode={Screen.fullScreenMode} res={Screen.currentResolution.width}x{Screen.currentResolution.height}");
    }
}


