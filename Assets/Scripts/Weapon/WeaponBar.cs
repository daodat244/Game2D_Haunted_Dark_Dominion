using UnityEngine;
using UnityEngine.UI;

public class WeaponBar : MonoBehaviour
{
    [Header("Slot Icons (Image)")]
    [SerializeField] private Image[] slotIcons;   // gán Icon con của mỗi Slot

    [Header("Outlines (Highlight images)")]
    [SerializeField] private GameObject[] outlines;

    [Header("Weapons")]
    [SerializeField] private GameObject[] weapons; // prefab gắn trên Player
    [SerializeField] private Sprite[] weaponIcons; // icon tương ứng với weapons

    private int currentIndex = 0;

    private void Start()
    {
        // Gán icon lúc khởi tạo
        for (int i = 0; i < slotIcons.Length; i++)
        {
            if (i < weaponIcons.Length && weaponIcons[i] != null)
            {
                slotIcons[i].sprite = weaponIcons[i];
                slotIcons[i].enabled = true;
            }
            else
            {
                slotIcons[i].enabled = false;
            }
        }

        UpdateHighlight();
        EquipWeapon(0);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelectSlot(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SelectSlot(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) SelectSlot(4);
    }

    private void SelectSlot(int index)
    {
        currentIndex = index;
        UpdateHighlight();
        EquipWeapon(index);
    }

    private void UpdateHighlight()
    {
        for (int i = 0; i < outlines.Length; i++)
            outlines[i].SetActive(i == currentIndex);
    }

    private void EquipWeapon(int index)
    {
        for (int i = 0; i < weapons.Length; i++)
            if (weapons[i] != null)
                weapons[i].SetActive(i == index);
    }

    public void SetWeaponIcon(int slotIndex, Sprite icon)
    {
        if (slotIndex < 0 || slotIndex >= slotIcons.Length) return;

        var img = slotIcons[slotIndex];
        img.sprite = icon;
        img.enabled = true;

        // auto add AspectRatioFitter nếu chưa có
        var fitter = img.GetComponent<AspectRatioFitter>();
        if (fitter == null) fitter = img.gameObject.AddComponent<AspectRatioFitter>();
        fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
    }
}
