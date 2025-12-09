using UnityEngine;
using HandTrackingCore;

/// <summary>
/// Unity MonoBehaviour that integrates the core hand tracking system with Unity.
/// This component coordinates between the UDP receiver, parser, and Unity renderer.
/// </summary>
public class HandTrackingManager : MonoBehaviour
{
    [Header("UDP Configuration")]
    [Tooltip("UDP port to receive hand tracking data")]
    public int port = 5032;

    [Tooltip("Start receiving data automatically on Start()")]
    public bool startReceiving = true;

    [Tooltip("Print received data to console for debugging")]
    public bool printToConsole = false;

    [Header("Renderer")]
    [Tooltip("The Unity renderer component")]
    public UnityHandRenderer renderer;

    private HandTrackingController controller;

    void Start()
    {
        if (renderer == null)
        {
            Debug.LogError("HandTrackingManager: UnityHandRenderer is not assigned!");
            return;
        }

        // Create the core controller with the Unity renderer
        controller = new HandTrackingController(renderer, port);

        // Subscribe to data updates if needed for debugging or custom logic
        controller.OnHandTrackingDataUpdated += OnHandDataUpdated;

        if (startReceiving)
        {
            controller.Start();
            Debug.Log($"HandTrackingManager: Started receiving on port {port}");
        }
    }

    void OnDestroy()
    {
        if (controller != null)
        {
            controller.Dispose();
        }
    }

    private void OnHandDataUpdated(HandTrackingData data)
    {
        if (printToConsole)
        {
            Debug.Log($"Hand tracking data updated at {data.Timestamp}");
        }
    }

    /// <summary>
    /// Manually start receiving hand tracking data
    /// </summary>
    public void StartReceiving()
    {
        if (controller != null)
        {
            controller.Start();
        }
    }

    /// <summary>
    /// Manually stop receiving hand tracking data
    /// </summary>
    public void StopReceiving()
    {
        if (controller != null)
        {
            controller.Stop();
        }
    }

    /// <summary>
    /// Get the latest hand tracking data
    /// </summary>
    public HandTrackingData GetLatestData()
    {
        return controller?.LatestData;
    }
}
