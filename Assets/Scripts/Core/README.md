# Hand Tracking Core Library - Integration Guide

## Overview

This library provides a Unity-independent hand tracking system that receives 3D hand landmark data via UDP and can be integrated with any 3D rendering system (Unity, Unreal Engine, custom engines, etc.).

## Architecture

The system is divided into three main layers:

### 1. Core Layer (Unity-Independent)
Located in `Assets/Scripts/Core/`

- **`UDPReceiver.cs`**: Handles UDP network communication
- **`HandTrackingParser.cs`**: Parses hand tracking data from string format
- **`I3DRenderer.cs`**: Defines the interface for 3D system integration and the main controller

These components have no Unity dependencies and can be used in any C# application.

### 2. Adapter Layer
- **`UnityHandRenderer.cs`**: Unity implementation of `I3DRenderer`
- **`HandTrackingManager.cs`**: Unity MonoBehaviour that coordinates the system

### 3. Legacy Layer (Deprecated)
- `HandTracking.cs`: Original Unity-specific implementation (kept for backward compatibility)
- `UDP_Receive.cs`: Original Unity-specific UDP receiver (kept for backward compatibility)

## Data Format

The system expects hand tracking data in the following format via UDP:

```
[x1,y1,z1,x2,y2,z2,...,x21,y21,z21]
```

Where:
- 21 hand landmark points
- Each point has 3 coordinates (x, y, z)
- Data is sent as a comma-separated string enclosed in brackets

## Using with Unity

### Quick Start

1. Add the `HandTrackingManager` component to a GameObject
2. Add the `UnityHandRenderer` component to the same or another GameObject
3. Assign the `UnityHandRenderer` to the `HandTrackingManager`'s renderer field
4. Create 21 GameObjects for hand points and assign them to the `handPoints` array in `UnityHandRenderer`
5. Configure the UDP port (default: 5032)

### Example Scene Setup

```csharp
// GameObject hierarchy:
// HandTrackingSystem (GameObject)
//   - HandTrackingManager (Component)
//   - UnityHandRenderer (Component)
//     - handPoints[0] to handPoints[20] (GameObjects)
```

## Integrating with Custom 3D Systems

To use this system with your own 3D engine, implement the `I3DRenderer` interface:

```csharp
using HandTrackingCore;

public class MyCustomRenderer : I3DRenderer
{
    public void Initialize(int numHandPoints)
    {
        // Initialize your 3D objects/resources
    }

    public void UpdateHandPoint(int pointIndex, Point3D position)
    {
        // Update the 3D position of the hand point in your system
        // position.X, position.Y, position.Z contain the coordinates
    }

    public void UpdateHandLine(int startPointIndex, int endPointIndex)
    {
        // Optional: Draw lines between hand points
    }

    public void Cleanup()
    {
        // Clean up resources
    }
}
```

### Using the Controller

```csharp
using HandTrackingCore;

// Create your renderer
var myRenderer = new MyCustomRenderer();

// Create the controller
var controller = new HandTrackingController(myRenderer, port: 5032);

// Subscribe to data updates (optional)
controller.OnHandTrackingDataUpdated += (data) => {
    // Handle hand tracking data
    Console.WriteLine($"Received {data.Points.Length} hand points");
};

// Start receiving
controller.Start();

// When done
controller.Stop();
controller.Dispose();
```

## Coordinate Transformation

The raw hand tracking coordinates typically need transformation for your 3D space. The Unity implementation uses:

```csharp
float x = 7f - position.X / 100f;
float y = position.Y / 100f;
float z = position.Z / 100f;
```

Adjust these transformations based on your coordinate system and scale.

## Example: Console Application

Here's a minimal example of using the library in a console application:

```csharp
using System;
using HandTrackingCore;

class ConsoleRenderer : I3DRenderer
{
    public void Initialize(int numHandPoints)
    {
        Console.WriteLine($"Initialized with {numHandPoints} points");
    }

    public void UpdateHandPoint(int pointIndex, Point3D position)
    {
        Console.WriteLine($"Point {pointIndex}: {position}");
    }

    public void UpdateHandLine(int startPointIndex, int endPointIndex) { }
    
    public void Cleanup() { }
}

class Program
{
    static void Main()
    {
        var renderer = new ConsoleRenderer();
        var controller = new HandTrackingController(renderer, 5032);
        
        controller.Start();
        Console.WriteLine("Listening for hand tracking data... Press Enter to exit.");
        Console.ReadLine();
        
        controller.Dispose();
    }
}
```

## Example: Unreal Engine Integration

```cpp
// Pseudo-code for Unreal Engine (would need actual C++/C# interop)
class UUnrealHandRenderer : public I3DRenderer
{
public:
    void Initialize(int numHandPoints) override
    {
        // Create USceneComponents for each hand point
        for (int i = 0; i < numHandPoints; i++)
        {
            USceneComponent* Point = CreateDefaultSubobject<USceneComponent>(...);
            HandPoints.Add(Point);
        }
    }

    void UpdateHandPoint(int pointIndex, Point3D position) override
    {
        // Convert to Unreal's FVector and update location
        FVector UnrealPos(position.X, position.Y, position.Z);
        HandPoints[pointIndex]->SetRelativeLocation(UnrealPos);
    }
};
```

## Dependencies

The core library only requires:
- .NET Framework 4.x or .NET Standard 2.0+
- System.Net.Sockets
- System.Threading

No Unity or external dependencies required for the core functionality.

## Thread Safety

The `UDPReceiver` runs on a background thread. The `OnDataReceived` event is fired from this thread, so ensure thread-safe operations when updating your 3D system.

For Unity, use `UnityMainThreadDispatcher` or update in the main `Update()` loop if needed.

## Performance Considerations

- UDP is used for low-latency, real-time data transmission
- Data is received on a background thread to avoid blocking
- Parse operations are lightweight and can handle high-frequency updates
- Consider implementing frame rate limiting if receiving data faster than your render loop

## Troubleshooting

### No Data Received
- Check that the UDP port is not blocked by firewall
- Verify the hand tracking source is sending to the correct IP and port
- Use `printToConsole = true` to debug incoming data

### Wrong Coordinate System
- Adjust the coordinate transformation in your `I3DRenderer` implementation
- Check the scale and offset values

### Thread Issues
- Ensure UI/rendering updates happen on the appropriate thread for your system
- Use synchronization primitives if needed

## Contributing

The core library is designed to be extensible. To add support for a new 3D system:

1. Implement `I3DRenderer` for your system
2. Create a controller instance with your renderer
3. Start receiving data

## License

See LICENSE file in the repository root.
