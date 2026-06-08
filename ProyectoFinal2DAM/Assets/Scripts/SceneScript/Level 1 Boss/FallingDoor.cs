using UnityEngine;

public class FallingDoor : MonoBehaviour
{
    public float targetY = -5.7f;
    public float fallSpeed = 10f;
    public bool isFalling = false;

    private Rigidbody2D rb;
    private Vector3 initialPosition;

    void Awake()
    {
        initialPosition = transform.position;
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.useFullKinematicContacts = true;
        }
    }

    public void ResetDoor()
    {
        isFalling = false;
        transform.position = initialPosition;
    }

    void OnEnable()
    {
        isFalling = true;
    }

    void Update()
    {
        if (isFalling)
        {
            Vector3 pos = transform.position;
            pos.y = Mathf.MoveTowards(pos.y, targetY, fallSpeed * Time.deltaTime);
            transform.position = pos;

            if (Mathf.Abs(pos.y - targetY) < 0.001f)
            {
                isFalling = false;
                // Once reached, we can keep it kinematic or switch to static/dynamic if needed.
                // Keeping it kinematic is fine for a door.
            }
        }
    }

    public void SetTargetY(float y)
    {
        targetY = y;
    }
}
