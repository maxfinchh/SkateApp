using UnityEngine;

public enum RunState
{
    Running,
    FinalBonus,
    GameOver
}

public sealed class SkateRunnerGameManager : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    [SerializeField] private ObstacleSpawner spawner;
    [SerializeField] private float finalBonusSeconds = 2.4f;

    public RunState State { get; private set; } = RunState.Running;
    public int Coins { get; private set; }
    public int Score { get; private set; }
    public int Combo { get; private set; } = 1;

    private float runStartZ;
    private float finalBonusTimer;
    private bool restarting;

    private void Awake()
    {
        if (player == null)
        {
            player = FindFirstObjectByType<PlayerController>();
        }

        if (spawner == null)
        {
            spawner = FindFirstObjectByType<ObstacleSpawner>();
        }
    }

    private void Start()
    {
        StartRun();
    }

    private void Update()
    {
        if (State == RunState.Running && player != null)
        {
            Score = Mathf.Max(Score, Mathf.RoundToInt((player.transform.position.z - runStartZ) * 4f));
        }

        if (State == RunState.FinalBonus)
        {
            finalBonusTimer -= Time.deltaTime;
            Score += Mathf.RoundToInt(40f * Time.deltaTime * Combo);

            if (finalBonusTimer <= 0f)
            {
                State = RunState.GameOver;
                player.StopRun();
            }
        }

        if (State == RunState.GameOver && !restarting && Input.GetMouseButtonDown(0))
        {
            restarting = true;
            StartRun();
        }
    }

    public void StartRun()
    {
        restarting = false;
        State = RunState.Running;
        Coins = 0;
        Score = 0;
        Combo = 1;

        if (player != null)
        {
            player.ResetRun();
            runStartZ = player.transform.position.z;
        }

        if (spawner != null)
        {
            spawner.ResetRun();
        }
    }

    public void AddCoins(int amount)
    {
        Coins += amount;
        AddScore(amount * 25);
    }

    public void AddTrickScore(int basePoints)
    {
        AddScore(basePoints * Combo);
        Combo = Mathf.Clamp(Combo + 1, 1, 12);
    }

    public void AddGrindScore(float grindSeconds)
    {
        AddScore(Mathf.RoundToInt(grindSeconds * 80f * Combo));
    }

    public void AddScore(int amount)
    {
        Score += amount;
    }

    public void FailRun()
    {
        if (State != RunState.Running)
        {
            return;
        }

        State = RunState.FinalBonus;
        finalBonusTimer = finalBonusSeconds;
        AddScore(500 * Combo);
        player.StartFinalBonus();
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(24, 24, 320, 28), $"Score {Score}");
        GUI.Label(new Rect(24, 52, 320, 28), $"Coins {Coins}");
        GUI.Label(new Rect(24, 80, 320, 28), $"Combo x{Combo}");

        if (State == RunState.FinalBonus)
        {
            GUI.Label(new Rect(24, 118, 360, 32), "Final ramp bonus");
        }
        else if (State == RunState.GameOver)
        {
            GUI.Label(new Rect(24, 118, 520, 32), "Run complete - tap/click to restart");
        }
    }
}
