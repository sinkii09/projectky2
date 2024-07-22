using UnityEngine;

public class Bullet : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        Destroy(gameObject); // Destroy the bullet on collision
    }
}
