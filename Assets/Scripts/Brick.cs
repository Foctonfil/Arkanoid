using UnityEngine;

public class Brick : MonoBehaviour
{
    public int pointsPerHitPoint = 50;

    static readonly Color[] Colors =
    {
        new Color(0.3f, 0.85f, 1f),
        new Color(1f, 0.85f, 0.2f),
        new Color(1f, 0.35f, 0.35f),
    };

    SpriteRenderer sr;
    int hp, maxHp;

    public void Init(int hitPoints)
    {
        sr = GetComponent<SpriteRenderer>();
        hp = maxHp = hitPoints;
        sr.color = Colors[hp - 1];
    }

    public void Hit()
    {
        if (hp <= 0) return;
        var gm = GameManager.Instance;
        if (--hp > 0)
        {
            sr.color = Colors[hp - 1];
            gm.Play(gm.brickClip);
            return;
        }
        Destroy(gameObject);
        gm.BrickDestroyed(transform.position, maxHp * pointsPerHitPoint);
    }
}
