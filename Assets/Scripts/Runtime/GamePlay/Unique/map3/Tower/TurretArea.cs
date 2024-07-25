using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TurretArea : MonoBehaviour
{
    public List<Turret> turrets = new List<Turret>(); // List of Turret scripts
    private List<Transform> playersInArea = new List<Transform>(); // List of players in the area

    void Awake()
    {
        if(turrets.Count == 0)
        {
            turrets.AddRange(GetComponentsInChildren<Turret>());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(NetworkManager.Singleton.IsServer)
        {
            if (other.CompareTag("Player"))
            {
                playersInArea.Add(other.transform);
                Debug.Log("Player entered turret area: " + other.name);
                UpdateTurretTargets();
            }
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            if (other.CompareTag("Player"))
            {
                playersInArea.Remove(other.transform);
                Debug.Log("Player exited turret area: " + other.name);
                UpdateTurretTargets();
            }
        }
    }

    // Method to update turret targets based on players in the area
    private void UpdateTurretTargets()
    {
        int turretCount = turrets.Count;
        int playerCount = playersInArea.Count;

        // Clear all current targets
        foreach(var turret in turrets)
        {
            turret.ClearTargetPlayer();
        }

        // Distribute players among turrets
        for(int i = 0; i < playerCount; i++)
        {
            int turretIndex = i % turretCount;
            turrets [turretIndex].SetTargetPlayer(playersInArea [i]);
        }
    }
}
