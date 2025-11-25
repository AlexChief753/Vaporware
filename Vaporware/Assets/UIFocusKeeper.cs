
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIFocusKeeper : MonoBehaviour
{
    public Selectable firstSelected;

    private bool usingMouse = false;
    private float mouseCooldown = 0.25f;   // time before controller can take over again
    private float mouseTimer = 0f;

    private bool controllerUsedOnce = false;

    void OnEnable()
    {
        SetFocus(firstSelected);
        usingMouse = false;
        mouseTimer = 0f;
    }

    void Update()
    {
        var es = EventSystem.current;
        if (!gameObject.activeInHierarchy || es == null)
            return;

        // Detect mouse movement or mouse click
        if (Input.GetAxisRaw("Mouse X") != 0 ||
            Input.GetAxisRaw("Mouse Y") != 0 ||
            Input.GetMouseButtonDown(0) ||
            Input.GetMouseButtonDown(1))
        {
            usingMouse = true;
            mouseTimer = 0f;
        }

        // Count up after last mouse input
        if (usingMouse)
        {
            mouseTimer += Time.unscaledDeltaTime;

            // While in mouse mode never force controller selection
            if (mouseTimer < mouseCooldown)
                return;
            else
                usingMouse = false; // Controller allowed again
        }

        // Detect controller input
        bool controllerInput =
            // Controller
            Mathf.Abs(Input.GetAxisRaw("DPadX")) > 0.1f ||
            Mathf.Abs(Input.GetAxisRaw("DPadY")) > 0.1f ||
            Input.GetButtonDown("Submit") ||
            Input.GetButtonDown("Cancel") ||

            // Keyboard Arrow Keys
            Input.GetKeyDown(KeyCode.UpArrow) ||
            Input.GetKeyDown(KeyCode.DownArrow) ||
            Input.GetKeyDown(KeyCode.LeftArrow) ||
            Input.GetKeyDown(KeyCode.RightArrow);

        if (controllerInput)
        {
            controllerUsedOnce = true;

            if (es.currentSelectedGameObject == null ||
                !es.currentSelectedGameObject.activeInHierarchy)
            {
                SetFocus(firstSelected);
            }
            return;
        }

        // Do not auto-select if controller has never been used
        if (!controllerUsedOnce)
            return;

        // Mouse mode - never auto-select anything
        if (usingMouse)
            return;

        // If neither controller or mouse used, do nothing
    }

    private void SetFocus(Selectable s)
    {
        if (s == null) return;
        var es = EventSystem.current;
        es.SetSelectedGameObject(null);
        es.SetSelectedGameObject(s.gameObject);
    }
}


