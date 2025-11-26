using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossUI : MonoBehaviour
{
    public GameObject root;
    public Image bossImage;
    public TextMeshProUGUI bossNameText;

    void Awake()
    {
        // Make boss panel is hidden when the scene starts
        if (root != null)
            root.SetActive(false);
    }

    public void ForceShowBoss(Boss boss)
    {
        if (boss == null) return;

        root.SetActive(true);
        bossNameText.text = boss.bossName;
        bossImage.sprite = boss.bossPortrait;
    }

    public void Hide()
    {
        root.SetActive(false);
    }
}



