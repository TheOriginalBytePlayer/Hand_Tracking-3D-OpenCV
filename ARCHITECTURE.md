# Architecture Diagram

```
┌────────────────────────────────────────────────────────────────────────────┐
│                    HAND TRACKING SYSTEM ARCHITECTURE                        │
└────────────────────────────────────────────────────────────────────────────┘

┌────────────────────────────────────────────────────────────────────────────┐
│  EXTERNAL SYSTEM (Not in this repo)                                        │
├────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ┌──────────────────────────────────────────────┐                         │
│  │  OpenCV Hand Tracking (Python/C++)           │                         │
│  │  • Captures webcam video                      │                         │
│  │  • Detects hand landmarks (21 points)         │                         │
│  │  • Calculates 3D positions                    │                         │
│  └────────────────┬─────────────────────────────┘                         │
│                   │                                                         │
└───────────────────┼─────────────────────────────────────────────────────────┘
                    │
                    │ UDP (Port 5032)
                    │ Format: [x1,y1,z1,x2,y2,z2,...,x21,y21,z21]
                    │
                    ▼
┌────────────────────────────────────────────────────────────────────────────┐
│  CORE LAYER (Unity-Independent) - Assets/Scripts/Core/                     │
├────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ┌──────────────────────────────────────────────┐                         │
│  │  UDPReceiver.cs                              │                         │
│  │  • Listens on background thread               │                         │
│  │  • Receives UDP packets                       │                         │
│  │  • Fires OnDataReceived event                 │                         │
│  └────────────────┬─────────────────────────────┘                         │
│                   │                                                         │
│                   │ Event: OnDataReceived(string data)                     │
│                   │                                                         │
│                   ▼                                                         │
│  ┌──────────────────────────────────────────────┐                         │
│  │  HandTrackingParser.cs                       │                         │
│  │  • Parses string data                         │                         │
│  │  • Validates format                           │                         │
│  │  • Returns HandTrackingData                   │                         │
│  └────────────────┬─────────────────────────────┘                         │
│                   │                                                         │
│                   │ HandTrackingData (21 x Point3D)                        │
│                   │                                                         │
│                   ▼                                                         │
│  ┌──────────────────────────────────────────────┐                         │
│  │  HandTrackingController.cs                   │                         │
│  │  • Coordinates everything                     │                         │
│  │  • Manages lifecycle                          │                         │
│  │  • Calls I3DRenderer methods                  │                         │
│  └────────────────┬─────────────────────────────┘                         │
│                   │                                                         │
└───────────────────┼─────────────────────────────────────────────────────────┘
                    │
                    │ I3DRenderer Interface
                    │ • Initialize(numPoints)
                    │ • UpdateHandPoint(index, position)
                    │ • UpdateHandLine(start, end)
                    │ • Cleanup()
                    │
        ┌───────────┼───────────┬───────────────────┬─────────────┐
        │           │           │                   │             │
        ▼           ▼           ▼                   ▼             ▼
┌──────────┐ ┌──────────┐ ┌──────────┐      ┌──────────┐  ┌──────────┐
│  Unity   │ │ Console  │ │  Unreal  │      │ Web App  │  │  Your    │
│ Renderer │ │ Renderer │ │ Renderer │ ...  │ Renderer │  │ Custom   │
└──────────┘ └──────────┘ └──────────┘      └──────────┘  │ System   │
                                                            └──────────┘

┌────────────────────────────────────────────────────────────────────────────┐
│  UNITY ADAPTER LAYER - Assets/Scripts/                                     │
├────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ┌──────────────────────────────────────────────┐                         │
│  │  UnityHandRenderer.cs (MonoBehaviour)        │                         │
│  │  implements I3DRenderer                       │                         │
│  │  • Manages 21 hand point GameObjects          │                         │
│  │  • Transforms coordinates to Unity space      │                         │
│  │  • Updates GameObject positions               │                         │
│  └──────────────────────────────────────────────┘                         │
│                             ▲                                               │
│                             │                                               │
│  ┌──────────────────────────┴──────────────────┐                         │
│  │  HandTrackingManager.cs (MonoBehaviour)     │                         │
│  │  • Unity entry point                         │                         │
│  │  • Creates HandTrackingController             │                         │
│  │  • Manages Unity lifecycle                    │                         │
│  └──────────────────────────────────────────────┘                         │
│                                                                             │
└────────────────────────────────────────────────────────────────────────────┘
                                │
                                │ Updates
                                ▼
┌────────────────────────────────────────────────────────────────────────────┐
│  UNITY SCENE                                                                │
├────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  GameObject (Hand Tracking System)                                         │
│  ├── HandTrackingManager (Component)                                       │
│  └── UnityHandRenderer (Component)                                         │
│      ├── handPoints[0] → Sphere (Thumb tip)                               │
│      ├── handPoints[1] → Sphere (Thumb IP)                                │
│      ├── handPoints[2] → Sphere (Thumb MCP)                               │
│      ├── ...                                                               │
│      └── handPoints[20] → Sphere (Pinky tip)                              │
│                                                                             │
│  LineRenderer Components (between hand points)                             │
│  • Draw skeleton lines                                                     │
│  • Visualize hand structure                                                │
│                                                                             │
└────────────────────────────────────────────────────────────────────────────┘


┌────────────────────────────────────────────────────────────────────────────┐
│  LEGACY LAYER (Backward Compatible) - Assets/Scripts/                      │
├────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ┌──────────────────────────────────────────────┐                         │
│  │  UDP_Receive.cs (MonoBehaviour)              │ ← Original               │
│  │  • Original Unity-dependent UDP receiver      │   Implementation         │
│  └──────────────────────────────────────────────┘   (Still works!)        │
│                             │                                               │
│                             ▼                                               │
│  ┌──────────────────────────────────────────────┐                         │
│  │  HandTracking.cs (MonoBehaviour)             │                         │
│  │  • Original hand tracking implementation      │                         │
│  │  • Parses and updates GameObjects directly    │                         │
│  └──────────────────────────────────────────────┘                         │
│                                                                             │
│  ┌──────────────────────────────────────────────┐                         │
│  │  LineCode.cs (MonoBehaviour)                 │                         │
│  │  • Draws lines between hand points            │                         │
│  └──────────────────────────────────────────────┘                         │
│                                                                             │
└────────────────────────────────────────────────────────────────────────────┘


DATA FLOW EXAMPLE:
═════════════════

1. OpenCV detects hand → Sends "[100,200,300,110,210,310,...]" via UDP

2. UDPReceiver receives → Fires event with string data

3. HandTrackingController handles event → Calls parser

4. HandTrackingParser parses → Returns HandTrackingData with 21 Point3D objects

5. HandTrackingController loops through points → Calls renderer.UpdateHandPoint(i, point)

6. Your I3DRenderer implementation (Unity/Custom/etc.) → Updates 3D objects


KEY BENEFITS:
════════════

✅ Core is Unity-independent (can run in console app, web app, etc.)
✅ Clean separation: networking → parsing → rendering
✅ Easy to test each layer independently
✅ Thread-safe event-based architecture
✅ Type-safe structured data (no raw strings in your code)
✅ Extensible via I3DRenderer interface
✅ Backward compatible (old code still works)


INTEGRATION POINTS:
═══════════════════

1. Unity: Use HandTrackingManager + UnityHandRenderer
2. Console: Implement I3DRenderer, print to console
3. Web: Implement I3DRenderer, send via WebSocket/SignalR
4. Unreal: Implement I3DRenderer in C++, convert coordinates
5. Custom: Implement I3DRenderer for your 3D system
```
