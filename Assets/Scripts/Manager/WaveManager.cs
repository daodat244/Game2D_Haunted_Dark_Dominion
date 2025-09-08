using System.Collections;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private EnemySpawner spawner;
    [SerializeField] private GameUI gameUI;
    [SerializeField] private int maxWave = 36; // 👈 giới hạn tối đa 36 wave

    [Header("SFX")]
    [SerializeField] private AudioClip bossClip;
    [SerializeField] private AudioClip gameClip;
    [SerializeField] private AudioClip victoryClip;
    [SerializeField] private AudioClip waveClearClip;

    public int currentWave { get; private set; } = 0;
    public int enemiesToKill { get; private set; }
    public int enemiesKilled { get; private set; }
    private bool isTransitioning = false;

    void Start()
    {
        StartNextWave();
        MusicManager.Instance.PlayMusic(gameClip, 0.3f);
    }

    public void EnemyKilled()
    {
        enemiesKilled++;
        gameUI.UpdateWaweProgress();

        if (enemiesKilled >= enemiesToKill)
        {
            if (!isTransitioning)
            {
                // 👇 Clear toàn bộ enemy còn sống trước khi qua wave
                Enemy[] aliveEnemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
                foreach (var e in aliveEnemies)
                {
                    Destroy(e.gameObject);
                }

                // Nếu còn spawnRoutine đang chạy thì dừng lại
                spawner.StopSpawning();

                StartCoroutine(HandleWaveClear());
            }
        }

    }

    private IEnumerator HandleWaveClear()
    {
        isTransitioning = true;
        yield return null;

        // Hiện Wave Clear màu xanh lá
        gameUI.ShowNotification($"Wave {currentWave} Cleared!", 3f, Color.green);

        yield return new WaitForSecondsRealtime(3f);

        StartNextWave();
        isTransitioning = false;
    }

    private void StartNextWave()
    {
        currentWave++;
        if (currentWave != 1)
        {
            SoundFXManager.Instance.PlaySoundFXClip(waveClearClip, transform, 0.5f);
        }

        // Nếu là wave 18 → mid-boss
        if (currentWave == 18)
        {
            Debug.Log("⚔️ Mid-Boss xuất hiện!");
            gameUI.ShowNotification("Warning: Mid Boss appears.");
            StartCoroutine(SpawnBossWithDelay(3f, false));
            return;
        }

        // Nếu là wave cuối → final boss
        if (currentWave >= maxWave)
        {
            Debug.Log("⚔️ Wave cuối cùng! Final Boss xuất hiện!");
            gameUI.ShowNotification("Warning: The final boss appears.");
            StartCoroutine(SpawnBossWithDelay(3f, true));
            return;
        }

        enemiesKilled = 0;

        if (currentWave > 1 && currentWave % 4 == 0)
        {
            Time.timeScale = 0f;
            gameUI.ShowUpgradeChoices();
            return;
        }

        enemiesToKill = currentWave * 2;
        spawner.StartSpawning(enemiesToKill, currentWave);
        gameUI.UpdateWaweProgress();
    }

    public void ContinueWaveAfterUpgrade()
    {
        enemiesToKill = currentWave * 2;

        // 👇 Gọi spawner với wave hiện tại
        spawner.StartSpawning(enemiesToKill, currentWave);

        gameUI.UpdateWaweProgress();
    }

    // Spawn boss sau delay
    private IEnumerator SpawnBossWithDelay(float delay, bool isFinalBoss)
    {
        MusicManager.Instance.PlayMusic(bossClip, 0.3f);
        yield return new WaitForSeconds(delay);

        spawner.SpawnBoss(isFinalBoss); // 👈 truyền flag boss cuối hay không
        gameUI.UpdateWaweProgress();
    }

    public void BossDefeated(bool isFinalBoss)
    {
        if (isFinalBoss)
        {
            StartCoroutine(ShowVictoryWithDelay(3f));
        }
        else
        {
            StartNextWave();
            MusicManager.Instance.PlayMusic(gameClip, 0.3f);
        }
    }

    private IEnumerator ShowVictoryWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameUI.OpenVictoryPanel();
        SoundFXManager.Instance.PlaySoundFXClip(victoryClip, transform, 0.4f);
        Time.timeScale = 0f; // dừng game sau khi hiện Victory UI
    }
}
