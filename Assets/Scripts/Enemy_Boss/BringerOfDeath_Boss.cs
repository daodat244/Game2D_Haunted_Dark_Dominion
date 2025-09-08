using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class BringerOfDeath_Boss : Enemy_Boss
{
    [Header("Combat")]
    [SerializeField] private int attackDamage = 7;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private float actualAttackRange = 7f;
    [SerializeField] private string attackStateName = "Attack";
    [SerializeField] private string attackTriggerName = "Attack";

    [Header("Attack Range Offset")]
    [SerializeField] private Vector3 attackRangeOffset = Vector3.zero; // Điều chỉnh vị trí tâm tầm đánh
    [SerializeField] private float attackRangeHeight = 2f; // Chiều cao của capsule tấn công

    [Header("Skill")]
    [SerializeField] private float teleCooldown = 6f;
    [SerializeField] private float castCooldown = 8f;
    [SerializeField] private float castRange = 12f;
    [SerializeField] private string castStateName = "Cast";
    [SerializeField] private string castTriggerName = "Cast";
    [SerializeField] private GameObject spellPrefab;
    [SerializeField] private float teleRange = 14f;
    [SerializeField] private string teleStateName = "Teleport"; // Trạng thái Tele trong Animator
    [SerializeField] private string teleTriggerName = "Tele"; // Trigger Tele trong Animator

    [SerializeField] private string tele_endStateName = "Teleport1"; // Trạng thái Tele trong Animator
    [SerializeField] private string tele_endTriggerName = "Tele_end"; // Trigger Tele trong Animator

    [Header("Cast Spawn Settings")]
    [SerializeField] private int castSpawnCount = 10;
    [SerializeField] private float castSpawnRadius = 10f;
    [SerializeField] private bool castSpawnAroundPlayer = true;

    [Header("Detect / Move")]
    [SerializeField] private float chaseSpeed = 3.5f;


    [Header("Audio Clip")]
    [SerializeField] private AudioClip attackClip;
    [SerializeField] private AudioClip spellClip;
    [SerializeField] private AudioClip teleportClip;

    private float lastAttackTime;
    private float lastTeleTime;
    private float lastCastTime;

    private bool isTeleporting;
    private bool isCasting;

    private NavMeshAgent navAgent;

    protected override void Start()
    {
        base.Start();
        lastTeleTime = Time.time;
        lastCastTime = Time.time;

        navAgent = GetComponent<NavMeshAgent>();
        if (AgentReady())
        {
            navAgent.updateRotation = false;
            navAgent.updateUpAxis = false;
            navAgent.speed = chaseSpeed;

            // Cải thiện khả năng tránh chướng ngại vật
            navAgent.radius = 1.5f;           // Bán kính tránh chướng ngại vật
            navAgent.height = 1f;             // Chiều cao của agent
            navAgent.avoidancePriority = 50;  // Độ ưu tiên tránh (0-99)
            navAgent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance; // Chất lượng tránh cao
            navAgent.agentTypeID = 0;         // Sử dụng agent type mặc định
        }

        if (target == null && player != null)
        {
            target = player.transform;
        }
    }

    private bool AgentReady()
    {
        return navAgent != null && navAgent.enabled && navAgent.isOnNavMesh;
    }

    // Kiểm tra xem player có trong tầm đánh không
    private bool IsPlayerInAttackRange(Vector3 playerPos)
    {
        Vector3 attackCenter = transform.position + GetFlippedOffset();
        Vector3 distance = playerPos - attackCenter;

        // Kiểm tra xem player có trong vùng hình vuông không
        bool inRangeX = Mathf.Abs(distance.x) <= actualAttackRange;
        bool inRangeY = Mathf.Abs(distance.y) <= attackRangeHeight;

        return inRangeX && inRangeY;
    }

    // Lấy offset đã được flip theo hướng của Boss
    private Vector3 GetFlippedOffset()
    {
        Vector3 flippedOffset = attackRangeOffset;

        // Nếu Boss đang nhìn sang trái (scale.x < 0), flip offset theo trục X
        if (transform.localScale.x < 0)
        {
            flippedOffset.x = -attackRangeOffset.x;
        }

        return flippedOffset;
    }

    protected override void Update()
    {
        if (player == null && target == null)
        {
            base.Update();
            return;
        }

        Vector3 targetPos = target != null ? target.position : player.transform.position;
        float dist = Vector2.Distance(transform.position, targetPos);

        // Kiểm tra trạng thái hiện tại
        bool inAttackNow = false;
        bool inCastNow = false;
        if (animator != null)
        {
            var st = animator.GetCurrentAnimatorStateInfo(0);
            var next = animator.GetNextAnimatorStateInfo(0);
            bool trans = animator.IsInTransition(0);
            inAttackNow = st.IsName(attackStateName) || (trans && next.IsName(attackStateName));
            inCastNow = st.IsName(castStateName) || (trans && next.IsName(castStateName));
        }

        // Nếu đang teleport, tấn công hoặc cast thì không di chuyển
        if (isTeleporting || inAttackNow || inCastNow || isCasting)
        {
            if (AgentReady())
            {
                navAgent.isStopped = true;
                navAgent.velocity = Vector3.zero; // Reset velocity để tránh trôi
            }
            if (animator != null) animator.SetBool("IsWalking", false);
            FlipByAgentVelocity();
            return;
        }

        // Cast chỉ khi hết hồi chiêu và player trong tầm cast
        if (!isCasting && Time.time >= lastCastTime + castCooldown && dist <= castRange)
        {
            if (AgentReady())
            {
                navAgent.isStopped = true;
                navAgent.velocity = Vector3.zero; // Reset velocity để tránh trôi
            }
            isCasting = true;
            StartCoroutine(CastSpell());
            if (animator != null) animator.SetBool("IsWalking", false);
            return;
        }

        // Teleport chỉ khi hết hồi chiêu và đang ngoài tầm đánh
        if (Time.time >= lastTeleTime + teleCooldown && !IsPlayerInAttackRange(targetPos))
        {
            if (AgentReady())
            {
                navAgent.isStopped = true;
                navAgent.velocity = Vector3.zero; // Reset velocity để tránh trôi
            }
            StartCoroutine(Teleport());
            if (animator != null) animator.SetBool("IsWalking", false);
            return;
        }

        // Di chuyển đuổi theo mục tiêu bằng NavMeshAgent
        if (!IsPlayerInAttackRange(targetPos))
        {
            if (AgentReady())
            {
                navAgent.isStopped = false;
                navAgent.speed = chaseSpeed;
                navAgent.SetDestination(targetPos);
                FlipByAgentVelocity();
                if (animator != null) animator.SetBool("IsWalking", true);
            }
        }
        else
        {
            // Trong tầm đánh: dừng agent để không trôi
            if (AgentReady())
            {
                navAgent.isStopped = true;
                navAgent.velocity = Vector3.zero; // Reset velocity để tránh trôi
            }
            if (animator != null) animator.SetBool("IsWalking", false);
            TryAttack();
            FlipByAgentVelocity();
        }
    }

    private void TryAttack()
    {
        if (player == null && target == null) return;

        Vector3 targetPos = target != null ? target.position : player.transform.position;
        bool inRange = IsPlayerInAttackRange(targetPos);
        bool offCooldown = Time.time >= lastAttackTime + attackCooldown;

        bool inAttackNow = false;
        if (animator != null)
        {
            var st = animator.GetCurrentAnimatorStateInfo(0);
            var next = animator.GetNextAnimatorStateInfo(0);
            bool trans = animator.IsInTransition(0);
            inAttackNow = st.IsName(attackStateName) || (trans && next.IsName(attackStateName));
        }

        if (inRange && offCooldown && !inAttackNow)
        {
            lastAttackTime = Time.time;
            if (animator != null) animator.SetTrigger(attackTriggerName);
        }
    }

    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
        if (!isDead && animator != null)
        {
            animator.SetTrigger("Hurt");
        }
    }

    private IEnumerator CastSpell()
    {
        // Kích hoạt animation Cast
        if (animator != null)
        {
            animator.ResetTrigger(attackTriggerName);
            animator.SetBool("IsWalking", false);
            animator.SetTrigger(castTriggerName);
            SoundFXManager.Instance.PlaySoundFXClip(spellClip, transform, 0.3f);
        }

        // Chờ Cast kết thúc hoặc rời state
        yield return null; // đợi 1 frame để cập nhật state
        var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        float safetyTimer = 0f;
        const float maxWaitCast = 2f; // đủ cho clip Cast
        while (stateInfo.IsName(castStateName) && stateInfo.normalizedTime < 1f && safetyTimer < maxWaitCast)
        {
            yield return null;
            safetyTimer += Time.deltaTime;
            stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        }

        // Spell sẽ được spawn thông qua AnimationEvent OnCastComplete()
        // Không cần spawn ở đây nữa

        lastCastTime = Time.time; // reset cooldown cast
        isCasting = false;
        if (AgentReady()) navAgent.isStopped = false;
    }

    private IEnumerator Teleport()
    {
        isTeleporting = true;
        SoundFXManager.Instance.PlaySoundFXClip(teleportClip, transform, 1f);

        // 1) Gây hiệu ứng biến mất tại chỗ với Tele
        if (animator != null)
        {
            animator.ResetTrigger(attackTriggerName);
            animator.SetBool("IsWalking", false);
            animator.SetTrigger(teleTriggerName);

            // Chờ Tele kết thúc hoặc rời state
            yield return null; // đợi 1 frame để cập nhật state
            var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            float safetyTimer = 0f;
            const float maxWaitTele = 0.7f; // đủ cho clip Tele ngắn
            while (stateInfo.IsName(teleStateName) && stateInfo.normalizedTime < 1f && safetyTimer < maxWaitTele)
            {
                yield return null;
                safetyTimer += Time.deltaTime;
                stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            }
        }

        // 2) Dịch chuyển đến vị trí Player và phát Teleport1 (xuất hiện)
        Vector3 targetPosition = transform.position;
        if (target != null)
        {
            targetPosition = target.position;
        }
        else if (player != null)
        {
            targetPosition = player.transform.position;
        }
        transform.position = targetPosition;

        if (animator != null)
        {
            animator.SetTrigger(tele_endTriggerName);

            // 2a) Đợi thật sự vào state Teleport1
            yield return null; // đợi 1 frame để cập nhật state
            var stateInfo2 = animator.GetCurrentAnimatorStateInfo(0);
            float waitEnterTimer = 0f;
            const float maxWaitEnter = 0.5f; // đủ cho transition ngắn
            while (!stateInfo2.IsName(tele_endStateName) && waitEnterTimer < maxWaitEnter)
            {
                yield return null;
                waitEnterTimer += Time.deltaTime;
                stateInfo2 = animator.GetCurrentAnimatorStateInfo(0);
            }

            // 2b) Nếu đã vào Teleport1, chờ đến khi nó hoàn tất hoặc rời state
            if (stateInfo2.IsName(tele_endStateName))
            {
                float safetyTimer2 = 0f;
                const float maxWaitTeleEnd = 1.2f; // Teleport1 ~0.9s
                while (stateInfo2.IsName(tele_endStateName) && stateInfo2.normalizedTime < 1f && safetyTimer2 < maxWaitTeleEnd)
                {
                    yield return null;
                    safetyTimer2 += Time.deltaTime;
                    stateInfo2 = animator.GetCurrentAnimatorStateInfo(0);
                }
            }

            // 3) Kích hoạt Attack ngay sau Teleport1
            animator.SetTrigger(attackTriggerName);
            lastAttackTime = Time.time; // đồng bộ cooldown
        }

        lastTeleTime = Time.time; // reset cooldown teleport
        isTeleporting = false;
        if (AgentReady()) navAgent.isStopped = false;
        FlipByAgentVelocity();
    }

    // Gọi từ Animation Event khi Cast animation hoàn tất
    public void OnCastComplete()
    {
        if (spellPrefab == null) return;

        // Xác định tâm spawn (quanh player hoặc quanh boss)
        Vector3 center = transform.position;
        if (castSpawnAroundPlayer && player != null)
        {
            center = player.transform.position;
        }

        // Spawn nhiều spell trong hình tròn bán kính cấu hình
        for (int i = 0; i < castSpawnCount; i++)
        {
            Vector2 offset = Random.insideUnitCircle * castSpawnRadius;
            Vector3 pos = new Vector3(center.x + offset.x, center.y + offset.y, center.z);
            Instantiate(spellPrefab, pos, Quaternion.identity);
        }
    }
    public void OnAttackDamagePoint()
    {
        if (player == null && target == null) return;

        Vector3 targetPos = target != null ? target.position : player.transform.position;

        // Sử dụng cùng logic kiểm tra tầm đánh như IsPlayerInAttackRange
        if (IsPlayerInAttackRange(targetPos))
        {
            var p = target != null ? target.GetComponent<Player>() : player.GetComponent<Player>();
            if (p != null) p.TakeDamage(attackDamage);
        }
    }

    private void FlipByAgentVelocity()
    {
        if (AgentReady())
        {
            Vector2 v = navAgent.velocity;
            if (v.x > 0.1f)
            {
                transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
            }
            else if (v.x < -0.1f)
            {
                transform.localScale = new Vector3(-1.3f, 1.3f, 1.3f);
            }
        }
    }
    private void OnDrawGizmosSelected()
    {
        // Tầm tấn công từ boss với offset đã flip
        Vector3 attackCenter = transform.position + GetFlippedOffset();

        // Vẽ hình vuông tấn công thay vì hình tròn
        Gizmos.color = Color.red;
        Vector3 size = new Vector3(actualAttackRange * 2, attackRangeHeight * 2, 0.1f);
        Gizmos.DrawWireCube(attackCenter, size);

        // Hiển thị offset bằng một điểm nhỏ
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(attackCenter, 0.1f);

        // Đường kẻ từ boss đến tâm tấn công
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, attackCenter);

        // Vẽ các tầm khác
        Gizmos.color = Color.green; Gizmos.DrawWireSphere(transform.position, castRange);
        Gizmos.color = Color.white; Gizmos.DrawWireSphere(transform.position, teleRange);
    }

    public void OnDeathAnimationEnd()
    {
        Destroy(gameObject);
    }
}
