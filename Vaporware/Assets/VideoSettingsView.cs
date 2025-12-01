using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class VideoSettingsView : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Dropdown displayModeDropdown;
    public TMP_Dropdown resolutionDropdown;

    // --- PlayerPrefs keys ---
    private const string DISPLAY_MODE_KEY = "DisplayMode";
    private const string RES_W_KEY = "ResWidth";
    private const string RES_H_KEY = "ResHeight";
    private const string RES_HZ_KEY = "ResHz";
    // Back-compat
    private const string LEGACY_RES_INDEX = "ResolutionIndex";

    private enum DisplayMode { BorderlessFullscreen = 0, Fullscreen = 1, Windowed = 2 }

    private Resolution[] filteredResolutions;
    private int currentResolutionIndex;
    private bool hasInitialized = false;

    // ===== 1) BOOTSTRAP: runs once per app launch, before any scene =====
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ApplySavedSettingsAtBoot()
    {
        // Read saved mode (default: Borderless)
        int savedMode = PlayerPrefs.GetInt(DISPLAY_MODE_KEY, (int)DisplayMode.BorderlessFullscreen);
        var mode = ToUnityModeStatic((DisplayMode)savedMode);

        // Build a filtered + deduped list (same filter as the panel uses)
        Resolution[] filtered = BuildFilteredResolutions();

        // Try exact saved W/H/Hz; if none saved, try legacy index; else fallback to current
        int w = PlayerPrefs.GetInt(RES_W_KEY, 0);
        int h = PlayerPrefs.GetInt(RES_H_KEY, 0);
        float hz = PlayerPrefs.GetFloat(RES_HZ_KEY, 0f);

        Resolution target = FindBestResolution(filtered, w, h, hz);
        if (target.width == 0) // fallback if prefs not set yet
        {
            int legacyIdx = Mathf.Clamp(PlayerPrefs.GetInt(LEGACY_RES_INDEX, 0), 0, filtered.Length > 0 ? filtered.Length - 1 : 0);
            target = (filtered.Length > 0) ? filtered[legacyIdx] : Screen.currentResolution;
        }

        if (target.width == 0) target = Screen.currentResolution;

        Screen.SetResolution(target.width, target.height, mode);
    }

    // ===== 2) PANEL LIFECYCLE =====
    private void Start()
    {
        SetupDisplayModeDropdown();
        SetupResolutionDropdown();
        hasInitialized = true;
    }

    // --- UI setup ---
    private void SetupDisplayModeDropdown()
    {
        displayModeDropdown.ClearOptions();
        displayModeDropdown.AddOptions(new List<string> { "Borderless Fullscreen", "Fullscreen", "Windowed" });

        int savedMode = PlayerPrefs.GetInt(DISPLAY_MODE_KEY, (int)DisplayMode.BorderlessFullscreen);
        displayModeDropdown.value = savedMode;
        displayModeDropdown.onValueChanged.AddListener(OnDisplayModeChanged);
    }

    private void SetupResolutionDropdown()
    {
        filteredResolutions = BuildFilteredResolutions();
        if (filteredResolutions.Length == 0) filteredResolutions = new[] { Screen.currentResolution };

        resolutionDropdown.ClearOptions();

        // Pick index that matches saved W/H/Hz (or current if not found)
        int savedW = PlayerPrefs.GetInt(RES_W_KEY, Screen.currentResolution.width);
        int savedH = PlayerPrefs.GetInt(RES_H_KEY, Screen.currentResolution.height);
        float savedHz = PlayerPrefs.GetFloat(RES_HZ_KEY, (float)Screen.currentResolution.refreshRateRatio.value);

        currentResolutionIndex = IndexOfClosest(filteredResolutions, savedW, savedH, savedHz);

        // Build labels (dedup already handled)
        var labels = new List<string>(filteredResolutions.Length);
        for (int i = 0; i < filteredResolutions.Length; i++)
        {
            var r = filteredResolutions[i];
            labels.Add($"{r.width} x {r.height} ({r.refreshRateRatio.value:F0}Hz)");
        }

        resolutionDropdown.AddOptions(labels);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();

        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
    }

    // --- UI events ---
    private void OnDisplayModeChanged(int index)
    {
        if (!hasInitialized) return;

        PlayerPrefs.SetInt(DISPLAY_MODE_KEY, index);
        PlayerPrefs.Save();

        var r = filteredResolutions[currentResolutionIndex];
        Screen.SetResolution(r.width, r.height, ToUnityMode((DisplayMode)index));
    }

    private void OnResolutionChanged(int index)
    {
        if (!hasInitialized) return;

        currentResolutionIndex = index;
        var r = filteredResolutions[index];

        // Save robustly by dimensions + hz
        PlayerPrefs.SetInt(RES_W_KEY, r.width);
        PlayerPrefs.SetInt(RES_H_KEY, r.height);
        PlayerPrefs.SetFloat(RES_HZ_KEY, (float)r.refreshRateRatio.value);
        PlayerPrefs.Save();

        var mode = (DisplayMode)displayModeDropdown.value;
        Screen.SetResolution(r.width, r.height, ToUnityMode(mode));
    }

    // ===== 3) Helpers =====
    private static FullScreenMode ToUnityMode(DisplayMode mode) => ToUnityModeStatic(mode);
    private static FullScreenMode ToUnityModeStatic(DisplayMode mode)
    {
        switch (mode)
        {
            case DisplayMode.Fullscreen: return FullScreenMode.ExclusiveFullScreen;
            case DisplayMode.Windowed: return FullScreenMode.Windowed;
            default: return FullScreenMode.FullScreenWindow; // Borderless
        }
    }

    private static Resolution[] BuildFilteredResolutions()
    {
        Resolution[] all = Screen.resolutions;
        var list = new List<Resolution>();

        foreach (var r in all)
        {
            float aspect = (float)r.width / r.height;
            if (aspect > 1.76f && aspect < 1.78f) // ~16:9
            {
                float hz = (float)r.refreshRateRatio.value;
                if (hz >= 119f && hz <= 121f)      // ~120 Hz
                {
                    // Dedup same W/H/Hz
                    bool dup = false;
                    for (int i = 0; i < list.Count; i++)
                    {
                        var e = list[i];
                        if (e.width == r.width && e.height == r.height &&
                            Mathf.Abs((float)e.refreshRateRatio.value - hz) < 0.5f)
                        {
                            dup = true; break;
                        }
                    }
                    if (!dup) list.Add(r);
                }
            }
        }

        // Sort ascending
        list.Sort((a, b) =>
        {
            int cmp = a.width.CompareTo(b.width);
            if (cmp != 0) return cmp;
            cmp = a.height.CompareTo(b.height);
            if (cmp != 0) return cmp;
            return ((float)a.refreshRateRatio.value).CompareTo((float)b.refreshRateRatio.value);
        });

        // Fallback to current if filter returns empty
        if (list.Count == 0) list.Add(Screen.currentResolution);
        return list.ToArray();
    }

    private static Resolution FindBestResolution(Resolution[] pool, int w, int h, float hz)
    {
        if (pool == null || pool.Length == 0) return default;
        if (w == 0 || h == 0 || hz == 0f) return default;

        // Prefer exact match first
        for (int i = 0; i < pool.Length; i++)
        {
            var r = pool[i];
            if (r.width == w && r.height == h && Mathf.Abs((float)r.refreshRateRatio.value - hz) < 0.6f)
                return r;
        }

        // Otherwise pick the closest by area then hz
        int bestIdx = IndexOfClosest(pool, w, h, hz);
        return pool[Mathf.Clamp(bestIdx, 0, pool.Length - 1)];
    }

    private static int IndexOfClosest(Resolution[] pool, int w, int h, float hz)
    {
        int best = 0;
        double bestScore = double.MaxValue;

        for (int i = 0; i < pool.Length; i++)
        {
            var r = pool[i];
            double areaDiff = System.Math.Abs((double)r.width * r.height - (double)w * h);
            double hzDiff = System.Math.Abs((double)r.refreshRateRatio.value - hz);
            double score = areaDiff + hzDiff * 1000.0; // weight hz less than pixels

            if (score < bestScore)
            {
                bestScore = score;
                best = i;
            }
        }
        return best;
    }
}







