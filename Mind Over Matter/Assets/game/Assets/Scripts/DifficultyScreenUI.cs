using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class DifficultyScreenUI : MonoBehaviour
{
    [Header("Scenes")]
    [SerializeField] private string raceSceneName = "Race";
    [SerializeField] private string startScreenSceneName = "start screen";

    [Header("UI (optional: auto-find by name)")]
    [SerializeField] private Button easyButton;
    [SerializeField] private Button normalButton;
    [SerializeField] private Button hardButton;
    [SerializeField] private Button startButton;
    [SerializeField] private Button backButton;
    [SerializeField] private TMP_Text blurb;   // optional info text

    private GameDifficulty pending = GameDifficulty.Normal;

    void Awake()
    {
        EnsureEventSystem();

        // Auto-find if not assigned (uses child names; rename to match if needed)
        if (!easyButton) easyButton = FindButton("Easy");
        if (!normalButton) normalButton = FindButton("Normal");
        if (!hardButton) hardButton = FindButton("Hard");
        if (!startButton) startButton = FindButton("Start");
        if (!backButton) backButton = FindButton("Back");
        if (!blurb) blurb = FindText("Blurb");

        // Wire listeners
        Wire(easyButton, () => Select(GameDifficulty.Easy));
        Wire(normalButton, () => Select(GameDifficulty.Normal));
        Wire(hardButton, () => Select(GameDifficulty.Hard));
        Wire(startButton, OnStartRace);
        Wire(backButton, OnBack);

        // Initial state (carry over last choice if already set)
        pending = DifficultySettings.Selected;
        UpdateBlurb();
        UpdateButtonStates();
    }

    // --- Button handlers ---
    private void OnStartRace()
    {
        DifficultySettings.Selected = pending;
        SceneManager.LoadScene(raceSceneName);
    }

    private void OnBack()
    {
        SceneManager.LoadScene(startScreenSceneName);
    }

    private void Select(GameDifficulty d)
    {
        pending = d;
        UpdateBlurb();
        UpdateButtonStates();
    }

    // --- UI updates ---
    private void UpdateBlurb()
    {
        if (!blurb) return;

        // Preview tuning for the pending difficulty (without committing it yet)
        var prev = DifficultySettings.Selected;
        DifficultySettings.Selected = pending;
        var t = DifficultySettings.GetTuning();
        DifficultySettings.Selected = prev;

        blurb.text =
            $"Difficulty: {pending}\n" ;
    }


    private void UpdateButtonStates()
    {
        // Simple visual cue: disable the selected difficulty button
        if (easyButton) easyButton.interactable = pending != GameDifficulty.Easy;
        if (normalButton) normalButton.interactable = pending != GameDifficulty.Normal;
        if (hardButton) hardButton.interactable = pending != GameDifficulty.Hard;
    }

    // --- Helpers ---
    private void Wire(Button b, UnityEngine.Events.UnityAction onClick)
    {
        if (!b) return;
        b.onClick.RemoveAllListeners();
        b.onClick.AddListener(onClick);
    }

    private Button FindButton(string name)
    {
        foreach (var b in GetComponentsInChildren<Button>(true))
            if (b.name == name || b.gameObject.name == name) return b;
        return null;
    }

    private TMP_Text FindText(string name)
    {
        foreach (var t in GetComponentsInChildren<TMP_Text>(true))
            if (t.name == name || t.gameObject.name == name) return t;
        return null;
    }

    private void EnsureEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            var go = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
            go.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
            go.AddComponent<StandaloneInputModule>();
#endif
            DontDestroyOnLoad(go);
        }
    }
}
