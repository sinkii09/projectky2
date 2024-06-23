using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PostGamePlayerCard : MonoBehaviour
{
    [SerializeField] TMP_Text placeText;
    [SerializeField] TMP_Text playerNameText;
    [SerializeField] TMP_Text playerKDText;
    [SerializeField] TMP_Text scoreText;
    [SerializeField] TMP_Text goldText;

    public int playerPlace;
    public void UpdateUI(CharacterPostGameState user)
    {
        placeText.text = user.Place.ToString();
        playerNameText.text = user.ClientName.ToString();
        playerKDText.text = $"{user.Kill}/{user.Dead}";
        scoreText.text = $"+{user.Score}";
        goldText.text = user.RankingPoint.ToString();
    }
}
