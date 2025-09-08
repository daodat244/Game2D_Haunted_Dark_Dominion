using UnityEngine;

public class BE_Explosion : Enemy
{
    [Header("Stats")]
    [SerializeField] private GameObject explosionPrefabs;
    [SerializeField] private float extraHealth = 10f;  // Máu cộng thêm cho Explosion Enemy
    [SerializeField] private float customMoveSpeed = 3f;  // Tốc độ di chuyển riêng cho BE_Explosion

    [Header("SFX")]
    [SerializeField] private AudioClip[] explosionDamageClipRandom;
    [SerializeField] private AudioClip[] explosionClip;

    // Override phương thức Start của lớp Enemy
    protected override void Start()
    {
        base.Start(); // Gọi phương thức Start() của lớp cha Enemy

        // Cộng thêm máu cho BE_Explosion khi khởi tạo
        maxHealth += extraHealth; // Tăng maxHealth so với base Enemy
        currentHealth = maxHealth; // Cập nhật lại currentHealth nếu cần
        UpdateHpBar();  // Cập nhật lại thanh máu

        // Sửa tốc độ di chuyển của BE_Explosion
        enemyMoveSpeed = customMoveSpeed;  // Sửa tốc độ di chuyển của BE_Explosion
        damageSoundClipRandom = explosionDamageClipRandom;
    }

    private void CreateExplosion()
    {
        if (explosionPrefabs != null)
        {
            Instantiate(explosionPrefabs, transform.position, Quaternion.identity);
            SoundFXManager.Instance.PlayRandomSoundFXClip(explosionClip, transform, 1f);
        }
    }

    protected override void Death()
    {
        waweManager.EnemyKilled();
        CreateExplosion(); // Tạo explosion khi Enemy chết
        DropOneItem();
        Destroy(gameObject); // Sau khi explosion, hủy Enemy
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            CreateExplosion(); // Tạo explosion khi chạm vào Player
            Destroy(gameObject);
            waweManager.EnemyKilled();
        }
    }
}
