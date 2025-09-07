using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;

public class ExternalCarController : MonoBehaviour
{
    [Header("Car Control Parameters")]
    [Range(-1f, 1f)]
    public float targetSpeed = 0f; // -1 (full reverse) to 1 (full forward)
    
    [Range(-1f, 1f)]
    public float targetSteerAngle = 0f; // -1 (full left) to 1 (full right)
    
    [Header("Control Settings")]
    public bool useExternalControl = true;
    public bool showDebugInfo = true;
    
    [Header("ROS2 Control Settings")]
    public bool useROSControl = false; // ROS 모드 활성화 여부
    public string speedTopicName = "/car_control/speed";
    public string steeringTopicName = "/car_control/steering";
    public float rosDataTimeout = 1.0f; // ROS 데이터 타임아웃 (초)
    
    [Header("Mode Switching")]
    public KeyCode modeSwitchKey = KeyCode.T; // 모드 전환 키
    
    private PrometeoCarController carController;
    private ROSConnection ros;
    private float lastROSDataTime = 0f;
    private bool rosDataReceived = false;
    
    void Start()
    {
        carController = GetComponent<PrometeoCarController>();
        if (carController == null)
        {
            Debug.LogError("ExternalCarController: PrometeoCarController not found on this GameObject!");
            enabled = false;
            return;
        }
        
        // Enable external control on the PrometeoCarController
        carController.useExternalControl = true;
        
        // 초기값을 0으로 확실히 설정
        targetSpeed = 0f;
        targetSteerAngle = 0f;
        
        // ROS 연결 초기화
        if (useROSControl)
        {
            InitializeROS();
        }
        
        Debug.Log($"ExternalCarController initialized successfully! Mode: {(useROSControl ? "ROS" : "Keyboard")}");
    }
    
    void Update()
    {
        // Handle mode switching
        HandleModeSwitching();
        
        // Update control values based on current mode
        if (useROSControl)
        {
            // ROS 모드: ROS 메시지로만 제어 (targetSpeed, targetSteerAngle은 ROS 콜백에서 설정됨)
            CheckROSDataTimeout();
        }
        else
        {
            // 키보드 모드: 키보드 입력으로 targetSpeed, targetSteerAngle 설정
            HandleKeyboardInput();
        }
        
        if (useExternalControl && carController != null)
        {
            // Apply external control
            ApplyExternalControl();
        }
        
        if (showDebugInfo)
        {
            ShowDebugInfo();
        }
    }
    
    void ApplyExternalControl()
    {
        // Set speed control
        carController.SetExternalThrottleInput(targetSpeed);
        
        // Set steering control
        carController.SetExternalSteeringInput(targetSteerAngle);
        
        // Handle handbrake (can be controlled externally if needed)
        // carController.SetExternalHandbrakeInput(false);
    }
    
    void ShowDebugInfo()
    {
        string mode = useROSControl ? "ROS" : "Keyboard";
        string rosStatus = useROSControl ? (rosDataReceived ? "Connected" : "No Data") : "N/A";
        string keyStatus = "";
        
        // 현재 눌린 키 표시
        if (Input.GetKey(KeyCode.W)) keyStatus += "W ";
        if (Input.GetKey(KeyCode.S)) keyStatus += "S ";
        if (Input.GetKey(KeyCode.A)) keyStatus += "A ";
        if (Input.GetKey(KeyCode.D)) keyStatus += "D ";
        if (string.IsNullOrEmpty(keyStatus)) keyStatus = "None";
        
        Debug.Log($"[{mode} Mode] Speed: {targetSpeed:F2}, Steer: {targetSteerAngle:F2}, Car Speed: {carController.carSpeed:F1} km/h, Keys: {keyStatus}, ROS Status: {rosStatus}");
    }
    
    // Public methods for external control
    public void SetSpeed(float speed)
    {
        targetSpeed = Mathf.Clamp(speed, -1f, 1f);
    }
    
    public void SetSteerAngle(float steerAngle)
    {
        targetSteerAngle = Mathf.Clamp(steerAngle, -1f, 1f);
    }
    
    public void SetControl(float speed, float steerAngle)
    {
        SetSpeed(speed);
        SetSteerAngle(steerAngle);
    }
    
    public float GetCurrentSpeed()
    {
        return carController != null ? carController.carSpeed : 0f;
    }
    
    public Vector3 GetVelocity()
    {
        return carController != null ? carController.GetComponent<Rigidbody>().velocity : Vector3.zero;
    }
    
