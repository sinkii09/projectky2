
using TMPro;
using UnityEngine;


public class PostGamePlayerCard : MonoBehaviour
{
    [SerializeField] TMP_Text placeText;
    [SerializeField] TMP_Text playerNameText;
    [SerializeField] TMP_Text playerKDText;
    [SerializeField] TMP_Text scoreText;
    [SerializeField] TMP_Text rankText;

    public int playerPlace;

    internal void UpdateUI(PlayerResult user)
    {
        placeText.text = user.place.ToString();
        playerNameText.text = user.name;
        playerKDText.text = $"{user.kills}/{user.deaths}";
        scoreText.text = user.rpEarn >= 0 ? $"+{user.rpEarn}": $"-{user.rpEarn}";
        rankText.text = user.rank;
    }
}
