using UnityEngine;
using System.Collections; 

public class BE_Skeleton : Enemy
{
    private Coroutine damageCoroutine; // Coroutine để áp dụng damage
    [SerializeField] private AudioClip skeletonDamageClip;
    [SerializeField] private AudioClip[] skeletonDamageClipRandom;

    protected override void Start()
    {
        base.Start();
        damageSoundClip = skeletonDamageClip;
        damageSoundClipRandom = skeletonDamageClipRandom;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (player != null)
            {
                // Gây damage ngay khi Player vào vùng va chạm
                player.TakeDamage(enterDamage);

                // Bắt đầu coroutine để gây damage liên tục
                if (damageCoroutine == null)
                {
                    damageCoroutine = StartCoroutine(ApplyDamageOverTime());
                }
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        // Không cần xử lý ở đây nữa, vì damage đã được xử lý trong Coroutine
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Dừng coroutine khi Player rời khỏi vùng va chạm
            if (damageCoroutine != null)
            {
                StopCoroutine(damageCoroutine);
                damageCoroutine = null;
            }
        }
    }

    // Coroutine gây damage liên tục
    private IEnumerator ApplyDamageOverTime()
    {
        while (true)  // Lặp lại liên tục
        {
            if (player != null)
            {
                player.TakeDamage(stayDamage); // Gây sát thương mỗi lần
            }
            yield return new WaitForSeconds(1f); // Đợi 1 giây (hoặc thời gian tùy chỉnh)
        }
    }

    public void OnDeathAnimationEnd()
    {
        Destroy(gameObject);
    }
}
