using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;
using RosMessageTypes.Std;
using RosMessageTypes.Geometry;

public class LidarPointCloudPublisher : MonoBehaviour
{
    [Header("ROS Settings")]
    public string topicName = "/lidar/pointcloud";
    public float publishFrequency = 10f; // 10 Hz
    
    [Header("Lidar Reference")]
    public LidarMid360 lidarSensor;
    
    [Header("Point Cloud Settings")]
    public int maxPointsPerMessage = 100000; // 성능을 위한 최대 포인트 수 제한
    public bool enablePointCloudPublishing = true;
    
    [Header("Coordinate System")]
    public bool useLidarFrame = true; // true: lidar_link, false: base_link
    public string frameId = "lidar_link";
    
    [Header("Debug")]
    public bool enableDebugLogging = true;
    public float logInterval = 2f; // seconds
    
    private ROSConnection ros;
    private Coroutine publishCoroutine;
    private Coroutine logCoroutine;
    private int publishedMessageCount = 0;
    private float lastPublishTime = 0f;
    
    void Start()
    {
        // ROS 연결 초기화
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<PointCloud2Msg>(topicName);
        
        // LidarMid360 컴포넌트 찾기
        if (lidarSensor == null)
        {
            lidarSensor = GetComponent<LidarMid360>();
            if (lidarSensor == null)
            {
                lidarSensor = FindObjectOfType<LidarMid360>();
            }
        }
        
        if (lidarSensor == null)
        {
            Debug.LogError("LidarPointCloudPublisher: LidarMid360 component not found! Please assign a lidar sensor.");
            enabled = false;
            return;
        }
        
        Debug.Log($"LidarPointCloudPublisher initialized - Topic: {topicName}, Frequency: {publishFrequency}Hz, Frame: {frameId}");
        
        // 발행 시작
        if (enablePointCloudPublishing)
        {
            StartPublishing();
        }
        
        // 로깅 시작
        if (enableDebugLogging)
        {
            StartLogging();
        }
    }
    
    public void StartPublishing()
    {
        if (publishCoroutine != null)
        {
            StopCoroutine(publishCoroutine);
        }
        publishCoroutine = StartCoroutine(PublishPointCloudRoutine());
        enablePointCloudPublishing = true;
    }
    
    public void StopPublishing()
    {
        if (publishCoroutine != null)
        {
            StopCoroutine(publishCoroutine);
            publishCoroutine = null;
        }
        enablePointCloudPublishing = false;
    }
    
    public void StartLogging()
    {
        if (logCoroutine != null)
        {
            StopCoroutine(logCoroutine);
        }
        logCoroutine = StartCoroutine(LogRoutine());
        enableDebugLogging = true;
    }
    
    public void StopLogging()
    {
        if (logCoroutine != null)
        {
            StopCoroutine(logCoroutine);
            logCoroutine = null;
        }
        enableDebugLogging = false;
    }
    
    private IEnumerator PublishPointCloudRoutine()
    {
        while (true)
        {
            PublishPointCloud();
            yield return new WaitForSeconds(1f / publishFrequency);
        }
    }
    
    private IEnumerator LogRoutine()
    {
        while (true)
        {
            LogPublisherStatus();
            yield return new WaitForSeconds(logInterval);
        }
    }
    
    private void PublishPointCloud()
    {
        if (lidarSensor == null || !lidarSensor.IsScanning()) return;
        
        // LidarMid360에서 포인트 클라우드 데이터 가져오기
        List<Vector3> pointCloudData = lidarSensor.GetPointCloudData();
        
        if (pointCloudData.Count == 0)
        {
            if (enableDebugLogging)
            {
                Debug.LogWarning("[LIDAR PUBLISHER] No point cloud data available for publishing");
            }
            return;
        }
        
        // 포인트 수 제한 (성능 최적화)
        int pointsToPublish = Mathf.Min(pointCloudData.Count, maxPointsPerMessage);
        
        // PointCloud2 메시지 생성
        var pointCloudMsg = CreatePointCloud2Message(pointCloudData, pointsToPublish);
        
        // ROS로 발행
        ros.Publish(topicName, pointCloudMsg);
        
        // 통계 업데이트
        publishedMessageCount++;
        lastPublishTime = Time.time;
        
        if (enableDebugLogging)
        {
            Debug.Log($"[LIDAR PUBLISHER] Published point cloud: {pointsToPublish} points (Message #{publishedMessageCount})");
        }
    }
    
