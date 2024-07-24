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
    public static event Action<InventoryItem> OnEquipItemSuccess;

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

    public EquippedItem GetClientItem(ulong clientId,string category = "")
    {
        if(clientEquipment.ContainsKey(clientId) && clientEquipment[clientId] != null)
        {
            return clientEquipment[clientId].Where(item => item.category == category).First();
        }
        Debug.Log($"can find any item with {category}");
        return null;
    }
    public void EquipItem(InventoryItem item)
    {
        if(item.equipped)
        {
            Debug.Log("item is already equipped");
        }
        else
        {
            EquipItemRequest(item.itemDetails._id);
        }
    }
    void EquipItemRequest(string itemId)
    {
        UserManager.Instance.EquipItemRequest(itemId, EquipItemSuccess, EquipItemFailed);
    }
    void EquipItemSuccess(InventoryItem item)
    {
        FetchInventory();
        OnEquipItemSuccess?.Invoke(item);
    }
    void EquipItemFailed(string message)
    {
        Debug.Log(message);
    }
    public List<InventoryItem> GetItemsByCategory(string category)
    {
        return localInventory.Where(item => item.itemDetails.category == category).ToList();
    }
}
