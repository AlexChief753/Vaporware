using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static CharacterID;

public class CharacterSelectUI : MonoBehaviour
{
    [Header("Data")]
    public CharacterRosterSO roster;

    [Header("Tabs")]
    public Button trinityTabButton;
    public Button placeholderTabButton;

    [Header("View")]
    public Image portraitImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public Transform statsParent;
    public TextMeshProUGUI statLinePrefab;
    public Button selectButton;

    public event Action onCharacterConfirmed;

    private CharacterDefinitionSO currentDef;
    private readonly List<TextMeshProUGUI> statLabels = new();

    void Awake()
    {
        if (selectButton) selectButton.onClick.AddListener(ConfirmSelection);
        if (trinityTabButton) trinityTabButton.onClick.AddListener(() => ShowTab(CharacterId.Trinity));
        if (placeholderTabButton) placeholderTabButton.onClick.AddListener(() => ShowTab(CharacterId.Placeholder));
    }

    void Start()
    {
        // Default to last saved character if a save exists, otherwise Trinity
        var fallback = CharacterId.Trinity;
        if (GameSession.startMode == StartMode.LoadGame && GameSession.pendingSaveData != null)
        {
            if (!string.IsNullOrEmpty(GameSession.pendingSaveData.characterId) &&
                Enum.TryParse(GameSession.pendingSaveData.characterId, out CharacterId loadedId))
                fallback = loadedId;
        }
        ShowTab(fallback);
    }

    public CharacterId CurrentId { get; private set; } = CharacterId.Trinity;
    public void ShowTab(CharacterId id)
    {
        var def = roster.GetById(id);
        if (!def) { Debug.LogError("Character not found in roster: " + id); return; }

        currentDef = def;
        CurrentId = id;

        if (nameText) nameText.text = def.displayName;
        if (descriptionText) descriptionText.text = def.shortDescription;

        if (portraitImage)
        {
            portraitImage.enabled = def.portrait != null;
            portraitImage.sprite = def.portrait;
        }

        foreach (var t in statLabels) if (t) Destroy(t.gameObject);
        statLabels.Clear();

        if (statsParent && statLinePrefab)
        {
            foreach (var line in def.statLines)
            {
                var label = Instantiate(statLinePrefab, statsParent);
                label.text = "· " + line; // bullet point. Might have to replace this with just a UI of a bullet point because of spacing issues
                label.gameObject.SetActive(true);
                statLabels.Add(label);
            }
        }
    }


    private void ConfirmSelection()
    {
        if (!currentDef) return;
        CharacterRuntime.Set(currentDef.id);
        onCharacterConfirmed?.Invoke();
        Destroy(gameObject);
    }
}
