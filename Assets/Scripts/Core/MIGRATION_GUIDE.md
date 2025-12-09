# Migration Guide: Old vs New Architecture

## Overview

This guide helps you understand the differences between the original Unity-dependent implementation and the new Unity-independent core library.

## Architecture Comparison

### Old Architecture (Unity-Dependent)

```
OpenCV Hand Tracking (External)
    |
    | (UDP)
    v
UDP_Receive.cs (Unity MonoBehaviour)
    |
    v
HandTracking.cs (Unity MonoBehaviour)
    |
    v
Unity GameObjects (Hand Points)
```

**Problems:**
- Tightly coupled to Unity
- Cannot be used with other 3D systems
- Hard to test without Unity
- UDP and parsing logic mixed with rendering

### New Architecture (Modular & Extensible)

```
OpenCV Hand Tracking (External)
    |
    | (UDP)
    v
UDPReceiver.cs (Core, no Unity dependency)
    |
    v
HandTrackingParser.cs (Core, no Unity dependency)
    |
    v
HandTrackingController.cs (Core, coordinates everything)
    |
    v
I3DRenderer Interface (Abstraction layer)
    |
    +----> UnityHandRenderer (Unity implementation)
    |
    +----> YourCustomRenderer (Your 3D system)
    |
    +----> ConsoleRenderer (Testing/debugging)
```

**Benefits:**
- Core functionality independent of Unity
- Works with any 3D system via interface
- Easy to test and debug
- Clean separation of concerns
- Reusable across projects

## Code Comparison

### Old: UDP_Receive.cs (Unity-Dependent)

```csharp
using UnityEngine;
using System.Net.Sockets;

public class UDP_Receive : MonoBehaviour
{
    Thread receiveThread;
    UdpClient client;
    public int port = 5032;
    public bool startReceiving = true;
    public string data;  // Shared state - not thread-safe!
    
    public void Start()
    {
        receiveThread = new Thread(ReceiveData);
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }
    
    private void ReceiveData()
    {
        client = new UdpClient(port);
        while (startReceiving)
        {
            // Receive and store in public field
            data = Encoding.UTF8.GetString(dataByte);
            if (printToConsole) { print(data); }
        }
    }
}
```

### New: UDPReceiver.cs (Core, Unity-Independent)

```csharp
using System;
using System.Net.Sockets;

namespace HandTrackingCore
{
    public class UDPReceiver : IDisposable
    {
        private Thread receiveThread;
        private UdpClient client;
        
        public event Action<string> OnDataReceived;  // Event-based, thread-safe
        public event Action<Exception> OnError;
        
        public void StartReceiving() { /* ... */ }
        public void StopReceiving() { /* ... */ }
        
        private void ReceiveData()
        {
            // Properly raises events instead of storing in field
            OnDataReceived?.Invoke(data);
        }
        
        public void Dispose() { /* Proper cleanup */ }
    }
}
```

### Old: HandTracking.cs (Unity-Dependent)

```csharp
using UnityEngine;

public class HandTracking : MonoBehaviour
{
    public UDP_Receive uDP_Receive;
    public GameObject[] handPoints;
    
    void Update()
    {
        string data = uDP_Receive.data;  // Polling shared state
        
        // Manual parsing
        data = data.Remove(0, 1);
        data = data.Remove(data.Length - 1, 1);
        string[] points = data.Split(',');
        
        // Direct Unity calls
        for (int i = 0; i < 21; i++)
        {
            float x = 7 - float.Parse(points[i * 3]) / 100;
            float y = float.Parse(points[i * 3 + 1]) / 100;
            float z = float.Parse(points[i * 3 + 2]) / 100;
            
            handPoints[i].transform.localPosition = new Vector3(x, y, z);
        }
    }
}
```

### New: Separated Concerns

#### Core Parsing (Unity-Independent)
```csharp
namespace HandTrackingCore
{
    public class HandTrackingParser
    {
        public static HandTrackingData Parse(string data)
        {
            // Clean, testable parsing logic
            // Returns structured data
            // No Unity dependencies
        }
    }
}
```

#### Unity Renderer (Unity-Specific)
```csharp
using UnityEngine;
using HandTrackingCore;

public class UnityHandRenderer : MonoBehaviour, I3DRenderer
{
    public GameObject[] handPoints;
    
    public void UpdateHandPoint(int pointIndex, Point3D position)
    {
        // Unity-specific rendering
        float x = xOffset - position.X / coordinateScale;
        float y = position.Y / coordinateScale;
        float z = position.Z / coordinateScale;
        
        handPoints[pointIndex].transform.localPosition = new Vector3(x, y, z);
    }
}
```

