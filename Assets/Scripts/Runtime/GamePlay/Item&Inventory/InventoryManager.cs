using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class ItemDetails
{
    public string _id;
    public string name;
    public string description;
    public int price;
    public string category;
}
[System.Serializable]
public class InventoryItem
{
    public string itemId;
    public int quantity;
    public string purchaseDate;
    public ItemDetails itemDetails;
}   
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    public List<InventoryItem> localInventory = new List<InventoryItem>();
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    void FetchInventorySuccess(List<InventoryItem> list)
    {
        localInventory = list;
    }

    public List<InventoryItem> GetItemsByCategory(string category)
    {
        return localInventory.Where(item => item.itemDetails.category == category).ToList();
    }
}
