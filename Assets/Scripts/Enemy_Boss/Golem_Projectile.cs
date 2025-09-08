using UnityEngine;

public class Golem_Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float projectileDamage = 35f;
    [SerializeField] private float projectileSpeed = 8f;
    [SerializeField] private float projectileDuration = 1.5f;
    [SerializeField] private float projectileRange = 15f; // Khoảng cách tối đa projectile có thể bay
    
    [Header("Effects")]
    [SerializeField] private GameObject hitEffect; // Effect khi trúng target
    [SerializeField] private GameObject destroyEffect; // Effect khi tự hủy
    [SerializeField] protected Transform destroyEffectPosition; // Vị trí để spawn destroy effect
    
    private Transform target;
    private Vector3 direction;
    private float distanceTraveled = 0f;
    private Vector3 startPosition;
    private bool hasHit = false;
    
    void Start()
    {
        // Lưu vị trí bắt đầu
        startPosition = transform.position;
        
        // Tìm player làm target
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;
            // Tính hướng bay
            direction = (target.position - transform.position).normalized;
        }
        else
        {
            // Nếu không tìm thấy player, bay theo hướng mặc định
            direction = transform.right;
        }
        
        // Flip projectile theo hướng bay
        FlipProjectile();
        
        // Tự hủy sau thời gian duration
        Destroy(gameObject, projectileDuration);
    }

    void Update()
    {
        if (hasHit) return;
        
        // Di chuyển projectile
        transform.Translate(direction * projectileSpeed * Time.deltaTime, Space.World);
        
        // Tính khoảng cách đã bay
        distanceTraveled = Vector3.Distance(startPosition, transform.position);
        
        // Tự hủy nếu bay quá xa
        if (distanceTraveled >= projectileRange)
        {
            DestroyProjectile();
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Projectile hit: {other.name} with tag: {other.tag}");
        
        // Kiểm tra xem có phải player không
        if (other.CompareTag("Player"))
        {
            // Gây damage cho player
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage(projectileDamage);
                Debug.Log($"Golem Projectile hit player for {projectileDamage} damage!");
            }
            
            // Tạo hit effect
            if (hitEffect != null)
            {
                Instantiate(hitEffect, transform.position, Quaternion.identity);
            }
            
            // Đánh dấu đã trúng
            hasHit = true;
            
            // Hủy projectile
            DestroyProjectile();
        }
        // Kiểm tra va chạm với wall/obstacle
        else if (other.CompareTag("Wall"))
        {
            Debug.Log($"Projectile hit wall/obstacle: {other.name}");
            
            // Tạo destroy effect trước khi hủy projectile
            if (destroyEffect != null)
            {
                Vector3 effectPosition = destroyEffectPosition != null ? destroyEffectPosition.position : transform.position;
                GameObject effect = Instantiate(destroyEffect, effectPosition, Quaternion.identity);
                // Đảm bảo effect có script DestroyEffect
                if (effect.GetComponent<DestroyEffect>() == null)
                {
                    effect.AddComponent<DestroyEffect>();
                }
                Debug.Log("Destroy effect created!");
            }
            else
            {
                Debug.LogWarning("Destroy effect is null!");
            }
            
            // Hủy projectile
            DestroyProjectile();
        }
    }
    
    private void DestroyProjectile()
    {
        Debug.Log("DestroyProjectile called");
        
        // Tạo destroy effect nếu có (luôn tạo khi không phải hit player)
        if (destroyEffect != null && !hasHit)
        {
            Vector3 effectPosition = destroyEffectPosition != null ? destroyEffectPosition.position : transform.position;
            GameObject effect = Instantiate(destroyEffect, effectPosition, Quaternion.identity);
            // Đảm bảo effect có script DestroyEffect
            if (effect.GetComponent<DestroyEffect>() == null)
            {
                effect.AddComponent<DestroyEffect>();
            }
            Debug.Log("Destroy effect created in DestroyProjectile!");
        }
        else if (destroyEffect == null)
        {
            Debug.LogWarning("Destroy effect is null in DestroyProjectile!");
        }
        
        // Hủy projectile
        Destroy(gameObject);
    }
    
    // Backup collision detection nếu OnTriggerEnter2D không hoạt động
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"Projectile collision with: {collision.gameObject.name} on layer: {collision.gameObject.layer}");
        
        // Tạo destroy effect cho mọi va chạm
        if (destroyEffect != null)
        {
            Vector3 effectPosition = destroyEffectPosition != null ? destroyEffectPosition.position : transform.position;
            GameObject effect = Instantiate(destroyEffect, effectPosition, Quaternion.identity);
            if (effect.GetComponent<DestroyEffect>() == null)
            {
                effect.AddComponent<DestroyEffect>();
            }
            Debug.Log("Destroy effect created via collision!");
        }
        
        DestroyProjectile();
    }
    
    // Phương thức để set target từ bên ngoài (nếu cần)
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target != null)
        {
            direction = (target.position - transform.position).normalized;
            FlipProjectile();
        }
    }
    
    // Phương thức để set direction từ bên ngoài
    public void SetDirection(Vector3 newDirection)
    {
        direction = newDirection.normalized;
        FlipProjectile();
    }
    
    // Phương thức để set damage từ bên ngoài
    public void SetDamage(float newDamage)
    {
        projectileDamage = newDamage;
    }
    
    // Phương thức để flip projectile theo hướng bay
    private void FlipProjectile()
    {
        if (direction.x < 0)
        {
            // Bay sang trái - flip projectile
            transform.localScale = new Vector3(-1.3f, 1.3f, 1.3f);
        }
        else
        {
            // Bay sang phải - giữ nguyên
            transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
        }
    }
}
