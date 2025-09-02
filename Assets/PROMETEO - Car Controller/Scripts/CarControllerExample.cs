using UnityEngine;

public class CarControllerExample : MonoBehaviour
{
    [Header("Example Control")]
    public ExternalCarController externalCarController;
    
    [Header("Test Controls")]
    [Range(-1f, 1f)]
    public float testSpeed = 0f;
    [Range(-1f, 1f)]
    public float testSteerAngle = 0f;
    
    [Header("Automatic Test")]
    public bool runAutomaticTest = false;
    public float testDuration = 5f;
    
    private float testStartTime;
    private bool testRunning = false;
    
    void Start()
    {
        if (externalCarController == null)
        {
            externalCarController = GetComponent<ExternalCarController>();
        }
        
        if (externalCarController == null)
        {
            Debug.LogError("CarControllerExample: ExternalCarController not found!");
            enabled = false;
        }
    }
    
    void Update()
    {
        if (externalCarController == null) return;
        
        // Manual control through inspector
        externalCarController.SetControl(testSpeed, testSteerAngle);
        
        // Automatic test
        if (runAutomaticTest && !testRunning)
        {
            StartAutomaticTest();
        }
        
        if (testRunning)
        {
            UpdateAutomaticTest();
        }
        
        // Keyboard override for testing
        HandleKeyboardInput();
    }
    
    void HandleKeyboardInput()
    {
        float keyboardSpeed = 0f;
        float keyboardSteer = 0f;
        
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
            keyboardSpeed = 1f;
        else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
            keyboardSpeed = -1f;
            
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            keyboardSteer = -1f;
        else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            keyboardSteer = 1f;
        
        // Only override if keyboard input is detected
        if (Mathf.Abs(keyboardSpeed) > 0.1f || Mathf.Abs(keyboardSteer) > 0.1f)
        {
            externalCarController.SetControl(keyboardSpeed, keyboardSteer);
            testSpeed = keyboardSpeed;
            testSteerAngle = keyboardSteer;
        }
    }
    
    void StartAutomaticTest()
    {
        testStartTime = Time.time;
        testRunning = true;
        Debug.Log("Starting automatic car control test...");
    }
    
    void UpdateAutomaticTest()
    {
        float elapsed = Time.time - testStartTime;
        
        if (elapsed > testDuration)
        {
            testRunning = false;
            runAutomaticTest = false;
            testSpeed = 0f;
            testSteerAngle = 0f;
            Debug.Log("Automatic test completed!");
            return;
        }
        
        // Simple test pattern: accelerate forward, turn in circles
        float progress = elapsed / testDuration;
        
        if (progress < 0.2f)
        {
            // Accelerate forward
            testSpeed = Mathf.Lerp(0f, 1f, progress * 5f);
            testSteerAngle = 0f;
        }
        else if (progress < 0.8f)
        {
            // Drive in circles
            testSpeed = 0.8f;
            testSteerAngle = Mathf.Sin((progress - 0.2f) * 10f) * 0.8f;
        }
        else
        {
            // Decelerate
            testSpeed = Mathf.Lerp(0.8f, 0f, (progress - 0.8f) * 5f);
            testSteerAngle = 0f;
        }
    }
    
    void OnGUI()
    {
        if (externalCarController == null) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("Car Controller Example");
        GUILayout.Label($"Current Speed: {externalCarController.GetCurrentSpeed():F1} km/h");
        GUILayout.Label($"Target Speed: {testSpeed:F2}");
        GUILayout.Label($"Target Steer: {testSteerAngle:F2}");
        
        if (testRunning)
        {
            GUILayout.Label("Automatic Test Running...");
        }
        
        GUILayout.Label("Controls:");
        GUILayout.Label("Arrow Keys / WASD: Manual control");
        GUILayout.Label("Inspector: Test values");
        GUILayout.EndArea();
    }
}