using UnityEngine;

/// <summary>
/// WebGL Input Tracker - Universal input monitoring for ANY Unity game
/// Tracks ALL user interactions: mouse, keyboard, touch, gamepad
/// Works with ANY Unity game without requiring specific setup
/// </summary>
public class WebGLInputTracker : MonoBehaviour
{
    [Header("Input Tracking Settings")]
    [SerializeField] private bool trackMouse = true;
    [SerializeField] private bool trackKeyboard = true;
    [SerializeField] private bool trackTouch = true;
    [SerializeField] private bool trackGamepad = true;
    
    [Header("Tracking Frequency")]
    [SerializeField] private float trackingInterval = 0.1f; // 10 times per second
    
    // Input tracking variables
    private float trackingTimer = 0f;
    private Vector3 lastMousePos;
    private bool[] lastKeyStates = new bool[512]; // Track up to 512 keys
    private int mouseClicks = 0;
    private int keyPresses = 0;
    private int touchCount = 0;
    private int gamepadInputs = 0;
    
    // Movement tracking
    private float mouseDistance = 0f;
    private float totalInputTime = 0f;
    
    // Firebase integration
    private bool isFirebaseReady = false;
    
    void Start()
    {
        Debug.Log("🖱️ WebGL Input Tracker started");
        Debug.Log("🎯 Tracking: Mouse, Keyboard, Touch, Gamepad");
        
        // Initialize tracking
        lastMousePos = Input.mousePosition;
        
        // Initialize Firebase
        StartCoroutine(InitializeFirebase());
    }
    
