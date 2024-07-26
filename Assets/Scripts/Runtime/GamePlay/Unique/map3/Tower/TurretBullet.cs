using Unity.Netcode;
using UnityEngine;

public class TurretBullet : NetworkBehaviour
{
    [SerializeField] int damage;
    [SerializeField] GameObject visual;
    void OnCollisionEnter(Collision collision)
    {
        if (IsServer)
        {
            if(collision.gameObject.CompareTag("Player"))
            {
                var character = collision.gameObject.GetComponent<IDamageable>();
                if(character != null && character.IsDamageable())
                {
                    character.ReceiveHP(-damage);
                }
            }
            Destroy(gameObject); 
        }
    }
}
