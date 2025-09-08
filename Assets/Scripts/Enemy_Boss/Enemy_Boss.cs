using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

public abstract class Enemy_Boss : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] protected Transform target;
    private NavMeshAgent agent;
    [SerializeField] protected float enemyMoveSpeed = 1f;
    [SerializeField] protected float maxHealth = 20f;
    [SerializeField] protected float enterDamage = 5f;
    [SerializeField] protected float stayDamage = 0.000002f;

    protected Player player;
    protected Animator animator;
    protected float currentHealth;
    protected bool isDead = false; // Flag kiểm tra nếu Enemy đã chết
    private bool isFinalBoss = false;
    private WaveManager waveManager;
    protected GameUI gameUI;

    protected virtual void Start()
    {
        player = FindAnyObjectByType<Player>();
        if (player != null)
        {
            target = player.transform; // Gán target bằng transform của Player
        }

        gameUI = FindFirstObjectByType<GameUI>();
        if (gameUI != null)
        {
            gameUI.ShowBossHpBar(maxHealth);
        }

        waveManager = FindAnyObjectByType<WaveManager>();
        animator = GetComponent<Animator>();

        currentHealth = maxHealth;
        gameUI.UpdateBossHp(currentHealth, maxHealth);

        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false; // Tắt tự động quay hướng của NavMeshAgent
        agent.updateUpAxis = false;   // Tắt việc ảnh hưởng đến trục y (vì bạn đang làm 2D)
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
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);
        gameUI.UpdateBossHp(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Death();
        }
    }

    protected void Death()
    {
        if (isDead) return;

        if (gameUI != null)
            gameUI.HideBossHpBar();

        isDead = true;
        if (animator != null)
            animator.SetTrigger("Death");

        // báo WaveManager
        waveManager.BossDefeated(isFinalBoss);

        // Tắt NavMeshAgent và collider
        if (agent != null) { agent.isStopped = true; agent.enabled = false; }
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        GetComponent<Enemy_Boss>().enabled = false;
    }

    public void SetIsFinalBoss(bool value)
    {
        isFinalBoss = value;
    }
}
