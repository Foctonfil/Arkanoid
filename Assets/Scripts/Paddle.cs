using UnityEngine;
using UnityEngine.InputSystem;

public class Paddle : MonoBehaviour
{
    public float speed = 12f;
    public float normalWidth = 2f;
    public float bigWidth = 3f;
    public float wallX = 7.5f;

    Rigidbody2D rb;
    SpriteRenderer sr;
    BoxCollider2D box;
    Vector2 startPosition;
    float bigUntil;

    public float HalfWidth => sr.size.x / 2f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        box = GetComponent<BoxCollider2D>();
        startPosition = transform.position;
    }

    void FixedUpdate()
    {
        float width = Time.time < bigUntil ? bigWidth : normalWidth;
        sr.size = new Vector2(width, sr.size.y);
        box.size = sr.size;

        var state = GameManager.Instance.CurrentState;
        var kb = Keyboard.current;
        if (kb == null || (state != GameManager.State.Serve && state != GameManager.State.Playing)) return;

        float input = (kb.rightArrowKey.isPressed || kb.dKey.isPressed ? 1f : 0f) - (kb.leftArrowKey.isPressed || kb.aKey.isPressed ? 1f : 0f);
        float limit = wallX - width / 2f;
        float x = Mathf.Clamp(rb.position.x + input * speed * Time.fixedDeltaTime, -limit, limit);
        rb.MovePosition(new Vector2(x, startPosition.y));
    }

    public void Enlarge(float seconds) => bigUntil = Time.time + seconds;

    public void ResetPaddle()
    {
        bigUntil = 0f;
        rb.position = startPosition;
        transform.position = startPosition;
    }
}
