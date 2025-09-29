using UnityEngine;
using static CharacterID;

public class CharacterSelectGate : MonoBehaviour
{
    [Header("Assign the Character Select UI Prefab")]
    public CharacterSelectUI characterSelectPrefab;

    void Start()
    {
        if (GameSession.startMode == StartMode.NewGame)
        {
            ShowCharacterSelect();
        }
        else // LoadGame
        {
            // Ensure runtime has the saved character
            var idStr = GameSession.pendingSaveData != null ? GameSession.pendingSaveData.characterId : null;
            if (!string.IsNullOrEmpty(idStr) && System.Enum.TryParse(idStr, out CharacterId id))
                CharacterRuntime.Set(id);
            else
                CharacterRuntime.Set(CharacterId.Trinity);
        }
    }

    private void ShowCharacterSelect()
    {
        if (!characterSelectPrefab) { Debug.LogError("[CharacterSelectGate] Prefab missing"); return; }

        var ui = Instantiate(characterSelectPrefab);
        Time.timeScale = 0f; // pause gameplay while in menu
        ui.onCharacterConfirmed += () =>
        {
            Time.timeScale = 1f; // resume once picked
        };
    }
}
