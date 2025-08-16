using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Text;
using UnityEngine.EventSystems;

public class EndScreenUI : MonoBehaviour
{
    [Header("UI (assign in Inspector)")]
    [SerializeField] private TMP_Text resultsText;
    [SerializeField] private Button startScreenButton;

    [Header("Scene")]
    [SerializeField] private string startScreenSceneName = "start screen";

    void Awake()
    {
        // reset possible leftovers from race
        Time.timeScale = 1f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        EnsureFreshEventSystem();
        EnsureGraphicRaycaster();
        DisableInvisibleBlockers();

        if (!startScreenButton)
        {
            Debug.LogError("EndScreenUI: Start Screen Button is not assigned.");
        }
        else
        {
            // make sure button can receive raycasts and shows tint
            if (startScreenButton.targetGraphic == null)
            {
                var img = startScreenButton.GetComponent<Image>();
                if (!img) img = startScreenButton.gameObject.AddComponent<Image>();
                img.raycastTarget = true;
                startScreenButton.targetGraphic = img;
            }
            startScreenButton.navigation = new Navigation { mode = Navigation.Mode.None };
            startScreenButton.onClick.RemoveAllListeners();
            startScreenButton.onClick.AddListener(GoToStart);
            startScreenButton.interactable = true;
        }
    }

    void Start()
    {
        if (!resultsText) return;

        var order = RaceResultStore.FinalOrder;
        if (order == null || order.Count == 0)
        {
            resultsText.text = "No results.";
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine("Final Results:");
        for (int i = 0; i < order.Count; i++)
        {
            string place = (i == 0) ? "1st" : (i == 1) ? "2nd" : (i == 2) ? "3rd" : (i + 1) + "th";
            sb.AppendLine(place + ": " + order[i].name + " - " + order[i].timeSeconds.ToString("0.00") + "s");
        }
        resultsText.text = sb.ToString();
    }

    private void GoToStart()
    {
        RaceResultStore.Clear();
        Time.timeScale = 1f;
        SceneManager.LoadScene(startScreenSceneName, LoadSceneMode.Single);
    }

    // helpers

    private void EnsureFreshEventSystem()
    {
        // remove any DontDestroyOnLoad EventSystems brought from earlier scenes
        var existing = Object.FindObjectsOfType<EventSystem>();
        for (int i = 0; i < existing.Length; i++)
        {
            Destroy(existing[i].gameObject);
        }

        var go = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
        go.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
        go.AddComponent<StandaloneInputModule>();
#endif
        // important: do NOT DontDestroyOnLoad here; we want a clean one per scene
    }

    private void EnsureGraphicRaycaster()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (!canvas)
        {
            // create a basic overlay canvas if scene is missing one
            var go = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }
        if (!canvas.GetComponent<GraphicRaycaster>())
        {
            canvas.gameObject.AddComponent<GraphicRaycaster>();
        }
    }

    private void DisableInvisibleBlockers()
    {
        // turn off raycast blocking on fully hidden overlays that might have been left active
        var groups = FindObjectsOfType<CanvasGroup>(true);
        foreach (var g in groups)
        {
            if (g.alpha <= 0.001f && g.blocksRaycasts)
                g.blocksRaycasts = false;
        }
    }
}


