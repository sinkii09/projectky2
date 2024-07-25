using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI itemNameTxt;
    [SerializeField] TextMeshProUGUI uniqueTxt;
    [SerializeField] TextMeshProUGUI priceTxt;
    [SerializeField] Image itemImage;
    [SerializeField] Button purchaseBtn;

    string itemID;
    public void Initialize(string id, string name, bool unique, int price, string iconName)
    {
        itemNameTxt.text = name;
        uniqueTxt.text = unique ? "unique": "common";
        priceTxt.text = price.ToString();
        itemImage.sprite = LoadIcon(iconName);
        itemID = id;
        CheckAvailableItem(id);
        purchaseBtn.onClick.AddListener(() => {PurchaseItem(); AudioManager.Instance.PlaySFX("Btn_click01"); });
    }
    private void OnDestroy()
    {
        purchaseBtn.onClick.RemoveAllListeners();
    }
    private void CheckAvailableItem(string itemId)
    {
        var iventory = InventoryManager.Instance.localInventory;
        foreach (var item in iventory)
        {
            if(item.itemDetails._id == itemId)
            {
                purchaseBtn.interactable = false;
            }
        }
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
    void PurchaseItem()
    {
        purchaseBtn.interactable = false;
        UserManager.Instance.PurchaseItem(itemID,PurchaseItemSuccess,PurchaseItemFailed);
    }
    void PurchaseItemSuccess()
    {
        PopupManager.Instance.ShowPopup("buying Success!");
        InventoryManager.Instance.FetchInventory();
        purchaseBtn.interactable=true;
    }
    void PurchaseItemFailed(string message)
    {
        PopupManager.Instance.ShowPopup(message);
        purchaseBtn.interactable = true;
    }
}
