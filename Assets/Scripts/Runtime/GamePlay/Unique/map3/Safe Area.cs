using Unity.Netcode;
using UnityEngine;

public class SafePoint : NetworkBehaviour
{
    ServerCharacter player;

    void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;
        if(other.CompareTag("Player"))
        {
            Debug.Log("Enter safe area!");
            player = other.GetComponent<ServerCharacter>();
            if(player != null)
            {
                // Calculate healRate as 10% of player's maxHealth
                float healRate = player.CharacterStats.BaseHP * 0.1f;
                other.GetComponent<DamageReceiver>().ReceiveHP((int)healRate);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!IsServer) return;
        if (other.CompareTag("Player"))
        {
            if(player != null)
            {
                player = null;
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            player = other.GetComponent<ServerCharacter>();
            if (player != null)
            {
                // Calculate healRate as 10% of player's maxHealth
                float healRate = player.CharacterStats.BaseHP * 0.1f;
                other.GetComponent<DamageReceiver>().ReceiveHP((int)healRate);
            }
        }
    }
}
