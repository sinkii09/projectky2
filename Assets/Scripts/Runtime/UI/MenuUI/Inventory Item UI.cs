using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI itemName;
    [SerializeField] private TextMeshProUGUI itemUnique;
    [SerializeField] private Image itemImage;
    [SerializeField] private Button equipButton;
    [SerializeField] private TextMeshProUGUI equipText;

    InventoryItem item;
    public void Initialize(InventoryItem item)
    {
        this.item = item;
        itemName.text = item.itemDetails.name;
        itemUnique.text = item.itemDetails.unique ? "unique" : "common"; ;
        itemImage.sprite = LoadIcon(item.itemDetails.icon);
        equipButton.onClick.AddListener(() => { EquipItem(item); });
        if(item.equipped)
        {
            equipText.text = "unequip";
            equipButton.image.color = Color.black;
        }
        else
        {
            equipText.text = "equip";
            equipButton.image.color = Color.yellow;
        }

        InventoryManager.OnEquipNewItem += InventoryManager_OnEquipNewItem;
        InventoryManager.OnUnequipItem += InventoryManager_OnUnequipItem;
    }

    private void OnDestroy()
    {
        equipButton.onClick.RemoveAllListeners();
        InventoryManager.OnEquipNewItem -= InventoryManager_OnEquipNewItem;
        InventoryManager.OnUnequipItem -= InventoryManager_OnUnequipItem;
    }
    private Sprite LoadIcon(string iconName)
    {
        Sprite icon = Resources.Load<Sprite>($"Icons/{iconName}");
        if (icon == null)
        {
            Debug.LogError($"Icon {iconName} not found in Resources/Icons/");
        }
        return icon;
    }
    void EquipItem(InventoryItem item)
    {
        if(item.equipped)
        {
            InventoryManager.Instance.UnequipItem(item);
        }
        else
        {
            InventoryManager.Instance.EquipItem(item);
        }
    }

    private void InventoryManager_OnUnequipItem(InventoryItem obj)
    {
        if (obj == item)
        {
            equipText.text = "equip";
            equipButton.image.color = Color.yellow;
        }
    }

    private void InventoryManager_OnEquipNewItem(InventoryItem obj)
    {
        if(obj == item)
        {
            equipText.text = "unequip";
            equipButton.image.color = Color.black;
        }
    }

}
