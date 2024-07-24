using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HatSlot : MonoBehaviour
{
    [SerializeField] Transform holder;

    [SerializeField] GameObject hat_Crow_Visual;
    [SerializeField] GameObject hat_Gentleman_Visual;
    [SerializeField] GameObject hat_Winter_Visual;
    [SerializeField] GameObject hat_Viking_Visual;
    [SerializeField] GameObject hat_Wizard_Visual;

    private void Awake()
    {
        foreach (Transform child in holder)
        {
            child.gameObject.SetActive(false);
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
            case "Crow Hat":
                hat_Crow_Visual.SetActive(true);
                break;
            case "Gentleman Hat":
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
            default: break;
        }
    }
}
