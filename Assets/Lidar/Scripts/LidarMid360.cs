using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LidarMid360 : MonoBehaviour
{
    [Header("Lidar Settings")]
    [Range(5f, 260f)]
    public float maxRange = 260f; // Mid360's max range is 260 meters
    [Range(0.05f, 40f)]
    public float minRange = 0.05f;
    [Range(1, 50)]
    public int scanFrequency = 10; // Hz
    
    [Header("3D Scan Pattern")]
    [Range(0.1f, 2f)]
    public float horizontalAngularResolution = 1.0f; // degrees
    [Range(0.1f, 2f)]
    public float verticalAngularResolution = 1.0f; // degrees
    [Range(-45f, 0f)]
    public float minVerticalAngle = -30f; // degrees (down)
    [Range(0f, 45f)]
    public float maxVerticalAngle = 30f; // degrees (up)
    
    private int horizontalSteps;
    private int verticalSteps;
    private int totalPointsPerScan;

    [Header("Visualization")]
    public bool showPointCloud = true;
    public Material lidarPointMaterial;
    public float pointSize = 0.02f;
    public Color pointColor = Color.red;
    public bool showLaserRays = false;
    public LineRenderer laserLineRenderer;

    [Header("Performance")]
    public int maxVisualizationPoints = 5000;
    public LayerMask scanLayerMask = -1;

    [Header("Debug")]
    public bool enableDataLogging = true;
    public float logInterval = 1f; // seconds

    [HideInInspector]
    public List<Vector3> pointCloudData = new List<Vector3>();
    private List<GameObject> visualizationPoints = new List<GameObject>();
    private Transform lidarTransform;
    private Coroutine scanCoroutine;
    private Coroutine logCoroutine;
    [HideInInspector]
    public float lastScanTime = 0f;

    void Start()
    {
        lidarTransform = transform;
        
        // Calculate scan pattern
        CalculateScanPattern();
        
        // Create material for points if not assigned
        if (lidarPointMaterial == null)
        {
            lidarPointMaterial = new Material(Shader.Find("Sprites/Default"));
            lidarPointMaterial.color = pointColor;
        }

        // Start scanning
        StartScanning();
        
        // Start logging if enabled
        if (enableDataLogging)
        {
            StartLogging();
        }
    }

    private void CalculateScanPattern()
    {
        horizontalSteps = Mathf.RoundToInt(360f / horizontalAngularResolution);
        float verticalRange = maxVerticalAngle - minVerticalAngle;
        verticalSteps = Mathf.RoundToInt(verticalRange / verticalAngularResolution) + 1;
        totalPointsPerScan = horizontalSteps * verticalSteps;
        
        Debug.Log($"[LIDAR] 3D Scan Pattern: {horizontalSteps}H × {verticalSteps}V = {totalPointsPerScan} total points");
        Debug.Log($"[LIDAR] Vertical FOV: {minVerticalAngle}° to {maxVerticalAngle}° ({verticalRange}°)");
    }

    public void StartScanning()
    {
        if (scanCoroutine != null)
        {
            StopCoroutine(scanCoroutine);
        }
        scanCoroutine = StartCoroutine(ScanRoutine());
    }

    public void StopScanning()
    {
        if (scanCoroutine != null)
        {
            StopCoroutine(scanCoroutine);
            scanCoroutine = null;
        }
    }

    public void StartLogging()
    {
        if (logCoroutine != null)
        {
            StopCoroutine(logCoroutine);
        }
        logCoroutine = StartCoroutine(LogRoutine());
    }

    public void StopLogging()
    {
        if (logCoroutine != null)
        {
            StopCoroutine(logCoroutine);
            logCoroutine = null;
        }
    }

    private IEnumerator LogRoutine()
    {
        while (true)
        {
            LogLidarData();
            yield return new WaitForSeconds(logInterval);
        }
    }

    private IEnumerator ScanRoutine()
    {
        while (true)
        {
            PerformScan();
            yield return new WaitForSeconds(1f / scanFrequency);
        }
    }

    private void PerformScan()
    {
        pointCloudData.Clear();
        
        int hitCount = 0;
        int rayIndex = 0;
        
        // 3D 스캔: 수직 각도별로 수평 360도 스캔
        for (int v = 0; v < verticalSteps; v++)
        {
            float verticalAngle = minVerticalAngle + (v * verticalAngularResolution);
            
            for (int h = 0; h < horizontalSteps; h++)
            {
                float horizontalAngle = h * horizontalAngularResolution;
                
                // 3D 방향 벡터 계산 (수평 회전 + 수직 회전)
                Vector3 direction = Quaternion.Euler(verticalAngle, horizontalAngle, 0) * Vector3.forward;
                Vector3 worldDirection = lidarTransform.TransformDirection(direction);
                
                RaycastHit hit;
                if (Physics.Raycast(lidarTransform.position, worldDirection, out hit, maxRange, scanLayerMask))
                {
                    float distance = hit.distance;
                    if (distance >= minRange && distance <= maxRange)
                    {
                        pointCloudData.Add(hit.point);
                        hitCount++;
                    }
                }
                
                // Debug every 500th ray for performance
                if (enableDataLogging && rayIndex % 500 == 0)
                {
                    bool didHit = Physics.Raycast(lidarTransform.position, worldDirection, out hit, maxRange, scanLayerMask);
                    Debug.Log($"[LIDAR DEBUG] Ray {rayIndex}: H={horizontalAngle:F1}° V={verticalAngle:F1}° " +
                             $"direction={worldDirection}, hit={didHit}, " +
                             (didHit ? $"distance={hit.distance:F2}m, object={hit.collider.name}" : "no hit"));
                }
                
                rayIndex++;
            }
        }
        
        // Log scan results
        if (enableDataLogging)
        {
            Debug.Log($"[LIDAR SCAN] 3D Scan completed: {hitCount}/{totalPointsPerScan} rays hit objects");
            Debug.Log($"[LIDAR SCAN] Scan pattern: {horizontalSteps}H × {verticalSteps}V steps");
            Debug.Log($"[LIDAR SCAN] Vertical FOV: {minVerticalAngle}° to {maxVerticalAngle}°");
            Debug.Log($"[LIDAR SCAN] Position: {lidarTransform.position}, Points found: {pointCloudData.Count}");
        }

        if (showPointCloud)
        {
            UpdateVisualization();
        }

        lastScanTime = Time.time;
    }

    private void LogLidarData()
    {
        Debug.Log($"[LIDAR] === Lidar Data Report ===");
        Debug.Log($"[LIDAR] Timestamp: {Time.time:F2}s");
        Debug.Log($"[LIDAR] Position: {transform.position}");
        Debug.Log($"[LIDAR] Rotation: {transform.rotation.eulerAngles}");
        Debug.Log($"[LIDAR] Point Count: {pointCloudData.Count}");
        Debug.Log($"[LIDAR] Scan Frequency: {scanFrequency} Hz");
        Debug.Log($"[LIDAR] Range: {minRange}m - {maxRange}m");
        Debug.Log($"[LIDAR] Last Scan Time: {lastScanTime:F2}s");
        Debug.Log($"[LIDAR] Is Scanning: {IsScanning()}");
        
        if (pointCloudData.Count > 0)
        {
            // Log some sample points
            int sampleCount = Mathf.Min(5, pointCloudData.Count);
            Debug.Log($"[LIDAR] Sample Points ({sampleCount} of {pointCloudData.Count}):");
            for (int i = 0; i < sampleCount; i++)
            {
                Vector3 point = pointCloudData[i];
                float distance = Vector3.Distance(transform.position, point);
                Debug.Log($"[LIDAR]   Point {i}: {point} (Distance: {distance:F2}m)");
            }
            
            // Calculate statistics
            float minDistance = float.MaxValue;
            float maxDistance = 0f;
            float avgDistance = 0f;
            
            foreach (Vector3 point in pointCloudData)
            {
                float dist = Vector3.Distance(transform.position, point);
                minDistance = Mathf.Min(minDistance, dist);
                maxDistance = Mathf.Max(maxDistance, dist);
                avgDistance += dist;
            }
            avgDistance /= pointCloudData.Count;
            
            Debug.Log($"[LIDAR] Distance Stats - Min: {minDistance:F2}m, Max: {maxDistance:F2}m, Avg: {avgDistance:F2}m");
        }
        else
        {
            Debug.LogWarning("[LIDAR] No point cloud data available!");
        }
        
        Debug.Log($"[LIDAR] === End Report ===");
    }

    private void UpdateVisualization()
    {
        // Clear old visualization points
        foreach (GameObject point in visualizationPoints)
        {
            if (point != null)
                DestroyImmediate(point);
        }
        visualizationPoints.Clear();

        // Create new visualization points (limited for performance)
        int pointsToShow = Mathf.Min(pointCloudData.Count, maxVisualizationPoints);
        for (int i = 0; i < pointsToShow; i++)
        {
            GameObject pointObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pointObj.name = "LidarPoint_" + i;
            pointObj.transform.position = pointCloudData[i];
            pointObj.transform.localScale = Vector3.one * pointSize;
            pointObj.transform.SetParent(this.transform);
            
            // Remove collider for performance
            Collider collider = pointObj.GetComponent<Collider>();
            if (collider != null)
                DestroyImmediate(collider);
            
            // Apply material
            Renderer renderer = pointObj.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = lidarPointMaterial;
            }
            
            visualizationPoints.Add(pointObj);
        }
    }

    void OnDrawGizmos()
    {
        if (!showLaserRays || !Application.isPlaying) return;

        Gizmos.color = Color.green;
        
        // Draw horizontal range circles at different vertical angles
        DrawWireCircle(transform.position, maxRange, 32); // Center
        
        // Draw vertical FOV boundaries
        Vector3 upDirection = Quaternion.Euler(maxVerticalAngle, 0, 0) * Vector3.forward;
        Vector3 downDirection = Quaternion.Euler(minVerticalAngle, 0, 0) * Vector3.forward;
        
        // Draw upper and lower FOV circles
        Vector3 upperPos = transform.position + transform.TransformDirection(upDirection) * maxRange * 0.5f;
        Vector3 lowerPos = transform.position + transform.TransformDirection(downDirection) * maxRange * 0.5f;
        
        Gizmos.color = Color.yellow;
        DrawWireCircle(upperPos, maxRange * 0.3f, 16);
        DrawWireCircle(lowerPos, maxRange * 0.3f, 16);
        
        // Draw sample 3D rays
        Gizmos.color = Color.red;
        for (int v = 0; v < 3; v++) // 3 vertical samples
        {
            float vAngle = minVerticalAngle + (v * (maxVerticalAngle - minVerticalAngle) / 2f);
            
            for (int h = 0; h < 12; h++) // 12 horizontal samples (every 30 degrees)
            {
                float hAngle = h * 30f;
                Vector3 direction = Quaternion.Euler(vAngle, hAngle, 0) * Vector3.forward;
                Vector3 worldDir = transform.TransformDirection(direction) * maxRange * 0.8f;
                Gizmos.DrawRay(transform.position, worldDir);
            }
        }
        
        // Draw FOV cone outline
        Gizmos.color = Color.cyan;
        for (int i = 0; i < 8; i++)
        {
            float angle = i * 45f;
            Vector3 topRay = Quaternion.Euler(maxVerticalAngle, angle, 0) * Vector3.forward;
            Vector3 bottomRay = Quaternion.Euler(minVerticalAngle, angle, 0) * Vector3.forward;
            
            Gizmos.DrawRay(transform.position, transform.TransformDirection(topRay) * maxRange);
            Gizmos.DrawRay(transform.position, transform.TransformDirection(bottomRay) * maxRange);
        }
    }

    // Public methods for accessing lidar data
    public List<Vector3> GetPointCloudData()
    {
        return new List<Vector3>(pointCloudData);
    }

    public int GetPointCount()
    {
        return pointCloudData.Count;
    }

    public float GetLastScanTime()
    {
        return lastScanTime;
    }

    public bool IsScanning()
    {
        return scanCoroutine != null;
    }

    void OnDestroy()
    {
        StopScanning();
        StopLogging();
        
        // Clean up visualization points
        foreach (GameObject point in visualizationPoints)
        {
            if (point != null)
                DestroyImmediate(point);
        }
    }

    private void DrawWireCircle(Vector3 center, float radius, int segments)
    {
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + new Vector3(radius, 0, 0);
        
        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }
}