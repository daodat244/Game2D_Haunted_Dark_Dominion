using UnityEngine;

public class Laser_Cast_Prefabs : MonoBehaviour
{
    [Header("Lifetime")]
    [SerializeField] private float autoDestroyAfter = 4f;
    [SerializeField] private bool followParent = true;

    [Header("Visual")]
    [SerializeField] private GameObject[] enableOnSpawn;

    private Transform parent;

    private void OnEnable()
    {
        if (enableOnSpawn != null)
        {
            foreach (var go in enableOnSpawn)
            {
                if (go != null) go.SetActive(true);
            }
        }

        if (autoDestroyAfter > 0f)
        {
            Destroy(gameObject, autoDestroyAfter);
        }

        parent = transform.parent;
    }

    private void LateUpdate()
    {
        if (followParent && parent != null)
        {
            // Giữ dính theo parent vị trí/rotation (đề phòng animator thay đổi)
            transform.position = parent.position;
            transform.rotation = parent.rotation;
        }
    }
}
