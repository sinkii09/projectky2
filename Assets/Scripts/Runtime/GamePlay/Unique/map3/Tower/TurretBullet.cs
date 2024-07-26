using Unity.Netcode;
using UnityEngine;

public class TurretBullet : NetworkBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        if (IsServer)
        {
            Destroy(gameObject); // Destroy the bullet on collision
        }
    }
}
