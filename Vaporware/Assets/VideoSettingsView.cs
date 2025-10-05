using UnityEngine;
using UnityEngine.UI;

public class VideoSettingsView : MonoBehaviour
{
    public Toggle fullscreenToggle;

    void OnEnable()
    {
        var s = SettingsService.Instance;
        if (fullscreenToggle)
            fullscreenToggle.SetIsOnWithoutNotify(s.Current.fullscreen);
    }

    public void OnFullscreenChanged(bool on)
    {
        var s = SettingsService.Instance;
        s.Current.fullscreen = on;
        s.ApplyAll();
        s.Save();
    }
}


