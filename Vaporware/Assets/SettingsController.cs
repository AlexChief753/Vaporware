// SettingsController.cs
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SettingsController : MonoBehaviour
{
    [Header("Roots")]
    public GameObject panelRoot;
    public GameObject audioPanel;
    public GameObject videoPanel;
    public GameObject controlsPanel;

    [Header("Focus")]
    public Button firstTabButton; 

    private Canvas _parentCanvas;
    private bool _weEnabledCanvas;
    private Action _onClosed;

    private bool _wasPausedBefore;

    public bool IsOpen => panelRoot && panelRoot.activeInHierarchy;

    void Awake()
    {
        if (panelRoot) panelRoot.SetActive(false);
        _parentCanvas = GetComponentInParent<Canvas>(true);
    }

    public void Open(Action onClosed = null)
    {
        _onClosed = onClosed;

        _wasPausedBefore = Time.timeScale == 0f;

        if (panelRoot) panelRoot.SetActive(true);

        // Only pause if we were in gameplay AND not already paused
        bool inGame = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "SampleScene";
        if (inGame && !_wasPausedBefore)
            Time.timeScale = 0f;

        // FIrst tab
        ShowTab("Controls");

        // Focus
        var es = EventSystem.current;
        if (es && firstTabButton)
        {
            es.SetSelectedGameObject(null);
            es.SetSelectedGameObject(firstTabButton.gameObject);
        }
    }

    public void Close()
    {
        if (panelRoot) panelRoot.SetActive(false);

        bool inGame = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "SampleScene";

        // Only unpause if WE paused it
        if (inGame && !_wasPausedBefore)
            Time.timeScale = 1f;

        SettingsService.Instance?.Save();
        _onClosed?.Invoke();
        _onClosed = null;
    }

    public void ShowTab(string tab)
    {
        audioPanel.SetActive(tab == "Audio");
        videoPanel.SetActive(tab == "Video");
        controlsPanel.SetActive(tab == "Controls");
    }

    public void ShowAudio() => ShowTab("Audio");
    public void ShowVideo() => ShowTab("Video");
    public void ShowControls() => ShowTab("Controls");
}
