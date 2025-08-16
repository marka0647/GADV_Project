using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class RaceManager : MonoBehaviour

{
    [Header("Who to track")]
    [SerializeField] private Transform player;   // PlayerAlt transform
    [SerializeField] private Transform bot1;     // BotAlt transform
    [SerializeField] private Transform bot2;     // BotAlt transform

    [Header("UI (live only)")]
    [SerializeField] private TMP_Text liveText;  // live 1st/2nd/3rd during race

    [Header("Distance settings")]
    [SerializeField] private float metersPerUnit = 1f;
    [SerializeField] private bool useGameManagerForPlayerDistance = true;
    [SerializeField] private float tieEpsilonMeters = 0.5f;

    [Header("Finish rules")]
    [SerializeField] private float targetDistanceMeters = 500f;
    [SerializeField] private string endScreenSceneName = "EndScreen"; // name of your end scene

    [Header("Names (display only)")]
    [SerializeField] private string playerName = "Player";
    [SerializeField] private string bot1Name = "Bot 1";
    [SerializeField] private string bot2Name = "Bot 2";

    // internals
    private float playerStartX, bot1StartX, bot2StartX;
    private bool playerFinished = false, bot1Finished = false, bot2Finished = false;
    private float playerFinishTime = -1f, bot1FinishTime = -1f, bot2FinishTime = -1f;
    private float timeAtStart = 0f;
    private bool endTriggered = false;

    void Start()
    {
        if (!player)
        {
            var p = FindFirstObjectByType<PlayerAlt>();
            if (p) player = p.transform;
        }
        if (!bot1 || !bot2)
        {
            // Better to assign in Inspector, but try to auto-find one BotAlt if present.
            // Duplicate and assign for two bots if needed.
            var bots = FindFirstObjectByType<MonoBehaviour>();
        }

        if (!player || !bot1 || !bot2)
        {
            Debug.LogWarning("RaceRankingLive3: assign Player, Bot 1, and Bot 2 transforms.");
            enabled = false;
            return;
        }

        playerStartX = player.position.x;
        bot1StartX = bot1.position.x;
        bot2StartX = bot2.position.x;

        if (liveText) liveText.text = "";

        timeAtStart = Time.time;
    }

    void Update()
    {
        var gm = GameManager.Instance;
        float now = gm ? gm.ElapsedSeconds : (Time.time - timeAtStart);

        // Distances
        float playerMeters = GetPlayerMeters(gm);
        float bot1Meters = Mathf.Max(0f, (bot1.position.x - bot1StartX) * metersPerUnit);
        float bot2Meters = Mathf.Max(0f, (bot2.position.x - bot2StartX) * metersPerUnit);

        // Live 1/2/3
        if (!endTriggered && liveText)
            liveText.text = BuildLiveBoard(playerMeters, bot1Meters, bot2Meters);

        // Finish checks
        if (!playerFinished && playerMeters >= targetDistanceMeters) { playerFinished = true; playerFinishTime = now; }
        if (!bot1Finished && bot1Meters >= targetDistanceMeters) { bot1Finished = true; bot1FinishTime = now; }
        if (!bot2Finished && bot2Meters >= targetDistanceMeters) { bot2Finished = true; bot2FinishTime = now; }

        // When all three finish, store results and load end screen
        if (!endTriggered && playerFinished && bot1Finished && bot2Finished)
        {
            endTriggered = true;

            var order = new List<RaceResultStore.Entry>
            {
                new RaceResultStore.Entry { name = playerName, timeSeconds = playerFinishTime },
                new RaceResultStore.Entry { name = bot1Name,   timeSeconds = bot1FinishTime   },
                new RaceResultStore.Entry { name = bot2Name,   timeSeconds = bot2FinishTime   },
            };
            order.Sort((a, b) => a.timeSeconds.CompareTo(b.timeSeconds));

            RaceResultStore.SetResults(order, targetDistanceMeters);

            // Load your end scene
            SceneManager.LoadScene(endScreenSceneName);
        }
    }

    private float GetPlayerMeters(GameManager gm)
    {
        if (useGameManagerForPlayerDistance && gm != null)
            return gm.DistanceMeters;
        return Mathf.Max(0f, (player.position.x - playerStartX) * metersPerUnit);
    }

    private string BuildLiveBoard(float p, float b1, float b2)
    {
        var live = new List<(string name, float m)>
        {
            (playerName, p), (bot1Name, b1), (bot2Name, b2)
        };
        live.Sort((a, b) => b.m.CompareTo(a.m));

        string l1 = $"1st: {live[0].name}";
        string l2 = $"2nd: {live[1].name}";
        string l3 = $"3rd: {live[2].name}";

        // Tie hints
        if (Mathf.Abs(live[0].m - live[1].m) <= tieEpsilonMeters)
            l2 = $"Tied 1st: {live[0].name} & {live[1].name}";
        if (Mathf.Abs(live[1].m - live[2].m) <= tieEpsilonMeters)
            l3 = $"Tied 2nd: {live[1].name} & {live[2].name}";

        return l1 + "\n" + l2 + "\n" + l3;
    }
}



