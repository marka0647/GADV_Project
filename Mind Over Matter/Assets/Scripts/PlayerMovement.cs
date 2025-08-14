using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 0f;
    public float maxSpeed = 10f;
    private float baseSpeed = 2f;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        speed = baseSpeed;
    }

    void Update()
    {
        rb.linearVelocity = new Vector2(speed, rb.linearVelocity.y);
    }

    public void IncreaseSpeed(float amount)
    {
        speed = Mathf.Min(speed + amount, maxSpeed);
    }

    public void ResetSpeed()
    {
        speed = 0f;
    }

    public void ResumeBaseSpeed()
    {
        speed = baseSpeed;
    }
}