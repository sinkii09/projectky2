using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostSpawner : MonoBehaviour
{
    public GameObject ghostPrefab; // Ghost prefab to be spawned
    public List<Transform> spawnLocations; // List of spawn locations
    public float nextSpawn = 60f; // Time between spawns
    public float firstSpawn = 60f;
    private List<GameObject> spawnedGhosts = new List<GameObject>(); // List to keep track of spawned ghosts
    private int maxGhosts;

    void Start()
    {
        maxGhosts = spawnLocations.Count; // Set max ghosts to the number of spawn locations
        StartCoroutine(SpawnGhosts());
    }

    IEnumerator SpawnGhosts()
    {
        yield return new WaitForSeconds(firstSpawn); // Initial wait before first spawn

        while(true)
        {
            spawnedGhosts.RemoveAll(ghost => ghost == null); // Remove destroyed ghosts from the list

            if(spawnedGhosts.Count < maxGhosts)
            {
                for(int i = 0; i < spawnLocations.Count && spawnedGhosts.Count < maxGhosts; i++)
                {
                    Transform spawnLocation = spawnLocations [i];
                    GameObject spawnedGhost = Instantiate(ghostPrefab, spawnLocation.position, spawnLocation.rotation);
                    GhostAI ghostAI = spawnedGhost.GetComponent<GhostAI>();
                    if(ghostAI != null)
                    {
                        ghostAI.SetSpawner(this); // Set the spawner reference in the GhostAI script
                    }
                    spawnedGhosts.Add(spawnedGhost);
                }
            }

            yield return new WaitForSeconds(nextSpawn); // Wait for the next spawn
        }
    }

    public void RemoveGhost(GameObject ghost)
    {
        if(spawnedGhosts.Contains(ghost))
        {
            spawnedGhosts.Remove(ghost);
        }
    }
}
