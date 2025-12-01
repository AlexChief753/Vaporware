using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static CharacterID;

public class CharacterSelectNav : MonoBehaviour
{
    [Header("Refs")]
    public CharacterSelectUI ui;
    public Button trinityTabButton;
    public Button placeholderTabButton;
    public Button selectButton;


    void Awake()
    {
        // Fallback auto-wiring if dropped on the prefab after UI:
        if (!ui) ui = GetComponent<CharacterSelectUI>();

        // Ensure the two tabs behave like typical selectable buttons via explicit nav
        WireExplicitNavigation();
    }

    void OnEnable()
    {
        // When the menu opens, make sure something is selected for gamepad users
        var es = EventSystem.current;
        if (es != null && es.currentSelectedGameObject == null && trinityTabButton)
            es.SetSelectedGameObject(trinityTabButton.gameObject);
    }

    void Update()
    {
        // Bumper / Keyboard cycling between tabs
        // Left bumper / Q  previous tab
        if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.JoystickButton4))
            Cycle(-1);

        // Right bumper / E next tab
        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.JoystickButton5))
            Cycle(+1);

        // arrow keys can also cycle when a tab is focused
        if (IsTabFocused())
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow)) Cycle(-1);
            if (Input.GetKeyDown(KeyCode.RightArrow)) Cycle(+1);
        }

        // If selection gets lost when clicking empty space, restore focus
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == null && trinityTabButton)
            EventSystem.current.SetSelectedGameObject(trinityTabButton.gameObject);
    }

    void Cycle(int dir)
    {
        if (ui == null) return;
        var next = NextId(ui.CurrentId, dir);

        // Switch the visible tab/page
        ui.ShowTab(next);

        // Move UI focus to the corresponding tab button so d-pad works naturally
        var targetBtn = (next == CharacterId.Trinity) ? trinityTabButton : placeholderTabButton;
        if (targetBtn && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(targetBtn.gameObject);
    }

    CharacterId NextId(CharacterId current, int dir)
    {
        // Two tabs only for now unless we make more than 2 characters
        if (dir > 0)
            return current == CharacterId.Trinity ? CharacterId.Placeholder : CharacterId.Trinity;
        else
            return current == CharacterId.Trinity ? CharacterId.Placeholder : CharacterId.Trinity;
    }

    bool IsTabFocused()
    {
        var es = EventSystem.current;
        if (es == null) return false;
        var go = es.currentSelectedGameObject;
        if (!go) return false;
        return (trinityTabButton && go == trinityTabButton.gameObject) ||
               (placeholderTabButton && go == placeholderTabButton.gameObject);
    }

    void WireExplicitNavigation()
    {
        // Make left/right on the tabs jump between each other, and down moves to Select button
        if (trinityTabButton)
        {
            var nav = new Navigation { mode = Navigation.Mode.Explicit };
            nav.selectOnRight = placeholderTabButton;
            nav.selectOnDown = selectButton;
            trinityTabButton.navigation = nav;
        }

        if (placeholderTabButton)
        {
            var nav = new Navigation { mode = Navigation.Mode.Explicit };
            nav.selectOnLeft = trinityTabButton;
            nav.selectOnDown = selectButton;
            placeholderTabButton.navigation = nav;
        }

        if (selectButton)
        {
            // Up from Select goes back to the currently active tab, left/right keep default
            var nav = selectButton.navigation;
            nav.mode = Navigation.Mode.Explicit;
            nav.selectOnUp = (ui != null && ui.CurrentId == CharacterId.Placeholder)
                             ? placeholderTabButton
                             : trinityTabButton;
            selectButton.navigation = nav;
        }
    }
}