    // Keyboard input handling
    void HandleKeyboardInput()
    {
        // Handle WASD input for direct control
        HandleWASDInput();
        
        // R key - Reset car position and velocity
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetCarPosition();
            ResetControlInputs();
        }
        
        // Space key - Reset control inputs
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ResetControlInputs();
        }
    }
    
    // Reset car position to origin and stop all movement
    void ResetCarPosition()
    {
        if (carController == null) return;
        
        // Reset position to origin
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        
        // Reset velocity and angular velocity
        Rigidbody rb = carController.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        
        Debug.Log("Car position and velocity reset to origin!");
    }
    
    // Reset control inputs to zero
    void ResetControlInputs()
    {
        targetSpeed = 0f;
        targetSteerAngle = 0f;
        
        Debug.Log("Control inputs reset - Speed: 0, Steer: 0");
    }
    
    // Handle WASD keyboard input for direct control
    void HandleWASDInput()
    {
        // Speed control with W/S keys
        if (Input.GetKey(KeyCode.W))
        {
            targetSpeed = 1f; // Forward
        }
        else if (Input.GetKey(KeyCode.S))
        {
            targetSpeed = -1f; // Reverse
        }
        else
        {
            // 즉시 0으로 설정 (점진적 감소 제거)
            targetSpeed = 0f;
        }
        
        // Steering control with A/D keys
        if (Input.GetKey(KeyCode.A))
        {
            targetSteerAngle = -1f; // Turn left
        }
        else if (Input.GetKey(KeyCode.D))
        {
            targetSteerAngle = 1f; // Turn right
        }
        else
        {
            // 즉시 0으로 설정 (점진적 감소 제거)
            targetSteerAngle = 0f;
        }
    }
    
    // ROS 관련 메서드들
    void InitializeROS()
    {
        try
        {
            ros = ROSConnection.GetOrCreateInstance();
            ros.Subscribe<Float32Msg>(speedTopicName, OnSpeedMessageReceived);
            ros.Subscribe<Float32Msg>(steeringTopicName, OnSteeringMessageReceived);
            
            Debug.Log($"ROS initialized - Speed Topic: {speedTopicName}, Steering Topic: {steeringTopicName}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to initialize ROS: {e.Message}");
            useROSControl = false;
        }
    }
    
    void OnSpeedMessageReceived(Float32Msg message)
    {
        targetSpeed = Mathf.Clamp(message.data, -1f, 1f);
        lastROSDataTime = Time.time;
        rosDataReceived = true;
        
        if (showDebugInfo)
        {
            Debug.Log($"[ROS] Speed received: {targetSpeed:F2}");
        }
    }
    
    void OnSteeringMessageReceived(Float32Msg message)
    {
        targetSteerAngle = Mathf.Clamp(message.data, -1f, 1f);
        lastROSDataTime = Time.time;
        rosDataReceived = true;
        
        if (showDebugInfo)
        {
            Debug.Log($"[ROS] Steering received: {targetSteerAngle:F2}");
        }
    }
    
    void CheckROSDataTimeout()
    {
        if (Time.time - lastROSDataTime > rosDataTimeout)
        {
            if (rosDataReceived)
            {
                Debug.LogWarning("[ROS] Data timeout - switching to safe mode (stopping car)");
                targetSpeed = 0f;
                targetSteerAngle = 0f;
                rosDataReceived = false;
            }
        }
    }
    
    void HandleModeSwitching()
    {
        if (Input.GetKeyDown(modeSwitchKey))
        {
            ToggleControlMode();
        }
    }
    
    public void ToggleControlMode()
    {
        useROSControl = !useROSControl;
        
        if (useROSControl)
        {
            // ROS 모드로 전환
            if (ros == null)
            {
                InitializeROS();
            }
            ResetControlInputs();
            Debug.Log("Switched to ROS control mode");
        }
        else
        {
            // 키보드 모드로 전환
            ResetControlInputs();
            Debug.Log("Switched to keyboard control mode");
        }
    }
    
    public void SetControlMode(bool useROS)
    {
        if (useROSControl != useROS)
        {
            ToggleControlMode();
        }
    }
    
    public bool IsROSControlActive()
    {
        return useROSControl && rosDataReceived;
    }
    
    public bool IsKeyboardControlActive()
    {
        return !useROSControl;
    }
    
    void OnDestroy()
    {
        // ROS 구독 해제
        if (ros != null)
        {
            try
            {
                ros.Unsubscribe(speedTopicName);
                ros.Unsubscribe(steeringTopicName);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Error unsubscribing from ROS topics: {e.Message}");
            }
        }
    }
}