using UnityEngine;

public class Flight_Bullet : MonoBehaviour
{
    private Vector2 dir;
    private float speed;
    private float damage;
    private float life;
    private float dieAt;

    public void Init(Vector2 direction, float bulletSpeed, float bulletDamage, float lifetime)
    {
        dir    = direction.normalized;
        speed  = bulletSpeed;
        damage = bulletDamage;
        life   = lifetime;
        dieAt  = Time.time + life;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        // Nếu sprite mặc định nhìn lên: angle -= 90f;
    }

    private void Update()
    {
        transform.position += (Vector3)(dir * speed * Time.deltaTime);
        if (Time.time >= dieAt) Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var p = other.GetComponent<Player>();
            if (p) p.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}
