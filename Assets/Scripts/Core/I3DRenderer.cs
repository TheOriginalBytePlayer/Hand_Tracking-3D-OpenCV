using System;

namespace HandTrackingCore
{
    /// <summary>
    /// Interface for 3D rendering systems to implement.
    /// Allows the hand tracking system to work with any 3D engine (Unity, Unreal, custom engines, etc.)
    /// </summary>
    public interface I3DRenderer
    {
        /// <summary>
        /// Update the position of a hand point in the 3D space
        /// </summary>
        /// <param name="pointIndex">Index of the hand point (0-20)</param>
        /// <param name="position">The 3D position to set</param>
        void UpdateHandPoint(int pointIndex, Point3D position);

        /// <summary>
        /// Update the line connecting two hand points
        /// </summary>
        /// <param name="startPointIndex">Index of the start point</param>
        /// <param name="endPointIndex">Index of the end point</param>
        void UpdateHandLine(int startPointIndex, int endPointIndex);

        /// <summary>
        /// Initialize the renderer with the number of hand points
        /// </summary>
        void Initialize(int numHandPoints);

        /// <summary>
        /// Clean up resources
        /// </summary>
        void Cleanup();
    }

    /// <summary>
    /// Main hand tracking controller that coordinates UDP reception, data parsing, and 3D rendering.
    /// This class is independent of any specific 3D engine.
    /// </summary>
    public class HandTrackingController : IDisposable
    {
        private UDPReceiver udpReceiver;
        private I3DRenderer renderer;
        private HandTrackingData latestData;

        public bool IsReceiving => udpReceiver != null;
        public HandTrackingData LatestData => latestData;

        /// <summary>
        /// Event fired when new hand tracking data is received and parsed
        /// </summary>
        public event Action<HandTrackingData> OnHandTrackingDataUpdated;

        /// <summary>
        /// Create a new hand tracking controller
        /// </summary>
        /// <param name="renderer">The 3D renderer implementation</param>
        /// <param name="port">UDP port to listen on (default: 5032)</param>
        public HandTrackingController(I3DRenderer renderer, int port = 5032)
        {
            this.renderer = renderer;
            this.udpReceiver = new UDPReceiver(port);
            
            // Subscribe to UDP data events
            udpReceiver.OnDataReceived += HandleDataReceived;
            
            // Initialize the renderer
            renderer.Initialize(HandTrackingData.NUM_HAND_POINTS);
        }

        /// <summary>
        /// Start receiving hand tracking data
        /// </summary>
        public void Start()
        {
            udpReceiver.StartReceiving();
        }

        /// <summary>
        /// Stop receiving hand tracking data
        /// </summary>
        public void Stop()
        {
            udpReceiver.StopReceiving();
        }

        private void HandleDataReceived(string data)
        {
            // Parse the data
            if (HandTrackingParser.TryParse(data, out HandTrackingData handData))
            {
                latestData = handData;
                
                // Update the renderer
                for (int i = 0; i < HandTrackingData.NUM_HAND_POINTS; i++)
                {
                    Point3D point = handData.GetPoint(i);
                    renderer.UpdateHandPoint(i, point);
                }

                // Fire event
                OnHandTrackingDataUpdated?.Invoke(handData);
            }
        }

        public void Dispose()
        {
            Stop();
            udpReceiver?.Dispose();
            renderer?.Cleanup();
        }
    }
}
