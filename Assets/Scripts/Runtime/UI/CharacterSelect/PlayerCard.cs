using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class PlayerCard : MonoBehaviour
{
    [SerializeField] private CharacterDatabase characterDatabase;
    [SerializeField] private GameObject visuals;
    [SerializeField] private Image characterIconImage;
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text characterNameText;
    [SerializeField] private Image LockedImage; 
    public void UpdateDisplay(CharacterSelectState state)
    {
        if (state.CharacterId != -1)
        {
            var character = characterDatabase.GetCharacterById(state.CharacterId);
            characterIconImage.sprite = character.Icon;
            characterIconImage.enabled = true;
            characterNameText.text = character.DisplayName;
        }
        else
        {
            characterIconImage.enabled = false;
            
        }

        playerNameText.text = state.IsLockedIn ? $"{state.ClientName}" : $"{state.ClientName} (Picking...)";
        visuals.SetActive(true);
        LockedImage.gameObject.SetActive(state.IsLockedIn);
        
        playerNameText.gameObject.SetActive(true);
        characterNameText.gameObject.SetActive(true);
    }

    public void DisableDisplay()
    {
        playerNameText.gameObject.SetActive(false);
        characterNameText.gameObject.SetActive(false);
        characterIconImage.enabled = false;
        LockedImage?.gameObject.SetActive(false);
    }
}
