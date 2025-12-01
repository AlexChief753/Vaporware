using System;
using System.Collections.Generic;
using UnityEngine;
using static CharacterID;

[AddComponentMenu("Game/Characters/Character Art Manager")]
public class CharacterArtManager : MonoBehaviour
{
    [Header("Where to spawn the art (empty child objects under each panel/HUD)")]
    [Tooltip("Empty RectTransform inside your Level Complete panel where the success art prefab should spawn.")]
    public Transform levelCompleteParent;

    [Tooltip("Empty RectTransform inside your Game Over panel where the fail art prefab should spawn.")]
    public Transform gameOverParent;

    [Tooltip("Empty RectTransform on your HUD (visible during gameplay) where the in-level character art should spawn.")]
    public Transform levelArtParent;

    [Tooltip("Empty RectTransform inside your Pause Menu panel where the pause art prefab should spawn.")]
    public Transform pauseMenuParent;

    [Tooltip("If true, removes previous children under the parent before spawning.")]
    public bool clearExistingChildrenBeforeSpawn = true;

    [Header("Per-Character UI Prefabs")]
    public List<CharacterArtSet> artSets = new()
    {
        new CharacterArtSet { id = CharacterId.Trinity },
        new CharacterArtSet { id = CharacterId.Placeholder },
    };

    [Serializable]
    public class CharacterArtSet
    {
        public CharacterId id;
        public GameObject successPrefab; // Level Complete
        public GameObject failPrefab;    // Game Over
        public GameObject levelPrefab;   // During gameplay (HUD)
        public GameObject pausePrefab;   // Pause Menu panel
    }

    // internals
    private readonly Dictionary<CharacterId, CharacterArtSet> _lookup = new();
    private CharacterId _activeId;

    private bool _lastLevelCompleteActive;
    private bool _lastGameOverActive;
    private bool _lastLevelArtActive;
    private bool _lastPauseMenuActive;

    private GameObject _levelArtInstance;

    void Awake()
    {
        _lookup.Clear();
        foreach (var set in artSets)
            if (!_lookup.ContainsKey(set.id)) _lookup.Add(set.id, set);

        _activeId = CharacterRuntime.Selected;
    }

    void Start()
    {
        if (IsActive(levelArtParent))
            SpawnLevelArt(true);
        if (IsActive(pauseMenuParent))
            SpawnPauseArt();
    }

    void Update()
    {
        // Character selection changed, refresh visible panels
        if (_activeId != CharacterRuntime.Selected)
        {
            _activeId = CharacterRuntime.Selected;
            TryRefreshVisible();
        }

        // Level Complete toggle
        bool levelCompleteActive = IsActive(levelCompleteParent);
        if (levelCompleteActive && !_lastLevelCompleteActive) SpawnSuccessArt();
        _lastLevelCompleteActive = levelCompleteActive;

        // Game Over toggle
        bool gameOverActive = IsActive(gameOverParent);
        if (gameOverActive && !_lastGameOverActive) SpawnFailArt();
        _lastGameOverActive = gameOverActive;

        // Level (HUD) art toggle
        bool levelArtActive = IsActive(levelArtParent);
        if (levelArtActive && !_lastLevelArtActive) SpawnLevelArt(true);
        _lastLevelArtActive = levelArtActive;

        // Pause Menu toggle
        bool pauseMenuActive = IsActive(pauseMenuParent);
        if (pauseMenuActive && !_lastPauseMenuActive) SpawnPauseArt();
        _lastPauseMenuActive = pauseMenuActive;
    }

    private void TryRefreshVisible()
    {
        if (IsActive(levelCompleteParent)) SpawnSuccessArt();
        if (IsActive(gameOverParent)) SpawnFailArt();
        if (IsActive(levelArtParent)) SpawnLevelArt(false);
        if (IsActive(pauseMenuParent)) SpawnPauseArt();
    }

    private void SpawnSuccessArt()
    {
        if (!_lookup.TryGetValue(_activeId, out var set) || set.successPrefab == null || !IsActive(levelCompleteParent)) return;
        SpawnUnder(levelCompleteParent, set.successPrefab, true);
    }

    private void SpawnFailArt()
    {
        if (!_lookup.TryGetValue(_activeId, out var set) || set.failPrefab == null || !IsActive(gameOverParent)) return;
        SpawnUnder(gameOverParent, set.failPrefab, true);
    }

    private void SpawnLevelArt(bool respectClearSetting)
    {
        if (!_lookup.TryGetValue(_activeId, out var set) || set.levelPrefab == null || !IsActive(levelArtParent)) return;

        if (_levelArtInstance != null) Destroy(_levelArtInstance);

        if (respectClearSetting && clearExistingChildrenBeforeSpawn)
            ClearChildren(levelArtParent);

        _levelArtInstance = Instantiate(set.levelPrefab, levelArtParent);
        NormalizeUITransform(_levelArtInstance.transform);
    }

    private void SpawnPauseArt()
    {
        if (!_lookup.TryGetValue(_activeId, out var set) || set.pausePrefab == null || !IsActive(pauseMenuParent)) return;
        SpawnUnder(pauseMenuParent, set.pausePrefab, true);
    }

    private void SpawnUnder(Transform parent, GameObject prefab, bool normalize)
    {
        if (clearExistingChildrenBeforeSpawn)
            ClearChildren(parent);

        var go = Instantiate(prefab, parent);
        if (normalize) NormalizeUITransform(go.transform);
    }

    private static void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
            UnityEngine.Object.Destroy(parent.GetChild(i).gameObject);
    }

    private static void NormalizeUITransform(Transform t)
    {
        if (t is RectTransform rt)
        {
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.localScale = Vector3.one;
        }
        else
        {
            t.localPosition = Vector3.zero;
            t.localScale = Vector3.one;
        }
    }

    private static bool IsActive(Transform t) => t != null && t.gameObject.activeInHierarchy;
}


