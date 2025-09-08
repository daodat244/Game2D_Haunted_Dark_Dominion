using UnityEngine;

public enum ItemType
{
    Gem,
    HealthPotion,
    SpeedPotion,
    DamagePotion,
    InfiniteAmmo
}

public class ItemEffect : MonoBehaviour
{
    [Header("Loại vật phẩm")]
    public ItemType itemType;

    [Header("Giá trị hiệu ứng")]
    public int gemValue = 1;          // nếu là gem

    [Header("Heal")]
    public float healAmount = 20f;    // nếu là potion hồi máu

    [Header("Speed Potion")]
    public float speedAmount = 2f;    // nếu là speed potion
    public float speedDuration = 2f;  // thời gian buff speed

    [Header("Damage Potion")]
    public float damageMultiplier = 2f;   // tăng damage gấp 2
    public float damageDuration = 5f;     // thời gian buff

    [Header("Infinite Ammo Potion")]
    public float infiniteAmmoDuration = 8f; // thời gian vô hạn đạn

    [Header("SFX")]
    public AudioClip lootClip;

    private void Start()
    {
        // Nếu sau 3 giây không nhặt thì biến mất
        Destroy(gameObject, 5f);
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        Player player = other.GetComponent<Player>();
        if (player != null)
        {
            ApplyEffect(player);
            SoundFXManager.Instance.PlaySoundFXClip(lootClip, transform, 0.5f);
            Destroy(gameObject); // xoá item sau khi nhặt
        }
    }

    private void ApplyEffect(Player player)
    {
        switch (itemType)
        {
            case ItemType.Gem:
                player.AddGems(gemValue);
                break;
            case ItemType.HealthPotion:
                player.Heal(healAmount);
                break;
            case ItemType.SpeedPotion:
                player.AddSpeed(speedAmount, speedDuration);
                break;
            case ItemType.DamagePotion:
                player.AddDamageMultiplier(damageMultiplier, damageDuration);
                break;
            case ItemType.InfiniteAmmo:
                player.AddInfiniteAmmo(infiniteAmmoDuration);
                break;
        }
    }
}
