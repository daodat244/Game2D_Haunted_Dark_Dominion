using UnityEngine;

public class ExplosionFX : MonoBehaviour
{
    [SerializeField] private float damage = 30f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Player player = collision.GetComponent<Player>();
            if (player != null) player.TakeDamage(damage);
        }
        else if (collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null) enemy.TakeDamage(damage);
        }
    }

    public void DestroyExplosion()
    {
        Destroy(gameObject);  // Hủy explosion sau khi nó hoàn thành
    }
}
