using UnityEngine;
using HandTrackingCore;

/// <summary>
/// Unity implementation of the I3DRenderer interface.
/// This adapter allows the core hand tracking system to work with Unity.
/// </summary>
public class UnityHandRenderer : MonoBehaviour, I3DRenderer
{
    [Header("Hand Points")]
    [Tooltip("Array of GameObjects representing the 21 hand points")]
    public GameObject[] handPoints;

    [Header("Coordinate Transformation")]
    [Tooltip("Scale factor for coordinates (default: 100)")]
    public float coordinateScale = 100f;

    [Tooltip("X-axis offset (default: 7)")]
    public float xOffset = 7f;

    private bool isInitialized = false;

    public void Initialize(int numHandPoints)
    {
        if (handPoints == null || handPoints.Length != numHandPoints)
        {
            Debug.LogError($"UnityHandRenderer: Expected {numHandPoints} hand points, but found {(handPoints == null ? 0 : handPoints.Length)}");
            return;
        }
        
        isInitialized = true;
        Debug.Log($"UnityHandRenderer initialized with {numHandPoints} hand points");
    }

    public void UpdateHandPoint(int pointIndex, Point3D position)
    {
        if (!isInitialized)
            return;

        if (pointIndex < 0 || pointIndex >= handPoints.Length)
            return;

        if (handPoints[pointIndex] == null)
            return;

        // Transform coordinates from hand tracking space to Unity space
        // Original formula: x = 7 - x/100, y = y/100, z = z/100
        float x = xOffset - position.X / coordinateScale;
        float y = position.Y / coordinateScale;
        float z = position.Z / coordinateScale;

        handPoints[pointIndex].transform.localPosition = new Vector3(x, y, z);
    }

    public void UpdateHandLine(int startPointIndex, int endPointIndex)
    {
        // Line rendering is handled separately by LineCode components
        // This method is not needed for the current Unity implementation
    }

    public void Cleanup()
    {
        isInitialized = false;
    }

    void OnDestroy()
    {
        Cleanup();
    }
}
