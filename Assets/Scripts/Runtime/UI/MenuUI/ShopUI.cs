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
        RefreshUIItem();
    }
    private void OnDisable()
    {
        RefreshUIItem();
    }
    private void Update()
    {
        balanceTxt.text = ShopManager.Instance.userBalance.ToString();
    }
    private void ShopManager_OnFetchShopComplete()
    {
        RefreshUIItem();
        foreach (var item in ShopManager.Instance.ShopItems)
        {
            var shopItemUI = Instantiate(shopItemUIPrefab, contentHolder);
            shopItemUI.Initialize(item._id, item.name, item.unique, item.price, item.icon);
            shopItems.Add(shopItemUI);
        }
    }
    void RefreshUIItem()
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
    void DisableShop()
    {
        gameObject.SetActive(false);
    }
}
