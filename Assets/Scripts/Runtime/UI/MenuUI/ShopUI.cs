using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    [SerializeField] ShopItemUI shopItemUIPrefab;
    [SerializeField] Transform contentHolder;
    [SerializeField] Button returnButton;
    [SerializeField] TextMeshProUGUI balanceTxt;
    List<ShopItemUI> shopItems = new List<ShopItemUI>();
    private void Start()
    {
        ShopManager.OnFetchShopComplete += ShopManager_OnFetchShopComplete;
        returnButton.onClick.AddListener(() => { DisableShop(); AudioManager.Instance.PlaySFX("Btn_click01"); });
    }
    private void OnDestroy()
    {
        ShopManager.OnFetchShopComplete -= ShopManager_OnFetchShopComplete;
        returnButton?.onClick.RemoveAllListeners();
    }
    private void OnEnable()
    {
        if(shopItems.Count > 0)
        {
            foreach (ShopItemUI item in shopItems)
            {
                Destroy(item.gameObject);
            }
            shopItems.Clear();
        }
    }
    private void OnDisable()
    {
        if (shopItems.Count > 0)
        {
            foreach (ShopItemUI item in shopItems)
            {
                Destroy(item.gameObject);
            }
            shopItems.Clear();
        }
    }
    private void Update()
    {
        balanceTxt.text = ClientSingleton.Instance.Manager.User.Data.playerGold.ToString();
    }
    private void ShopManager_OnFetchShopComplete()
    {
        shopItems.Clear();
        foreach (var item in ShopManager.Instance.ShopItems)
        {
            var shopItemUI = Instantiate(shopItemUIPrefab, contentHolder);
            shopItemUI.Initialize(item._id, item.name, item.unique, item.price, item.icon);
            shopItems.Add(shopItemUI);
        }
    }
    void DisableShop()
    {
        gameObject.SetActive(false);
    }
}
