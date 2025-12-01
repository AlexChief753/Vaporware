using System.Collections.Generic;
using UnityEngine;
using static CharacterID;

[CreateAssetMenu(fileName = "CharacterRoster", menuName = "Game/Characters/Roster")]
public class CharacterRosterSO : ScriptableObject
{
    public List<CharacterDefinitionSO> characters = new();

    public CharacterDefinitionSO GetById(CharacterId id)
        => characters.Find(c => c && c.id == id);
}
