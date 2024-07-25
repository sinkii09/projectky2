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

    private void Awake()
    {
        foreach (Transform child in holder)
        {
            child.gameObject.SetActive(false);
        }
    }
    private void Start()
    {
        InventoryManager.OnEquipItemSuccess += InventoryManager_OnEquipItemSuccess;
    }
    private void OnDestroy()
    {
        InventoryManager.OnEquipItemSuccess -= InventoryManager_OnEquipItemSuccess;
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
                ShowHat(item.name);
            }
        }
    }
    private void InventoryManager_OnEquipItemSuccess(InventoryItem item)
    {
        Debug.Log($"show hat: {item.itemDetails.name}");
        ShowHat(item.itemDetails.name);
    }
    void FindEquippedHatItem()
    {
        var hatList = InventoryManager.Instance.GetItemsByCategory("hat");
        foreach (InventoryItem item in hatList)
        {
            if(item.equipped)
            {
                ShowHat(item.itemDetails.name);
            }
        }
    }
    public void ShowHat(string hatName)
    {
        if (holder == null) return;
        foreach(Transform child in holder)
        {
            child.gameObject.SetActive(false);
        }
        switch (hatName)
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
                Debug.Log($"Unavailable Hat name: {hatName}");
                break;
        }
    }
}
