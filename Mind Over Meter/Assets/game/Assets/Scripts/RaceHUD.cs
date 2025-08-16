using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RaceHUD : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text distanceText; // e.g. "123 m"
    [SerializeField] private TMP_Text timerText;    // e.g. "MM:SS"
    [SerializeField] private Image stackFill;       // Image Type = Filled (Horizontal)
    [SerializeField] private TMP_Text stackLabel;   // optional "2/3"

    void Update()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        // Distance (effective: base constant rate + nudges)
        int meters = Mathf.FloorToInt(gm.EffectiveDistanceMeters);
        if (distanceText) distanceText.text = meters + " m";

        // Global timer MM:SS
        float t = gm.ElapsedSeconds;
        int minutes = (int)(t / 60f);
        int seconds = (int)(t % 60f);
        if (timerText) timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        // Stacks bar fill
        if (stackFill)
        {
            float fill = gm.MaxSpeedStacks > 0
                ? (float)gm.CurrentStacks / gm.MaxSpeedStacks
                : 0f;
            stackFill.fillAmount = fill;
        }

        if (stackLabel)
        {
            stackLabel.text = gm.CurrentStacks + "/" + gm.MaxSpeedStacks;
        }
    }
}

