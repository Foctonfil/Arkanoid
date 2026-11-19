using UnityEngine;

public class Ball : MonoBehaviour
{
    public float speed = 8f;
    public float maxBounceAngle = 60f;
    public float minVertical = 0.25f;
    public float lostY = -6.5f;

    Rigidbody2D rb;
    bool launched;

    public Collider2D Collider { get; private set; }
    public Vector2 Direction => rb.linearVelocity.normalized;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        Collider = GetComponent<Collider2D>();
        rb.simulated = false;
    }

    void Update()
    {
        var gm = GameManager.Instance;
        if (!launched) transform.position = gm.paddle.transform.position + Vector3.up * 0.4f;
        else if (transform.position.y < lostY) gm.BallLost(this);
    }

    void FixedUpdate()
    {
        if (!launched) return;
        var v = rb.linearVelocity.normalized;
        if (Mathf.Abs(v.y) < minVertical) v = new Vector2(v.x, Mathf.Sign(v.y) * minVertical).normalized;
        rb.linearVelocity = v * speed;
    }

    public void Launch(Vector2 direction)
    {
        launched = true;
        rb.simulated = true;
        rb.linearVelocity = direction.normalized * speed;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        var gm = GameManager.Instance;
        if (collision.gameObject.TryGetComponent(out Paddle paddle) && transform.position.y > paddle.transform.position.y)
        {
            float offset = Mathf.Clamp((transform.position.x - paddle.transform.position.x) / paddle.HalfWidth, -1f, 1f);
            float angle = offset * maxBounceAngle * Mathf.Deg2Rad;
            rb.linearVelocity = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle)) * speed;
            gm.Play(gm.hitClip);
        }
        else if (collision.gameObject.TryGetComponent(out Brick brick)) brick.Hit();
        else gm.Play(gm.hitClip);
    }
}
