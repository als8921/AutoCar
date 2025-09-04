using System.Collections;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;

public class PosePublisher : MonoBehaviour
{
    [Header("ROS Settings")]
    public string topicName = "/object_pose";
    public float publishFrequency = 10f;
    
    [Header("Target Object")]
    public Transform targetObject;
    
    [Header("Coordinate System")]
    public bool useWorldCoordinates = true;
    
    private ROSConnection ros;
    private Coroutine publishCoroutine;
    
    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<PoseStampedMsg>(topicName);
        
        if (targetObject == null)
            targetObject = transform;
            
        StartPublishing();
    }
    
    public void StartPublishing()
    {
        if (publishCoroutine != null)
        {
            StopCoroutine(publishCoroutine);
        }
        publishCoroutine = StartCoroutine(PublishPoseRoutine());
    }
    
    public void StopPublishing()
    {
        if (publishCoroutine != null)
        {
            StopCoroutine(publishCoroutine);
            publishCoroutine = null;
        }
    }
    
    private IEnumerator PublishPoseRoutine()
    {
        while (true)
        {
            PublishPose();
            yield return new WaitForSeconds(1f / publishFrequency);
        }
    }
    
    private void PublishPose()
    {
        if (targetObject == null) return;
        
        Vector3 position = targetObject.position;
        Quaternion rotation = targetObject.rotation;
        
        var poseMsg = new PoseStampedMsg
        {
            header = new RosMessageTypes.Std.HeaderMsg
            {
                stamp = new RosMessageTypes.BuiltinInterfaces.TimeMsg
                {
                    sec = (int)Time.time,
                    nanosec = (uint)((Time.time - (int)Time.time) * 1e9)
                },
                frame_id = useWorldCoordinates ? "world" : "base_link"
            },
            pose = new PoseMsg
            {
                position = new PointMsg
                {
                    x = position.z,  // 앞쪽 (Unity z축)
                    y = -position.x, // 왼쪽 (Unity -x축)
                    z = position.y   // 위쪽 (Unity y축)
                },
                orientation = new QuaternionMsg
                {
                    x = rotation.z,
                    y = -rotation.x,
                    z = rotation.y,
                    w = rotation.w
                }
            }
        };
        
        ros.Publish(topicName, poseMsg);
    }
    
    void OnDestroy()
    {
        StopPublishing();
    }
}