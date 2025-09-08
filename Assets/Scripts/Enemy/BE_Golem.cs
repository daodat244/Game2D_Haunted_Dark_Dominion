using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class BE_Golem : Enemy
{
    [Header("Golem Config")]
    [SerializeField] private float rushRange = 6f;   // tầm dùng skill
    [SerializeField] private float preRushPause = 0.3f; // đứng yên trước khi lao
    [SerializeField] private float rushSpeed = 12f;  // tốc độ khi lao
    [SerializeField] private float rushAccel = 60f;  // gia tốc khi lao (giúp phanh mượt)
    [SerializeField] private float cooldown = 8f;   // hồi chiêu
    [SerializeField] private float contactDistanceFallback = 0.45f; // fallback nếu không dùng trigger
    [SerializeField] private Vector3 aoeOffset = new Vector3(0f, -10f, 0f);

    [Header("Attack (gọi bằng Animation Event)")]
    [SerializeField] private float aoeRadius = 2.2f;
    [SerializeField] private float aoeDamage = 20f;
    [SerializeField] private int circleSegments = 48; // độ mịn vòng tròn
    [SerializeField] private float lineWidth = 0.05f;
    [SerializeField] private AudioClip[] golemDamageClipRandom;
    private LineRenderer aoeLine;

    private NavMeshAgent nav;
    private float baseSpeed, baseAccel, baseStop;
    private bool isRushing = false;
    private bool onCooldown = false;
    private bool attackStarted = false;

    protected override void Start()
    {
        base.Start();
        nav = GetComponent<NavMeshAgent>();

        // lưu cấu hình gốc để trả lại sau
        baseSpeed = enemyMoveSpeed;
        baseAccel = nav.acceleration;
        baseStop = nav.stoppingDistance;
        damageSoundClipRandom = golemDamageClipRandom;
    }

    protected override void Update()
    {
        if (isDead) return;

        // đang rush thì chỉ việc bám theo player bằng agent
        if (isRushing)
        {
            if (player) nav.SetDestination(player.transform.position);

            // fallback: nếu không dùng trigger, khi đủ gần thì tấn công
            if (!attackStarted && player &&
                Vector2.Distance(transform.position, player.transform.position) <= contactDistanceFallback)
            {
                TryStartAttack();
            }
            return;
        }

        // nếu hết hồi chiêu & trong tầm → bắt đầu skill
        if (!onCooldown && player &&
            Vector2.Distance(transform.position, player.transform.position) <= rushRange)
        {
            StartCoroutine(RushSkill());
            return;
        }

        // còn lại: đi bộ bình thường
        MoveToPlayer();
        // (nếu có state Walk trong animator bạn có thể set trigger/bool ở đây)
    }

    private IEnumerator RushSkill()
    {
        onCooldown = true;       // chặn spam ngay từ khi khởi động skill
        isRushing = false;
        attackStarted = false;

        // 1) phanh cứng & pause nhẹ
        if (nav)
        {
            nav.isStopped = true;
            nav.velocity = Vector3.zero;
            nav.speed = 0f;
            nav.ResetPath();
        }
        if (animator) animator.SetTrigger("Golem_Rush");
        yield return new WaitForSeconds(preRushPause);

        // 2) tăng tốc & đuổi player bằng agent (né tường)
        if (nav)
        {
            nav.autoBraking = true;   // để agent tự giảm tốc khi gần đích
            nav.acceleration = rushAccel;
            nav.stoppingDistance = 0f;     // áp sát player
            nav.speed = rushSpeed;
            nav.isStopped = false;
        }
        isRushing = true;

        // Đợi tới khi attack xong (attackStarted sẽ được bật trong OnTriggerEnter2D hoặc fallback ở Update)
        float safetyTimeout = 2f; // an toàn tránh kẹt vô hạn
        float t = 0f;
        while (isRushing && !attackStarted && t < safetyTimeout)
        {
            t += Time.deltaTime;
            yield return null;
        }

        // Nếu vì lý do nào đó vẫn chưa attack thì ép tấn công
        if (!attackStarted)
            TryStartAttack();

        // Cooldown 5s kể từ lúc dùng skill
        yield return new WaitForSeconds(cooldown);
        onCooldown = false;
    }

    private void TryStartAttack()
    {
        if (attackStarted) return;
        attackStarted = true;

        // đứng yên để ra đòn, tránh trôi
        if (nav)
        {
            nav.isStopped = true;
            nav.velocity = Vector3.zero;
            nav.speed = 0f;
        }

        if (animator) animator.SetTrigger("Golem_Attack");
        // DoAoeHit() sẽ được gọi bằng Animation Event trong clip Golem_Attack
    }

    // Khi animation Attack kết thúc (ví dụ Animation Event khác hoặc OnStateExit),
    // gọi hàm này từ Animator để reset về đi bộ bình thường:
    private void EndAttack()
    {
        isRushing = false;

        if (nav)
        {
            nav.stoppingDistance = baseStop;
            nav.acceleration = baseAccel;
            nav.speed = baseSpeed;  // trả speed gốc
            nav.isStopped = false;      // tiếp tục đi bộ bình thường
        }
    }

    // Nếu bạn muốn "chạm collider là tấn công", đảm bảo có ít nhất 1 collider là Trigger
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isRushing || attackStarted) return;
        if (other.CompareTag("Player"))
        {
            TryStartAttack();
        }
    }

    // Animation Event trong clip Golem_Attack gọi hàm này đúng frame đánh
    private void DoAoeHit()
    {
        Vector3 aoeCenter = transform.position + aoeOffset;
        Collider2D[] hits = Physics2D.OverlapCircleAll(aoeCenter, aoeRadius);
        foreach (var h in hits)
        {
            if (h.CompareTag("Player"))
            {
                var p = h.GetComponent<Player>();
                if (p) p.TakeDamage(aoeDamage);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red; Gizmos.DrawWireSphere(transform.position, rushRange);
        Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position + aoeOffset, aoeRadius);
    }

    private void OnDeathAnimationEnd()
    {
        CleanupTelegraph(true);  // <-- dọn line trước khi phá Golem
        Destroy(gameObject);
    }

    private void EnsureLine()
    {
        if (aoeLine) return;
        aoeLine = new GameObject("AOE_Line").AddComponent<LineRenderer>();
        aoeLine.transform.SetParent(null);
        aoeLine.useWorldSpace = true;
        aoeLine.loop = true;
        aoeLine.startWidth = aoeLine.endWidth = lineWidth;
        aoeLine.positionCount = circleSegments;
        aoeLine.enabled = false;
        // vật liệu mặc định
        aoeLine.material = new Material(Shader.Find("Sprites/Default"));
        aoeLine.startColor = aoeLine.endColor = new Color(1f, 0f, 0f, 0.9f);
        aoeLine.sortingLayerName = "Shadow";  // hoặc "UI" / "Foreground" tùy bạn đã tạo trong Project Settings > Tags & Layers
    }

    private void StartAoeTelegraph_Line()
    {
        EnsureLine();
        aoeLine.enabled = true;
        Vector3 c = transform.position + aoeOffset;
        for (int i = 0; i < circleSegments; i++)
        {
            float ang = (i / (float)circleSegments) * Mathf.PI * 2f;
            Vector3 p = new Vector3(Mathf.Cos(ang), Mathf.Sin(ang), 0f) * aoeRadius + c;
            aoeLine.SetPosition(i, p);
        }
    }

    private void HideAoeTelegraph_Line()
    {
        CleanupTelegraph(true);
    }

    private void CleanupTelegraph(bool destroy = false)
    {
        if (!aoeLine) return;
        if (destroy) Destroy(aoeLine.gameObject);
        else aoeLine.enabled = false;
    }
}
