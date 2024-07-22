using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Teleport : NetworkBehaviour
{
    public Transform telepointTo;
    private bool canTeleport = true; // Flag to control teleportation
    private Collider triggerCollider;

    private void Start()
    {
        triggerCollider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(!IsServer) { return; }
        if(other.CompareTag("Player") && canTeleport)
        {
            var player = other.gameObject.GetComponent<ServerCharacter>();
            player.SetupTeleport(true,this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsServer) { return; }
        if (other.CompareTag("Player"))
        {
            var player = other.gameObject.GetComponent<ServerCharacter>();
            player.SetupTeleport(false, this);
        }
    }
    public void StartTele(ServerCharacter serverCharacter)
    {
        
        if(IsServer && canTeleport)
        {
            StartCoroutine(TeleportPlayer(serverCharacter));
        }
    }
    IEnumerator TeleportPlayer(ServerCharacter serverCharacter)
    {
        canTeleport = false; // Disable teleportation
        triggerCollider.enabled = false; // Disable the trigger collider


        yield return new WaitForSeconds(1);

        serverCharacter.physicsWrapper.Transform.position = telepointTo.position;

        yield return new WaitForSeconds(5); // Wait for 3 seconds
        canTeleport = true; // Enable teleportation again
        triggerCollider.enabled = true; // Re-enable the trigger collider
    }
}
