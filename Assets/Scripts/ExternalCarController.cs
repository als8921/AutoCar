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
        // Handle keyboard input
        HandleKeyboardInput();
        
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
            // Gradually return to 0 when no key is pressed
            targetSpeed = Mathf.Lerp(targetSpeed, 0f, Time.deltaTime * 5f);
            if (Mathf.Abs(targetSpeed) < 0.01f)
            {
                targetSpeed = 0f;
            }
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
            // Gradually return to 0 when no key is pressed
            targetSteerAngle = Mathf.Lerp(targetSteerAngle, 0f, Time.deltaTime * 5f);
            if (Mathf.Abs(targetSteerAngle) < 0.01f)
            {
                targetSteerAngle = 0f;
            }
        }
    }
}