    private PointCloud2Msg CreatePointCloud2Message(List<Vector3> pointCloudData, int pointCount)
    {
        // PointCloud2 메시지 구조 설정
        var pointCloudMsg = new PointCloud2Msg
        {
            header = new HeaderMsg
            {
                stamp = new RosMessageTypes.BuiltinInterfaces.TimeMsg
                {
                    sec = (int)Time.time,
                    nanosec = (uint)((Time.time - (int)Time.time) * 1e9)
                },
                frame_id = frameId
            },
            height = 1, // 1D point cloud
            width = (uint)pointCount,
            fields = new PointFieldMsg[3], // x, y, z
            is_bigendian = false,
            point_step = 12, // 3 floats * 4 bytes = 12 bytes per point
            row_step = (uint)(pointCount * 12),
            data = new byte[pointCount * 12] // 12 bytes per point
        };
        
        // PointField 설정 (x, y, z 좌표)
        pointCloudMsg.fields[0] = new PointFieldMsg
        {
            name = "x",
            offset = 0,
            datatype = 7, // FLOAT32
            count = 1
        };
        
        pointCloudMsg.fields[1] = new PointFieldMsg
        {
            name = "y", 
            offset = 4,
            datatype = 7, // FLOAT32
            count = 1
        };
        
        pointCloudMsg.fields[2] = new PointFieldMsg
        {
            name = "z",
            offset = 8,
            datatype = 7, // FLOAT32
            count = 1
        };
        
        // 포인트 데이터 변환 및 저장
        for (int i = 0; i < pointCount; i++)
        {
            Vector3 unityPoint = pointCloudData[i];
            Vector3 rosPoint = ConvertUnityToROS(unityPoint);
            
            int byteOffset = i * 12;
            
            // x 좌표 (4 bytes)
            byte[] xBytes = System.BitConverter.GetBytes(rosPoint.x);
            System.Array.Copy(xBytes, 0, pointCloudMsg.data, byteOffset, 4);
            
            // y 좌표 (4 bytes)
            byte[] yBytes = System.BitConverter.GetBytes(rosPoint.y);
            System.Array.Copy(yBytes, 0, pointCloudMsg.data, byteOffset + 4, 4);
            
            // z 좌표 (4 bytes)
            byte[] zBytes = System.BitConverter.GetBytes(rosPoint.z);
            System.Array.Copy(zBytes, 0, pointCloudMsg.data, byteOffset + 8, 4);
        }
        
        return pointCloudMsg;
    }
    
    private Vector3 ConvertUnityToROS(Vector3 unityPoint)
    {
        // Unity 좌표계 (왼손 좌표)를 ROS 좌표계 (오른손 좌표)로 변환
        // Unity: X(오른쪽), Y(위), Z(앞)
        // ROS: X(앞), Y(왼쪽), Z(위)
        
        return new Vector3(
            unityPoint.z,   // Unity Z -> ROS X (앞쪽)
            -unityPoint.x,  // Unity -X -> ROS Y (왼쪽)
            unityPoint.y    // Unity Y -> ROS Z (위쪽)
        );
    }
    
    private void LogPublisherStatus()
    {
        Debug.Log($"[LIDAR PUBLISHER] === Status Report ===");
        Debug.Log($"[LIDAR PUBLISHER] Topic: {topicName}");
        Debug.Log($"[LIDAR PUBLISHER] Frame ID: {frameId}");
        Debug.Log($"[LIDAR PUBLISHER] Publish Frequency: {publishFrequency} Hz");
        Debug.Log($"[LIDAR PUBLISHER] Max Points per Message: {maxPointsPerMessage}");
        Debug.Log($"[LIDAR PUBLISHER] Published Messages: {publishedMessageCount}");
        Debug.Log($"[LIDAR PUBLISHER] Last Publish Time: {lastPublishTime:F2}s");
        Debug.Log($"[LIDAR PUBLISHER] Is Publishing: {IsPublishing()}");
        Debug.Log($"[LIDAR PUBLISHER] Lidar Scanning: {lidarSensor != null && lidarSensor.IsScanning()}");
        
        if (lidarSensor != null)
        {
            Debug.Log($"[LIDAR PUBLISHER] Current Point Count: {lidarSensor.GetPointCount()}");
            Debug.Log($"[LIDAR PUBLISHER] Last Scan Time: {lidarSensor.GetLastScanTime():F2}s");
        }
        
        Debug.Log($"[LIDAR PUBLISHER] === End Report ===");
    }
    
    // 공개 메서드들
    public void SetPublishFrequency(float frequency)
    {
        publishFrequency = Mathf.Clamp(frequency, 1f, 30f);
        if (Application.isPlaying && enablePointCloudPublishing)
        {
            StartPublishing(); // 코루틴 재시작
        }
    }
    
    public void SetMaxPointsPerMessage(int maxPoints)
    {
        maxPointsPerMessage = Mathf.Clamp(maxPoints, 100, 500000);
    }
    
    public void SetFrameId(string newFrameId)
    {
        frameId = newFrameId;
    }
    
    public bool IsPublishing()
    {
        return publishCoroutine != null;
    }
    
    public int GetPublishedMessageCount()
    {
        return publishedMessageCount;
    }
    
    public float GetLastPublishTime()
    {
        return lastPublishTime;
    }
    
    void OnDestroy()
    {
        StopPublishing();
        StopLogging();
    }
    
    void OnValidate()
    {
        // 에디터에서 값 검증
        publishFrequency = Mathf.Clamp(publishFrequency, 1f, 30f);
        maxPointsPerMessage = Mathf.Clamp(maxPointsPerMessage, 100, 50000);
        logInterval = Mathf.Clamp(logInterval, 0.5f, 10f);
    }
}
