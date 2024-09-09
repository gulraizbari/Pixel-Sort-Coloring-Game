using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FPSCounter : MonoBehaviour
{
    public TextMeshProUGUI fpsText; // Assign a UI Text element to display the FPS
    private float deltaTime = 0.0f;

    
    void Update()
    {
        // Calculate the frame time and update deltaTime
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;

        // Calculate FPS
        float fps = 1.0f / deltaTime;

        // Display FPS on the assigned Text element
        if (fpsText != null)
        {
            fpsText.text = string.Format("{0:0.} FPS", fps);
        }
    }
}