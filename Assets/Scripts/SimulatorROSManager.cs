using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using System.Reflection;
using System;

public class SimulatorROSManager : MonoBehaviour
{
    [Header("ROS Connection")]
    public ROSConnection rosConnectionComponent; // Inspector에서 ROSConnector 할당
    
    [Header("Settings")]
    public bool autoConnect = true;
    
    private bool isConnected = false;
    
    void Start()
    {
        // ROSConnection 컴포넌트 자동 찾기
        if (rosConnectionComponent == null)
        {
            rosConnectionComponent = FindObjectOfType<ROSConnection>();
        }
        
        if (rosConnectionComponent == null)
        {
            Debug.LogError("ROSConnection component not found! Please add ROSConnector to the scene.");
            return;
        }
        
        // 설정 적용
        ApplyROSSettings();
        
        if (autoConnect)
        {
            ConnectToROS();
        }
    }
    
    void ApplyROSSettings()
    {
        // ROSSettingsManager에서 설정 로드
        ROSSettingsManager.Instance.LoadROSSettings();
        
        string ip = ROSSettingsManager.Instance.rosIPAddress;
        int port = ROSSettingsManager.Instance.rosPort;
        
        Debug.Log($"Applying ROS settings: {ip}:{port}");
        
        try
        {
            // Reflection을 사용하여 ROSConnection의 IP와 Port 설정
            Type rosType = typeof(ROSConnection);
            
            FieldInfo ipField = rosType.GetField("m_RosIPAddress", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo portField = rosType.GetField("m_RosPort", BindingFlags.NonPublic | BindingFlags.Instance);
            
            if (ipField != null)
            {
                ipField.SetValue(rosConnectionComponent, ip);
                Debug.Log($"ROS IP set to: {ip}");
            }
            else
            {
                Debug.LogWarning("Could not find ROS IP field");
            }
            
            if (portField != null)
            {
                portField.SetValue(rosConnectionComponent, port);
                Debug.Log($"ROS Port set to: {port}");
            }
            else
            {
                Debug.LogWarning("Could not find ROS Port field");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to apply ROS settings: {e.Message}");
        }
    }
    
    void ConnectToROS()
    {
        try
        {
            // ROSConnection 활성화
            Type rosType = typeof(ROSConnection);
            
            // ConnectOnStart를 true로 설정
            FieldInfo connectOnStartField = rosType.GetField("m_ConnectOnStart", BindingFlags.NonPublic | BindingFlags.Instance);
            if (connectOnStartField != null)
            {
                connectOnStartField.SetValue(rosConnectionComponent, true);
            }
            
            // Start 메서드 호출하여 연결 시작
            MethodInfo startMethod = rosType.GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance);
            if (startMethod != null)
            {
                startMethod.Invoke(rosConnectionComponent, null);
            }
            
            isConnected = true;
            Debug.Log("ROS connection initiated");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to connect to ROS: {e.Message}");
        }
    }
    
    public bool IsConnected()
    {
        return isConnected;
    }
    
    public string GetConnectionInfo()
    {
        return $"{ROSSettingsManager.Instance.rosIPAddress}:{ROSSettingsManager.Instance.rosPort}";
    }
}