using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Video;

public class GameUI : MonoBehaviour
{
    [Header("Ammo / Wave / Progress")]
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private Pistol Pistol;
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private Image progressBar;
    [SerializeField] private WaveManager waweManager;

    [Header("Player UI")]
    [SerializeField] private Image hpBar;
    [SerializeField] private Image dashBar;

    [SerializeField] private TextMeshProUGUI hpText;    // ← Text hiển thị HP
    [SerializeField] private TextMeshProUGUI dashText;  // ← Text hiển thị Dash cooldown
    [SerializeField] private TextMeshProUGUI gemsText;
    [SerializeField] private PlayerStatsSO playerStatsSO;

    [Header("Pause Menu")]
    [SerializeField] private GameObject pauseGameUI;

    [Header("Upgrade UI")]
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private Transform cardContainer;
    [SerializeField] private UpgradeCard cardPrefab;
    [SerializeField] private UpgradeDatabaseSO upgradeDatabase;

    [Header("GameOver UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI gameOverDes;

    [Header("Confirm Exit UI")]
    [SerializeField] private GameObject confirmExitPanel;

    [Header("Victory UI")]
    [SerializeField] private GameObject victoryPanel;

    [Header("Notification")]

    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private CanvasGroup notificationCanvasGroup; // 👈 thay vì GameObject

    [Header("Boss UI")]
    [SerializeField] private GameObject bossHpPanel; // chứa thanh máu boss
    [SerializeField] private Image bossHpBar;
    [SerializeField] private TextMeshProUGUI bossHpText;

    [Header("Options")]
    [SerializeField] private GameObject optionPanel;

    [Header("Damage Buff UI")]
    [SerializeField] private GameObject damageBuffPanel;
    [SerializeField] private Image damageBuffBar;

    [Header("Infinite Ammo UI")]
    [SerializeField] private GameObject ammoBuffPanel;
    [SerializeField] private Image ammoBuffBar;

    [Header("SFX")]
    [SerializeField] private AudioClip upgradePanelClip;
    [SerializeField] private AudioClip gameoverClip;


    private Coroutine notificationRoutine;
    private UpgradeCard chosenCard;
    private List<UpgradeCard> spawnedCards = new List<UpgradeCard>();
    private bool isPaused = false;

    void Start()
    {
        UpdateAmmoText();
        UpdateWaweProgress();
        UpdateGemText();
        damageBuffPanel.SetActive(false);
        ammoBuffPanel.SetActive(false);

        if (pauseGameUI != null)
            pauseGameUI.SetActive(false);

        if (upgradePanel != null)
            upgradePanel.SetActive(false);
    }

    void Update()
    {
        UpdateAmmoText();

        // Pause / Resume
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused == false)
                PauseGame();
        }

        // Test mở Upgrade UI (phím U)
        if (Input.GetKeyDown(KeyCode.U))
        {
            ShowUpgradeChoices();
        }


    }

    public void UpdateGemText()
    {
        if (gemsText != null && playerStatsSO != null)
        {
            gemsText.text = playerStatsSO.gems.ToString();
        }
    }

    // ====== Ammo / Wave / Progress ======
    public void UpdateAmmoText()
    {
        if (ammoText != null && Pistol != null)
            ammoText.text = Pistol.currentAmmo.ToString();
    }

    public void UpdateWaweProgress()
    {
        if (waveText != null && waweManager != null)
        {
            int enemiesLeft = waweManager.enemiesToKill - waweManager.enemiesKilled;
            waveText.text = $"Wave {waweManager.currentWave}\nEnemy Left: {enemiesLeft}";
        }

        if (progressBar != null && waweManager != null)
        {
            float progress = (float)waweManager.enemiesKilled / waweManager.enemiesToKill;
            progressBar.fillAmount = progress;
        }
    }

