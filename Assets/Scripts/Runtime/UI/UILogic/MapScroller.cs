using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapScroller : MonoBehaviour
{
    [SerializeField] Image mapImage;
    [SerializeField] TextMeshProUGUI mapName;
    [SerializeField] Button prevBtn;
    [SerializeField] Button nextBtn;
    [SerializeField] GameObject body;

    [SerializeField] Sprite map1Sprite;
    [SerializeField] Sprite map2Sprite;

    [SerializeField] string map1Name;
    [SerializeField] string map2Name;

    Dictionary<Map, Sprite> mapSpriteDictionary;
    Dictionary<Map, string> mapNameDictionary;
    List<Map> mapList;
    int currentIndex = 0;

    private void Start()
    {
        mapSpriteDictionary = new Dictionary<Map, Sprite>
        {
            {Map.Map1, map1Sprite },
            {Map.Map2, map2Sprite },
        };
        mapNameDictionary = new Dictionary<Map, string>
        {
            {Map.Map1, map1Name },
            {Map.Map2, map2Name },
        }; 
        mapList = new List<Map>(mapSpriteDictionary.Keys);

        if(mapList.Count > 0 )
        {
            mapImage.sprite = mapSpriteDictionary[mapList[currentIndex]];
            mapName.text = mapNameDictionary[mapList[currentIndex]];
        }

        prevBtn.onClick.AddListener(() => { ShowPrevMap(); AudioManager.Instance.PlaySFXNumber(0); });
        nextBtn.onClick.AddListener(() => { ShowNextMap(); AudioManager.Instance.PlaySFXNumber(0); });

    }

    private void ShowNextMap()
    {
        if(currentIndex < mapList.Count - 1)
        {
            currentIndex++;
        }
        else
        {
            currentIndex = 0;
        }
        mapImage.sprite = mapSpriteDictionary[mapList[currentIndex]];
        mapName.text = mapNameDictionary[mapList[currentIndex]];
    }

    private void ShowPrevMap()
    {
        if(currentIndex > 0)
        {
            currentIndex--;
        }
        else
        {
            currentIndex = mapList.Count - 1;
        }
        mapImage.sprite= mapSpriteDictionary[mapList[currentIndex]];
        mapName.text = mapNameDictionary[mapList[currentIndex]];
    }
    public Map GetCurrentMap()
    {
        return mapList[currentIndex];
    }
    public void ToFindMatchState(bool isActive)
    {
        body.SetActive(!isActive);
        if(!isActive)
        {
            mapName.text = mapNameDictionary[mapList[currentIndex]];
        }
        else
        {
            mapName.text = "In Queue";
        }
    }
}
