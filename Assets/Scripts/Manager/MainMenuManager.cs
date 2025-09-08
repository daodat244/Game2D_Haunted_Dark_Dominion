using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private PlayerStatsSO playerStatsSO;

    [Header("Panels")]
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject optionPanel;
    [SerializeField] private GameObject confirmQuitGame;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI gemsText;

    [Header("Upgrade UI")]
    [SerializeField] private TextMeshProUGUI healthLevelText;
    [SerializeField] private TextMeshProUGUI healthCostText;
    [SerializeField] private TextMeshProUGUI speedLevelText;
    [SerializeField] private TextMeshProUGUI speedCostText;
    [SerializeField] private TextMeshProUGUI damageLevelText;
    [SerializeField] private TextMeshProUGUI damageCostText;
    [SerializeField] private Image healthFillBar;
    [SerializeField] private Image speedFillBar;
    [SerializeField] private Image damageFillBar;


    void Start()
    {
        if (playerStatsSO == null)
        {
            Debug.LogError("PlayerStatsSO is not assigned!");
            return;
        }
        playerStatsSO.LoadStats();
        UpdateUI();
    }

    public void UpdateUI()
    {
        gemsText.text = playerStatsSO.gems.ToString();

        // Health upgrade (hiển thị giá trị + thêm máu)
        float healthBonus = playerStatsSO.healthLevel * playerStatsSO.healthPerUpgrade;
        healthLevelText.text = $"+{healthBonus}";
        if (playerStatsSO.healthLevel < playerStatsSO.maxHealthUpgrades)
            healthCostText.text = $"{playerStatsSO.GetUpgradeCost(playerStatsSO.healthLevel)}";
        else
            healthCostText.text = "MAX";
        healthFillBar.fillAmount = (float)playerStatsSO.healthLevel / playerStatsSO.maxHealthUpgrades;

        // Speed upgrade (hiển thị giá trị + thêm speed)
        float speedBonus = playerStatsSO.speedLevel * playerStatsSO.speedPerUpgrade;
        speedLevelText.text = $"+{speedBonus:F1}"; // :F1 = làm tròn 1 chữ số thập phân
        if (playerStatsSO.speedLevel < playerStatsSO.maxSpeedUpgrades)
            speedCostText.text = $"{playerStatsSO.GetUpgradeCost(playerStatsSO.speedLevel)}";
        else
            speedCostText.text = "MAX";
        speedFillBar.fillAmount = (float)playerStatsSO.speedLevel / playerStatsSO.maxSpeedUpgrades;

        // Damage upgrade
        float damageBonus = playerStatsSO.damageLevel * playerStatsSO.damagePerUpgrade;
        damageLevelText.text = $"+{damageBonus}";
        if (playerStatsSO.damageLevel < playerStatsSO.maxDamageUpgrades)
            damageCostText.text = $"{playerStatsSO.GetUpgradeCost(playerStatsSO.damageLevel) * 2}"; // giá 20/ cấp, nhân 2
        else
            damageCostText.text = "MAX";
        damageFillBar.fillAmount = (float)playerStatsSO.damageLevel / playerStatsSO.maxDamageUpgrades;
    }


    public void UpgradeHealth()
    {
        if (playerStatsSO.healthLevel >= playerStatsSO.maxHealthUpgrades) return;

        int cost = playerStatsSO.GetUpgradeCost(playerStatsSO.healthLevel);
        if (playerStatsSO.gems >= cost)
        {
            playerStatsSO.gems -= cost;
            playerStatsSO.healthLevel++;
            playerStatsSO.SaveStats();
            UpdateUI();
        }
    }

    public void UpgradeSpeed()
    {
        if (playerStatsSO.speedLevel >= playerStatsSO.maxSpeedUpgrades) return;

        int cost = playerStatsSO.GetUpgradeCost(playerStatsSO.speedLevel);
        if (playerStatsSO.gems >= cost)
        {
            playerStatsSO.gems -= cost;
            playerStatsSO.speedLevel++;
            playerStatsSO.SaveStats();
            UpdateUI();
        }
    }

    public void UpgradeDamage()
    {
        if (playerStatsSO.damageLevel >= playerStatsSO.maxDamageUpgrades) return;

        int cost = playerStatsSO.GetUpgradeCost(playerStatsSO.damageLevel) * 2; // 20 gems cấp 1, 40 cấp 2...
        if (playerStatsSO.gems >= cost)
        {
            playerStatsSO.gems -= cost;
            playerStatsSO.damageLevel++;
            playerStatsSO.SaveStats();
            UpdateUI();
        }
    }


    // ==== Menu buttons ====
    public void StartGame()
    {
        SceneManager.LoadScene("Game");
        if (MusicManager.Instance != null) // tránh null
            MusicManager.Instance.ResumeMusic();
    }
    public void Upgrade()
    {
        mainMenuPanel.SetActive(false);
        upgradePanel.SetActive(true);
        UpdateUI();
    }

    public void OpenOption()
    {
        optionPanel.SetActive(true);
    }
    public void BackToMenu()
    {
        mainMenuPanel.SetActive(true);
        upgradePanel.SetActive(false);
        optionPanel.SetActive(false);
    }

    public void CancelQuitGame()
    {
        confirmQuitGame.SetActive(false);
    }

    public void ConfirmQuitGame()
    {
        confirmQuitGame.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game closed!");
    }
}
