using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LeaderboardCard : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI orderTxt;
    [SerializeField] TextMeshProUGUI nameTxt;
    [SerializeField] TextMeshProUGUI rankPointTxt;

    public void Initialize(string order, string name, string point)
    {
        orderTxt.text = order;
        nameTxt.text = name;
        rankPointTxt.text = point;
    }
}
