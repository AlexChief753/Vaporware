using System.Collections.Generic;
using UnityEngine;
using static CharacterID;

[CreateAssetMenu(fileName = "CharacterDefinition", menuName = "Game/Characters/Definition")]
public class CharacterDefinitionSO : ScriptableObject
{
    public CharacterId id;
    public string displayName = "New Character";
    [TextArea(2, 4)] public string shortDescription = "Describe this character...";
    public List<string> statLines = new List<string>();
    public Sprite portrait;
}
