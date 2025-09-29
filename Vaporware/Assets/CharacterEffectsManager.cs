
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static CharacterID;

[AddComponentMenu("Game/Characters/Character Effects Manager")]
public class CharacterEffectsManager : MonoBehaviour
{
    [Header("HUD")]
    public TextMeshProUGUI characterNameText;

    [Header("Per-Character Modifiers")]
    public List<CharacterModifier> modifiers = new()
    {
        // Trinity: lower score per line, boost rare item odds
        new CharacterModifier {
            id = CharacterId.Trinity,
            displayName = "Trinity",
            scoreMultiplier = 0.90f,
            // rarity weight multipliers (1 = normal):
            commonMul = 1.00f, uncommonMul = 1.00f, rareMul = 1.50f, epicMul = 1.00f, legendaryMul = 1.00f
        },
        // Placeholder: all normal
        new CharacterModifier {
            id = CharacterId.Placeholder,
            displayName = "Placeholder",
            scoreMultiplier = 1.00f,
            commonMul = 1.00f, uncommonMul = 1.00f, rareMul = 1.00f, epicMul = 1.00f, legendaryMul = 1.00f
        },
    };

    [Serializable]
    public class CharacterModifier
    {
        public CharacterId id;
        public string displayName = "Unnamed";
        [Range(0.1f, 3f)] public float scoreMultiplier = 1f;

        // per-rarity multipliers for shop weights
        [Header("Shop rarity weight multipliers (1 = normal)")]
        [Range(0f, 50f)] public float commonMul = 1f;
        [Range(0f, 50f)] public float uncommonMul = 1f;
        [Range(0f, 50f)] public float rareMul = 1f;
        [Range(0f, 50f)] public float epicMul = 1f;
        [Range(0f, 50f)] public float legendaryMul = 1f;
    }

    private static float _activeMultiplier = 1f; // used by GameGrid.ScoreAdd
    private static string _activeName = "";

    // active rarity multipliers (static so ShopService can read without refs)
    private static float _mulCommon = 1f, _mulUncommon = 1f, _mulRare = 1f, _mulEpic = 1f, _mulLegendary = 1f;

    private readonly Dictionary<CharacterId, CharacterModifier> _byId = new();
    private CharacterId _lastSelected;

    void Awake()
    {
        _byId.Clear();
        foreach (var m in modifiers)
            if (!_byId.ContainsKey(m.id)) _byId.Add(m.id, m);

        _lastSelected = CharacterRuntime.Selected;
        RefreshActiveFromSelection(_lastSelected);
    }

    void Start()
    {
        if (characterNameText != null) characterNameText.text = _activeName;
    }

    void Update()
    {
        if (CharacterRuntime.Selected != _lastSelected)
        {
            _lastSelected = CharacterRuntime.Selected;
            RefreshActiveFromSelection(_lastSelected);
            if (characterNameText != null) characterNameText.text = _activeName;
        }
    }

    private void RefreshActiveFromSelection(CharacterId id)
    {
        if (_byId.TryGetValue(id, out var mod))
        {
            _activeMultiplier = Mathf.Clamp(mod.scoreMultiplier, 0.1f, 3f);
            _activeName = mod.displayName;

            // commit rarity multipliers for the active character
            _mulCommon = mod.commonMul;
            _mulUncommon = mod.uncommonMul;
            _mulRare = mod.rareMul;
            _mulEpic = mod.epicMul;
            _mulLegendary = mod.legendaryMul;
        }
        else
        {
            _activeMultiplier = 1f;
            _activeName = id.ToString();

            _mulCommon = _mulUncommon = _mulRare = _mulEpic = _mulLegendary = 1f;
        }
    }

    // static hook used inside GameGrid.ScoreAdd
    public static int ApplyCharacterScoring(int basePoints)
    {
        return Mathf.RoundToInt(basePoints * _activeMultiplier);
    }

    // static hook for ShopService
    public static float GetRarityWeightMultiplier(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common: return _mulCommon;
            case ItemRarity.Uncommon: return _mulUncommon;
            case ItemRarity.Rare: return _mulRare;
            case ItemRarity.Epic: return _mulEpic;
            case ItemRarity.Legendary: return _mulLegendary;
            default: return 1f;
        }
    }
}




