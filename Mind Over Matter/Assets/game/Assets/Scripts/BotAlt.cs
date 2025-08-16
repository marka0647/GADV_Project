using System.Collections.Generic;
using UnityEngine;
using TMPro; // only needed if you wire the optional popup text

[RequireComponent(typeof(CharacterController))]
public class BotAlt : MonoBehaviour
{
    // The ONLY public setting: chance the bot answers correctly (0..1)
    [Range(0f, 1f)] public float correctChance = 0.8f;

    private readonly Stack<float> speedBoosts = new Stack<float>();

    // Run & physics (private but editable in Inspector)
    [Header("Run Settings")]
    [SerializeField] private float baseRunSpeed = 5f;
    [SerializeField] private float maxRunSpeed = 15f;
    [SerializeField] private float gravity = 9.81f * 2f;

    // Question / segment rules (mirror player: every 50m)
    [Header("Segments")]
    [SerializeField] private float questionIntervalMeters = 50f;
    [SerializeField] private int totalSegmentsToSimulate = 10; // simulate N segments then stop

    // Distance-based boost curve (same as player)
    [Header("Speed Boost Curve")]
    [SerializeField] private float maxBoostAtEarly = 3f;  // boost at 0..earlyWindow meters
    [SerializeField] private float minBoostAtLate = 0.5f;// boost at >= lateClamp meters
    [SerializeField] private float earlyWindowMeters = 5f;  // 0..5m -> max
    [SerializeField] private float lateClampMeters = 40f; // >=40m -> min

    // Optional tiny popup above the bot (leave null for no UI)
    [Header("Optional Popup")]
    [SerializeField] private TMP_Text popupText;
    [SerializeField] private float popupSeconds = 0.6f;

    // Stacks are private to the bot
    [SerializeField] private int maxStacks = 3;
    private int botStacks = 0;

    // Internals
    private CharacterController controller;
    private Vector3 verticalVel;
    private float currentRunSpeed;
    private float lastX;

    private float botDistanceMeters = 0f;
    private int lastSegment = -1;
    private float segmentStartDist = 0f;
    private bool answeredThisSegment = false;
    private float targetAnswerMeters = 0f;
    private bool willBeCorrect = false;
    private int simulatedCount = 0;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        ApplySelectedDifficulty();            // <-- set chance from difficulty pick

        currentRunSpeed = baseRunSpeed;
        verticalVel = Vector3.zero;
        lastX = transform.position.x;

        lastSegment = Mathf.FloorToInt(botDistanceMeters / questionIntervalMeters);
        segmentStartDist = lastSegment * questionIntervalMeters;
        PrepareSegment();
    }


    private void Update()
    {
        // 1) Auto-run with gravity
        Vector3 run = Vector3.right * currentRunSpeed;
        verticalVel += gravity * Time.deltaTime * Vector3.down;
        if (controller.isGrounded) verticalVel = Vector3.down;
        controller.Move((run + verticalVel) * Time.deltaTime);

        // 2) Update bot "race" distance from actual movement along +X
        float dx = transform.position.x - lastX;
        if (dx > 0f) botDistanceMeters += dx;
        lastX = transform.position.x;

        // 3) Segment change check
        int seg = Mathf.FloorToInt(botDistanceMeters / questionIntervalMeters);
        if (seg != lastSegment)
        {
            // If the segment ended without an answer, count as wrong
            if (!answeredThisSegment)
                ApplyWrong();

            // Next segment
            lastSegment = seg;
            segmentStartDist = seg * questionIntervalMeters;
            PrepareSegment();
        }

        // 4) Attempt answer within the current segment
        if (simulatedCount <= totalSegmentsToSimulate && !answeredThisSegment)
        {
            float segMeters = Mathf.Clamp(botDistanceMeters - segmentStartDist, 0f, questionIntervalMeters);

            // When we reach the scheduled attempt distance, answer
            if (segMeters >= targetAnswerMeters)
            {
                if (willBeCorrect)
                    ApplyCorrect(segMeters);  // boost only if stack increases
                else
                    ApplyWrong();

                answeredThisSegment = true;    // lock until next 50m
            }
        }
    }

    public enum BotRole { Bot1, Bot2 }

    [Header("Difficulty Override")]
    [SerializeField] private bool useDifficultySettings = true;
    [SerializeField] private BotRole role = BotRole.Bot1;

    // --- add this method anywhere in the class ---
    public void ApplySelectedDifficulty()
    {
        if (!useDifficultySettings) return;
        // assumes you have the DifficultySettings static class from earlier
        var t = DifficultySettings.GetTuning();
        correctChance = (role == BotRole.Bot1) ? t.bot1CorrectChance : t.bot2CorrectChance;
    }

    private void PrepareSegment()
    {
        answeredThisSegment = false;

        // Done simulating?
        if (simulatedCount >= totalSegmentsToSimulate)
            return;

        // 50% by default, or whatever you set in correctChance
        willBeCorrect = Random.value < correctChance;

        // choose a point in [0..50) meters to attempt the answer
        targetAnswerMeters = Random.Range(0f, questionIntervalMeters);

        simulatedCount++;
    }

    private void ApplyCorrect(float segMeters)
    {
        if (botStacks < maxStacks)
        {
            botStacks++;

            float boost = ComputeDistanceBasedBoost(segMeters);
            float old = currentRunSpeed;
            float @new = Mathf.Clamp(currentRunSpeed + boost, baseRunSpeed, maxRunSpeed);

            float applied = @new - old;
            if (applied > 0f) speedBoosts.Push(applied);

            currentRunSpeed = @new;

            if (popupText != null)
            {
                StopAllCoroutines();
                StartCoroutine(FlashPopup("+" + applied.ToString("0.##")));
            }
        }
        else
        {
            // at max stacks: maintain speed (no extra boost)
        }

    }

    private void ApplyWrong()
    {
        if (botStacks > 0) botStacks--;
        if (speedBoosts.Count > 0)
        {
            float last = speedBoosts.Pop();
            currentRunSpeed = Mathf.Clamp(currentRunSpeed - last, baseRunSpeed, maxRunSpeed);
        }
    }

    private float ComputeDistanceBasedBoost(float segMeters)
    {
        segMeters = Mathf.Clamp(segMeters, 0f, questionIntervalMeters);

        if (segMeters <= earlyWindowMeters)
            return maxBoostAtEarly;

        if (segMeters < lateClampMeters)
        {
            float t = (segMeters - earlyWindowMeters) / Mathf.Max(0.0001f, (lateClampMeters - earlyWindowMeters));
            return Mathf.Lerp(maxBoostAtEarly, minBoostAtLate, t);
        }
        return minBoostAtLate;
    }

    private System.Collections.IEnumerator FlashPopup(string text)
    {
        popupText.gameObject.SetActive(true);
        popupText.text = text;

        float t = 0f;
        Color c = popupText.color;
        c.a = 1f;
        popupText.color = c;

        while (t < popupSeconds)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, t / popupSeconds);
            popupText.color = c;
            yield return null;
        }

        popupText.gameObject.SetActive(false);
    }
}

