using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillNotifyHolder : MonoBehaviour
{
    [SerializeField] KillNotifyCard notifyCardPrefab;
    [SerializeField] int maxShowAmount;
    List<KillNotifyCard> notifyCardList = new List<KillNotifyCard>();
    Queue<KillNotifyCard> notifyCardQueue = new Queue<KillNotifyCard>();
    private void Update()
    {
        if(notifyCardList.Count > 0)
        {
            foreach (KillNotifyCard card in notifyCardList)
            {
                if (!card.IsActive)
                {
                    card.Show();
                }
            }
        }
    }
    public void UpdateCardList(string kill, string dead)
    {
        var card = Instantiate(notifyCardPrefab,transform);
        card.Init(kill,dead,this);
        if(notifyCardList.Count < maxShowAmount)
        {
            notifyCardList.Add(card);
        }
        else
        {
            notifyCardQueue.Enqueue(card);
        }
    }
    internal void Close(KillNotifyCard killNotifyCard)
    {
        notifyCardList.Remove(killNotifyCard);
        if( notifyCardQueue.Count > 0 )
        {
            notifyCardList.Add(notifyCardQueue.Peek());
            notifyCardQueue.Dequeue();
        }
    }
}
