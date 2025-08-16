using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class QuizManager : MonoBehaviour
{
    [System.Serializable]
    public class Question
    {
        [TextArea] public string questionText;
        public string[] answers = new string[4];   // fixed order; correctAnswerIndex indexes this
        [Range(0, 3)] public int correctAnswerIndex;
    }

    [Header("Data")]
    public List<Question> questions = new List<Question>();

    [Header("UI")]
    public TMP_Text questionDisplay;
    public Button[] answerButtons;          // size 4
    public TMP_Text[] answerButtonsText;    // size 4 (same order as buttons)

    [Header("Distance gating")]
    [SerializeField] private float questionIntervalMeters = 50f; // new question every 50 m
    [SerializeField] private int totalQuestionsToAsk = 10;        // ask first N from the shuffled list

    [Header("Time limit")]
    [SerializeField] private float questionTimeLimit = 10f;       // timeout counts as wrong

    [Header("Speed boost (distance-based, same as player settings)")]
    [SerializeField] private PlayerAlt player;      // auto-found if left empty
    [SerializeField] private float maxBoostAtEarly = 3f;   // at 0..earlyWindow meters
    [SerializeField] private float minBoostAtLate = 0.5f; // at >= lateClamp meters
    [SerializeField] private float earlyWindowMeters = 5f;   // 0..5 m -> max boost
    [SerializeField] private float lateClampMeters = 40f;  // >=40 m -> min boost

    // Internal state
    private List<int> shuffledOrder;
    private int nextQuestionPtr = 0;       // index into shuffledOrder
    private int askedCount = 0;            // how many shown so far
    private int activeQuestionDataIndex = -1;
    private bool questionLocked = false;   // after answer/timeout, wait until next 50 m
    private int lastSegment = -1;          // floor(Distance/interval) we last handled
    private float segmentStartMeters = 0f;
    private float questionStartTime = 0f;
    private Coroutine timerCo;

    void Start()
    {
        if (player == null) player = FindFirstObjectByType<PlayerAlt>();
        AssignButtonListeners();
        BuildShuffledOrder();

        var gm = GameManager.Instance;
        float d = gm != null ? gm.DistanceMeters : 0f;
        lastSegment = Mathf.FloorToInt(d / questionIntervalMeters);
        segmentStartMeters = lastSegment * questionIntervalMeters;

        ShowNextQuestion(); // first question at start
    }

    void OnDisable()
    {
        if (timerCo != null) StopCoroutine(timerCo);
        timerCo = null;
    }

    void Update()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        int seg = Mathf.FloorToInt(gm.DistanceMeters / questionIntervalMeters);
        if (seg != lastSegment)
        {
            // If the 50 m window ended without an answer -> count as wrong, lock this panel
            if (!questionLocked)
            {
                if (timerCo != null) { StopCoroutine(timerCo); timerCo = null; }
                HandleWrong();
                questionLocked = true;
                SetButtonsInteractable(false);
            }

            // Move into next segment and spawn the next question
            lastSegment = seg;
            segmentStartMeters = seg * questionIntervalMeters;
            ShowNextQuestion();
        }
    }

    private void AssignButtonListeners()
    {
        for (int i = 0; i < answerButtons.Length; i++)
        {
            int idx = i;
            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(() => OnAnswerPressed(idx));
        }
    }

    private void BuildShuffledOrder()
    {
        int n = questions.Count;
        shuffledOrder = new List<int>(n);
        for (int i = 0; i < n; i++) shuffledOrder.Add(i);

        // Fisher–Yates
        for (int i = n - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int tmp = shuffledOrder[i];
            shuffledOrder[i] = shuffledOrder[j];
            shuffledOrder[j] = tmp;
        }
        nextQuestionPtr = 0;
        askedCount = 0;
    }

    private void ShowNextQuestion()
    {
        int maxToAsk = Mathf.Min(totalQuestionsToAsk, questions.Count);
        if (askedCount >= maxToAsk)
        {
            // done asking; keep locked
            questionLocked = true;
            SetButtonsInteractable(false);
            return;
        }

        if (nextQuestionPtr >= shuffledOrder.Count)
        {
            // No more questions available; keep locked
            questionLocked = true;
            SetButtonsInteractable(false);
            return;
        }

        activeQuestionDataIndex = shuffledOrder[nextQuestionPtr++];
        askedCount++;

        var q = questions[activeQuestionDataIndex];

        if (questionDisplay) questionDisplay.text = q.questionText;
        for (int i = 0; i < 4; i++)
            if (answerButtonsText[i]) answerButtonsText[i].text = q.answers[i];

        questionLocked = false;
        SetButtonsInteractable(true);

        // start timeout timer
        if (timerCo != null) StopCoroutine(timerCo);
        questionStartTime = Time.time;
        timerCo = StartCoroutine(QuestionTimer());
    }

    private IEnumerator QuestionTimer()
    {
        float endTime = questionStartTime + questionTimeLimit;
        while (Time.time < endTime && !questionLocked)
            yield return null;

        if (!questionLocked)
        {
            HandleWrong();                 // timeout counts as wrong
            questionLocked = true;
            SetButtonsInteractable(false);
        }
        timerCo = null;
    }

    private void OnAnswerPressed(int pressedIndex)
    {
        if (questionLocked || activeQuestionDataIndex < 0) return;

        var q = questions[activeQuestionDataIndex];

        questionLocked = true;
        SetButtonsInteractable(false);
        if (timerCo != null) { StopCoroutine(timerCo); timerCo = null; }

        if (pressedIndex == q.correctAnswerIndex)
        {
            // Raise stack; returns true only if stack actually increased (not already at 3)
            bool raised = GameManager.Instance.OnAnswerCorrect();

            // Distance-based boost; apply only if stack increased (maintain speed at 3)
            if (raised && player != null)
            {
                float boost = ComputeDistanceBasedBoost();
                player.AddSpeed(boost);
            }
        }
        else
        {
            HandleWrong();
        }
        // Do not advance here; we wait for the next 50 m boundary.
    }

    private void HandleWrong()
    {
        GameManager.Instance.OnAnswerWrong();
        // No speed penalty by default; add one here if you want: if (player) player.AddSpeed(-penalty);
    }

    private float ComputeDistanceBasedBoost()
    {
        var gm = GameManager.Instance;
        float d = gm != null ? gm.DistanceMeters : 0f;

        // meters progressed within this 50 m segment
        float segMeters = Mathf.Clamp(d - segmentStartMeters, 0f, questionIntervalMeters);

        if (segMeters <= earlyWindowMeters)
            return maxBoostAtEarly;

        if (segMeters < lateClampMeters)
        {
            float t = (segMeters - earlyWindowMeters) / Mathf.Max(0.0001f, (lateClampMeters - earlyWindowMeters));
            return Mathf.Lerp(maxBoostAtEarly, minBoostAtLate, t);
        }
        return minBoostAtLate;
    }

    private void SetButtonsInteractable(bool value)
    {
        if (answerButtons == null) return;
        for (int i = 0; i < answerButtons.Length; i++)
            if (answerButtons[i]) answerButtons[i].interactable = value;
    }
}