#### Controller (Coordinates Everything)
```csharp
namespace HandTrackingCore
{
    public class HandTrackingController : IDisposable
    {
        private UDPReceiver udpReceiver;
        private I3DRenderer renderer;
        
        public HandTrackingController(I3DRenderer renderer, int port)
        {
            this.renderer = renderer;
            this.udpReceiver = new UDPReceiver(port);
            udpReceiver.OnDataReceived += HandleDataReceived;
        }
        
        private void HandleDataReceived(string data)
        {
            if (HandTrackingParser.TryParse(data, out var handData))
            {
                for (int i = 0; i < 21; i++)
                {
                    renderer.UpdateHandPoint(i, handData.GetPoint(i));
                }
            }
        }
    }
}
```

## Migration Steps

### For Unity Projects (Backward Compatible)

**Option 1: Keep using old scripts (no changes needed)**
- `UDP_Receive.cs` and `HandTracking.cs` still work
- No breaking changes

**Option 2: Migrate to new system (recommended)**

1. Add new components to your GameObject:
   ```csharp
   // Remove old components (optional - they can coexist)
   // - UDP_Receive
   // - HandTracking
   
   // Add new components
   - HandTrackingManager
   - UnityHandRenderer
   ```

2. Configure UnityHandRenderer:
   - Assign your 21 hand point GameObjects to the `handPoints` array
   - Adjust `coordinateScale` and `xOffset` if needed

3. Configure HandTrackingManager:
   - Set the UDP port
   - Assign the UnityHandRenderer to the `renderer` field

4. Done! The new system will now handle everything.

### For Non-Unity Projects

1. Copy the Core library files to your project:
   - `UDPReceiver.cs`
   - `HandTrackingParser.cs`
   - `I3DRenderer.cs`

2. Implement `I3DRenderer` for your 3D system:
   ```csharp
   public class MyRenderer : I3DRenderer
   {
       public void Initialize(int numHandPoints) { /* Setup */ }
       public void UpdateHandPoint(int pointIndex, Point3D position) { /* Update 3D object */ }
       public void UpdateHandLine(int start, int end) { /* Draw line */ }
       public void Cleanup() { /* Cleanup */ }
   }
   ```

3. Create and use the controller:
   ```csharp
   var renderer = new MyRenderer();
   var controller = new HandTrackingController(renderer, 5032);
   controller.Start();
   ```

## Key Improvements

### 1. Thread Safety
- **Old**: Shared mutable state (`data` field) accessed from multiple threads
- **New**: Event-based communication, thread-safe by design

### 2. Testability
- **Old**: Hard to test without Unity runtime
- **New**: Core logic is pure C#, easy to unit test

### 3. Separation of Concerns
- **Old**: UDP, parsing, and rendering mixed together
- **New**: Each component has single responsibility

### 4. Extensibility
- **Old**: Cannot use with other 3D systems
- **New**: Works with any system implementing `I3DRenderer`

### 5. Error Handling
- **Old**: Errors printed to console, no structured handling
- **New**: Proper exception handling with error events

### 6. Resource Management
- **Old**: No explicit cleanup, relies on Unity's lifecycle
- **New**: Implements IDisposable for proper resource cleanup

## Performance Considerations

### Old Implementation
- Polling in `Update()` (60+ times per second)
- String manipulation every frame
- Parsing happens on main thread

### New Implementation
- Event-driven (only processes when data arrives)
- Parsing happens on background thread
- Main thread only updates 3D objects
- More efficient overall

## Backward Compatibility

The old scripts (`UDP_Receive.cs` and `HandTracking.cs`) remain in the project for backward compatibility. You can:

1. **Continue using the old system** - It still works as before
2. **Use both systems** - They can coexist in the same project
3. **Migrate gradually** - Test the new system alongside the old one

## Recommended Next Steps

1. **For Unity users**: Try the new system in a test scene
2. **For other 3D systems**: Implement `I3DRenderer` for your platform
3. **For testing**: Use the console application example to verify data flow
4. **For production**: Migrate to the new system for better maintainability

## Getting Help

If you encounter issues during migration:
1. Check the README.md for detailed documentation
2. Review the EXAMPLE_CONSOLE.md for standalone usage
3. Compare your setup with the working old implementation
4. Ensure UDP data format matches: `[x1,y1,z1,...,x21,y21,z21]`
