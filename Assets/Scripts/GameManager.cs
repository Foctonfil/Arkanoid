using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public enum State { Menu, Serve, Playing, Paused, Over }

    public static GameManager Instance { get; private set; }

    public Paddle paddle;
    public Ball ballPrefab;
    public Brick brickPrefab;
    public PowerUp powerUpPrefab;
    public Transform board;
    public Text scoreText, levelText, livesText, messageText;
    public GameObject overlay;
    public AudioClip hitClip, brickClip, breakClip, loseClip, powerUpClip, winClip, overClip;
    public int startLives = 3;
    public float powerUpChance = 0.15f;
    public float bigPaddleSeconds = 10f;

    public State CurrentState { get; private set; }

    static readonly string[][] Levels =
    {
        new[]
        {
            "1111111111111",
            "1111111111111",
            "1111111111111",
            "1111111111111",
        },
        new[]
        {
            "2222222222222",
            "2111111111112",
            "2122222222212",
            "2111111111112",
            "2222222222222",
        },
        new[]
        {
            "3333333333333",
            "3222222222223",
            "3211111111123",
            "32.3333333.23",
            "3211111111123",
            "3222222222223",
        },
    };

    readonly List<Ball> balls = new List<Ball>();
    AudioSource audioSource;
    int score, lives, level, bricksLeft;

    void Awake()
    {
        Instance = this;
        audioSource = GetComponent<AudioSource>();
    }

    void Start() => ShowMenu();

    void Update()
    {
        var cam = Camera.main;
        cam.orthographicSize = Mathf.Max(6f, 8f / cam.aspect);

        var kb = Keyboard.current;
        if (kb == null) return;
        bool space = kb.spaceKey.wasPressedThisFrame;
        bool esc = kb.escapeKey.wasPressedThisFrame;

        if ((CurrentState == State.Menu || CurrentState == State.Over) && space) StartGame();
        else if (CurrentState == State.Serve && space) Launch();
        else if (CurrentState == State.Playing && esc) SetPaused(true);
        else if (CurrentState == State.Paused && esc) SetPaused(false);
    }

    void ShowMenu()
    {
        NewRun();
        CurrentState = State.Menu;
        Message("BRICK BREAKER\n\n<size=26>ESPAÇO para jogar\n\n← → ou A D para mover\nESC para pausar</size>", true);
    }

    void NewRun()
    {
        score = 0;
        lives = startLives;
        level = 0;
        ClearBallsAndPowerUps();
        paddle.ResetPaddle();
        LoadLevel();
        UpdateHud();
    }

    void StartGame()
    {
        if (CurrentState == State.Over) NewRun();
        Serve();
    }

    void LoadLevel()
    {
        foreach (Transform child in board) Destroy(child.gameObject);
        bricksLeft = 0;
        var rows = Levels[level];
        for (int r = 0; r < rows.Length; r++)
        {
            for (int c = 0; c < rows[r].Length; c++)
            {
                int hp = rows[r][c] - '0';
                if (hp < 1 || hp > 3) continue;
                var position = new Vector3((c - (rows[r].Length - 1) / 2f) * 1.05f, 4.3f - r * 0.55f);
                Instantiate(brickPrefab, position, Quaternion.identity, board).Init(hp);
                bricksLeft++;
            }
        }
    }

    void Serve()
    {
        ClearBallsAndPowerUps();
        paddle.ResetPaddle();
        balls.Add(Instantiate(ballPrefab));
        CurrentState = State.Serve;
        Message($"FASE {level + 1}\n\n<size=26>ESPAÇO para lançar</size>", false);
        UpdateHud();
    }

    void Launch()
    {
        balls[0].Launch(new Vector2(Random.Range(-0.5f, 0.5f), 1f));
        CurrentState = State.Playing;
        Message("", false);
    }

    void SetPaused(bool paused)
    {
        Time.timeScale = paused ? 0f : 1f;
        CurrentState = paused ? State.Paused : State.Playing;
        Message(paused ? "PAUSADO\n\n<size=26>ESC para continuar</size>" : "", paused);
    }

    void EndGame(bool won)
    {
        CurrentState = State.Over;
        ClearBallsAndPowerUps();
        Play(won ? winClip : overClip);
        Message($"{(won ? "VOCÊ VENCEU!" : "FIM DE JOGO")}\n\n<size=26>PONTOS: {score}\n\nESPAÇO para jogar novamente</size>", true);
        UpdateHud();
    }

    public void BrickDestroyed(Vector3 position, int points)
    {
        score += points;
        Play(breakClip);
        if (Random.value < powerUpChance)
            Instantiate(powerUpPrefab, position, Quaternion.identity).Init((PowerUp.Type)Random.Range(0, 3));
        UpdateHud();

        if (--bricksLeft > 0) return;
        level++;
        if (level >= Levels.Length) { EndGame(true); return; }
        Play(winClip);
        LoadLevel();
        Serve();
    }

    public void BallLost(Ball ball)
    {
        balls.Remove(ball);
        Destroy(ball.gameObject);
        if (balls.Count > 0 || CurrentState != State.Playing) return;
        lives--;
        Play(loseClip);
        if (lives > 0) Serve();
        else EndGame(false);
    }

    public void ApplyPowerUp(PowerUp.Type type)
    {
        Play(powerUpClip);
        if (type == PowerUp.Type.BigPaddle) paddle.Enlarge(bigPaddleSeconds);
        else if (type == PowerUp.Type.ExtraLife) lives++;
        else if (balls.Count > 0) SplitBall(balls[0]);
        UpdateHud();
    }

    void SplitBall(Ball source)
    {
        foreach (float angle in new[] { -30f, 30f })
        {
            var ball = Instantiate(ballPrefab, source.transform.position, Quaternion.identity);
            foreach (var other in balls) Physics2D.IgnoreCollision(ball.Collider, other.Collider);
            ball.Launch(Quaternion.Euler(0f, 0f, angle) * source.Direction);
            balls.Add(ball);
        }
    }

    void ClearBallsAndPowerUps()
    {
        foreach (var ball in balls) Destroy(ball.gameObject);
        balls.Clear();
        foreach (var powerUp in FindObjectsByType<PowerUp>()) Destroy(powerUp.gameObject);
    }

    void Message(string text, bool dim)
    {
        messageText.text = text;
        overlay.SetActive(dim);
    }

    void UpdateHud()
    {
        scoreText.text = $"PONTOS {score}";
        levelText.text = $"FASE {Mathf.Min(level + 1, Levels.Length)}";
        livesText.text = $"VIDAS {lives}";
    }

    public void Play(AudioClip clip) => audioSource.PlayOneShot(clip);
}
