using UnityEngine;

public class DisableWallAfterTime : MonoBehaviour
{
    public float disableTime = 180f; // 3 minutes in seconds
    private float timer = 0f;
    private bool isClickable = true;

    void Update()
    {
        if(isClickable)
        {
            timer += Time.deltaTime;

            if(timer >= disableTime)
            {
                isClickable = false;
                DisableGameObject();
            }

            /*// Debug output
            Debug.Log("Time remaining: " + (disableTime - timer).ToString("F1") + " seconds");*/
        }
    }

    void DisableGameObject()
    {
        // Disable this GameObject (and all its children)
        gameObject.SetActive(false);
    }
}
