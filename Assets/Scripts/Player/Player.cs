using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private PlayerStatsSO baseStats;

    [Header("SFX")]
    [SerializeField] private AudioClip[] footstepClip;
    [SerializeField] private AudioClip[] dashClip;
    [SerializeField] private AudioClip[] takeDamageClip;

    [Header("Movement")]
    private float moveSpeed;

    [Header("Health")]
    private float maxHealth;
    private float currentHealth;

    [Header("Damage")]
    [SerializeField] private float baseDamage;
    private float damageMultiplier = 1f;
    public float DamageMultiplier => damageMultiplier;

    public float BaseDamage => baseDamage;

    [Header("Dash")]
    [SerializeField] private float dashSpeedMultiplier = 2f;
    [SerializeField] private float dashDuration = 0.4f;
    private float dashCooldown;
    private float dashTime = 0f;
    private float dashCooldownTime = 0f;
    private bool isDashing = false;

    private Rigidbody2D rb;
    private Animator animator;
    private GameUI gameUI; // tham chiếu UI
    private float footstepDelay = 0.3f; // Delay giữa các âm thanh bước chân khi di chuyển bình thường
    private float dashFootstepDelay = 0.15f; // Delay ngắn hơn khi dash
    private float lastFootstepTime; // Thời gian lần cuối phát âm thanh
    private bool hasInfiniteAmmo = false;
    public bool HasInfiniteAmmo => hasInfiniteAmmo;
    private Coroutine damageBuffRoutine;
    private float damageBuffEndTime;
    private Coroutine infiniteAmmoRoutine;
    private float infiniteAmmoEndTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        gameUI = FindFirstObjectByType<GameUI>();
    }

    void Start()
    {
        // Khởi tạo từ giá trị gốc (ScriptableObject)
        if (baseStats.gems == 0)
        {
            baseStats.LoadStats();
        }

        baseDamage = baseStats.GetBaseDamage();
        moveSpeed = baseStats.GetMoveSpeed();
        dashCooldown = baseStats.dashCooldown;
        maxHealth = baseStats.GetMaxHealth();
        currentHealth = maxHealth;

        gameUI.UpdateHpBar(currentHealth, maxHealth);
        gameUI.UpdateDashBar(1f);
        gameUI.UpdateGemText();
    }

    void Update()
    {
        HandleMovement();
        HandleAnimations();

        if (dashCooldownTime > 0)
        {
            dashCooldownTime -= Time.deltaTime;
            if (dashCooldownTime < 0) dashCooldownTime = 0;

            gameUI.UpdateDashBar(1f - (dashCooldownTime / dashCooldown), dashCooldownTime);
        }

    }

    void HandleMovement()
    {
        // Xử lý input và di chuyển
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        float speed = isDashing ? moveSpeed * dashSpeedMultiplier : moveSpeed;
        rb.linearVelocity = input.normalized * speed;

        // Phát âm thanh bước chân
        if (input != Vector2.zero && SoundFXManager.Instance != null && footstepClip != null)
        {
            float currentDelay = isDashing ? dashFootstepDelay : footstepDelay;
            if (Time.time >= lastFootstepTime + currentDelay)
            {
                SoundFXManager.Instance.PlayRandomSoundFXClip(footstepClip, transform, 0.5f);
                lastFootstepTime = Time.time;
            }
        }

        // Xử lý dash
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isDashing && dashCooldownTime <= 0)
        {
            isDashing = true;
            dashTime = dashDuration;
            dashCooldownTime = dashCooldown;
            gameUI.UpdateDashBar(0f);

            // Phát âm thanh dash một lần khi bắt đầu dash
            if (SoundFXManager.Instance != null && dashClip != null)
            {
                SoundFXManager.Instance.PlayRandomSoundFXClip(dashClip, transform, 0.3f);
            }
        }

        if (isDashing)
        {
            dashTime -= Time.deltaTime;
            if (dashTime <= 0f) isDashing = false;
        }

        if (dashCooldownTime > 0f)
        {
            dashCooldownTime -= Time.deltaTime;
            gameUI.UpdateDashBar(1f - dashCooldownTime / dashCooldown, dashCooldownTime);
        }
    }

    void HandleAnimations()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        bool isMoving = Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f;
        animator.SetBool("isRun", isMoving);

        if (isMoving)
        {
            if (Mathf.Abs(horizontal) > Mathf.Abs(vertical))
            {
                if (horizontal > 0.1f)
                {
                    if (vertical > 0.1f) animator.Play("PlayerIdle_RightUp");
                    else if (vertical < -0.1f) animator.Play("PlayerIdle_RightDown");
                    else animator.Play("PlayerIdle_RightDown");
                }
                else if (horizontal < -0.1f)
                {
                    if (vertical > 0.1f) animator.Play("PlayerIdle_LeftUp");
                    else if (vertical < -0.1f) animator.Play("PlayerIdle_LeftDown");
                    else animator.Play("PlayerIdle_LeftDown");
                }
            }
            else
            {
                if (vertical > 0.1f) animator.Play("PlayerIdle_Up");
                else if (vertical < -0.1f) animator.Play("PlayerIdle_Down");
            }
        }
        else
        {
            animator.Play("PlayerTrueIdle_Down");
        }
    }

    public virtual void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);
        gameUI.UpdateHpBar(currentHealth, maxHealth);
        SoundFXManager.Instance.PlayRandomSoundFXClip(takeDamageClip, transform, 0.5f);

        if (currentHealth <= 0) Death();
    }

    private void Death()
    {
        rb.linearVelocity = Vector2.zero;
        animator.SetTrigger("Death");
        GetComponent<Player>().enabled = false;
        gameUI.OpenGameOverPanel();
    }

    public void OnDeathAnimationEnd()
    {
        animator.speed = 0;
    }

    public void ApplyUpgrade(UpgradeSO upgrade)
    {
        if (upgrade == null) return;

        // Health
        maxHealth += upgrade.extraHealth;
        currentHealth += upgrade.extraHealth;
        gameUI.UpdateHpBar(currentHealth, maxHealth);

        // Dash cooldown giảm
        dashCooldown -= upgrade.dashCooldownReduction;
        dashCooldown = Mathf.Max(0.1f, dashCooldown);

        // Tăng tốc độ
        moveSpeed += upgrade.extraSpeed;

        // Tăng gems (nếu có)
        if (upgrade.extraGems > 0)
        {
            baseStats.gems += (int)upgrade.extraGems;
            gameUI.UpdateGemText();
            baseStats.SaveStats();
        }

        // 👇 Giảm thời gian thay đạn
        if (upgrade.reloadTimeReduction > 0)
        {
            Pistol pistol = FindFirstObjectByType<Pistol>();
            if (pistol != null)
            {
                pistol.ApplyReloadUpgrade(upgrade.reloadTimeReduction);
            }
        }
    }


    public void AddGems(int amount)
    {
        baseStats.gems += amount;         // cộng vào tài nguyên trong SO
        baseStats.SaveStats();
        gameUI.UpdateGemText();           // cập nhật UI
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        gameUI.UpdateHpBar(currentHealth, maxHealth);
    }

    public void AddSpeed(float amount, float duration)
    {
        StartCoroutine(SpeedBuff(amount, duration));
    }

    private IEnumerator SpeedBuff(float amount, float duration)
    {
        moveSpeed += amount;
        yield return new WaitForSeconds(duration);
        moveSpeed -= amount;
    }

    public void AddDamageMultiplier(float multiplier, float duration)
    {
        // Nếu đã có buff, hủy coroutine cũ để reset thời gian
        if (damageBuffRoutine != null)
            StopCoroutine(damageBuffRoutine);

        damageBuffRoutine = StartCoroutine(DamageBuff(multiplier, duration));
        gameUI.ShowDamageBuff(duration);   // UI fill lại đúng thời gian
    }

    private IEnumerator DamageBuff(float multiplier, float duration)
    {
        damageMultiplier *= multiplier;
        damageBuffEndTime = Time.time + duration;

        while (Time.time < damageBuffEndTime)
        {
            // update UI theo thời gian còn lại
            float remaining = damageBuffEndTime - Time.time;
            gameUI.UpdateDamageBuff(remaining);
            yield return null;
        }

        damageMultiplier /= multiplier;
        damageBuffRoutine = null;
        gameUI.HideDamageBuff();
    }
    public void AddInfiniteAmmo(float duration)
    {
        if (infiniteAmmoRoutine != null)
            StopCoroutine(infiniteAmmoRoutine);

        infiniteAmmoRoutine = StartCoroutine(InfiniteAmmoBuff(duration));
        gameUI.ShowInfiniteAmmoBuff(duration);
    }

    private IEnumerator InfiniteAmmoBuff(float duration)
    {
        hasInfiniteAmmo = true;
        infiniteAmmoEndTime = Time.time + duration;

        while (Time.time < infiniteAmmoEndTime)
        {
            float remaining = infiniteAmmoEndTime - Time.time;
            gameUI.UpdateInfiniteAmmoBuff(remaining);
            yield return null;
        }

        hasInfiniteAmmo = false;
        infiniteAmmoRoutine = null;
        gameUI.HideInfiniteAmmoBuff();
    }
}
