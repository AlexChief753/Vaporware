using UnityEngine;
using TMPro;

public class GameOverStatsUI : MonoBehaviour
{
    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI totalScoreText;
    [SerializeField] private TextMeshProUGUI levelScoreText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI currencyText;

    [Header("Root")]
    [SerializeField] private GameObject root;

    private static string FormatTime(float seconds)
    {
        if (seconds < 0) seconds = 0;
        int m = Mathf.FloorToInt(seconds / 60f);
        int s = Mathf.FloorToInt(seconds % 60f);
        return $"{m:00}:{s:00}";
    }


    public void ShowNow(float levelTimeConfigured, float currentTimeRemaining)
    {
        // Stats come straight from GameGrid
        if (levelText) levelText.text = ": " + GameGrid.level.ToString();
        if (totalScoreText) totalScoreText.text = ": " + GameGrid.totalScore.ToString();
        if (levelScoreText) levelScoreText.text = ": " + GameGrid.levelScore.ToString();

        float elapsed = Mathf.Clamp(levelTimeConfigured - currentTimeRemaining, 0f, levelTimeConfigured);
        if (timeText) timeText.text = ": " + FormatTime(elapsed);

        if (currencyText) currencyText.text = ": " + GameGrid.currency.ToString();

        if (root) root.SetActive(true);
        else gameObject.SetActive(true);
    }
}

