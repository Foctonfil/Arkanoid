using UnityEngine;

public class PowerUp : MonoBehaviour
{
    public enum Type { BigPaddle, MultiBall, ExtraLife }

    public Sprite[] sprites;
    public float fallSpeed = 3f;
    public float lostY = -7f;

    Type type;

    public void Init(Type powerUpType)
    {
        type = powerUpType;
        GetComponent<SpriteRenderer>().sprite = sprites[(int)type];
        GetComponent<Rigidbody2D>().linearVelocity = Vector2.down * fallSpeed;
    }

    void Update()
    {
        if (transform.position.y < lostY) Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent(out Paddle _)) return;
        GameManager.Instance.ApplyPowerUp(type);
        Destroy(gameObject);
    }
}
