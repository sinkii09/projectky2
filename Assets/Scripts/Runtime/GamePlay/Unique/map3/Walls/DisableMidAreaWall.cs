using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomWallDisabler : MonoBehaviour
{
    public List<GameObject> walls; // List of wall children objects
    public float delayTime = 60f; // Public variable for delay time in seconds
    private int currentIndex1 = 0; // Index to track the first wall
    private int currentIndex2 = 1; // Index to track the second wall
    private bool isStarted = false; // Flag to check if the coroutine has started

    void Start()
    {
        StartCoroutine(StartAfterDelay());
    }

    IEnumerator StartAfterDelay()
    {
        // Wait for the specified delay time
        yield return new WaitForSeconds(delayTime);

        StartWallDisabling();
    }

    void StartWallDisabling()
    {
        if(isStarted)
            return; // If already started, do nothing

        isStarted = true; // Set the flag to indicate the coroutine has started
        ShuffleWalls(); // Shuffle the walls list at the start
        StartCoroutine(DisableWalls()); // Start the wall disabling coroutine
    }

    void ShuffleWalls()
    {
        for(int i = 0; i < walls.Count; i++)
        {
            GameObject temp = walls [i];
            int randomIndex = Random.Range(i, walls.Count);
            walls [i] = walls [randomIndex];
            walls [randomIndex] = temp;
        }
    }

    IEnumerator DisableWalls()
    {
        while(true)
        {
            if(walls.Count < 2)
                yield break; // Exit if fewer than 2 walls are in the list

            // If all walls have been disabled once, shuffle the list again and reset the indices
            if(currentIndex1 >= walls.Count || currentIndex2 >= walls.Count)
            {
                ShuffleWalls();
                currentIndex1 = 0;
                currentIndex2 = 1;
            }

            // Get the current walls to disable
            GameObject currentWall1 = walls [currentIndex1];
            GameObject currentWall2 = walls [currentIndex2];

            // Disable the walls
            currentWall1.SetActive(false);
            currentWall2.SetActive(false);

            // Wait for 5 seconds
            yield return new WaitForSeconds(5);

            // Enable the walls
            currentWall1.SetActive(true);
            currentWall2.SetActive(true);

            // Move to the next walls
            currentIndex1 += 2;
            currentIndex2 += 2;

            // If we've reached the end of the list, reset indices
            if(currentIndex2 >= walls.Count)
            {
                ShuffleWalls();
                currentIndex1 = 0;
                currentIndex2 = 1;
            }
        }
    }
}
