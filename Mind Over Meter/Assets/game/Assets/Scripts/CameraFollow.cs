using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;       // drag your PlayerAlt here
    [SerializeField] public float stopFollowAtX = 520f;  // camera stops following once target passes this X
    [SerializeField] private Vector3 offset = new Vector3(0f, 5f, -10f);
    [SerializeField] private float smoothTime = 0.15f;

    private float velX = 0f;
    private float initialY;
    private float initialZ;

    private void Start()
    {
        if (target == null)
        {
            var p = FindFirstObjectByType<PlayerAlt>();
            if (p != null) target = p.transform;
        }
        initialY = transform.position.y;
        initialZ = transform.position.z;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        float desiredX = target.position.x + offset.x;

        // Stop following once we hit the world-X cap
        float cappedX = Mathf.Min(desiredX, stopFollowAtX);

        float newX = Mathf.SmoothDamp(transform.position.x, cappedX, ref velX, smoothTime);
        transform.position = new Vector3(newX, initialY + offset.y, initialZ + offset.z);
    }
}
