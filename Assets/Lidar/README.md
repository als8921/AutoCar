# Lidar Mid360 for Unity AutoCar

This package adds a 3D Lidar sensor to your Unity car project, specifically designed to simulate the Livox Mid360 360-degree lidar.

## Features

- **360-degree scanning**: Full horizontal field of view
- **Realistic range**: 0.05m to 260m (Mid360 specifications)
- **Configurable parameters**: Scan frequency, angular resolution, point cloud size
- **Real-time visualization**: Point cloud display with customizable materials
- **Performance optimized**: Adjustable visualization point limits
- **Easy integration**: Simple script to add to any car

## Setup Instructions

### 1. Add Lidar to Your Car

1. Select your car GameObject in the hierarchy
2. Add the `CarLidarIntegration` component
3. Assign the `LidarMid360` prefab to the "Lidar Prefab" field
4. Adjust the mount position (default: 0, 1.5, 0 - on top of the car)

### 2. Configure Lidar Settings

In the `LidarMid360` component, you can adjust:

- **Max Range**: Maximum detection distance (default: 260m)
- **Min Range**: Minimum detection distance (default: 0.05m)  
- **Scan Frequency**: How many scans per second (default: 10 Hz)
- **Angular Resolution**: Angle between scan rays (default: 0.25°)
- **Points Per Scan**: Number of points in one 360° scan (default: 1440)

### 3. Visualization Options

- **Show Point Cloud**: Enable/disable point cloud visualization
- **Point Size**: Size of visualization points
- **Point Color**: Color of the lidar points
- **Max Visualization Points**: Limit points shown for performance

## Usage

### Runtime Control

```csharp
// Get the car's lidar integration component
CarLidarIntegration lidarIntegration = car.GetComponent<CarLidarIntegration>();

// Control scanning
lidarIntegration.StartLidarScanning();
lidarIntegration.StopLidarScanning();

// Get data
List<Vector3> pointCloud = lidarIntegration.GetLidarPointCloud();
int pointCount = lidarIntegration.GetLidarPointCount();
bool isScanning = lidarIntegration.IsLidarScanning();
```

### Direct Lidar Access

```csharp
// Get the lidar component directly
LidarMid360 lidar = FindObjectOfType<LidarMid360>();

// Access point cloud data
List<Vector3> points = lidar.GetPointCloudData();
```

## File Structure

```
Assets/Lidar/
├── Scripts/
│   ├── LidarMid360.cs              # Main lidar scanning logic
│   └── CarLidarIntegration.cs      # Car integration helper
├── Materials/
│   └── LidarPointMaterial.mat      # Material for point visualization
├── Prefabs/
│   └── LidarMid360.prefab         # Complete lidar prefab
└── README.md                       # This file
```

## Technical Specifications (Livox Mid360)

- **Range**: 0.05m - 260m
- **Accuracy**: ±2cm (simulated)
- **FOV**: 360° × 38.4°
- **Data Rate**: Up to 200,000 points/second
- **Angular Resolution**: 0.1° - 0.4° (configurable)

## Performance Tips

1. **Reduce visualization points**: Lower `maxVisualizationPoints` for better FPS
2. **Adjust scan frequency**: Lower frequency uses less CPU
3. **Use layer masks**: Limit what objects the lidar can detect
4. **Disable visualization**: Turn off point cloud display when not needed

## Troubleshooting

**No points showing up?**
- Check that there are objects within the lidar range
- Verify the scan layer mask includes the target objects
- Make sure `showPointCloud` is enabled

**Poor performance?**
- Reduce `maxVisualizationPoints`
- Lower the `scanFrequency`
- Reduce `pointsPerScan`

**Lidar not attached to car?**
- Make sure the `LidarMid360.prefab` is assigned in `CarLidarIntegration`
- Check that the car has the `PrometeoCarController` component

## Integration with Car Controller

The lidar automatically moves with the car and provides real-time environmental scanning. The point cloud data can be used for:

- Obstacle detection
- SLAM (Simultaneous Localization and Mapping)
- Autonomous navigation
- Environmental mapping
- Safety systems

## License

This lidar implementation is compatible with the existing car controller license terms.