using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarLidarIntegration : MonoBehaviour
{
    [Header("Lidar Settings")]
    public GameObject lidarPrefab;
    public Vector3 lidarMountPosition = new Vector3(0, 1.5f, 0);
    public Vector3 lidarMountRotation = new Vector3(0, 0, 0);
    
    [Header("References")]
    public PrometeoCarController carController;
    
    private GameObject lidarInstance;
    private LidarMid360 lidarScript;

    void Start()
    {
        // Find car controller if not assigned
        if (carController == null)
        {
            carController = GetComponent<PrometeoCarController>();
        }

        // Install the lidar
        InstallLidar();
    }

    public void InstallLidar()
    {
        if (lidarPrefab == null)
        {
            Debug.LogWarning("Lidar prefab not assigned. Please assign the LidarMid360 prefab to install the lidar.");
            return;
        }

        // Remove existing lidar if any
        if (lidarInstance != null)
        {
            DestroyImmediate(lidarInstance);
        }

        // Instantiate the lidar
        lidarInstance = Instantiate(lidarPrefab, transform);
        lidarInstance.transform.localPosition = lidarMountPosition;
        lidarInstance.transform.localRotation = Quaternion.Euler(lidarMountRotation);

        // Get the lidar script reference
        lidarScript = lidarInstance.GetComponent<LidarMid360>();

        if (lidarScript != null)
        {
            Debug.Log("Lidar Mid360 successfully installed on the car!");
        }
        else
        {
            Debug.LogError("LidarMid360 script not found on the prefab!");
        }
    }

    public void RemoveLidar()
    {
        if (lidarInstance != null)
        {
            DestroyImmediate(lidarInstance);
            lidarInstance = null;
            lidarScript = null;
            Debug.Log("Lidar removed from the car.");
        }
    }

    // Public methods to control the lidar
    public void StartLidarScanning()
    {
        if (lidarScript != null)
        {
            lidarScript.StartScanning();
        }
    }

    public void StopLidarScanning()
    {
        if (lidarScript != null)
        {
            lidarScript.StopScanning();
        }
    }

    public List<Vector3> GetLidarPointCloud()
    {
        if (lidarScript != null)
        {
            return lidarScript.GetPointCloudData();
        }
        return new List<Vector3>();
    }

    public bool IsLidarScanning()
    {
        if (lidarScript != null)
        {
            return lidarScript.IsScanning();
        }
        return false;
    }

    public int GetLidarPointCount()
    {
        if (lidarScript != null)
        {
            return lidarScript.GetPointCount();
        }
        return 0;
    }

    void OnValidate()
    {
        // Update lidar position in editor when values change
        if (lidarInstance != null && Application.isPlaying)
        {
            lidarInstance.transform.localPosition = lidarMountPosition;
            lidarInstance.transform.localRotation = Quaternion.Euler(lidarMountRotation);
        }
    }

    void OnDrawGizmos()
    {
        // Draw lidar mount position
        Gizmos.color = Color.cyan;
        Vector3 mountPos = transform.TransformPoint(lidarMountPosition);
        Gizmos.DrawWireCube(mountPos, new Vector3(0.2f, 0.15f, 0.2f));
        
        // Draw range circle at ground level
        Gizmos.color = Color.green;
        Vector3 groundPos = new Vector3(mountPos.x, transform.position.y, mountPos.z);
        DrawWireCircle(groundPos, 50f, 32); // Draw a 50m range indicator
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