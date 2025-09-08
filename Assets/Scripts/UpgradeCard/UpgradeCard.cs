using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeCard : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI cardTitle;
    [SerializeField] private TextMeshProUGUI descCard;
    [SerializeField] private GameObject outline; // viền highlight
    [SerializeField] private Image iconImage;    // ảnh upgrade

    private Action<UpgradeCard> onClickCallback;

    public string UpgradeName { get; private set; }
    public string Description { get; private set; }
    public UpgradeSO upgradeSO; // gán trong prefab khi spawn

    public void Setup(UpgradeSO upgrade, Action<UpgradeCard> onClick)
    {
        upgradeSO = upgrade;

        // Gán text
        cardTitle.text = upgrade.upgradeName;
        descCard.text = upgrade.description;

        // Gán icon
        if (iconImage != null && upgrade.icon != null)
            iconImage.sprite = upgrade.icon;

        onClickCallback = onClick;

        if (outline != null)
            outline.SetActive(false);
    }

    public void OnClickCard()
    {
        Debug.Log("Card clicked: " + upgradeSO.upgradeName);
        onClickCallback?.Invoke(this);
    }

    public void SetSelected(bool selected)
    {
        if (outline != null)
            outline.SetActive(selected);
    }
}
