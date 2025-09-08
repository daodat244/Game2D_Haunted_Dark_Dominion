using UnityEngine;

public class BossSpell : MonoBehaviour
{   
    [Header ("Spell")]
    [SerializeField] private int spellDamage = 20; // Sát thương của spell
    [SerializeField] private float spellDuration = 1.3f; // Thời gian tồn tại spell
    private float timer = 0f;

    void Start()
    {
        // Nếu cần, có thể khởi tạo thêm gì đó ở đây
    }

    void Update()
    {
        // Tăng thời gian của spell
        timer += Time.deltaTime;
        
        // Kiểm tra nếu thời gian spell đã hết
        if (timer >= spellDuration)
        {
            Destroy(gameObject); // Hủy spell khi hết thời gian
        }
    }

    // Khi spell va chạm với vật thể khác
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Kiểm tra xem vật thể va chạm có phải là Player không
        if (collision != null && collision.CompareTag("Player"))
        {
            // Gọi hàm nhận sát thương từ Player
            Player player = collision.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage(spellDamage); // Gọi TakeDamage() từ Player.cs
            }

            // Hủy spell ngay sau khi gây sát thương
            Destroy(gameObject);
        }
    }
}
