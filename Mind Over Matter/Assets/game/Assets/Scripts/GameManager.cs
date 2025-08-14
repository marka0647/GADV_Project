using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-1)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public float initialGameSpeed = 5f;
    public float gameSpeedIncrease = 0.1f;
    public float gameSpeed { get; private set; }

    //[SerializeField] private TextMeshProUGUI scoreText;
    //[SerializeField] private TextMeshProUGUI hiscoreText;
    //[SerializeField] private TextMeshProUGUI gameOverText;
    //[SerializeField] private Button retryButton;

    private PlayerAlt player;

    private float score;

    [SerializeField] private int maxSpeedStacks = 3;
    [SerializeField] private float speedPerStack = 2f;   // tweak in Inspector
    private int currentStacks = 0;

    // ---- Race / HUD data ----
    [SerializeField] private float targetDistanceMeters = 1000f;
    [SerializeField] private float distancePerSpeedUnit = 1f; // meters per (gameSpeed unit) per second

    public float DistanceMeters { get; private set; }
    public float ElapsedSeconds { get; private set; }

    // Expose read-only for HUD:
    public int CurrentStacks => currentStacks;         // you already have currentStacks private
    public int MaxSpeedStacks => maxSpeedStacks;       // idem
    public float CurrentSpeed => gameSpeed;            // read current game speed


    private void RecomputeSpeed()
    {
        // Base speed + stacks, never below default
        gameSpeed = initialGameSpeed + currentStacks * speedPerStack;
    }


    private void Awake()
    {
        if (Instance != null)
        {
            DestroyImmediate(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Start()
    {
        player = FindObjectOfType<PlayerAlt>();


        NewGame();
    }

    public void NewGame()
    {
        Obstacle[] obstacles = FindObjectsOfType<Obstacle>();

        foreach (var obstacle in obstacles)
        {
            Destroy(obstacle.gameObject);
        }

        score = 0f;
        currentStacks = 0;          // <-- reset stacks
        RecomputeSpeed();           // <-- sets gameSpeed from stacks
        enabled = true;

        DistanceMeters = 0f;
        ElapsedSeconds = 0f;

        currentStacks = 0;     // keep your stack reset here
        RecomputeSpeed();      // your helper that sets gameSpeed from stacks

        enabled = true;
        if (player != null) player.gameObject.SetActive(true);

        player.gameObject.SetActive(true);

    }

    public void GameOver()
    {
        gameSpeed = 0f;
        enabled = false;

        player.gameObject.SetActive(false);

    }

    private void Update()
    {

        //gameSpeed += gameSpeedIncrease * Time.deltaTime;
        ElapsedSeconds += Time.deltaTime;

        // Distance: correlate to speed
        if (DistanceMeters < targetDistanceMeters)
        {
            DistanceMeters += CurrentSpeed * (distancePerSpeedUnit * 0.8f) * Time.deltaTime;

            if (DistanceMeters >= targetDistanceMeters)
            {
                DistanceMeters = targetDistanceMeters;
                RaceComplete(); // define below (or call your existing finish/gameover)
            }
        }

    }
    private void RaceComplete()
    {
        // Stop advancing; you can call GameOver() or show finish UI here.
        enabled = false;
    }

    public void SetGameSpeed(float value)
    {
        gameSpeed = Mathf.Max(0f, value);
    }

    public void AddGameSpeed(float delta)
    {
        gameSpeed = Mathf.Max(0f, gameSpeed + delta);
    }
    public void AddSpeedOnCorrect(float amount)
    {
        gameSpeed = Mathf.Max(0f, gameSpeed + amount);
    }

    public void ResetSpeedToDefault()
    {
        gameSpeed = initialGameSpeed;
    }
    public void OnAnswerCorrect()
    {
        if (currentStacks < maxSpeedStacks)
            currentStacks++;
        RecomputeSpeed();           // at max: speed is maintained
    }

    public void OnAnswerWrong()
    {
        if (currentStacks > 0)
            currentStacks--;        // never below 0 stacks
        RecomputeSpeed();           // cannot go below initialGameSpeed
    }

    public float GetPlayerDistance()
    {
        return DistanceMeters;
    }
}

