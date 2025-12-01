using static CharacterID;

public static class CharacterRuntime
{
    public static CharacterId Selected = CharacterId.Trinity;
    public static bool HasSelection = false;

    public static void Set(CharacterId id)
    {
        Selected = id;
        HasSelection = true;
    }

    public static void Clear() => HasSelection = false;
}
