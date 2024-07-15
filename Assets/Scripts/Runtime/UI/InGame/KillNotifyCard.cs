using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class KillNotifyCard : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI killText;
    [SerializeField] TextMeshProUGUI deadText;
    [SerializeField] Animator animator;
    KillNotifyHolder holder;

    public bool IsActive { get; private set; }
    public void Init(string kill, string dead,KillNotifyHolder killNotifyHolder)
    {
        gameObject.SetActive(false);
        killText.text = kill;
        deadText.text = dead;
        holder = killNotifyHolder;
        IsActive = false;
    }
    public void Show()
    {
        gameObject.SetActive (true);
        animator.SetTrigger("active");
        IsActive = true;
    }
    public void Hide()
    {
        holder.Close(this);
        Destroy(gameObject);
    }
}
