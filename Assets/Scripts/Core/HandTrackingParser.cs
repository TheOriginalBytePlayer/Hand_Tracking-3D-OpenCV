using System;
using System.Globalization;

namespace HandTrackingCore
{
    /// <summary>
    /// Represents a 3D point in space
    /// </summary>
    public struct Point3D
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public Point3D(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public override string ToString()
        {
            return $"({X}, {Y}, {Z})";
        }
    }

    /// <summary>
    /// Represents hand tracking data with 21 landmark points
    /// </summary>
    public class HandTrackingData
    {
        public const int NUM_HAND_POINTS = 21;
        
        public Point3D[] Points { get; private set; }
        public DateTime Timestamp { get; private set; }

        public HandTrackingData()
        {
            Points = new Point3D[NUM_HAND_POINTS];
            Timestamp = DateTime.Now;
        }

        /// <summary>
        /// Get a specific hand point by index (0-20)
        /// </summary>
        public Point3D GetPoint(int index)
        {
            if (index < 0 || index >= NUM_HAND_POINTS)
                throw new ArgumentOutOfRangeException(nameof(index), $"Index must be between 0 and {NUM_HAND_POINTS - 1}");
            
            return Points[index];
        }

        /// <summary>
        /// Set a specific hand point by index (0-20)
        /// </summary>
        public void SetPoint(int index, Point3D point)
        {
            if (index < 0 || index >= NUM_HAND_POINTS)
                throw new ArgumentOutOfRangeException(nameof(index), $"Index must be between 0 and {NUM_HAND_POINTS - 1}");
            
            Points[index] = point;
        }
    }

    /// <summary>
    /// Parser for hand tracking data received via UDP.
    /// Expected format: [x1,y1,z1,x2,y2,z2,...,x21,y21,z21]
    /// </summary>
    public class HandTrackingParser
    {
        /// <summary>
        /// Parse hand tracking data from a string.
        /// Format: [x1,y1,z1,x2,y2,z2,...,x21,y21,z21]
        /// </summary>
        /// <param name="data">The raw data string</param>
        /// <returns>Parsed hand tracking data, or null if parsing fails</returns>
        public static HandTrackingData Parse(string data)
        {
            if (string.IsNullOrEmpty(data))
                return null;

            try
            {
                // Remove brackets
                data = data.Trim();
                if (data.StartsWith("["))
                    data = data.Substring(1);
                if (data.EndsWith("]"))
                    data = data.Substring(0, data.Length - 1);

                // Split by comma
                string[] points = data.Split(',');

                // We expect exactly 21 points * 3 coordinates = 63 values
                // Using >= allows for extra data that will be ignored
                if (points.Length < HandTrackingData.NUM_HAND_POINTS * 3)
                    return null;

                HandTrackingData result = new HandTrackingData();

                // Parse each point
                for (int i = 0; i < HandTrackingData.NUM_HAND_POINTS; i++)
                {
                    float x = float.Parse(points[i * 3], CultureInfo.InvariantCulture);
                    float y = float.Parse(points[i * 3 + 1], CultureInfo.InvariantCulture);
                    float z = float.Parse(points[i * 3 + 2], CultureInfo.InvariantCulture);

                    result.SetPoint(i, new Point3D(x, y, z));
                }

                return result;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Try to parse hand tracking data from a string.
        /// </summary>
        /// <param name="data">The raw data string</param>
        /// <param name="result">The parsed data if successful</param>
        /// <returns>True if parsing was successful, false otherwise</returns>
        public static bool TryParse(string data, out HandTrackingData result)
        {
            result = Parse(data);
            return result != null;
        }
    }
}
