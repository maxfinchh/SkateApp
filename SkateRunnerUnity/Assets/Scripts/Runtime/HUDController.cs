using UnityEngine;
using UnityEngine.UI;

public sealed class HUDController : MonoBehaviour
{
    [SerializeField] private SkateRunnerGameManager gameManager;
    [SerializeField] private GestureInput gestureInput;
    [SerializeField] private PlayerController playerController;

    private Text scoreText;
    private Text currencyText;
    private Text missionText;
    private Text eventText;
    private Text securityText;
    private Text finalBonusText;
    private Text controlHintText;
    private Text startTitleText;
    private Text startPromptText;
    private Text resultTitleText;
    private Text resultMainText;
    private Text resultStatsText;
    private Text resultMissionText;
    private Text resultUpgradeText;
    private Text resultFooterText;
    private Image screenFlash;
    private Image finalPowerFill;
    private Image finalPowerGlow;
    private Image manualBalanceMarker;
    private GameObject scorePanel;
    private GameObject currencyPanel;
    private GameObject missionPanel;
    private GameObject eventPanel;
    private GameObject securityPanel;
    private GameObject finalBonusPanel;
    private GameObject manualBalancePanel;
    private GameObject controlHintPanel;
    private GameObject startPanel;
    private GameObject gameOverPanel;

    private void Awake()
    {
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<SkateRunnerGameManager>();
        }

        if (gestureInput == null)
        {
            gestureInput = FindFirstObjectByType<GestureInput>();
        }

        if (playerController == null)
        {
            playerController = FindFirstObjectByType<PlayerController>();
        }

