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
    public bool equipped;
    public int quantity;
    public string purchaseDate;

}
[System.Serializable]
public class EquippedItem
{
    public string itemId;
    public string name;
    public string category;
    public int quantity;
    public string purchaseDate;
}

[System.Serializable]
public class UserEquippedItems
{
    public string userId;
    public EquippedItem[] equippedItems;
}

[System.Serializable]
public class UserEquippedItemsList
{
    public UserEquippedItems[] users;
}
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    public List<InventoryItem> localInventory = new List<InventoryItem>();

    public static event Action OnFetchInventorySuccess;
    public static event Action<InventoryItem> OnEquipNewItem;
    public static event Action<InventoryItem> OnUnequipItem;
    public static event Action OnSendEquipmentRequestDone;
    public Dictionary<ulong, string> matchplayClient = new Dictionary<ulong, string>();
    public Dictionary<ulong, EquippedItem[]> clientEquipment = new Dictionary<ulong, EquippedItem[]>();
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
    public void Dispose()
    {
        localInventory.Clear();
        matchplayClient.Clear();
        clientEquipment.Clear();
    }
    #region Fetch Inventory
    public void FetchInventory()
    {
        UserManager.Instance.FetchUserInventory(FetchInventorySuccess,FetchInventoryFailed);
    }
    void FetchInventorySuccess(List<InventoryItem> list)
    {
        localInventory.Clear();
        localInventory = list;
        OnFetchInventorySuccess?.Invoke();
    }
    void FetchInventoryFailed(string message)
    {
        Debug.Log(message);
    }
    #endregion
    public void AddAllPlayer()
    {
        Debug.Log("start fetch client inventory");
        List<string> userIdsList = new List<string>();
        var networkPlayers = FindObjectsOfType<NetworkPlayer>();
        if(networkPlayers != null)
        {
            foreach(NetworkPlayer player in networkPlayers)
            {
                matchplayClient[player.OwnerClientId] = player.UserId.Value;
                userIdsList.Add(player.UserId.Value);
            }
        }
        string[] userIdsArray = userIdsList.ToArray();
        UserManager.Instance.GetPlayersEquippedItems(userIdsArray, FetchOtherEquipmentSuccess, FetchOtherEquipmentFailed);
    }
    void FetchOtherEquipmentSuccess(UserEquippedItemsList list)
    {
        foreach (var user in list.users)
        {
            foreach (var client in matchplayClient.Keys)
            {
                if (user.userId == matchplayClient[client])
                {
                    clientEquipment[client] = user.equippedItems;
                    break;
                }
            }
        }
    }
    void FetchOtherEquipmentFailed(string message)
    {
        Debug.Log(message);
    }
    public void ChangeEquipItem(InventoryItem item)
    {
        if (item == null) return;
        var categorylist = GetItemsByCategory(item.itemDetails.category);
        var idx = 0;
        if (categorylist.Count > 0)
        {
            for (int i = 0; i < categorylist.Count; i++)
            {
                if (categorylist[i].itemDetails._id == item.itemDetails._id)
                {
                    if (i + 1 < categorylist.Count)
                    {
                        idx = i + 1;
                    }
                    else
                    {
                        idx = 0;
                    }
                    EquipItem(categorylist[idx]);
                    break;
                }
            }
        }
    }
    public EquippedItem GetClientItem(ulong clientId,string category = "")
    {
        if(clientEquipment.ContainsKey(clientId) && clientEquipment[clientId] != null)
        {
            if(clientEquipment[clientId].Where(item => item.category == category).Count() > 0)
            {
                return clientEquipment[clientId].Where(item => item.category == category).First();
            }
        }
        Debug.Log($"can't find any item with {category} of client: {clientId}");
        return null;
    }
    public void EquipItem(InventoryItem item)
    {
        Debug.Log(item.itemDetails.name);
        var categorylist = GetItemsByCategory(item.itemDetails.category);
        if (categorylist != null && categorylist.Count >0)
        {
            foreach (var categoryItem in categorylist)
            {
                if (categoryItem.equipped)
                {
                    UnequipItem(categoryItem);
                }
            }
        }
        item.equipped = true;
        OnEquipNewItem?.Invoke(item);
    }
    public void UnequipItem(InventoryItem item)
    {
        Debug.Log( $"unequip item: {item.itemDetails.name}");
        item.equipped = false;
        OnUnequipItem?.Invoke(item);
    }
    public void SendEquipItemRequest(InventoryItem item)
    {
        if(item!= null)
        {
            UserManager.Instance.EquipItemRequest(item.itemDetails._id, EquipItemSuccess, EquipItemFailed);
        }
        else
        {
            UserManager.Instance.EquipItemRequest("0", EquipItemSuccess, EquipItemFailed);
        }
    }
    void EquipItemSuccess(InventoryItem item)
    {
        FetchInventory();
        OnSendEquipmentRequestDone?.Invoke();
    }
    void EquipItemFailed(string message)
    {
        OnSendEquipmentRequestDone?.Invoke();
        Debug.Log(message);
    }
    public List<InventoryItem> GetItemsByCategory(string category)
    {
        return localInventory.Where(item => item.itemDetails.category == category).ToList();
    }
}
