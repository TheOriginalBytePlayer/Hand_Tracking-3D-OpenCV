# Hand Tracking 3D - Unity-Independent Core Library

## Overview

This repository now contains a **Unity-independent hand tracking core library** that can be used with any 3D rendering system (Unity, Unreal Engine, custom engines, web applications, etc.).

The core functionality receives 3D hand landmark data via UDP (typically from an OpenCV-based hand tracking system) and provides a clean interface for integrating with your 3D system of choice.

## What's New

✅ **Core Library** (`Assets/Scripts/Core/`)
- `UDPReceiver.cs` - Unity-independent UDP communication
- `HandTrackingParser.cs` - Data parsing and structured hand tracking data
- `I3DRenderer.cs` - Interface for 3D system integration + main controller
- Works with .NET Standard 2.0+ and modern .NET

✅ **Unity Adapter** (`Assets/Scripts/`)
- `UnityHandRenderer.cs` - Unity implementation of the rendering interface
- `HandTrackingManager.cs` - Unity MonoBehaviour that coordinates everything

✅ **Legacy Support**
- Original Unity scripts (`HandTracking.cs`, `UDP_Receive.cs`) remain for backward compatibility
- No breaking changes to existing Unity projects

## Quick Start

### For Unity Users

1. Add `HandTrackingManager` and `UnityHandRenderer` components to a GameObject
2. Assign 21 GameObjects for hand points to the `handPoints` array
3. Configure UDP port (default: 5032)
4. That's it! Start receiving hand tracking data.

See [Migration Guide](Assets/Scripts/Core/MIGRATION_GUIDE.md) for details.

### For Non-Unity Users

1. Copy the Core library files to your project
2. Implement the `I3DRenderer` interface for your 3D system
3. Create and start the `HandTrackingController`

See [Integration Guide](Assets/Scripts/Core/README.md) for details.

## Architecture

```
OpenCV Hand Tracking (External)
    ↓ UDP
UDPReceiver (Core)
    ↓
HandTrackingParser (Core)
    ↓
HandTrackingController (Core)
    ↓
I3DRenderer Interface
    ↓
    ├─→ UnityHandRenderer (Unity)
    ├─→ YourCustomRenderer (Your System)
    └─→ ConsoleRenderer (Testing)
```

## Documentation

- **[README.md](Assets/Scripts/Core/README.md)** - Complete integration guide
- **[MIGRATION_GUIDE.md](Assets/Scripts/Core/MIGRATION_GUIDE.md)** - Detailed comparison of old vs new architecture
- **[EXAMPLE_CONSOLE.md](Assets/Scripts/Core/EXAMPLE_CONSOLE.md)** - Console application examples

## Key Features

### ✨ Unity-Independent
The core library has zero Unity dependencies and can be used in:
- Console applications
- Web applications (ASP.NET, Blazor)
- Desktop applications (WPF, WinForms, Avalonia)
- Other game engines (Unreal, Godot)
- Custom 3D systems

### 🔌 Easy Integration
Simple interface (`I3DRenderer`) to implement for any 3D system:
```csharp
public interface I3DRenderer
{
    void Initialize(int numHandPoints);
    void UpdateHandPoint(int pointIndex, Point3D position);
    void UpdateHandLine(int startPointIndex, int endPointIndex);
    void Cleanup();
}
```

### 🧵 Thread-Safe
- Event-based architecture
- Background thread for UDP reception
- No polling required

### 🎯 Type-Safe
- Structured data types (`Point3D`, `HandTrackingData`)
- Strong typing throughout
- No raw string manipulation in your code

### 📊 Real-Time Performance
- Efficient event-driven processing
- Minimal allocations
- Supports high-frequency updates (30-60+ FPS)

### 🔄 Backward Compatible
- Original Unity scripts still work
- Gradual migration path
- Can run old and new systems side-by-side

## Data Format

The system expects hand tracking data via UDP:
```
[x1,y1,z1,x2,y2,z2,...,x21,y21,z21]
```
- 21 hand landmark points
- Comma-separated coordinates
- Enclosed in brackets

## Example: Custom 3D System Integration

```csharp
using HandTrackingCore;

// Implement the interface for your system
public class My3DSystemRenderer : I3DRenderer
{
    public void Initialize(int numHandPoints)
    {
        // Create your 3D objects
    }

    public void UpdateHandPoint(int pointIndex, Point3D position)
    {
        // Update position in your 3D system
        // position.X, position.Y, position.Z
    }

    public void UpdateHandLine(int start, int end) { }
    public void Cleanup() { }
}

// Use it
var renderer = new My3DSystemRenderer();
var controller = new HandTrackingController(renderer, port: 5032);
controller.OnHandTrackingDataUpdated += (data) => {
    // Optional: handle complete hand data updates
};
controller.Start();
```

## Example: Console Application

```csharp
using System;
using HandTrackingCore;

class ConsoleRenderer : I3DRenderer
{
    public void Initialize(int numHandPoints)
    {
        Console.WriteLine($"Tracking {numHandPoints} hand points");
    }

    public void UpdateHandPoint(int pointIndex, Point3D position)
    {
        Console.WriteLine($"Point {pointIndex}: {position}");
    }

    public void UpdateHandLine(int start, int end) { }
    public void Cleanup() { }
}

class Program
{
    static void Main()
    {
        var renderer = new ConsoleRenderer();
        var controller = new HandTrackingController(renderer, 5032);
        
        controller.Start();
        Console.WriteLine("Press Enter to exit...");
        Console.ReadLine();
        
        controller.Dispose();
    }
}
```

See [EXAMPLE_CONSOLE.md](Assets/Scripts/Core/EXAMPLE_CONSOLE.md) for more examples.

## Testing

The core library has been tested and verified:
- ✅ Compiles successfully with .NET 8.0+
- ✅ Parser correctly handles hand tracking data format
- ✅ Interface pattern works as expected
- ✅ Thread-safe event handling

## Benefits Over Original Implementation

| Feature | Old | New |
|---------|-----|-----|
| Unity Dependency | ✗ Required | ✅ Optional |
| Works with other 3D systems | ✗ No | ✅ Yes |
| Thread Safety | ⚠️ Shared mutable state | ✅ Event-based |
| Testability | ✗ Requires Unity | ✅ Pure C# |
| Separation of Concerns | ✗ Mixed | ✅ Clean layers |
| Resource Management | ⚠️ Unity lifecycle | ✅ IDisposable |
| Error Handling | ⚠️ Console only | ✅ Error events |

## Requirements

### Core Library
- .NET Standard 2.0+ or .NET 6.0+
- System.Net.Sockets
- System.Threading

### Unity Adapter
- Unity 2019.4 or later
- Any render pipeline

## Contributing

The modular architecture makes it easy to add support for new platforms:

1. Implement `I3DRenderer` for your target system
2. Create a controller with your renderer
3. Start receiving data!

## Future Enhancements

Potential additions (not implemented yet):
- Gesture recognition
- Hand pose estimation
- Multiple hand tracking
- Data recording/playback
- Network protocol variations (WebSocket, TCP)

## License

See [LICENSE](LICENSE) file.

## Support

For questions or issues:
1. Check the documentation in `Assets/Scripts/Core/`
2. Review the migration guide for common scenarios
3. Open an issue on GitHub

## Credits

Original Unity implementation by TheOriginalBytePlayer.
Core library separation and modularization added to enable cross-platform usage.