    // ====== Pause ======
    public void PauseGame()
    {
        Time.timeScale = 0f;
        if (pauseGameUI != null)
            pauseGameUI.SetActive(true);
        isPaused = true;
    }

    public void OpenOption()
    {
        if (optionPanel != null)
            optionPanel.SetActive(true);
    }

    public void CloseOption()
    {
        if (optionPanel != null)
            optionPanel.SetActive(false);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        if (pauseGameUI != null)
            pauseGameUI.SetActive(false);
        isPaused = false;
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f;
        MusicManager.Instance.PauseMusic();
        SceneManager.LoadScene("MainMenu");
    }

    // Gọi khi bấm nút "Thoát" trong Pause Menu
    public void AskConfirmExit()
    {
        if (confirmExitPanel != null)
            confirmExitPanel.SetActive(true);
    }

    // Gọi khi bấm nút "Có"
    public void ConfirmExitYes()
    {
        BackToMenu();  // dùng lại logic thoát về menu
    }

    // Gọi khi bấm nút "Không"
    public void ConfirmExitNo()
    {
        if (confirmExitPanel != null)
            confirmExitPanel.SetActive(false);
    }


    // ====== Player Bars ======
    public void UpdateHpBar(float current, float max)
    {
        if (hpBar != null)
            hpBar.fillAmount = current / max;

        if (hpText != null)
            hpText.text = $"{current}/{max}"; // ví dụ: 120/150
    }

    public void UpdateDashBar(float fillAmount, float cooldownTime = 0f)
    {
        if (dashBar != null)
            dashBar.fillAmount = fillAmount;

        if (dashText != null)
            dashText.text = cooldownTime > 0f ? $"{cooldownTime:F1}s" : "SHIFT";
    }


    // ====== Upgrade UI ======
    public void ShowUpgradeChoices()
    {
        SoundFXManager.Instance.PlaySoundFXClip(upgradePanelClip, transform, 0.5f);
        Time.timeScale = 0f;
        upgradePanel.SetActive(true);

        foreach (Transform child in cardContainer)
            Destroy(child.gameObject);
        spawnedCards.Clear();
        chosenCard = null;

        // 👇 Lấy wave hiện tại từ WaveManager
        int currentWave = FindFirstObjectByType<WaveManager>().currentWave;

        int allowedTier = 1;
        if (currentWave <= 12) allowedTier = 1;
        else if (currentWave <= 24) allowedTier = 2;
        else allowedTier = 3;

        // 👇 Lọc upgrade theo tier
        var availableUpgrades = upgradeDatabase.upgrades
            .FindAll(u => u.tier == allowedTier);

        if (availableUpgrades.Count == 0)
        {
            Debug.LogWarning("⚠ Không có upgrade nào cho tier " + allowedTier);
            return;
        }

        List<int> usedIndex = new List<int>();

        // Random 3 thẻ từ danh sách đã lọc
        while (spawnedCards.Count < 3 && usedIndex.Count < availableUpgrades.Count)
        {
            int rand = Random.Range(0, availableUpgrades.Count);
            if (!usedIndex.Contains(rand))
            {
                usedIndex.Add(rand);
                var data = availableUpgrades[rand];

                var card = Instantiate(cardPrefab, cardContainer);
                card.Setup(data, (selected) =>
                {
                    foreach (var c in spawnedCards)
                        c.SetSelected(false);

                    selected.SetSelected(true);
                    chosenCard = selected;
                });

                spawnedCards.Add(card);
            }
        }
    }

    public void ConfirmUpgradeChoice()
    {
        if (chosenCard != null)
        {
            Player player = FindFirstObjectByType<Player>();
            if (player != null)
                player.ApplyUpgrade(chosenCard.upgradeSO);

            Debug.Log("Xác nhận nâng cấp: " + chosenCard.UpgradeName);
        }

        upgradePanel.SetActive(false);
        Time.timeScale = 1f;

        // Sau khi chọn xong upgrade, gọi lại WaweManager để tiếp tục wave
        WaveManager waveManager = FindFirstObjectByType<WaveManager>();
        if (waveManager != null)
            waveManager.ContinueWaveAfterUpgrade();
    }

