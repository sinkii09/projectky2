using System;
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
    public string icon;
    public int price;
    public bool unique;
    public string category;
}
[System.Serializable]
public class InventoryItem
{
    public ItemDetails itemDetails;
    public int quantity;
    public string purchaseDate;
}   
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    public List<InventoryItem> localInventory = new List<InventoryItem>();

    public static event Action OnFetchInventorySuccess;
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
    public void FetchInventory()
    {
        UserManager.Instance.FetchUserInventory(FetchInventorySuccess,FetchInventoryFailed);
    }
    void FetchInventorySuccess(List<InventoryItem> list)
    {
        localInventory = list;
        OnFetchInventorySuccess?.Invoke();
    }
    void FetchInventoryFailed(string message)
    {
        Debug.Log(message);
    }

    public List<InventoryItem> GetItemsByCategory(string category)
    {
        return localInventory.Where(item => item.itemDetails.category == category).ToList();
    }
}
