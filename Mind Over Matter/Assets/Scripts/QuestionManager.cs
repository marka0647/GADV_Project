using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class QuestionManager : MonoBehaviour
{
    [System.Serializable]
    public class Question
    {
        public string questionText;
        public string[] answers;
        public int correctIndex;
    }

    public Text questionText;
    public Button[] answerButtons;
    public List<Question> questions;

    private PlayerMovement playerMovement;
    private float questionDelay = 5f;
    private float timer;
    private float timeTakenToAnswer;

    private Question currentQuestion;

    void Start()
    {
        playerMovement = GameObject.FindWithTag("Player").GetComponent<PlayerMovement>();
        HideQuestion();
        InvokeRepeating("AskNewQuestion", 3f, questionDelay);
    }

    void AskNewQuestion()
    {
        if (questions.Count == 0) return;

        int index = Random.Range(0, questions.Count);
        currentQuestion = questions[index];

        questionText.text = currentQuestion.questionText;

        for (int i = 0; i < answerButtons.Length; i++)
        {
            int capturedIndex = i;
            answerButtons[i].GetComponentInChildren<Text>().text = currentQuestion.answers[i];
            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(() => CheckAnswer(capturedIndex));
        }

        ShowQuestion();
        timeTakenToAnswer = Time.time;
    }

    void CheckAnswer(int selectedIndex)
    {
        HideQuestion();
        float responseTime = Time.time - timeTakenToAnswer;

        if (selectedIndex == currentQuestion.correctIndex)
        {
            float bonus = Mathf.Clamp01((questionDelay - responseTime) / questionDelay);
            float speedBonus = 1f + bonus * 2f;
            playerMovement.IncreaseSpeed(speedBonus);
        }
        else
        {
            playerMovement.ResetSpeed();
        }
    }

    void ShowQuestion()
    {
        questionText.transform.parent.gameObject.SetActive(true);
    }

    void HideQuestion()
    {
        questionText.transform.parent.gameObject.SetActive(false);
    }
}