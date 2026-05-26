using UnityEngine;

public enum RunState
{
    Ready,
    Running,
    FinalBonus,
    FinalBonusComplete,
    GameOver
}

public sealed class SkateRunnerGameManager : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    [SerializeField] private ObstacleSpawner spawner;
    [SerializeField] private MissionTracker missionTracker;
    [SerializeField] private PlayerUpgrades upgrades;
    [SerializeField] private ChaserPressure chaser;
    [SerializeField] private float finalBonusTimeoutSeconds = 30f;
    [SerializeField] private float finalRampDistance = 34f;
    [SerializeField] private float gameOverRestartDelay = 1.35f;
    [SerializeField] private bool useLegacyGui;

    private RoadSegmentLooper[] roadSegments;
    private FollowCamera[] followCameras;

    public RunState State { get; private set; } = RunState.Ready;
    public int Coins { get; private set; }
    public int Score { get; private set; }
    public int Combo { get; private set; } = 1;
    public int FinalDistanceMeters { get; private set; }
    public int BestScore { get; private set; }
    public int Gold { get; private set; }
    public PlayerUpgrades Upgrades => upgrades;
    public string MissionSummary => missionTracker != null ? missionTracker.Summary : string.Empty;
    public string EventMessage => eventTextTimer > 0f ? eventText : string.Empty;
    public float EventPulse => eventPulseTimer > 0f ? eventPulseTimer / eventPulseDuration : 0f;
    public float ScreenFlash => screenFlashTimer > 0f ? screenFlashTimer / screenFlashDuration : 0f;
    public float SecurityPressure => chaser != null ? chaser.Pressure : 0f;
    public int FinalBonusTrickCount => player != null ? player.FinalBonusTrickCount : 0;
    public int FinalBonusDistanceMeters => player != null ? Mathf.RoundToInt(player.CurrentFinalBonusDistance) : 0;
    public float FinalBonusPower => Mathf.Clamp01((player != null ? player.CurrentFinalBonusPower : 0f) + Mathf.Clamp01(finalBonusPulseTimer) * 0.55f);
    public float FinalBonusPulse => finalBonusPulseTimer;
    public bool CanRestart => State == RunState.GameOver && gameOverRestartTimer <= 0f;
    public string RestartPrompt => CanRestart ? "TAP TO RESTART" : "RESULTS LOCKED IN";
    public int SpeedUpgradeCost => upgrades != null ? upgrades.SpeedCost : 0;
    public int PopUpgradeCost => upgrades != null ? upgrades.JumpCost : 0;
    public int TrickUpgradeCost => upgrades != null ? upgrades.TrickCost : 0;
    public int CoinUpgradeCost => upgrades != null ? upgrades.CoinCost : 0;
    public string ResultSummary => $"RUN COMPLETE\nFINAL LAUNCH {FinalDistanceMeters}m  AIR x{FinalBonusTrickCount}\nCOINS {Coins}  GOLD {Gold}\n{MissionSummary}";
    public string UpgradeSummary => upgrades == null
        ? string.Empty
        : $"1 SPEED {upgrades.SpeedCost}  2 POP {upgrades.JumpCost}\n3 TRICKS {upgrades.TrickCost}  4 COINS {upgrades.CoinCost}";

    private float runStartZ;
    private float finalBonusTimer;
    private bool restarting;
    private string eventText;
    private float eventTextTimer;
    private float eventPulseTimer;
    private const float eventPulseDuration = 0.28f;
    private float screenFlashTimer;
    private const float screenFlashDuration = 0.22f;
    private float finalBonusPulseTimer;
    private float gameOverRestartTimer;

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

        if (missionTracker == null)
        {
            missionTracker = FindFirstObjectByType<MissionTracker>();
        }

        if (upgrades == null)
        {
            upgrades = FindFirstObjectByType<PlayerUpgrades>();
        }

        if (chaser == null)
        {
            chaser = FindFirstObjectByType<ChaserPressure>();
        }

        roadSegments = FindObjectsByType<RoadSegmentLooper>(FindObjectsSortMode.None);
        followCameras = FindObjectsByType<FollowCamera>(FindObjectsSortMode.None);
    }

    private void Start()
    {
        EnterReadyState();
    }

    private void Update()
    {
        bool startTouch = Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began;
        if (State == RunState.Ready && (Input.GetMouseButtonDown(0) || startTouch || Input.GetKeyDown(KeyCode.Space)))
        {
            StartRun();
        }

        if (State == RunState.Running && player != null)
        {
            Score = Mathf.Max(Score, Mathf.RoundToInt((player.transform.position.z - runStartZ) * 4f));
        }

        if (State == RunState.FinalBonus)
        {
            finalBonusTimer -= Time.deltaTime;

            if (finalBonusTimer <= 0f)
            {
                CompleteFinalBonus(player != null ? player.CurrentFinalBonusDistance : 0f);
            }
        }

        if (eventTextTimer > 0f)
        {
            eventTextTimer -= Time.deltaTime;
        }

        if (eventPulseTimer > 0f)
        {
            eventPulseTimer -= Time.deltaTime;
        }

        if (screenFlashTimer > 0f)
        {
            screenFlashTimer -= Time.deltaTime;
        }

        if (finalBonusPulseTimer > 0f)
        {
            finalBonusPulseTimer -= Time.deltaTime * 4.2f;
        }

        if (State == RunState.GameOver && gameOverRestartTimer > 0f)
        {
            gameOverRestartTimer -= Time.deltaTime;
        }

        if (CanRestart && !restarting && Input.GetMouseButtonDown(0))
        {
            restarting = true;
            StartRun();
        }

        if (State == RunState.GameOver)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                TryBuyUpgrade(1);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                TryBuyUpgrade(2);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                TryBuyUpgrade(3);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                TryBuyUpgrade(4);
            }
        }

        if (State == RunState.FinalBonus)
        {
            if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.E))
            {
                player.DebugFinalTrick(SwipeDirection.UpLeft);
            }
            else if (Input.GetKeyDown(KeyCode.R))
            {
                player.DebugFinalTrick(SwipeDirection.DownLeft);
            }
            else if (Input.GetKeyDown(KeyCode.F))
            {
                player.DebugFinalTrick(SwipeDirection.DownRight);
            }
        }
    }

    private void EnterReadyState()
    {
        restarting = false;
        State = RunState.Ready;
        Coins = 0;
        Score = 0;
        Combo = 1;
        FinalDistanceMeters = 0;
        finalBonusPulseTimer = 0f;
        screenFlashTimer = 0f;
        gameOverRestartTimer = 0f;
        eventText = string.Empty;
        eventTextTimer = 0f;
        missionTracker?.ResetRun();
        chaser?.ResetRun();
        ResetRoad();

        if (player != null)
        {
            player.ResetRun();
            player.StopRun();
            runStartZ = player.transform.position.z;
        }

        if (spawner != null)
        {
            spawner.ResetRun();
        }

        ResetCameras();
    }

    public void StartRun()
    {
        restarting = false;
        State = RunState.Running;
        Coins = 0;
        Score = 0;
        Combo = 1;
        FinalDistanceMeters = 0;
        finalBonusPulseTimer = 0f;
        screenFlashTimer = 0f;
        gameOverRestartTimer = 0f;
        eventText = "Hold to push";
        eventTextTimer = 2f;
        missionTracker?.ResetRun();
        chaser?.ResetRun();
        ResetRoad();

        if (player != null)
        {
            player.ResetRun();
            runStartZ = player.transform.position.z;
        }

        if (spawner != null)
        {
            spawner.ResetRun();
        }

        ResetCameras();
    }

    public void AddCoins(int amount)
    {
        int adjusted = amount + (upgrades != null ? upgrades.CoinBonus : 0);
        Coins += adjusted;
        missionTracker?.RecordCoins(adjusted);
        AddScore(adjusted * 25);
        ShowEvent($"+{adjusted} COIN");
    }

    public void AddTrickScore(int basePoints)
    {
        AddTrickScore("Trick", basePoints);
    }

    public void AddTrickScore(string trickName, int basePoints)
    {
        int adjustedPoints = Mathf.RoundToInt(basePoints * (upgrades != null ? upgrades.TrickMultiplier : 1f));
        AddScore(adjustedPoints * Combo);
        Combo = Mathf.Clamp(Combo + 1, 1, 12);
        missionTracker?.RecordTrick(trickName);
        ShowEvent($"{trickName} +{adjustedPoints}");
    }

    public void RecordFeature()
    {
        missionTracker?.RecordFeature();
    }

    public void AddGrindScore(float grindSeconds)
    {
        AddScore(Mathf.RoundToInt(grindSeconds * 80f * Combo));
    }

    public void AddScore(int amount)
    {
        Score += amount;
    }

    public void AddControlPenalty(string reason, int scorePenalty, int comboPenalty)
    {
        if (State != RunState.Running)
        {
            return;
        }

        Score = Mathf.Max(0, Score - Mathf.Max(0, scorePenalty));
        Combo = Mathf.Max(1, Combo - Mathf.Max(0, comboPenalty));
        screenFlashTimer = screenFlashDuration * 0.65f;
        ShowEvent(reason);
    }

    public void FailRun()
    {
        if (State != RunState.Running)
        {
            return;
        }

        State = RunState.FinalBonus;
        finalBonusTimer = finalBonusTimeoutSeconds;
        screenFlashTimer = screenFlashDuration;
        AddScore(500 * Combo);
        ShowEvent("Mega ramp: swipe for air time");

        float rampZ = player.transform.position.z + finalRampDistance;
        if (spawner != null)
        {
            spawner.StartFinalBonus(rampZ);
        }

        player.StartFinalBonus(rampZ);
    }

    public void AddSketchyPressure(string reason)
    {
        if (State != RunState.Running || chaser == null)
        {
            return;
        }

        if (chaser.AddSketchyAction())
        {
            screenFlashTimer = screenFlashDuration;
            ShowEvent("Caught by security");
            FailRun();
        }
        else
        {
            screenFlashTimer = screenFlashDuration;
            ShowEvent(reason);
        }
    }

    public void CompleteFinalBonus(float distance)
    {
        if (State != RunState.FinalBonus)
        {
            return;
        }

        FinalDistanceMeters = Mathf.Max(0, Mathf.RoundToInt(distance));
        missionTracker?.RecordFinalLaunch(FinalDistanceMeters);
        AddScore((500 + FinalDistanceMeters * 60) * Combo);
        BestScore = Mathf.Max(BestScore, Score);
        Gold += Mathf.Max(1, Coins + Mathf.RoundToInt(Score / 450f) + (missionTracker != null ? missionTracker.CompletedCount * 25 : 0));
        State = RunState.FinalBonusComplete;
        player.CenterForResults();
        player.StopRun();
        ResetCameras();
        ShowEvent($"+{FinalDistanceMeters}m launch");
        gameOverRestartTimer = gameOverRestartDelay;
        State = RunState.GameOver;
    }

    public void RecordFinalBonusTrickFeedback(string trickName)
    {
        finalBonusPulseTimer = 1f;
        ShowEvent($"{trickName} AIR +1");
    }

    private void TryBuyUpgrade(int slot)
    {
        if (upgrades == null)
        {
            return;
        }

        int cost = slot switch
        {
            1 => upgrades.SpeedCost,
            2 => upgrades.JumpCost,
            3 => upgrades.TrickCost,
            4 => upgrades.CoinCost,
            _ => 0
        };

        if (cost <= 0 || Gold < cost)
        {
            ShowEvent("Need more gold");
            return;
        }

        Gold -= cost;
        if (slot == 1)
        {
            upgrades.UpgradeSpeed();
            ShowEvent("Speed upgraded");
        }
        else if (slot == 2)
        {
            upgrades.UpgradeJump();
            ShowEvent("Pop upgraded");
        }
        else if (slot == 3)
        {
            upgrades.UpgradeTricks();
            ShowEvent("Tricks upgraded");
        }
        else if (slot == 4)
        {
            upgrades.UpgradeCoins();
            ShowEvent("Coins upgraded");
        }
    }

    private void OnGUI()
    {
        if (!useLegacyGui)
        {
            return;
        }

        GUI.Box(new Rect(16, 16, 190, 148), string.Empty);
        GUI.Label(new Rect(28, 26, 320, 28), $"Score {Score}");
        GUI.Label(new Rect(28, 52, 320, 28), $"Coins {Coins}");
        GUI.Label(new Rect(28, 78, 320, 28), $"Combo x{Combo}");
        GUI.Label(new Rect(28, 104, 320, 28), $"Best {BestScore}");
        GUI.Label(new Rect(28, 130, 320, 28), $"Gold {Gold}");

        if (chaser != null && chaser.Pressure > 0f)
        {
            GUI.Box(new Rect(Screen.width - 210f, 16f, 190f, 42f), $"Security {Mathf.RoundToInt(chaser.Pressure * 100f)}%");
        }

        if (missionTracker != null)
        {
            GUI.Box(new Rect(16f, Screen.height - 46f, Screen.width - 32f, 34f), missionTracker.Summary);
        }

        if (eventTextTimer > 0f)
        {
            GUI.Box(new Rect(Screen.width * 0.5f - 120f, 32f, 240f, 42f), eventText);
        }

        if (State == RunState.FinalBonus)
        {
            GUI.Box(new Rect(Screen.width * 0.5f - 150f, Screen.height * 0.5f - 32f, 300f, 64f), "Final ramp bonus");
        }
        else if (State == RunState.Ready)
        {
            GUI.Box(new Rect(Screen.width * 0.5f - 180f, Screen.height * 0.5f - 54f, 360f, 108f), "Skate Runner\nClick to play\nHold to push");
        }
        else if (State == RunState.GameOver)
        {
            string upgradeText = upgrades == null
                ? string.Empty
                : $"\n1 Speed {upgrades.SpeedCost}  2 Pop {upgrades.JumpCost}  3 Tricks {upgrades.TrickCost}  4 Coins {upgrades.CoinCost}";
            string restartText = CanRestart ? "Tap/click to restart" : "Results";
            GUI.Box(new Rect(Screen.width * 0.5f - 240f, Screen.height * 0.5f - 72f, 480f, 144f), $"Run complete\nFinal launch: {FinalDistanceMeters}m\nGold: {Gold}{upgradeText}\n{restartText}");
        }
    }

    private void ShowEvent(string message)
    {
        eventText = message;
        eventTextTimer = 0.8f;
        eventPulseTimer = eventPulseDuration;
    }

    public void ShowMessage(string message)
    {
        ShowEvent(message);
    }

    private void ResetRoad()
    {
        if (roadSegments == null || roadSegments.Length == 0)
        {
            roadSegments = FindObjectsByType<RoadSegmentLooper>(FindObjectsSortMode.None);
        }

        foreach (RoadSegmentLooper segment in roadSegments)
        {
            if (segment != null)
            {
                segment.ResetSegment();
            }
        }
    }

    private void ResetCameras()
    {
        if (followCameras == null || followCameras.Length == 0)
        {
            followCameras = FindObjectsByType<FollowCamera>(FindObjectsSortMode.None);
        }

        foreach (FollowCamera followCamera in followCameras)
        {
            if (followCamera != null)
            {
                followCamera.ResetCamera();
            }
        }
    }
}