    System.Collections.IEnumerator InitializeFirebase()
    {
        yield return new WaitForSeconds(2f); // Wait for Firebase
        
        try
        {
            CallJavaScript("UnityFirebase.startSession", "");
            isFirebaseReady = true;
            Debug.Log("✅ Firebase ready for input tracking");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"⚠️ Firebase not ready: {e.Message}");
        }
    }
    
    void Update()
    {
        trackingTimer += Time.deltaTime;
        totalInputTime += Time.deltaTime;
        
        if (trackingTimer >= trackingInterval)
        {
            TrackAllInputs();
            trackingTimer = 0f;
        }
    }
    
    void TrackAllInputs()
    {
        // Track mouse
        if (trackMouse)
        {
            TrackMouseInput();
        }
        
        // Track keyboard
        if (trackKeyboard)
        {
            TrackKeyboardInput();
        }
        
        // Track touch
        if (trackTouch)
        {
            TrackTouchInput();
        }
        
        // Track gamepad
        if (trackGamepad)
        {
            TrackGamepadInput();
        }
        
        // Send input data to Firebase
        SendInputData();
    }
    
    void TrackMouseInput()
    {
        Vector3 currentMousePos = Input.mousePosition;
        
        // Track mouse movement
        if (currentMousePos != lastMousePos)
        {
            float distance = Vector3.Distance(currentMousePos, lastMousePos);
            mouseDistance += distance;
            lastMousePos = currentMousePos;
        }
        
        // Track mouse clicks
        if (Input.GetMouseButtonDown(0)) // Left click
        {
            mouseClicks++;
            RecordInputEvent("MOUSE_CLICK", "LEFT");
        }
        
        if (Input.GetMouseButtonDown(1)) // Right click
        {
            mouseClicks++;
            RecordInputEvent("MOUSE_CLICK", "RIGHT");
        }
        
        if (Input.GetMouseButtonDown(2)) // Middle click
        {
            mouseClicks++;
            RecordInputEvent("MOUSE_CLICK", "MIDDLE");
        }
        
        // Track mouse wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            RecordInputEvent("MOUSE_SCROLL", scroll.ToString("F2"));
        }
    }
    
    void TrackKeyboardInput()
    {
        // Track all keys
        for (int i = 0; i < lastKeyStates.Length; i++)
        {
            bool currentState = Input.GetKey((KeyCode)i);
            
            if (currentState && !lastKeyStates[i])
            {
                keyPresses++;
                RecordInputEvent("KEY_PRESS", ((KeyCode)i).ToString());
            }
            
            lastKeyStates[i] = currentState;
        }
        
        // Track special keys
        if (Input.GetKeyDown(KeyCode.Space))
        {
            RecordInputEvent("SPECIAL_KEY", "SPACE");
        }
        
        if (Input.GetKeyDown(KeyCode.Return))
        {
            RecordInputEvent("SPECIAL_KEY", "ENTER");
        }
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            RecordInputEvent("SPECIAL_KEY", "ESCAPE");
        }
    }
    
    void TrackTouchInput()
    {
        if (Input.touchCount > 0)
        {
            touchCount += Input.touchCount;
            
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                
                if (touch.phase == TouchPhase.Began)
                {
                    RecordInputEvent("TOUCH_BEGIN", $"Touch_{i}");
                }
                else if (touch.phase == TouchPhase.Ended)
                {
                    RecordInputEvent("TOUCH_END", $"Touch_{i}");
                }
                else if (touch.phase == TouchPhase.Moved)
                {
                    RecordInputEvent("TOUCH_MOVE", $"Touch_{i}");
                }
            }
        }
    }
    
    void TrackGamepadInput()
    {
        // Track gamepad buttons
        string[] buttons = { "A", "B", "X", "Y", "LB", "RB", "Back", "Start" };
        
        for (int i = 0; i < buttons.Length; i++)
        {
            if (Input.GetButtonDown(buttons[i]))
            {
                gamepadInputs++;
                RecordInputEvent("GAMEPAD_BUTTON", buttons[i]);
            }
        }
        
        // Track gamepad axes
        float leftStickX = Input.GetAxis("Horizontal");
        float leftStickY = Input.GetAxis("Vertical");
        float rightStickX = Input.GetAxis("Mouse X");
        float rightStickY = Input.GetAxis("Mouse Y");
        
        if (Mathf.Abs(leftStickX) > 0.1f || Mathf.Abs(leftStickY) > 0.1f)
        {
            RecordInputEvent("GAMEPAD_STICK", $"Left: {leftStickX:F2}, {leftStickY:F2}");
        }
        
        if (Mathf.Abs(rightStickX) > 0.1f || Mathf.Abs(rightStickY) > 0.1f)
        {
            RecordInputEvent("GAMEPAD_STICK", $"Right: {rightStickX:F2}, {rightStickY:F2}");
        }
    }
    
    void RecordInputEvent(string eventType, string eventData)
    {
        string inputData = $"INPUT|{eventType}|{eventData}|{Time.time:F1}";
        
        if (isFirebaseReady)
        {
            CallJavaScript("UnityFirebase.recordDrivingEvent", inputData);
        }
        
        Debug.Log($"🎮 Input event: {eventType} - {eventData}");
    }
    
    void SendInputData()
    {
        if (!isFirebaseReady) return;
        
        // Create input summary
        string inputSummary = $"INPUT_SUMMARY|{totalInputTime:F1}|{mouseClicks}|{keyPresses}|{touchCount}|{gamepadInputs}|{mouseDistance:F1}";
        
        CallJavaScript("UnityFirebase.updateSessionStats", inputSummary);
    }
    
    void CallJavaScript(string methodName, string data)
    {
        try
        {
            Application.ExternalCall(methodName, data);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"JavaScript call failed: {methodName} - {e.Message}");
        }
    }
    
    // Public methods for external calls
    public void RecordCustomInput(string inputType, string inputData)
    {
        RecordInputEvent("CUSTOM_INPUT", $"{inputType}:{inputData}");
    }
    
    public void RecordGameAction(string action, string details)
    {
        RecordInputEvent("GAME_ACTION", $"{action}:{details}");
    }
    
    // Context menu for testing
    [ContextMenu("Test Input Tracking")]
    public void TestInputTracking()
    {
        Debug.Log("🧪 Testing input tracking...");
        
        RecordInputEvent("TEST", "Input tracking test");
        RecordCustomInput("TEST_CUSTOM", "Custom input test");
        RecordGameAction("TEST_ACTION", "Game action test");
        
        Debug.Log("✅ Input tracking test completed");
    }
    
    [ContextMenu("Get Input Stats")]
    public void GetInputStats()
    {
        Debug.Log("📊 Input Tracking Stats:");
        Debug.Log($"   Total Input Time: {totalInputTime:F1} seconds");
        Debug.Log($"   Mouse Clicks: {mouseClicks}");
        Debug.Log($"   Key Presses: {keyPresses}");
        Debug.Log($"   Touch Events: {touchCount}");
        Debug.Log($"   Gamepad Inputs: {gamepadInputs}");
        Debug.Log($"   Mouse Distance: {mouseDistance:F1} pixels");
        Debug.Log($"   Firebase Ready: {isFirebaseReady}");
    }
}
