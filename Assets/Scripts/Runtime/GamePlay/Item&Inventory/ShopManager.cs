
using System;
using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;

    public List<ShopItem> ShopItems = new List<ShopItem>();

    public static event Action OnFetchShopComplete;

    public int userBalance;
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
    public void FetchShopItem()
    {
        if(UserManager.Instance != null)
        {
            UserManager.Instance.FetchShopItems(FetchItemSuccess,FetchItemFailed);
        }
    }
    void FetchItemSuccess(ShopItemsList array)
    {
        ShopItems.Clear();
        foreach (ShopItem item in array.items)
        {
            ShopItems.Add(item);
        }
        OnFetchShopComplete?.Invoke();
    }
    void FetchItemFailed(string message)
    {
        Debug.LogWarning(message);
    }

    internal void UpdateBalance(int balance)
    {
        userBalance += balance;
    }
}
[System.Serializable]
public class ShopItem
{
    public string _id;
    public string name;
    public string description;
    public string icon;
    public int price;
    public bool unique;
    public string category;
    public int __v;
}

[System.Serializable]
public class ShopItemsList
{
    public ShopItem[] items;
}
