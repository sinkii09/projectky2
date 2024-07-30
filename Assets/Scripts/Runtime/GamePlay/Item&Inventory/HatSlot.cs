using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class HatSlot : MonoBehaviour
{
    [SerializeField] Transform holder;

    [SerializeField] GameObject hat_Crow_Visual;
    [SerializeField] GameObject hat_Gentleman_Visual;
    [SerializeField] GameObject hat_Winter_Visual;
    [SerializeField] GameObject hat_Viking_Visual;
    [SerializeField] GameObject hat_Wizard_Visual;

    public ulong owner { get; set; }

    string currentHat;
    public InventoryItem currentItem { get; private set; }

    private void Awake()
    {
        foreach (Transform child in holder)
        {
            child.gameObject.SetActive(false);
        }
    }
    private void Start()
    {
        InventoryManager.OnFetchInventorySuccess += InventoryManager_OnFetchInventorySuccess;
        InventoryManager.OnEquipNewItem += InventoryManager_OnEquipNewItem;
    }

    private void OnDestroy()
    {
        InventoryManager.OnEquipNewItem -= InventoryManager_OnEquipNewItem;
        InventoryManager.OnFetchInventorySuccess -= InventoryManager_OnFetchInventorySuccess;
    }
    public void Initialize(ulong clientId = 0)
    {
        owner = clientId;
        if(owner == 0)
        {
            FindEquippedHatItem();
        }
        else
        {
            var item = InventoryManager.Instance.GetClientItem(owner,"hat");
            if(item != null)
            {
                ShowHatByName(item.name);
            }
        }
    }
    private void InventoryManager_OnEquipNewItem(InventoryItem obj)
    {
        ShowHat(obj);
    }

    private void InventoryManager_OnFetchInventorySuccess()
    {
        FindEquippedHatItem();
    }

    void FindEquippedHatItem()
    {
        var hatList = InventoryManager.Instance.GetItemsByCategory("hat");
        foreach (InventoryItem item in hatList)
        {
            if(item.equipped)
            {
                ShowHat(item);
            }
        }
    }
    public void ShowHat(InventoryItem item)
    {
        if (item == currentItem) return;
        if (holder == null) return;
        currentItem = item;
        foreach (Transform child in holder)
        {
            child.gameObject.SetActive(false);
        }
        switch (item.itemDetails.name)
        {
            case "Crown Hat":
                hat_Crow_Visual.SetActive(true);
                break;
            case "Top Hat":
                hat_Gentleman_Visual.SetActive(true);
                break;
            case "Winter Hat":
                hat_Winter_Visual.SetActive(true);
                break;
            case "Viking Hat":
                hat_Viking_Visual.SetActive(true);
                break;
            case "Wizard Hat":
                hat_Wizard_Visual.SetActive(true);
                break;
            default:
                Debug.Log($"Unavailable Hat name: {item.itemDetails.name}");
                break;
        }
    }
    public void ShowHatByName(string itemName)
    {
        if (holder == null) return;
        foreach (Transform child in holder)
        {
            child.gameObject.SetActive(false);
        }
        switch (itemName)
        {
            case "Crown Hat":
                hat_Crow_Visual.SetActive(true);
                break;
            case "Top Hat":
                hat_Gentleman_Visual.SetActive(true);
                break;
            case "Winter Hat":
                hat_Winter_Visual.SetActive(true);
                break;
            case "Viking Hat":
                hat_Viking_Visual.SetActive(true);
                break;
            case "Wizard Hat":
                hat_Wizard_Visual.SetActive(true);
                break;
            default:
                Debug.Log($"Unavailable Hat name: {itemName}");
                break;
        }
    }
}
