using UnityEngine;
using TMPro;

public class ComboCounterUI : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private TextMeshProUGUI comboText;

    private int lastCombo = int.MinValue;

    private void Start()
    {
        // Initialize display once at start
        UpdateComboText();
    }

    private void Update()
    {
        if (GameGrid.comboCount != lastCombo)
        {
            UpdateComboText();
        }
    }

    private void UpdateComboText()
    {
        lastCombo = GameGrid.comboCount;

        if (comboText != null)
        {
            if (GameGrid.comboCount >= 2)
            {
                comboText.enabled = true;
                comboText.text = $"Combo: {GameGrid.comboCount}";
            }
            else
            {
                comboText.enabled = false; // hides the text completely
            }
        }
    }
}


