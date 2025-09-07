using UnityEngine;

public class ExternalCarController : MonoBehaviour
{
    [Header("Car Control Parameters")]
    [Range(-1f, 1f)]
    public float targetSpeed = 0f; // -1 (full reverse) to 1 (full forward) - Default 0.5 for testing
    
    [Range(-1f, 1f)]
    public float targetSteerAngle = 0f; // -1 (full left) to 1 (full right)
    
    [Header("Control Settings")]
    public bool useExternalControl = true;
    public bool showDebugInfo = true;
    
    private PrometeoCarController carController;
    
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
        
        Debug.Log("ExternalCarController initialized successfully!");
    }
    
    void Update()
    {
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
        Debug.Log($"External Control - Speed: {targetSpeed:F2}, Steer: {targetSteerAngle:F2}, Car Speed: {carController.carSpeed:F1} km/h, UseExternal: {carController.useExternalControl}");
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
}