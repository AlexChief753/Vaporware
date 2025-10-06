using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ControlsSettingsView : MonoBehaviour
{
    [Header("UI")]
    public Button gamepadButton;
    public Button mouseKbButton;
    public Image sheetImage;

    [Header("Sprites")]
    public Sprite gamepadSprite;
    public Sprite mouseKbSprite;

    void Awake()
    {
        // Wire buttons
        if (gamepadButton) gamepadButton.onClick.AddListener(() => SetSheet(0));
        if (mouseKbButton) mouseKbButton.onClick.AddListener(() => SetSheet(1));
    }

    void OnEnable()
    {
        // Read saved choice (default 0 = Gamepad)
        int idx = (SettingsService.Instance != null) ? SettingsService.Instance.Current.controlsSheetIndex : 0;
        ApplyIndex(idx, selectButton: false);
    }

    private void SetSheet(int idx)
    {
        ApplyIndex(idx, selectButton: true);
        // Persist
        if (SettingsService.Instance != null)
        {
            SettingsService.Instance.Current.controlsSheetIndex = idx;
            SettingsService.Instance.Save();
        }
    }

    private void ApplyIndex(int idx, bool selectButton)
    {
        // Swap sprite
        if (sheetImage)
            sheetImage.sprite = (idx == 0 ? gamepadSprite : mouseKbSprite);

        // Simple visual feedback: disable the active button so it looks "selected"
        if (gamepadButton) gamepadButton.interactable = (idx != 0);
        if (mouseKbButton) mouseKbButton.interactable = (idx != 1);

        // Move controller focus to the other still interactable button after clicking
        if (selectButton)
        {
            var es = EventSystem.current;
            if (es)
            {
                var next = (idx == 0 ? mouseKbButton : gamepadButton);
                if (next) { es.SetSelectedGameObject(null); es.SetSelectedGameObject(next.gameObject); }
            }
        }
    }
}

