using UnityEngine;

public class CanvasController : MonoBehaviour
{
    public GameObject targetCanvas;

    public void TurnOffCanvas()
    {
        if (targetCanvas != null)
        {
            targetCanvas.SetActive(false); // Deactivates the Canvas
        }
    }

    public void TurnOnCanvas()
    {
        if (targetCanvas != null)
        {
            targetCanvas.SetActive(true); // Activates the Canvas
        }
    }
}

