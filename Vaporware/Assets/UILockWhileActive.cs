using UnityEngine;
using UnityEngine.EventSystems;

public class UILockWhileActive : MonoBehaviour
{
    [Tooltip("All CanvasGroups to lock while this object is active (MainMenuCanvas, PauseMenuCanvas).")]
    public CanvasGroup[] toLock;

    [Tooltip("Optional: if set, we'll clear selection when locking and set this object as selected (the first tab button).")]
    public GameObject focusWhenLocked;

    private bool _locked;

    void OnEnable()
    {
        SetLocked(true);

        // If a focus target is provided, move controller/keyboard focus there
        if (focusWhenLocked)
        {
            var es = EventSystem.current;
            if (es)
            {
                es.SetSelectedGameObject(null);
                es.SetSelectedGameObject(focusWhenLocked);
            }
        }
    }

    void OnDisable()
    {
        SetLocked(false);
    }

    private void SetLocked(bool locked)
    {
        _locked = locked;
        if (toLock == null) return;

        foreach (var cg in toLock)
        {
            if (!cg) continue;
            // Keep visuals visible (alpha unchanged), but block interaction + raycasts
            cg.interactable = !locked;
            cg.blocksRaycasts = !locked;
        }

        // If we just locked, and something from a locked canvas was selected, clear it
        if (locked)
        {
            var es = EventSystem.current;
            if (es && es.currentSelectedGameObject)
            {
                // If selected belongs to any locked group, clear it
                foreach (var cg in toLock)
                {
                    if (!cg) continue;
                    if (es.currentSelectedGameObject.transform.IsChildOf(cg.transform))
                    {
                        es.SetSelectedGameObject(null);
                        break;
                    }
                }
            }
        }
    }
}

