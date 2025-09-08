using UnityEngine;
using UnityEngine.AI;

public class BE_Flight : Enemy
{
    [Header("Attack")]
    [SerializeField] private float attackRange = 6f;         // tầm bắn
    [SerializeField] private float attackCooldown = 1f;      // độ trễ giữa 2 lần bắn
    [SerializeField] private Transform firePos;              // điểm bắn (bạn gán trong Inspector)
    [SerializeField] private GameObject bulletPrefab;        // prefab Flight_Bullet
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private float bulletDamage = 10f;
    [SerializeField] private float bulletLifetime = 3f;

    [Header("SFX")]
    [SerializeField] private AudioClip[] flightDamageClipRandom;
    [SerializeField] private AudioClip[] flightBulletClip;

    private NavMeshAgent nav;            // tự lấy, vì 'agent' trong base là private
    private float nextShootTime = 0f;

    protected override void Start()
    {
        base.Start();
        nav = GetComponent<NavMeshAgent>();
        damageSoundClipRandom = flightDamageClipRandom;;
    }

    protected override void Update()
    {
        if (isDead || player == null) return;

        float dist = Vector2.Distance(transform.position, player.transform.position);

        if (dist <= attackRange)
        {
            // Đứng bắn
            if (nav)
            {
                nav.isStopped = true;
                nav.velocity = Vector3.zero;
            }

            // Quay mặt về Player (giữ sprite đúng hướng khi đứng bắn)
            Vector2 dir = (player.transform.position - transform.position).normalized;
            if (dir.x > 0.1f) transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
            if (dir.x < -0.1f) transform.localScale = new Vector3(-1.3f, 1.3f, 1.3f);

            // Bắn theo cooldown
            if (Time.time >= nextShootTime)
            {
                nextShootTime = Time.time + attackCooldown;
                if (animator) animator.SetTrigger("Flight_Attack"); // Animation Event sẽ gọi Fire()
            }
        }
        else
        {
            // Đuổi theo như base
            if (nav) nav.isStopped = false;
            MoveToPlayer(); // dùng navmesh như Enemy gốc  :contentReference[oaicite:1]{index=1}
        }
    }

    // === Animation Event (đặt trong clip Flight_Attack) gọi vào đúng frame bắn ===
    private void Fire()
    {
        if (bulletPrefab == null || firePos == null || player == null) return;

        // hướng bắn snapshot thời điểm bắn
        Vector2 dir = (player.transform.position - firePos.position).normalized;

        var go = Instantiate(bulletPrefab, firePos.position, Quaternion.identity);
        var bullet = go.GetComponent<Flight_Bullet>();
        if (bullet != null)
        {
            bullet.Init(dir, bulletSpeed, bulletDamage, bulletLifetime);
        }
        SoundFXManager.Instance.PlayRandomSoundFXClip(flightBulletClip, transform, 0.5f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    private void OnAnimationDeathEnd()
    {
        Destroy(gameObject);
    }
}
