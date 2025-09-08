using UnityEngine;

public class DestroyEffect : MonoBehaviour
{
    [Header("Effect Settings")]
    [SerializeField] private float effectDuration = 1f; // Thời gian effect tồn tại
    [SerializeField] private bool autoDestroy = true; // Tự động hủy effect
    
    private void Start()
    {
        // Tự động hủy effect sau khi hoàn thành
        if (autoDestroy)
        {
            Destroy(gameObject, effectDuration);
        }
    }
    
    // Phương thức để set duration từ bên ngoài
    public void SetDuration(float duration)
    {
        effectDuration = duration;
        if (autoDestroy)
        {
            Destroy(gameObject, effectDuration);
        }
    }
    
    // Phương thức để hủy effect ngay lập tức
    public void DestroyEffectImmediately()
    {
        Destroy(gameObject);
    }
}
