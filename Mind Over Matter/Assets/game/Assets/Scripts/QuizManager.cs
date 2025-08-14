using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class QuizManager : MonoBehaviour
{
    [System.Serializable]
    public class Question
    {
        public string questionText;
        public string[] answers = new string[4];
        public int correctAnswerIndex;
    }

    public List<Question> questions = new List<Question>();

    public TextMeshProUGUI questionDisplay;
    public TextMeshProUGUI[] answerButtonsText;   // size 4
    public Button[] answerButtons;                // size 4

    public GameObject player;                     // (optional; used for VFX/SFX)
    public float defaultSpeed = 8f;
    public float speedIncrement = 1f;

    // --- NEW: shuffled order state ---
    private List<int> shuffledOrder;
    private int currentShuffledIndex = 0;

    void Start()
    {
        AssignButtonListeners();
        BuildShuffledOrder();     // create randomized order like [5,0,3,1,...]
        DisplayQuestion();
    }

    // Build a fresh randomized order of question indices
    void BuildShuffledOrder()
    {
        shuffledOrder = new List<int>(questions.Count);
        for (int i = 0; i < questions.Count; i++) shuffledOrder.Add(i);

        // Fisher–Yates shuffle
        for (int i = shuffledOrder.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (shuffledOrder[i], shuffledOrder[j]) = (shuffledOrder[j], shuffledOrder[i]);
        }
        currentShuffledIndex = 0;
    }

    void DisplayQuestion()
    {
        if (questions.Count == 0) return;

        // If we’ve shown all questions, reshuffle and start over
        if (currentShuffledIndex >= shuffledOrder.Count)
        {
            BuildShuffledOrder();
        }

        var q = questions[shuffledOrder[currentShuffledIndex]];

        if (questionDisplay) questionDisplay.text = q.questionText;

        // Keep answer order exactly as authored
        for (int i = 0; i < answerButtonsText.Length; i++)
        {
            if (answerButtonsText[i])
                answerButtonsText[i].text = q.answers[i];
        }
    }

    void AssignButtonListeners()
    {
        for (int i = 0; i < answerButtons.Length; i++)
        {
            int index = i; // capture
            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(() => AnswerQuestion(index));
        }
    }

    void AnswerQuestion(int selectedIndex)
    {
        if (questions.Count == 0) return;

        var q = questions[shuffledOrder[currentShuffledIndex]];

        if (selectedIndex == q.correctAnswerIndex)
        {
            GameManager.Instance.OnAnswerCorrect();
            // If you still want player feedback, call ONLY the correct one:
            // var p = player ? player.GetComponent<PlayerAlt>() : null;
            // if (p) p.CorrectAnswer();
        }
        else
        {
            GameManager.Instance.OnAnswerWrong();
            // var p = player ? player.GetComponent<PlayerAlt>() : null;
            // if (p) p.WrongAnswer();
        }

        currentShuffledIndex++;
        DisplayQuestion();
    }
}

