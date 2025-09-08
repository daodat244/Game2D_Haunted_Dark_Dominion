using UnityEngine;

[CreateAssetMenu(fileName = "NewUpgrade", menuName = "Upgrades/Upgrade", order = 0)]
public class UpgradeSO : ScriptableObject
{
    public string upgradeName;
    [TextArea] public string description;

    public Sprite icon;

    [Header("Stats")]

    public float extraHealth;
    public float dashCooldownReduction;
    public float extraSpeed;

    [Header("Gems")]
    public float extraGems;

    [Header("Pistol Upgrade")]
    public float reloadTimeReduction; // 👈 giảm thời gian thay đạn

    [Range(1, 3)]
    public int tier;

}