        BuildHud();
    }

    private void Update()
    {
        if (gameManager == null)
        {
            return;
        }

        screenFlash.color = new Color(1f, 0.16f, 0.08f, gameManager.ScreenFlash * 0.34f);
        scoreText.text = $"SCORE {gameManager.Score}\nCOMBO x{gameManager.Combo}\nBEST {gameManager.BestScore}";
        currencyText.text = $"COINS {gameManager.Coins}\nGOLD {gameManager.Gold}";
        missionText.text = gameManager.MissionSummary;
        eventText.text = gameManager.EventMessage;
        eventText.enabled = !string.IsNullOrEmpty(gameManager.EventMessage);
        UpdatePulse(eventPanel, gameManager.EventPulse);

        float security = gameManager.SecurityPressure;
        securityText.enabled = security > 0f;
        securityText.text = $"SECURITY {Mathf.RoundToInt(security * 100f)}%";

        bool finalBonus = gameManager.State == RunState.FinalBonus;
        bool ready = gameManager.State == RunState.Ready;
        bool gameOver = gameManager.State == RunState.GameOver;
        bool showGameplayHud = !ready && !gameOver;
        scorePanel.SetActive(showGameplayHud);
        currencyPanel.SetActive(showGameplayHud);
        missionPanel.SetActive(showGameplayHud);
        eventPanel.SetActive(showGameplayHud && !string.IsNullOrEmpty(gameManager.EventMessage));
        securityPanel.SetActive(showGameplayHud && security > 0f);
        controlHintPanel.SetActive(showGameplayHud &&
                                   !finalBonus &&
                                   gestureInput != null &&
                                   (gestureInput.Scheme == ControlScheme.PushAndFlick || gestureInput.Scheme == ControlScheme.HoldDragSteer));

        if (gestureInput != null && gestureInput.Scheme == ControlScheme.PushAndFlick)
        {
            controlHintText.text = "HOLD TO PUSH / DRAG TO CARVE\nTRICK HEAD-ON: RAIL\nDOUBLE TAP: MANUAL";
        }
        else
        {
            controlHintText.text = "HOLD TO STEER\nFLICK UP TO OLLIE\nDIAGONAL FLICK + HOLD: GRIND";
        }

        finalBonusPanel.SetActive(finalBonus);
        if (finalBonus)
        {
            finalBonusText.text = $"MEGA RAMP\nDISTANCE {gameManager.FinalBonusDistanceMeters}m   AIR TRICKS x{gameManager.FinalBonusTrickCount}\nAIR POWER";
            finalPowerFill.fillAmount = gameManager.FinalBonusPower;
            finalPowerGlow.color = new Color(0.2f, 1f, 0.58f, 0.18f + gameManager.FinalBonusPulse * 0.72f);
            UpdatePulse(finalBonusPanel, gameManager.FinalBonusPulse * 1.45f);
        }

        bool showManualBalance = showGameplayHud && !finalBonus && playerController != null && playerController.IsManualing;
        manualBalancePanel.SetActive(showManualBalance);
        if (showManualBalance)
        {
            RectTransform markerRect = manualBalanceMarker.GetComponent<RectTransform>();
            markerRect.anchorMin = new Vector2(playerController.ManualBalance, 0f);
            markerRect.anchorMax = new Vector2(playerController.ManualBalance, 1f);
        }

        startPanel.SetActive(ready);
        if (ready)
        {
            startTitleText.text = "SKATE RUNNER";
            startPromptText.text = "CLICK TO PLAY\nHOLD TO PUSH";
        }

        gameOverPanel.SetActive(gameOver);
        if (gameOver)
        {
            resultTitleText.text = "RUN COMPLETE";
            resultMainText.text = $"FINAL LAUNCH {gameManager.FinalDistanceMeters}m";
            resultStatsText.text = $"SCORE {gameManager.Score}\nCOINS {gameManager.Coins}     GOLD {gameManager.Gold}\nAIR TRICKS x{gameManager.FinalBonusTrickCount}";
            resultMissionText.text = string.IsNullOrEmpty(gameManager.MissionSummary)
                ? "MISSIONS READY"
                : gameManager.MissionSummary;
            resultUpgradeText.text = $"UPGRADES   SPEED {gameManager.SpeedUpgradeCost}   POP {gameManager.PopUpgradeCost}\nTRICKS {gameManager.TrickUpgradeCost}   COINS {gameManager.CoinUpgradeCost}";
            resultFooterText.text = gameManager.RestartPrompt;
        }
    }

    private void BuildHud()
    {
        Canvas canvas = new GameObject("HUD Canvas").AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.transform.SetParent(transform);

        CanvasScaler scaler = canvas.gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(390f, 844f);
        scaler.matchWidthOrHeight = 0.5f;
        canvas.gameObject.AddComponent<GraphicRaycaster>();

        screenFlash = CreatePanel(canvas.transform, "Screen Flash", new Color(1f, 0.16f, 0.08f, 0f)).GetComponent<Image>();
        RectTransform flashRect = screenFlash.GetComponent<RectTransform>();
        flashRect.anchorMin = Vector2.zero;
        flashRect.anchorMax = Vector2.one;
        flashRect.offsetMin = Vector2.zero;
        flashRect.offsetMax = Vector2.zero;

        scoreText = CreateText(canvas.transform, "Score", new Vector2(18f, -18f), new Vector2(170f, 94f), TextAnchor.UpperLeft, 21, FontStyle.Bold);
        scorePanel = scoreText.transform.parent.gameObject;
        currencyText = CreateText(canvas.transform, "Currency", new Vector2(-18f, -18f), new Vector2(130f, 68f), TextAnchor.UpperRight, 21, FontStyle.Bold);
        currencyPanel = currencyText.transform.parent.gameObject;
        missionText = CreateText(canvas.transform, "Missions", new Vector2(16f, 16f), new Vector2(-16f, 56f), TextAnchor.MiddleCenter, 16, FontStyle.Bold);
        missionPanel = missionText.transform.parent.gameObject;
        eventText = CreateText(canvas.transform, "Event", new Vector2(0f, -118f), new Vector2(284f, 54f), TextAnchor.MiddleCenter, 24, FontStyle.Bold);
        eventPanel = eventText.transform.parent.gameObject;
        securityText = CreateText(canvas.transform, "Security", new Vector2(0f, -76f), new Vector2(220f, 40f), TextAnchor.MiddleCenter, 20, FontStyle.Bold);
        securityPanel = securityText.transform.parent.gameObject;

        controlHintPanel = CreatePanel(canvas.transform, "Control Hint Panel", new Color(0f, 0f, 0f, 0.46f));
        RectTransform controlHintRect = controlHintPanel.GetComponent<RectTransform>();
        controlHintRect.anchorMin = new Vector2(0.08f, 0f);
        controlHintRect.anchorMax = new Vector2(0.92f, 0f);
        controlHintRect.pivot = new Vector2(0.5f, 0f);
        controlHintRect.anchoredPosition = new Vector2(0f, 76f);
        controlHintRect.sizeDelta = new Vector2(0f, 58f);
        controlHintText = CreatePlainText(controlHintPanel.transform, "Control Hint", TextAnchor.MiddleCenter, 16, FontStyle.Bold);
        SetAnchors(controlHintText.gameObject, Vector2.zero, Vector2.one, new Vector2(10f, 4f), new Vector2(-10f, -4f));

        startPanel = CreatePanel(canvas.transform, "Start Panel", new Color(0.01f, 0.015f, 0.03f, 0.78f));
        RectTransform startRect = startPanel.GetComponent<RectTransform>();
        startRect.anchorMin = new Vector2(0.08f, 0.34f);
        startRect.anchorMax = new Vector2(0.92f, 0.64f);
        startRect.offsetMin = Vector2.zero;
        startRect.offsetMax = Vector2.zero;
        GameObject startHeader = CreatePanel(startPanel.transform, "Start Header", new Color(0.16f, 0.88f, 0.56f, 0.9f));
        SetAnchors(startHeader, new Vector2(0f, 0.7f), Vector2.one, Vector2.zero, Vector2.zero);
        startTitleText = CreatePlainText(startHeader.transform, "Start Title", TextAnchor.MiddleCenter, 30, FontStyle.Bold);
        SetAnchors(startTitleText.gameObject, Vector2.zero, Vector2.one, new Vector2(12f, 0f), new Vector2(-12f, 0f));
        startPromptText = CreatePlainText(startPanel.transform, "Start Prompt", TextAnchor.MiddleCenter, 25, FontStyle.Bold);
        SetAnchors(startPromptText.gameObject, new Vector2(0f, 0f), new Vector2(1f, 0.7f), new Vector2(14f, 8f), new Vector2(-14f, -8f));
        startPanel.SetActive(false);

        finalBonusPanel = CreatePanel(canvas.transform, "Final Bonus Panel", new Color(0.02f, 0.04f, 0.08f, 0.72f));
        RectTransform finalRect = finalBonusPanel.GetComponent<RectTransform>();
        finalRect.anchorMin = new Vector2(0.08f, 0.08f);
        finalRect.anchorMax = new Vector2(0.92f, 0.31f);
        finalRect.offsetMin = Vector2.zero;
        finalRect.offsetMax = Vector2.zero;

        finalBonusText = CreatePlainText(finalBonusPanel.transform, "Final Bonus Text", TextAnchor.MiddleCenter, 23, FontStyle.Bold);
        RectTransform finalTextRect = finalBonusText.GetComponent<RectTransform>();
        finalTextRect.anchorMin = new Vector2(0f, 0.36f);
        finalTextRect.anchorMax = Vector2.one;
        finalTextRect.offsetMin = new Vector2(12f, 0f);
        finalTextRect.offsetMax = new Vector2(-12f, -8f);

        GameObject meterBg = CreatePanel(finalBonusPanel.transform, "Final Power Meter", new Color(1f, 1f, 1f, 0.24f));
        RectTransform meterRect = meterBg.GetComponent<RectTransform>();
        meterRect.anchorMin = new Vector2(0.08f, 0.14f);
        meterRect.anchorMax = new Vector2(0.92f, 0.28f);
        meterRect.offsetMin = Vector2.zero;
        meterRect.offsetMax = Vector2.zero;

        finalPowerFill = CreatePanel(meterBg.transform, "Final Power Fill", new Color(0.16f, 1f, 0.52f, 0.88f)).GetComponent<Image>();
        finalPowerFill.type = Image.Type.Filled;
        finalPowerFill.fillMethod = Image.FillMethod.Horizontal;
        finalPowerFill.fillOrigin = (int)Image.OriginHorizontal.Left;
        RectTransform fillRect = finalPowerFill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        finalPowerGlow = CreatePanel(finalBonusPanel.transform, "Final Power Glow", new Color(0.2f, 1f, 0.58f, 0.22f)).GetComponent<Image>();
        RectTransform glowRect = finalPowerGlow.GetComponent<RectTransform>();
        glowRect.anchorMin = Vector2.zero;
        glowRect.anchorMax = Vector2.one;
        glowRect.offsetMin = Vector2.zero;
        glowRect.offsetMax = Vector2.zero;
        finalPowerGlow.transform.SetAsFirstSibling();
        finalBonusPanel.SetActive(false);

        manualBalancePanel = CreatePanel(canvas.transform, "Manual Balance Panel", new Color(0.02f, 0.04f, 0.08f, 0.72f));
        RectTransform manualRect = manualBalancePanel.GetComponent<RectTransform>();
        manualRect.anchorMin = new Vector2(0.16f, 0.32f);
        manualRect.anchorMax = new Vector2(0.84f, 0.39f);
        manualRect.offsetMin = Vector2.zero;
        manualRect.offsetMax = Vector2.zero;

        Text manualText = CreatePlainText(manualBalancePanel.transform, "Manual Balance Label", TextAnchor.MiddleCenter, 14, FontStyle.Bold);
        SetAnchors(manualText.gameObject, new Vector2(0f, 0.52f), Vector2.one, new Vector2(8f, 0f), new Vector2(-8f, 0f));
        manualText.text = "MANUAL BALANCE";

        GameObject manualTrack = CreatePanel(manualBalancePanel.transform, "Manual Balance Track", new Color(1f, 1f, 1f, 0.22f));
        SetAnchors(manualTrack, new Vector2(0.08f, 0.18f), new Vector2(0.92f, 0.46f), Vector2.zero, Vector2.zero);
        GameObject manualCenter = CreatePanel(manualTrack.transform, "Manual Balance Sweet Spot", new Color(0.16f, 1f, 0.52f, 0.55f));
        SetAnchors(manualCenter, new Vector2(0.38f, 0f), new Vector2(0.62f, 1f), Vector2.zero, Vector2.zero);
        manualBalanceMarker = CreatePanel(manualTrack.transform, "Manual Balance Marker", new Color(1f, 0.88f, 0.16f, 0.96f)).GetComponent<Image>();
        RectTransform manualMarkerRect = manualBalanceMarker.GetComponent<RectTransform>();
        manualMarkerRect.anchorMin = new Vector2(0.5f, 0f);
        manualMarkerRect.anchorMax = new Vector2(0.5f, 1f);
        manualMarkerRect.sizeDelta = new Vector2(10f, 0f);
        manualMarkerRect.anchoredPosition = Vector2.zero;
        manualBalancePanel.SetActive(false);

        gameOverPanel = CreatePanel(canvas.transform, "Results Panel", new Color(0.01f, 0.015f, 0.03f, 0.82f));
        RectTransform panelRect = gameOverPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.06f, 0.2f);
        panelRect.anchorMax = new Vector2(0.94f, 0.74f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        GameObject resultHeader = CreatePanel(gameOverPanel.transform, "Results Header", new Color(0.16f, 0.88f, 0.56f, 0.92f));
        SetAnchors(resultHeader, new Vector2(0f, 0.84f), Vector2.one, Vector2.zero, Vector2.zero);
        resultTitleText = CreatePlainText(resultHeader.transform, "Results Title", TextAnchor.MiddleCenter, 25, FontStyle.Bold);
        SetAnchors(resultTitleText.gameObject, Vector2.zero, Vector2.one, new Vector2(12f, 0f), new Vector2(-12f, 0f));

        resultMainText = CreatePlainText(gameOverPanel.transform, "Results Main Stat", TextAnchor.MiddleCenter, 30, FontStyle.Bold);
        SetAnchors(resultMainText.gameObject, new Vector2(0f, 0.68f), new Vector2(1f, 0.84f), new Vector2(14f, 0f), new Vector2(-14f, -2f));

        GameObject statPanel = CreatePanel(gameOverPanel.transform, "Results Stats Body", new Color(0f, 0f, 0f, 0.34f));
        SetAnchors(statPanel, new Vector2(0.05f, 0.39f), new Vector2(0.95f, 0.68f), Vector2.zero, Vector2.zero);
        resultStatsText = CreatePlainText(statPanel.transform, "Results Stats", TextAnchor.MiddleCenter, 21, FontStyle.Bold);
        SetAnchors(resultStatsText.gameObject, Vector2.zero, Vector2.one, new Vector2(12f, 8f), new Vector2(-12f, -8f));

        GameObject missionStrip = CreatePanel(gameOverPanel.transform, "Results Mission Strip", new Color(0.1f, 0.76f, 0.92f, 0.48f));
        SetAnchors(missionStrip, new Vector2(0.05f, 0.28f), new Vector2(0.95f, 0.38f), Vector2.zero, Vector2.zero);
        resultMissionText = CreatePlainText(missionStrip.transform, "Results Missions", TextAnchor.MiddleCenter, 15, FontStyle.Bold);
        SetAnchors(resultMissionText.gameObject, Vector2.zero, Vector2.one, new Vector2(10f, 2f), new Vector2(-10f, -2f));

        GameObject upgradeStrip = CreatePanel(gameOverPanel.transform, "Results Upgrade Strip", new Color(1f, 0.76f, 0.18f, 0.38f));
        SetAnchors(upgradeStrip, new Vector2(0.05f, 0.13f), new Vector2(0.95f, 0.27f), Vector2.zero, Vector2.zero);
        resultUpgradeText = CreatePlainText(upgradeStrip.transform, "Results Upgrades", TextAnchor.MiddleCenter, 16, FontStyle.Bold);
        SetAnchors(resultUpgradeText.gameObject, Vector2.zero, Vector2.one, new Vector2(10f, 4f), new Vector2(-10f, -4f));

        resultFooterText = CreatePlainText(gameOverPanel.transform, "Results Footer", TextAnchor.MiddleCenter, 21, FontStyle.Bold);
        SetAnchors(resultFooterText.gameObject, new Vector2(0f, 0.01f), new Vector2(1f, 0.12f), new Vector2(12f, 0f), new Vector2(-12f, -2f));
        gameOverPanel.SetActive(false);
        screenFlash.transform.SetAsLastSibling();
    }

    private static void UpdatePulse(GameObject panel, float pulse)
    {
        if (panel == null)
        {
            return;
        }

        float scale = 1f + Mathf.Clamp01(pulse) * 0.09f;
        panel.transform.localScale = Vector3.one * scale;
    }

    private static void SetAnchors(GameObject gameObject, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        RectTransform rect = gameObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
    }

    private static Text CreateText(Transform parent, string name, Vector2 anchoredPosition, Vector2 size, TextAnchor alignment, int fontSize, FontStyle style)
    {
        GameObject panel = CreatePanel(parent, $"{name} Panel", new Color(0f, 0f, 0f, 0.58f));
        RectTransform panelRect = panel.GetComponent<RectTransform>();

        if (name == "Score")
        {
            panelRect.anchorMin = new Vector2(0f, 1f);
            panelRect.anchorMax = new Vector2(0f, 1f);
            panelRect.pivot = new Vector2(0f, 1f);
        }
        else if (name == "Currency")
        {
            panelRect.anchorMin = new Vector2(1f, 1f);
            panelRect.anchorMax = new Vector2(1f, 1f);
            panelRect.pivot = new Vector2(1f, 1f);
        }
        else if (name == "Missions")
        {
            panelRect.anchorMin = new Vector2(0f, 0f);
            panelRect.anchorMax = new Vector2(1f, 0f);
            panelRect.pivot = new Vector2(0.5f, 0f);
        }
        else
        {
            panelRect.anchorMin = new Vector2(0.5f, 1f);
            panelRect.anchorMax = new Vector2(0.5f, 1f);
            panelRect.pivot = new Vector2(0.5f, 1f);
        }

        panelRect.anchoredPosition = anchoredPosition;
        panelRect.sizeDelta = size;

        Text text = new GameObject(name).AddComponent<Text>();
        text.transform.SetParent(panel.transform);
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.color = Color.white;
        text.alignment = alignment;
        text.raycastTarget = false;
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = 12;
        text.resizeTextMaxSize = fontSize;

        RectTransform rect = text.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(10f, 8f);
        rect.offsetMax = new Vector2(-10f, -8f);
        return text;
    }

    private static Text CreatePlainText(Transform parent, string name, TextAnchor alignment, int fontSize, FontStyle style)
    {
        Text text = new GameObject(name).AddComponent<Text>();
        text.transform.SetParent(parent);
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.color = Color.white;
        text.alignment = alignment;
        text.raycastTarget = false;
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = 12;
        text.resizeTextMaxSize = fontSize;
        return text;
    }

    private static GameObject CreatePanel(Transform parent, string name, Color color)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent);
        Image image = panel.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return panel;
    }
}
