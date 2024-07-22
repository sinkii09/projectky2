using UnityEngine;

public class SafePoint : MonoBehaviour
{
    CharacterMovement player;

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            Debug.Log("Enter safe area!");
            player = other.GetComponent<CharacterMovement>();
            if(player != null)
            {
                // Calculate healRate as 10% of player's maxHealth
                //float healRate = player.maxHealth * 0.1f;
                //player.StartHealing(healRate);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            Debug.Log("Out of safe area!");
            if(player != null)
            {
                //player.StopHealing();
                player = null;
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            player = other.GetComponent<CharacterMovement>();
            if(player != null) //&& !player.isHealing)
            {
                // Calculate healRate as 10% of player's maxHealth
                //float healRate = player.maxHealth * 0.1f;
                //player.StartHealing(healRate);
            }
        }
    }
}
