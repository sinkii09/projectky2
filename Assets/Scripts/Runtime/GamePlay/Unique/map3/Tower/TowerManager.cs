using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TowerManager : MonoBehaviour
{
    public List<GameObject> towers; // List to hold all tower GameObjects
    private Dictionary<GameObject, Tower> towerScripts = new Dictionary<GameObject, Tower>();
    public int countdownTime = 10;  // Countdown time in seconds

    void Start()
    {
        // Initialize the dictionary with towers and their scripts
        foreach(GameObject tower in towers)
        {
            Tower towerScript = tower.GetComponent<Tower>();
            if(towerScript != null)
            {
                towerScripts [tower] = towerScript;
            }
        }
    }

    public void HandleTowerDeath(Tower tower)
    {
        StartCoroutine(DisableAndReenableTower(tower));
    }

    IEnumerator DisableAndReenableTower(Tower tower)
    {
        //Debug.Log("Tower HP is 0, disabling for 1 minute.");
        tower.gameObject.SetActive(false);

        for(int i = countdownTime; i > 0; i--)
        {
            Debug.Log("Countdown: " + i + " seconds remaining.");
            yield return new WaitForSeconds(1);
        }

        tower.Restore();
    }
}
