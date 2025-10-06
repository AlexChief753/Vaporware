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

    public bool IsOpen => panelRoot && panelRoot.activeInHierarchy;

    void Awake()
    {
        if (panelRoot) panelRoot.SetActive(false);
        _parentCanvas = GetComponentInParent<Canvas>(true);
    }

    public void Open(Action onClosed = null)
    {
        _onClosed = onClosed;

        // Ensure parent canvas is active
        if (_parentCanvas && !_parentCanvas.gameObject.activeSelf)
        {
            _parentCanvas.gameObject.SetActive(true);
            _weEnabledCanvas = true;
        }

        if (panelRoot) panelRoot.SetActive(true);

        // Pause gameplay only if we're in the gameplay scene
        bool inGame = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "SampleScene";
        if (inGame) Time.timeScale = 0f;

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
        if (inGame) Time.timeScale = 1f;

        SettingsService.Instance?.Save();
        _onClosed?.Invoke();
        _onClosed = null;

        if (_weEnabledCanvas && _parentCanvas)
        {
            _parentCanvas.gameObject.SetActive(false);
            _weEnabledCanvas = false;
        }
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
