using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerIngameCard : MonoBehaviour
{
    [SerializeField] private GameObject visuals;
    [SerializeField] private Image characterIconImage;
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text playerHealth;
    [SerializeField] private TMP_Text playerKill;
    [SerializeField] private TMP_Text playerDead;

    public virtual void Initialize(CharacterInGameState state, CharacterDatabase characterDatabase)
    {
        var character = characterDatabase.GetCharacterById(state.CharacterId);
        characterIconImage.sprite = character.Icon;
        characterIconImage.enabled = true;
        playerNameText.text = state.ClientName;
        playerHealth.text = state.ClientId == NetworkManager.Singleton.LocalClientId ? $"HP: {state.Health}" : $"{state.Health}";
        playerKill.text = $"K: {state.Kill}";
        playerDead.text = $"D: {state.Dead}";
        visuals.SetActive(true);
    }
    public virtual void UpdateDisplay(CharacterInGameState state)
    {
        playerHealth.text = state.ClientId == NetworkManager.Singleton.LocalClientId ? $"HP: {state.Health}" : $"{state.Health}";
        playerKill.text = $"K: {state.Kill}";
        playerDead.text = $"D: {state.Dead}";
    }
    public virtual void DisableVisual()
    {
        visuals.SetActive( false );
    }
}
