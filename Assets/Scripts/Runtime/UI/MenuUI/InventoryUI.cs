using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [SerializeField]
    InventoryItemUI itemPrefab;
    [SerializeField]
    Transform itemHolder;
    [SerializeField]
    Button backButton;

    List<InventoryItemUI> inventoryItems = new List<InventoryItemUI>();
    private void Start()
    {
        backButton.onClick.AddListener(() => { Hide(); AudioManager.Instance.PlaySFX("Btn_click01"); });
        Hide();
    }
    private void OnDestroy()
    {
        backButton.onClick.RemoveAllListeners();
    }
    public void Show()
    {
        if(inventoryItems.Count > 0)
        {
            foreach(InventoryItemUI item in inventoryItems)
            {
                Destroy(item.gameObject); 
            }
            inventoryItems.Clear();
        }
        foreach(InventoryItem item in InventoryManager.Instance.localInventory)
        {
            var itemUI = Instantiate(itemPrefab,itemHolder);
            itemUI.Initialize(item);
            inventoryItems.Add(itemUI);
        }
        gameObject.SetActive(true);
    }
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
