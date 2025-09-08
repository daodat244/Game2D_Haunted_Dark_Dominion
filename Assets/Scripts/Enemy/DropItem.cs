using UnityEngine;

[System.Serializable]
public class DropItem
{
    public GameObject itemPrefab;   // Prefab vật phẩm
    [Range(0f, 1f)] 
    public float dropChance = 0.5f; // Xác suất rơi
    public int minAmount = 1;       // Số lượng tối thiểu
    public int maxAmount = 1;       // Số lượng tối đa
}
