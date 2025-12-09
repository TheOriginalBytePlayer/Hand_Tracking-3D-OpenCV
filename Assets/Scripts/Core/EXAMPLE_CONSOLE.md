# Example: Using Hand Tracking Core Library Outside Unity

This example demonstrates how to use the hand tracking core library in a standalone C# console application, completely independent of Unity.

## Console Application Example

Create a new C# console application and add the following code:

```csharp
using System;
using System.Threading;
using HandTrackingCore;

namespace HandTrackingConsoleExample
{
    // Simple console renderer that prints hand tracking data
    class ConsoleRenderer : I3DRenderer
    {
        private int updateCount = 0;
        private readonly int printEveryNUpdates = 30; // Print every 30 updates to avoid spam

        public void Initialize(int numHandPoints)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Console Renderer Initialized");
            Console.WriteLine($"Tracking {numHandPoints} hand points");
            Console.WriteLine("Waiting for hand tracking data...");
            Console.WriteLine();
        }

        public void UpdateHandPoint(int pointIndex, Point3D position)
        {
            updateCount++;
            
            // Only print occasionally to avoid console spam
            if (updateCount % (printEveryNUpdates * 21) == 0)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Hand Point {pointIndex:D2}: ({position.X:F2}, {position.Y:F2}, {position.Z:F2})");
            }
        }

        public void UpdateHandLine(int startPointIndex, int endPointIndex)
        {
            // Not used in console renderer
        }

        public void Cleanup()
        {
            Console.WriteLine($"\n[{DateTime.Now:HH:mm:ss}] Console Renderer Cleaned Up");
            Console.WriteLine($"Total updates received: {updateCount}");
        }
    }

    // Example custom renderer that stores data in memory
    class DataStorageRenderer : I3DRenderer
    {
        private Point3D[] handPoints;
        private DateTime lastUpdate;
        
        public Point3D[] CurrentHandPoints => handPoints;
        public DateTime LastUpdate => lastUpdate;

        public void Initialize(int numHandPoints)
        {
            handPoints = new Point3D[numHandPoints];
            Console.WriteLine($"Data Storage Renderer initialized with {numHandPoints} points");
        }

        public void UpdateHandPoint(int pointIndex, Point3D position)
        {
            if (pointIndex >= 0 && pointIndex < handPoints.Length)
            {
                handPoints[pointIndex] = position;
                lastUpdate = DateTime.Now;
            }
        }

        public void UpdateHandLine(int startPointIndex, int endPointIndex)
        {
            // Not needed for data storage
        }

        public void Cleanup()
        {
            Console.WriteLine("Data Storage Renderer cleaned up");
        }

        public void PrintCurrentData()
        {
            Console.WriteLine($"\nCurrent Hand Data (as of {lastUpdate:HH:mm:ss.fff}):");
            for (int i = 0; i < handPoints.Length; i++)
            {
                Console.WriteLine($"  Point {i:D2}: {handPoints[i]}");
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("========================================");
            Console.WriteLine("Hand Tracking Core Library - Console Demo");
            Console.WriteLine("========================================\n");

            // Example 1: Simple console output
            ConsoleExample();

            // Example 2: Data storage and retrieval
            // DataStorageExample();
        }

        static void ConsoleExample()
        {
            Console.WriteLine("Starting Console Renderer Example...\n");
            
            // Create renderer
            var renderer = new ConsoleRenderer();
            
            // Create controller with the renderer
            var controller = new HandTrackingController(renderer, port: 5032);
            
            // Subscribe to data updates
            controller.OnHandTrackingDataUpdated += (data) => {
                // You can access the full hand tracking data here
                // Console.WriteLine($"Received complete hand data at {data.Timestamp}");
            };

            // Start receiving
            controller.Start();
            Console.WriteLine("Listening for hand tracking data on port 5032...");
            Console.WriteLine("Press any key to stop.\n");
            
            // Wait for user input
            Console.ReadKey();
            
            // Cleanup
            Console.WriteLine("\nStopping...");
            controller.Dispose();
            Console.WriteLine("Done.");
        }

        static void DataStorageExample()
        {
            Console.WriteLine("Starting Data Storage Example...\n");
            
            // Create data storage renderer
            var renderer = new DataStorageRenderer();
            
            // Create controller
            var controller = new HandTrackingController(renderer, port: 5032);
            
            // Subscribe to updates
            int updateCount = 0;
            controller.OnHandTrackingDataUpdated += (data) => {
                updateCount++;
                if (updateCount % 100 == 0)
                {
                    Console.WriteLine($"Received {updateCount} updates...");
                }
            };

            // Start receiving
            controller.Start();
            Console.WriteLine("Collecting hand tracking data...");
            Console.WriteLine("Press any key to print current data, or 'Q' to quit.\n");
            
            // Interactive loop
            while (true)
            {
                var key = Console.ReadKey(true);
                
                if (key.Key == ConsoleKey.Q)
                    break;
                    
                // Print current data
                renderer.PrintCurrentData();
                Console.WriteLine("\nPress any key to refresh, or 'Q' to quit.");
            }
            
            // Cleanup
            Console.WriteLine("\nStopping...");
            controller.Dispose();
            Console.WriteLine($"Total updates received: {updateCount}");
            Console.WriteLine("Done.");
        }
    }
}
```

## Building the Console Application

### Option 1: Visual Studio
1. Create a new Console Application project
2. Copy the Core library files to your project:
   - `UDPReceiver.cs`
   - `HandTrackingParser.cs`
   - `I3DRenderer.cs`
3. Add the example code above as `Program.cs`
4. Build and run

### Option 2: dotnet CLI
```bash
# Create new console project
dotnet new console -n HandTrackingConsoleExample
cd HandTrackingConsoleExample

# Copy core files to project
cp /path/to/Assets/Scripts/Core/*.cs .

# Add the example code to Program.cs
# Build
dotnet build

# Run
dotnet run
```

## Testing with Simulated Data

If you don't have the hand tracking source running, you can test with a UDP sender:

```csharp
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class UDPTestSender
{
    static void Main()
    {
        var client = new UdpClient();
        var endpoint = new IPEndPoint(IPAddress.Loopback, 5032);
        
        Random random = new Random();
        
        while (true)
        {
            // Generate fake hand tracking data
            var data = "[";
            for (int i = 0; i < 21; i++)
            {
                float x = random.Next(-500, 500);
                float y = random.Next(0, 1000);
                float z = random.Next(-500, 500);
                
                data += $"{x},{y},{z}";
                if (i < 20) data += ",";
            }
            data += "]";
            
            // Send data
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            client.Send(bytes, bytes.Length, endpoint);
            
            Console.WriteLine($"Sent: {data.Substring(0, Math.Min(50, data.Length))}...");
            Thread.Sleep(33); // ~30 FPS
        }
    }
}
```

## Integration with Web Applications

You can also use this in ASP.NET or other web applications:

```csharp
public class HandTrackingService : IHostedService
{
    private HandTrackingController controller;
    private WebSocketRenderer renderer;
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        renderer = new WebSocketRenderer();
        controller = new HandTrackingController(renderer, 5032);
        controller.Start();
    }
    
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        controller?.Dispose();
    }
}
```

## Next Steps

- Implement your own `I3DRenderer` for your specific 3D system
- Add coordinate transformations specific to your use case
- Implement gesture recognition on top of the hand tracking data
- Add data persistence or streaming capabilities
