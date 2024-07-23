using UnityEngine;
using UnityEngine.UI;

public class TeleportProgressingBar : MonoBehaviour
{
    public Canvas announceCanvas; // Reference to the Canvas
    public Slider countdownSlider; // Reference to the Slider
    private float countdownTimer = 0f;

    private void Start()
    {
        announceCanvas.enabled = false; // Hide the canvas initially
        countdownSlider.gameObject.SetActive(false); // Hide the slider initially
        countdownSlider.maxValue = 1f; // Set the slider's max value to 1, will be adjusted dynamically
    }

    private void Update()
    {
        if(countdownTimer > 0)
        {
            countdownTimer -= Time.deltaTime;
            countdownSlider.value = Mathf.Clamp(countdownTimer, 0, countdownSlider.maxValue);

            if(countdownTimer <= 0)
            {
                HideAnnouncement(); // Hide the canvas after countdown completes
            }
        }
    }

    public void ShowAnnouncement(float duration)
    {
        announceCanvas.enabled = true; // Show the canvas
        countdownSlider.gameObject.SetActive(true); // Show the slider
        countdownSlider.maxValue = duration; // Set the slider's max value
        countdownTimer = duration; // Reset the countdown timer
    }

    public void HideAnnouncement()
    {
        announceCanvas.enabled = false; // Hide the canvas
        countdownSlider.gameObject.SetActive(false); // Hide the slider
    }
}
