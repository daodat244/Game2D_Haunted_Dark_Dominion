using UnityEngine;

public class Fire_Ball_Prefabs : MonoBehaviour
{
    [Header("Fire Ball Settings")]
    [SerializeField] private float moveSpeed = 16f;
    [SerializeField] private float damage = 30f;
    [SerializeField] private float explosionRadius = 2f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private Transform explosionEffectPosition; // Transform vị trí nổ

    [Header("SFX")]
    [SerializeField] private AudioClip explosionClip; // Transform vị trí nổ
    private bool hasExploded = false;
    private Vector3 moveDirection;
    private Vector3 targetPosition;
    private bool hasTarget = false;
    private Transform playerTarget; // Lưu reference đến player để theo dõi

    void Start()
    {
        // Tự hủy sau lifetime
        Destroy(gameObject, lifetime);

        // Set rotation z = -90
        transform.rotation = Quaternion.Euler(0, 0, -90);

        // Mặc định rơi thẳng xuống dưới
        moveDirection = Vector3.down;
    }

    void Update()
    {
        if (hasExploded) return;

        // Cập nhật hướng di chuyển nếu có player target (để theo dõi player di chuyển)
        if (hasTarget && playerTarget != null)
        {
            Vector3 directionToTarget = playerTarget.position - transform.position;
            moveDirection = directionToTarget.normalized;
        }

        // Di chuyển theo hướng đã tính toán
        Vector3 newPosition = transform.position + moveDirection * moveSpeed * Time.deltaTime;
        transform.position = newPosition;

        // Kiểm tra va chạm với player
        CheckCollision();
    }

    private void CheckCollision()
    {
        // Kiểm tra va chạm với player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
            if (distanceToPlayer <= explosionRadius)
            {
                Explode();
                return;
            }
        }

        // Nổ khi đến gần vị trí đích (cho cầu lửa có target)
        if (hasTarget)
        {
            float distanceToTarget = Vector2.Distance(transform.position, targetPosition);
            if (distanceToTarget <= 1f) // Nổ khi đến gần vị trí đích
            {
                Explode();
                return;
            }
        }
    }

    private void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        SoundFXManager.Instance.PlaySoundFXClip(explosionClip, transform, 0.8f);
        // Tạo hiệu ứng nổ (chỉ 1 lần)
        if (explosionEffect != null)
        {
            Vector3 explosionPos = explosionEffectPosition != null ? explosionEffectPosition.position : transform.position;
            GameObject explosion = Instantiate(explosionEffect, explosionPos, Quaternion.identity);
            // Đảm bảo hiệu ứng nổ tự hủy sau một thời gian để tránh rác
            Destroy(explosion, 0.767f);
        }

        // Gây damage cho player nếu trong tầm nổ
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
            if (distanceToPlayer <= explosionRadius)
            {
                // Tìm component Player hoặc script xử lý damage
                Player playerScript = player.GetComponent<Player>();
                if (playerScript != null)
                {
                    playerScript.TakeDamage(damage);
                }
                else
                {
                    // Fallback: tìm script có method TakeDamage
                    var damageable = player.GetComponent<MonoBehaviour>();
                    if (damageable != null)
                    {
                        var takeDamageMethod = damageable.GetType().GetMethod("TakeDamage");
                        if (takeDamageMethod != null)
                        {
                            takeDamageMethod.Invoke(damageable, new object[] { damage });
                        }
                    }
                }
            }
        }

        // Hủy object
        Destroy(gameObject);
    }

    // Được gọi từ bên ngoài để set hướng bay đến player
    public void SetTarget(Transform newPlayerTarget)
    {
        if (newPlayerTarget != null)
        {
            playerTarget = newPlayerTarget; // Lưu reference để theo dõi
            targetPosition = newPlayerTarget.position;
            hasTarget = true;
            Vector3 directionToTarget = newPlayerTarget.position - transform.position;
            moveDirection = directionToTarget.normalized;
        }
    }

    // Được gọi từ bên ngoài để set vị trí đích và rơi xuống đó
    public void SetTargetPosition(Vector3 targetPos)
    {
        targetPosition = targetPos;
        hasTarget = true;
        playerTarget = null; // Không theo dõi player, chỉ bay đến vị trí cố định
        Vector3 directionToTarget = targetPos - transform.position;
        moveDirection = directionToTarget.normalized;
    }

    // Được gọi từ bên ngoài để set hướng random (không nhắm vào player)
    public void SetRandomDirection()
    {
        // Tạo hướng random chéo xuống dưới
        float randomAngle = Random.Range(-45f, 45f); // Góc từ -45 đến +45 độ
        float radians = randomAngle * Mathf.Deg2Rad;

        // Hướng chéo xuống dưới với góc random
        moveDirection = new Vector3(Mathf.Sin(radians), -1f, 0).normalized;
        hasTarget = false;
    }

    // Được gọi từ bên ngoài để set hướng rơi thẳng xuống dưới
    public void SetStraightDown()
    {
        // Rơi thẳng xuống dưới
        moveDirection = Vector3.down;
        hasTarget = false;
    }
}
