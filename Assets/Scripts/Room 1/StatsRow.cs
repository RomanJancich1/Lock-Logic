using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatsRow : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] TMP_Text placeText;
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text timeText;
    [SerializeField] Image medalImage;

    [Header("Medals")]
    [SerializeField] Sprite gold;
    [SerializeField] Sprite silver;
    [SerializeField] Sprite bronze;

    public void Set(int place, string playerName, string timeStr)
    {
        if (placeText) placeText.text = place.ToString();
        if (nameText) nameText.text = playerName;
        if (timeText) timeText.text = timeStr;

        if (!medalImage) return;

        if (place == 1 && gold) { medalImage.enabled = true; medalImage.sprite = gold; }
        else if (place == 2 && silver) { medalImage.enabled = true; medalImage.sprite = silver; }
        else if (place == 3 && bronze) { medalImage.enabled = true; medalImage.sprite = bronze; }
        else { medalImage.enabled = false; }
    }
}
