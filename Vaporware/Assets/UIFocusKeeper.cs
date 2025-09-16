// UIFocusKeeper.cs
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIFocusKeeper : MonoBehaviour, IPointerDownHandler
{
    public Selectable firstSelected;

    void OnEnable() { Select(firstSelected); }

    void Update()
    {
        var es = EventSystem.current;
        if (!gameObject.activeInHierarchy || es == null) return;

        // If nothing is selected (often after a mouse click on a non-selectable), reselect
        if (es.currentSelectedGameObject == null ||
            !es.currentSelectedGameObject.activeInHierarchy)
        {
            Select(firstSelected);
        }
    }

    public void OnPointerDown(PointerEventData _)
    {
        // Clicking the background will immediately restore focus
        Select(firstSelected);
    }

    void Select(Selectable s)
    {
        if (s == null) return;
        var es = EventSystem.current;
        es.SetSelectedGameObject(null);
        es.SetSelectedGameObject(s.gameObject);
    }
}

