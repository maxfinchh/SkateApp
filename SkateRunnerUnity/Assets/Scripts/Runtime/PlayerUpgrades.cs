using UnityEngine;

public sealed class PlayerUpgrades : MonoBehaviour
{
    [SerializeField] private int speedLevel;
    [SerializeField] private int jumpLevel;
    [SerializeField] private int trickLevel;
    [SerializeField] private int coinLevel;

    public int SpeedLevel => speedLevel;
    public int JumpLevel => jumpLevel;
    public int TrickLevel => trickLevel;
    public int CoinLevel => coinLevel;

    public float SpeedMultiplier => 1f + speedLevel * 0.04f;
    public float JumpBonus => jumpLevel * 0.45f;
    public float TrickMultiplier => 1f + trickLevel * 0.08f;
    public int CoinBonus => coinLevel;

    public int SpeedCost => UpgradeCost(speedLevel);
    public int JumpCost => UpgradeCost(jumpLevel);
    public int TrickCost => UpgradeCost(trickLevel);
    public int CoinCost => UpgradeCost(coinLevel);

    public void UpgradeSpeed()
    {
        speedLevel++;
    }

    public void UpgradeJump()
    {
        jumpLevel++;
    }

    public void UpgradeTricks()
    {
        trickLevel++;
    }

    public void UpgradeCoins()
    {
        coinLevel++;
    }

    private static int UpgradeCost(int level)
    {
        return 120 + level * 90;
    }
}
