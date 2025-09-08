using UnityEngine;

/*
 * PROJECTILE SETUP GUIDE
 * 
 * Để projectile hoạt động đúng, hãy đảm bảo:
 * 
 * 1. PROJECTILE PREFAB:
 *    - Có Collider2D với "Is Trigger" = true
 *    - Có Rigidbody2D với "Body Type" = Kinematic
 *    - Có script Golem_Projectile
 * 
 * 2. WALL/OBSTACLE:
 *    - Có Collider2D (không cần Is Trigger)
 *    - Có tag "Wall" hoặc "Obstacle"
 *    - Hoặc nằm trên layer Default/Ground/Environment
 * 
 * 3. DESTROY EFFECT:
 *    - Được gán vào field destroyEffect trong Inspector
 *    - Có script DestroyEffect (sẽ tự động thêm nếu chưa có)
 * 
 * 4. DEBUG:
 *    - Kiểm tra Console để xem log messages
 *    - Nếu không thấy log, có thể collider không được setup đúng
 */

public class ProjectileSetupGuide : MonoBehaviour
{
    [Header("Setup Check")]
    [SerializeField] private bool checkProjectileSetup = true;
    
    void Start()
    {
        if (checkProjectileSetup)
        {
            CheckProjectileSetup();
        }
    }
    
    private void CheckProjectileSetup()
    {
        // Kiểm tra projectile
        Collider2D projectileCollider = GetComponent<Collider2D>();
        Rigidbody2D projectileRigidbody = GetComponent<Rigidbody2D>();
        Golem_Projectile projectileScript = GetComponent<Golem_Projectile>();
        
        if (projectileCollider == null)
        {
            Debug.LogError("Projectile missing Collider2D!");
        }
        else if (!projectileCollider.isTrigger)
        {
            Debug.LogWarning("Projectile Collider2D should have 'Is Trigger' = true!");
        }
        
        if (projectileRigidbody == null)
        {
            Debug.LogError("Projectile missing Rigidbody2D!");
        }
        else if (projectileRigidbody.bodyType != RigidbodyType2D.Kinematic)
        {
            Debug.LogWarning("Projectile Rigidbody2D should be 'Kinematic'!");
        }
        
        if (projectileScript == null)
        {
            Debug.LogError("Projectile missing Golem_Projectile script!");
        }
        
        // Kiểm tra destroy effect
        if (projectileScript != null)
        {
            var destroyEffectField = typeof(Golem_Projectile).GetField("destroyEffect", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (destroyEffectField != null)
            {
                GameObject destroyEffect = destroyEffectField.GetValue(projectileScript) as GameObject;
                if (destroyEffect == null)
                {
                    Debug.LogWarning("Destroy Effect not assigned in Inspector!");
                }
                else
                {
                    Debug.Log($"Destroy Effect assigned: {destroyEffect.name}");
                }
            }
        }
    }
}
