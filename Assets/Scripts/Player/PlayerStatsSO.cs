using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStats", menuName = "Player/Stats", order = 0)]
public class PlayerStatsSO : ScriptableObject
{
    [Header("Resources")]
    public int gems;

    [Header("Max Health Upgrade")]
    public int healthLevel = 0;
    public int maxHealthUpgrades = 10;
    public float baseHealth = 150f;
    public float healthPerUpgrade = 10f;

    [Header("Move Speed Upgrade")]
    public int speedLevel = 0;
    public int maxSpeedUpgrades = 5;
    public float baseSpeed = 5f;
    public float speedPerUpgrade = 0.5f;

    [Header("Base Damage Upgrade")]
    public int damageLevel = 0;
    public int maxDamageUpgrades = 8;
    public float baseDamage = 2f;       // Damage gốc Player
    public float damagePerUpgrade = 2f; // Mỗi cấp tăng 2 damage

    [Header("Dash Stats")]
    public float dashCooldown = 2f;

    public float GetMaxHealth()
    {
        return baseHealth + healthLevel * healthPerUpgrade;
    }

    public float GetMoveSpeed()
    {
        return baseSpeed + speedLevel * speedPerUpgrade;
    }

    public float GetBaseDamage()
    {
        return baseDamage + damageLevel * damagePerUpgrade;
    }

    public int GetUpgradeCost(int currentLevel)
    {
        // Giá: 10, 20, 30...
        return (currentLevel + 1) * 10;
    }
    public void LoadStats()
    {
        gems = PlayerPrefs.GetInt("PlayerGems", 0);

        healthLevel = PlayerPrefs.GetInt("PlayerHealthLevel", 0);
        speedLevel = PlayerPrefs.GetInt("PlayerSpeedLevel", 0);
        damageLevel = PlayerPrefs.GetInt("PlayerDamageLevel", 0);
    }

    public void SaveStats()
    {
        PlayerPrefs.SetInt("PlayerGems", gems);

        PlayerPrefs.SetInt("PlayerHealthLevel", healthLevel);
        PlayerPrefs.SetInt("PlayerSpeedLevel", speedLevel);
        PlayerPrefs.SetInt("PlayerDamageLevel", damageLevel);

        PlayerPrefs.Save();
    }



}
