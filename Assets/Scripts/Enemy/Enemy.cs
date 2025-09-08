using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using System.Collections.Generic;

public abstract class Enemy : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] Transform target;
    [SerializeField] protected float enemyMoveSpeed = 1f;
    [SerializeField] protected float maxHealth = 20f;
    [SerializeField] protected Image hpBar;
    [SerializeField] protected float enterDamage = 5f;
    [SerializeField] protected float stayDamage = 0.000002f;

    [Header("Drop Settings")]
    [SerializeField] private List<DropItem> dropTable;
    [SerializeField, Range(0f, 1f)] private float noDropChance = 0.2f; // 20% không rơi gì


    private NavMeshAgent agent;
    protected AudioClip damageSoundClip;
    protected AudioClip[] damageSoundClipRandom;
    protected WaveManager waweManager;
    protected Player player;
    protected Animator animator;
    protected float currentHealth;
    protected bool isDead = false; // Flag kiểm tra nếu Enemy đã chết

    protected virtual void Start()
    {
        waweManager = FindFirstObjectByType<WaveManager>();
        player = FindAnyObjectByType<Player>();
        if (player != null)
        {
            target = player.transform;
        }
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;
        UpdateHpBar();

        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        Debug.Log($"{name} spawned at {transform.position}, NavMeshAgent {agent != null}, player={player}");

        // 👇 Kiểm tra lỗi Z âm
        if (transform.position.z < -0.001f)
        {
            Debug.LogWarning($"⚠ Enemy {name} spawn lỗi Z âm ({transform.position.z}), retry wave!");
            EnemySpawner spawner = FindFirstObjectByType<EnemySpawner>();
            if (spawner != null)
            {
                spawner.RetryWave();
            }
        }
    }


    protected virtual void Update()
    {
        if (!isDead) // Chỉ di chuyển nếu chưa chết
        {
            MoveToPlayer();
        }
    }

    // Sử dụng NavMeshAgent để di chuyển đến Player
    protected void MoveToPlayer()
    {
        if (player != null && agent != null)
        {
            agent.SetDestination(target.position);  // Cập nhật điểm đến cho NavMeshAgent
            agent.speed = enemyMoveSpeed;  // Cập nhật tốc độ di chuyển

            // Flip hướng di chuyển dựa trên tốc độ di chuyển (velocity)
            FlipEnemy(agent.velocity);
        }
    }

    // Cập nhật hướng di chuyển của Enemy (Flip) theo velocity
    protected void FlipEnemy(Vector2 velocity)
    {
        if (velocity.x > 0.1f)
        {
            transform.localScale = new Vector3(1.3f, 1.3f, 1.3f); // Hướng sang phải
        }
        else if (velocity.x < -0.1f)
        {
            transform.localScale = new Vector3(-1.3f, 1.3f, 1.3f); // Hướng sang trái
        }
    }

    public virtual void TakeDamage(float damage)
    {
        // SoundFXManager.instance.PlaySoundFXClip(damageSoundClip, transform, 1f);
        SoundFXManager.Instance.PlayRandomSoundFXClip(damageSoundClipRandom, transform, 0.5f);
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);
        UpdateHpBar();

        if (currentHealth <= 0)
        {
            Death();
        }
    }

    protected virtual void Death()
    {
        if (waweManager != null)
        {
            waweManager.EnemyKilled();
        }

        if (isDead) return;  // Nếu đã chết, không gọi lại Death

        isDead = true; // Đánh dấu Enemy là đã chết
        if (animator != null)
        {
            animator.SetTrigger("Death");
        }

        // Tắt NavMeshAgent khi chết để ngừng di chuyển
        if (agent != null)
        {
            agent.isStopped = true;  // Dừng di chuyển
            agent.enabled = false;  // Tắt NavMeshAgent
        }

        // Tắt các hành động di chuyển và flip trong khi chết
        GetComponent<Enemy>().enabled = false; // Tắt AI Enemy khi chết

        // Nếu cần, vô hiệu hóa Collider và các component khác
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // Dừng animation nếu cần
        // animator.speed = 0; // Nếu bạn muốn dừng toàn bộ animation
        DropOneItem();
        Destroy(gameObject, 1.15f);
    }

    protected void UpdateHpBar()
    {
        if (hpBar != null)
        {
            hpBar.fillAmount = currentHealth / maxHealth;
        }
    }

    protected void DropOneItem()
    {
        // Roll trường hợp không rơi gì
        if (Random.value <= noDropChance)
            return;

        // Chọn 1 item theo % từ dropTable
        float totalChance = 0f;
        foreach (var drop in dropTable)
            totalChance += drop.dropChance;

        float roll = Random.value * totalChance;
        float cumulative = 0f;

        foreach (var drop in dropTable)
        {
            cumulative += drop.dropChance;
            if (roll <= cumulative)
            {
                int amount = Random.Range(drop.minAmount, drop.maxAmount + 1);
                for (int i = 0; i < amount; i++)
                    Instantiate(drop.itemPrefab, transform.position, Quaternion.identity);
                break;
            }
        }
    }


}
