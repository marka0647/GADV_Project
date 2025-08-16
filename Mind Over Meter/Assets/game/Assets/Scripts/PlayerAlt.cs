using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerAlt : MonoBehaviour
{
    private CharacterController character;
    private Vector3 velocity;
    private readonly Stack<float> speedBoosts = new Stack<float>();

    [Header("Run Speed")]
    [SerializeField] private float baseRunSpeed = 5f;   // starting speed
    [SerializeField] private float maxRunSpeed = 15f;   // cap the total speed
    private float currentRunSpeed;

    [Header("Physics")]
    public float gravity = 9.81f * 2f;

    private void Awake()
    {
        character = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        velocity = Vector3.zero;
        currentRunSpeed = baseRunSpeed;   // start running immediately
    }

    private void Update()
    {
        // Horizontal auto-run
        Vector3 run = Vector3.right * currentRunSpeed;

        // Simple gravity so the controller stays grounded
        velocity += gravity * Time.deltaTime * Vector3.down;
        if (character.isGrounded) velocity = Vector3.down;

        // Move: run + gravity
        character.Move((run + velocity) * Time.deltaTime);
    }

    public void LoseOneStackSpeed()
    {
        if (speedBoosts.Count > 0)
        {
            float last = speedBoosts.Pop();               // undo the last applied boost
            currentRunSpeed = Mathf.Clamp(currentRunSpeed - last, baseRunSpeed, maxRunSpeed);
        }
    }

    // Called by QuizManager to make the player faster
    public void AddSpeed(float delta)
    {
        // Clamp so we never go below baseRunSpeed, and never above maxRunSpeed
        float oldSpeed = currentRunSpeed;
        float newSpeed = Mathf.Clamp(currentRunSpeed + delta, baseRunSpeed, maxRunSpeed);

        // Record only the actual positive increment applied (after clamping)
        float applied = newSpeed - oldSpeed;
        if (applied > 0f)
            speedBoosts.Push(applied);

        currentRunSpeed = newSpeed;
    }


    // Optional helper if you want to set speed directly somewhere
    public void SetSpeed(float newSpeed)
    {
        currentRunSpeed = Mathf.Clamp(newSpeed, 0f, maxRunSpeed);
    }
}
