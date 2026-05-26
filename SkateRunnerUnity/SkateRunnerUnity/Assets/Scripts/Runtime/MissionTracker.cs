using UnityEngine;

public sealed class MissionTracker : MonoBehaviour
{
    private int coinsThisRun;
    private int tricksThisRun;
    private int grindsThisRun;
    private int featuresThisRun;
    private int finalLaunchMeters;

    public bool CoinMissionComplete => coinsThisRun >= 10;
    public bool TrickMissionComplete => tricksThisRun >= 5;
    public bool GrindMissionComplete => grindsThisRun >= 2;
    public bool FeatureMissionComplete => featuresThisRun >= 3;
    public bool FinalLaunchMissionComplete => finalLaunchMeters >= 20;

    public int CompletedCount =>
        (CoinMissionComplete ? 1 : 0) +
        (TrickMissionComplete ? 1 : 0) +
        (GrindMissionComplete ? 1 : 0) +
        (FeatureMissionComplete ? 1 : 0) +
        (FinalLaunchMissionComplete ? 1 : 0);

    public string Summary =>
        $"Coins {coinsThisRun}/10 | Tricks {tricksThisRun}/5 | Grinds {grindsThisRun}/2 | Spots {featuresThisRun}/3 | Launch {finalLaunchMeters}/20m";

    public void ResetRun()
    {
        coinsThisRun = 0;
        tricksThisRun = 0;
        grindsThisRun = 0;
        featuresThisRun = 0;
        finalLaunchMeters = 0;
    }

    public void RecordCoins(int amount)
    {
        coinsThisRun += amount;
    }

    public void RecordTrick(string trickName)
    {
        tricksThisRun++;
        if (trickName.Contains("Grind"))
        {
            grindsThisRun++;
        }
    }

    public void RecordFeature()
    {
        featuresThisRun++;
    }

    public void RecordFinalLaunch(int meters)
    {
        finalLaunchMeters = Mathf.Max(finalLaunchMeters, meters);
    }
}