    // ====== GameOver UI ======
    public void ExitToMenu()
    {
        Time.timeScale = 1f;
        MusicManager.Instance.PauseMusic();
        SceneManager.LoadScene("MainMenu");
    }

    public void PlayAgain()
    {
        gameOverPanel.SetActive(false);
        victoryPanel.SetActive(false);
        Time.timeScale = 1f;
        SceneManager.LoadScene("Game");
    }

    public void OpenGameOverPanel()
    {
        gameOverPanel.SetActive(true);
        SoundFXManager.Instance.PlaySoundFXClip(gameoverClip, transform, 0.5f);
        gameOverDes.text = "You have survived " + waweManager.currentWave + " waves.";
        Time.timeScale = 0f;
    }
    public void OpenVictoryPanel()
    {
        victoryPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    // ====== Notification UI ======
    public void ShowNotification(string message, float duration = 3f, Color? textColor = null)
    {
        if (notificationRoutine != null)
            StopCoroutine(notificationRoutine);

        notificationText.color = textColor ?? Color.white;
        notificationRoutine = StartCoroutine(FadeNotification(message, duration));
    }

    private IEnumerator FadeNotification(string message, float duration)
    {
        notificationText.text = message;
        notificationPanel.SetActive(true);
        notificationCanvasGroup.alpha = 0;

        // Fade In
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * 2f;
            notificationCanvasGroup.alpha = Mathf.Lerp(0, 1, t);
            yield return null;
        }

        // Giữ một lúc
        yield return new WaitForSecondsRealtime(duration - 2f);

        // Fade Out
        t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * 2f;
            notificationCanvasGroup.alpha = Mathf.Lerp(1, 0, t);
            yield return null;
        }

        notificationPanel.SetActive(false);
    }


    // ====== PLAYER BUFF UI ======
    private float damageBuffMaxDuration;
    private float ammoBuffMaxDuration;

    // DAMAGE BUFF
    public void ShowDamageBuff(float duration)
    {
        damageBuffPanel.SetActive(true);
        damageBuffMaxDuration = duration;
        damageBuffBar.fillAmount = 1f;
    }

    public void UpdateDamageBuff(float remaining)
    {
        if (damageBuffPanel.activeSelf)
            damageBuffBar.fillAmount = remaining / damageBuffMaxDuration;
    }

    public void HideDamageBuff()
    {
        damageBuffPanel.SetActive(false);
    }

    // INFINITE AMMO BUFF
    public void ShowInfiniteAmmoBuff(float duration)
    {
        ammoBuffPanel.SetActive(true);
        ammoBuffMaxDuration = duration;
        ammoBuffBar.fillAmount = 1f;
    }

    public void UpdateInfiniteAmmoBuff(float remaining)
    {
        if (ammoBuffPanel.activeSelf)
            ammoBuffBar.fillAmount = remaining / ammoBuffMaxDuration;
    }

    public void HideInfiniteAmmoBuff()
    {
        ammoBuffPanel.SetActive(false);
    }


    // ====== Boss UI ======

    // Gọi khi boss spawn
    public void ShowBossHpBar(float maxHp)
    {
        if (bossHpPanel != null) bossHpPanel.SetActive(true);

        UpdateBossHp(maxHp, maxHp); // full máu khi vừa xuất hiện
    }

    // Update máu trong lúc chiến đấu
    public void UpdateBossHp(float current, float max)
    {
        if (bossHpBar != null)
            bossHpBar.fillAmount = current / max;

        if (bossHpText != null)
            bossHpText.text = $"{current}/{max}";
    }

    // Gọi khi boss chết
    public void HideBossHpBar()
    {
        if (bossHpPanel != null) bossHpPanel.SetActive(false);
    }

}