using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Turret : NetworkBehaviour
{
    public float distanceToPlayer;
    public Transform head;
    public GameObject bullet;
    public float fireRate;
    private float nextFire;
    private Transform targetPlayer; // Track the player currently targeted by this turret

    public bool CanShoot { get; set; }
    void Start()
    {
        nextFire = 0f; // Initialize nextFire
        targetPlayer = null; // Initialize targetPlayer
    }

    void Update()
    {
        if(IsServer)
        {
            if (targetPlayer != null)
            {
                float distance = Vector3.Distance(targetPlayer.position, transform.position);
                if (distance < distanceToPlayer)
                {
                    head.LookAt(targetPlayer);

                    // Draw a raycast debug line
                    Debug.DrawRay(head.position, head.forward * distanceToPlayer, Color.red);

                    if (Time.time >= nextFire)
                    {
                        nextFire = Time.time + 1f / fireRate;
                        if(CanShoot)
                        {
                            Shoot();
                        }
                    }
                }
            }
        }

    }

    void Shoot()
    {
        GameObject clone = Instantiate(bullet, head.position, head.rotation);
        clone.GetComponent<Rigidbody>().AddForce(head.forward * 1500);
        Destroy(clone, 5); // Destroy the bullet after 5 seconds
    }

    // Method to set the target player
    public void SetTargetPlayer(Transform player)
    {
        targetPlayer = player;
    }

    // Method to clear the target player
    public void ClearTargetPlayer()
    {
        targetPlayer = null;
    }

    // Method to check if the turret is currently targeting a player
    public bool IsTargetingPlayer()
    {
        return targetPlayer != null;
    }
}
