using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class StartScreenUI : MonoBehaviour
{
    [Header("Scenes")]
    [SerializeField] private string difficultySceneName = "difficulty";

    [Header("UI (optional: leave empty; we’ll try to auto-find)")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private GameObject exitConfirmPanel;   // panel with Yes/No
    [SerializeField] private Button exitYesButton;
    [SerializeField] private Button exitNoButton;

    void Awake()
    {
        EnsureEventSystem();

        // Auto-find if not assigned (uses child names – adjust to your hierarchy)
        if (playButton == null) playButton = FindButton("Play");
        if (exitButton == null) exitButton = FindButton("Exit");
        if (exitConfirmPanel == null) exitConfirmPanel = FindObjectInChildren("ExitConfirmPanel");
        if (exitYesButton == null) exitYesButton = FindButton("Yes", exitConfirmPanel);
        if (exitNoButton == null) exitNoButton = FindButton("No", exitConfirmPanel);

        if (exitConfirmPanel) exitConfirmPanel.SetActive(false);

        // Wire listeners safely
        if (playButton != null)
        {
            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(OnPlay);
        }
        if (exitButton != null)
        {
            exitButton.onClick.RemoveAllListeners();
            exitButton.onClick.AddListener(OnExit);
        }
        if (exitYesButton != null)
        {
            exitYesButton.onClick.RemoveAllListeners();
            exitYesButton.onClick.AddListener(OnExitYes);
        }
        if (exitNoButton != null)
        {
            exitNoButton.onClick.RemoveAllListeners();
            exitNoButton.onClick.AddListener(OnExitNo);
        }
    }

    // --- Handlers ---
    private void OnPlay() => SceneManager.LoadScene(difficultySceneName);
    private void OnExit() { if (exitConfirmPanel) exitConfirmPanel.SetActive(true); }
    private void OnExitYes()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
    private void OnExitNo() { if (exitConfirmPanel) exitConfirmPanel.SetActive(false); }

    // --- Helpers ---
    private void EnsureEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            var go = new GameObject("EventSystem", typeof(EventSystem));
            // Add the right input module depending on your project input system
#if ENABLE_INPUT_SYSTEM
            go.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
            go.AddComponent<StandaloneInputModule>();
#endif
            DontDestroyOnLoad(go);
        }
    }

    private Button FindButton(string name, GameObject scope = null)
    {
        var root = scope ? scope.transform : transform;
        foreach (var b in root.GetComponentsInChildren<Button>(true))
            if (b.name == name || b.gameObject.name == name) return b;
        return null;
    }

    private GameObject FindObjectInChildren(string name)
    {
        foreach (Transform t in transform.GetComponentsInChildren<Transform>(true))
            if (t.name == name) return t.gameObject;
        return null;
    }
}
