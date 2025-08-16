using UnityEngine;

[DefaultExecutionOrder(-1)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Background Scroll")]
    [SerializeField] private float initialGameSpeed = 5f;
    public float gameSpeed { get; private set; } // Ground reads this

    [Header("Player")]
    [SerializeField] private PlayerAlt player;

    [Header("Stacks")]
    [SerializeField] private int maxSpeedStacks = 3;
    private int currentStacks = 0;
    public int CurrentStacks => currentStacks;
    public int MaxSpeedStacks => maxSpeedStacks;

    [Header("Race Distance / Timing")]
    [SerializeField] private float targetDistanceMeters = 500f;          // finish line
    [SerializeField] private float metersPerUnit = 1f;

    // internal tracker
    private float lastPlayerX = 0f;

    // Expose distance (for HUD)
    public float DistanceMeters { get; private set; }
    public float EffectiveDistanceMeters => DistanceMeters; // compatibility, if HUD uses this

    // Exposed for HUD
    public float ElapsedSeconds { get; private set; }
    public float CurrentSpeed => gameSpeed;

    private void Awake()
    {
        if (Instance != null) { DestroyImmediate(gameObject); return; }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Start()
    {
        if (player == null) player = FindFirstObjectByType<PlayerAlt>();
        if (player == null)
        {
            Debug.LogError("GameManager: No PlayerAlt found in the scene. Please assign a PlayerAlt instance.");
            enabled = false; // disable manager if no player
            return;
        }
        NewGame();
    }

    public void NewGame()
    {

        currentStacks = 0;
        RecomputeSpeed(); // fixed scroll speed

        ElapsedSeconds = 0f;
        DistanceMeters = 0f;
        if (player != null)
            lastPlayerX = player.transform.position.x;

        enabled = true;
        if (player) player.gameObject.SetActive(true);
    }

    public void GameOver()
    {
        gameSpeed = 0f; // stop scrolling on game over
        enabled = false;
        if (player) player.gameObject.SetActive(false);
    }

    private void Update()
    {
        // No auto speed ramp. gameSpeed stays constant.
        ElapsedSeconds += Time.deltaTime;

        if (player != null)
        {
            float currentX = player.transform.position.x;
            float dx = currentX - lastPlayerX;

            // Count only forward progress (ignore backward nudges)
            if (dx > 0f)
                DistanceMeters += dx * metersPerUnit;

            lastPlayerX = currentX;
        }

        // Finish check (if you have a target distance)
        if (DistanceMeters >= targetDistanceMeters)
        {
            DistanceMeters = targetDistanceMeters;
            // your finish logic (e.g., stop, show results, etc.)
        }
    }

    public bool OnAnswerCorrect()
    {
        if (currentStacks < maxSpeedStacks)
        {
            currentStacks++;
            return true;   // stack went up -> do your +1.5 move
        }
        return false;      // already at max -> no move

    }

    public bool OnAnswerWrong()
    {
        if (currentStacks > 0)
        {
            currentStacks--;
            if (player != null) player.LoseOneStackSpeed();  // <- undo one boost
            return true;                                     // stack went down
        }
        return false;
    }

    private void RecomputeSpeed()
    {
        // Fixed ground scroll for the entire game.
        gameSpeed = initialGameSpeed;
    }
}



