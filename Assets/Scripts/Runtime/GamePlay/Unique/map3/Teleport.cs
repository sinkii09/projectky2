using System.Collections;
using UnityEngine;

public class Teleport : MonoBehaviour
{
    public Transform telepointTo;
    private bool canTeleport = true; // Flag to control teleportation
    private bool playerInTrigger = false; // Flag to check if player is in the trigger
    private Collider playerCollider; // Store the player collider
    private Collider triggerCollider;

    private void Start()
    {
        triggerCollider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player") && canTeleport)
        {
            playerInTrigger = true; // Player is in the trigger
            playerCollider = other; // Store the player collider
            Debug.Log("Player entered teleport trigger. Press 'F' for teleport.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            playerInTrigger = false; // Player left the trigger
            playerCollider = null; // Clear the player collider
            Debug.Log("Player left teleport trigger.");
        }
    }

    private void Update()
    {
        if(playerInTrigger && Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("Player pressed 'F' to teleport.");
            StartCoroutine(TeleportPlayer());
        }
    }

    IEnumerator TeleportPlayer()
    {
        canTeleport = false; // Disable teleportation
        playerInTrigger = false; // Reset the flag
        triggerCollider.enabled = false; // Disable the trigger collider

        // Freeze the player's movement
        CharacterMovement characterMovement = playerCollider.GetComponent<CharacterMovement>();
        if(characterMovement != null)
        {
            characterMovement.enabled = false;
        }

        yield return new WaitForSeconds(1);

        playerCollider.transform.position = telepointTo.position;

        // Unfreeze the player's movement
        if(characterMovement != null)
        {
            characterMovement.enabled = true;
        }

        yield return new WaitForSeconds(3); // Wait for 3 seconds
        canTeleport = true; // Enable teleportation again
        triggerCollider.enabled = true; // Re-enable the trigger collider
    }
}
