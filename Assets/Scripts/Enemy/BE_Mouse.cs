using UnityEngine;

public class BE_Mouse : Enemy
{
    [SerializeField] private float extraHealth = 10f;  // Máu cộng thêm cho Explosion Enemy
    [SerializeField] private float customMoveSpeed = 5f;  // Tốc độ di chuyển riêng cho BE_Explosion
    [SerializeField] private float extraDamage = 5f;
    [SerializeField] private AudioClip[] mouseDamageClipRandom;
    protected override void Start()
    {
        base.Start();
        // Cộng thêm tốc độ vào base move speed của Enemy
        enemyMoveSpeed += customMoveSpeed;  // Cộng speed vào base move speed
        damageSoundClipRandom = mouseDamageClipRandom;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (player != null)
            {
                // Gây damage ngay khi Player vào vùng va chạm
                player.TakeDamage(enterDamage + extraDamage);
                Destroy(gameObject);
                waweManager.EnemyKilled();
            }
        }
    }

    protected override void Death()
    {
        base.Death();
    }

    public void OnDeathAnimationEnd()
    {
        Destroy(gameObject);
    }
}
