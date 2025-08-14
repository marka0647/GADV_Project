using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerAlt : MonoBehaviour
{
    private CharacterController character;
    private Vector3 direction;

    public float defaultSpeed = 5f;
    public float currentSpeed;
    public float speedIncrement = 2f;

    public float gravity = 9.81f * 2f;

    private int correctAnswerStreak = 0;

    private void Awake()
    {
        character = GetComponent<CharacterController>();
        currentSpeed = defaultSpeed;
    }

    private void OnEnable()
    {
        direction = Vector3.zero;
        currentSpeed = defaultSpeed;
        correctAnswerStreak = 0;
    }

    private void Update()
    {
        direction += gravity * Time.deltaTime * Vector3.down;

        if (character.isGrounded)
        {
            direction = Vector3.down;

        }

        direction.x = currentSpeed;
        character.Move(direction * Time.deltaTime);
    }

    // Call this method from QuizManager when the answer is correct
    public void CorrectAnswer()
    {
        correctAnswerStreak++;
        if (correctAnswerStreak <= 3)
        {
            currentSpeed += speedIncrement;
        }
    }

    // Call this method from QuizManager when the answer is wrong
    public void WrongAnswer()
    {
        correctAnswerStreak = 0;
        currentSpeed = defaultSpeed;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle"))
        {
            GameManager.Instance.GameOver();
        }
    }
}
