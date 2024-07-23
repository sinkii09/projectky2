using UnityEngine;
using TMPro;

public class PlayerCanvasManager : MonoBehaviour
{
    public Canvas announceCanvas; // Reference to the Canvas
    public TMP_Text announceText; // Reference to the TMP_Text component on the Canvas

    private void Start()
    {
        DisableCanvas(); // Ensure the canvas is disabled at the start
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Teleport"))
        {
            ShowAnnouncement("Press 'F' to teleport.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Teleport"))
        {
            HideAnnouncement();
        }
    }

    private void ShowAnnouncement(string message)
    {
        announceText.text = message; // Set the announcement text
        announceCanvas.enabled = true; // Show the canvas
    }

    private void HideAnnouncement()
    {
        announceCanvas.enabled = false; // Hide the canvas
    }

    public void DisableCanvas()
    {
        announceCanvas.enabled = false; // Disable the canvas
    }
}
