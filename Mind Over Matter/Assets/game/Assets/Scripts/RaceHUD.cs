using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RaceHUD : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text distanceText; // shows "123 m"
    [SerializeField] private TMP_Text timerText;    // shows "MM:SS"
    [SerializeField] private Image stackFill;       // Image type = Filled (Horizontal)
    [SerializeField] private TMP_Text stackLabel;   // optional "2/3"

    void Update()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        // Distance (integer meters)
        int meters = Mathf.FloorToInt(gm.DistanceMeters);
        distanceText.text = meters + " m";

        // Timer MM:SS
        float t = gm.ElapsedSeconds;
        int minutes = (int)(t / 60f);
        int seconds = (int)(t % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";

        // Stacks bar fill (0..1)
        if (stackFill)
        {
            float fill = gm.MaxSpeedStacks > 0
                ? (float)gm.CurrentStacks / gm.MaxSpeedStacks
                : 0f;
            stackFill.fillAmount = fill;
        }

        if (stackLabel)
            stackLabel.text = $"{gm.CurrentStacks}/{gm.MaxSpeedStacks}";
    }
}

