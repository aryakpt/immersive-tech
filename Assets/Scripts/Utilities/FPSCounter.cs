using UnityEngine;
using UnityEngine.UI;

public class MobileFPSCounter : MonoBehaviour
{
    public Text fpsText;
    private int frameCount;
    private float elapsedTime;
    private float updateInterval = 0.01f; // Update FPS every 0.5 seconds

    void Update()
    {
        frameCount++;
        elapsedTime += Time.unscaledDeltaTime;

        if (elapsedTime >= updateInterval)
        {
            int fps = Mathf.RoundToInt(frameCount / elapsedTime);
            fpsText.text = $"FPS: {fps}";
            frameCount = 0;
            elapsedTime = 0;
        }
    }
}