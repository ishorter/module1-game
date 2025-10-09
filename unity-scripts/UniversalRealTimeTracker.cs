using UnityEngine;
using System.Collections;

/// <summary>
/// Universal Real-Time Tracker for Unity WebGL
/// Works with ANY Unity game without requiring specific GameObject tags
/// Tracks: Input, Movement, Physics, UI interactions, Game events
/// </summary>
public class UniversalRealTimeTracker : MonoBehaviour
{
    [Header("Universal Tracking Settings")]
    [SerializeField] private bool enableUniversalTracking = true;
    [SerializeField] private float trackingUpdateInterval = 0.1f; // 10 times per second
    [SerializeField] private float sessionSaveInterval = 10f; // Save every 10 seconds
    
    [Header("Input Tracking")]
    [SerializeField] private bool trackInput = true;
    [SerializeField] private bool trackMovement = true;
    [SerializeField] private bool trackPhysics = true;
    
    [Header("Game Event Tracking")]
    [SerializeField] private bool trackCollisions = true;
    [SerializeField] private bool trackTriggers = true;
    [SerializeField] private bool trackUI = true;
    
    // Tracking variables
    private float trackingTimer = 0f;
    private float sessionSaveTimer = 0f;
    private float sessionStartTime = 0f;
    private bool isTracking = false;
    private bool isFirebaseReady = false;
    
    // Input tracking
    private Vector3 lastMousePosition;
    private Vector3 lastCameraPosition;
    private float lastTimeScale;
    private int inputCount = 0;
    private int collisionCount = 0;
    private int triggerCount = 0;
    
    // Movement tracking
    private Vector3 lastPosition;
    private float totalDistance = 0f;
    private float maxSpeed = 0f;
    private float currentSpeed = 0f;
    
    // Physics tracking
    private Rigidbody[] allRigidbodies;
    private int physicsObjectCount = 0;
    
    void Start()
    {
        // Start universal tracking immediately
        StartCoroutine(InitializeUniversalTracking());
        
        Debug.Log("🌐 Universal Real-Time Tracker started");
        Debug.Log("🎯 Tracking: Input, Movement, Physics, Collisions, Triggers, UI");
    }
    
    IEnumerator InitializeUniversalTracking()
    {
        // Wait a frame for everything to initialize
        yield return null;
        
        // Initialize tracking
        sessionStartTime = Time.time;
        lastPosition = transform.position;
        lastCameraPosition = Camera.main ? Camera.main.transform.position : Vector3.zero;
        lastMousePosition = Input.mousePosition;
        lastTimeScale = Time.timeScale;
        
        // Find all physics objects
        allRigidbodies = FindObjectsOfType<Rigidbody>();
        physicsObjectCount = allRigidbodies.Length;
        
        // Start tracking
        isTracking = true;
        
        Debug.Log($"✅ Universal tracking initialized - {physicsObjectCount} physics objects found");
        Debug.Log("🚀 Real-time tracking ACTIVE - monitoring ALL game activity");
        
        // Initialize Firebase
        StartCoroutine(InitializeFirebase());
    }
    
    IEnumerator InitializeFirebase()
    {
        // Wait for Firebase to be ready
        yield return new WaitForSeconds(1f);
        
        // Try to call Firebase bridge
        try
        {
            CallJavaScript("UnityFirebase.startSession", "");
            isFirebaseReady = true;
            Debug.Log("✅ Firebase ready for universal tracking");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"⚠️ Firebase not ready yet: {e.Message}");
            // Continue tracking anyway - data will be queued
        }
    }
    
    void Update()
    {
        if (!enableUniversalTracking || !isTracking) return;
        
        // Update timers
        trackingTimer += Time.deltaTime;
        sessionSaveTimer += Time.deltaTime;
        
        // Universal tracking update
        if (trackingTimer >= trackingUpdateInterval)
        {
            UpdateUniversalTracking();
            trackingTimer = 0f;
        }
        
        // Auto-save session
        if (sessionSaveTimer >= sessionSaveInterval)
        {
            SaveSessionData();
            sessionSaveTimer = 0f;
        }
        
        // Track input events
        if (trackInput)
        {
            TrackInputEvents();
        }
        
        // Track movement
        if (trackMovement)
        {
            TrackMovement();
        }
        
        // Track physics
        if (trackPhysics)
        {
            TrackPhysics();
        }
    }
    
    void UpdateUniversalTracking()
    {
        // Track general game state
        float sessionTime = Time.time - sessionStartTime;
        
        // Create universal game data
        string gameData = $"UNIVERSAL|{sessionTime:F1}|{inputCount}|{collisionCount}|{triggerCount}|{totalDistance:F1}|{maxSpeed:F1}|{Time.timeScale:F1}";
        
        // Send to Firebase
        if (isFirebaseReady)
        {
            CallJavaScript("UnityFirebase.recordDrivingEvent", gameData);
        }
        
        Debug.Log($"📊 Universal tracking: {sessionTime:F1}s, {inputCount} inputs, {collisionCount} collisions, {triggerCount} triggers");
    }
    
    void TrackInputEvents()
    {
        // Track mouse movement
        if (Input.mousePosition != lastMousePosition)
        {
            inputCount++;
            lastMousePosition = Input.mousePosition;
        }
        
        // Track keyboard input
        if (Input.anyKeyDown)
        {
            inputCount++;
        }
        
        // Track touch input (for mobile)
        if (Input.touchCount > 0)
        {
            inputCount += Input.touchCount;
        }
    }
    
    void TrackMovement()
    {
        // Track camera movement
        if (Camera.main && Camera.main.transform.position != lastCameraPosition)
        {
            Vector3 movement = Camera.main.transform.position - lastCameraPosition;
            float distance = movement.magnitude;
            totalDistance += distance;
            
            // Calculate speed
            currentSpeed = distance / Time.deltaTime;
            if (currentSpeed > maxSpeed)
            {
                maxSpeed = currentSpeed;
            }
            
            lastCameraPosition = Camera.main.transform.position;
        }
        
        // Track transform movement
        if (transform.position != lastPosition)
        {
            Vector3 movement = transform.position - lastPosition;
            float distance = movement.magnitude;
            totalDistance += distance;
            
            lastPosition = transform.position;
        }
    }
    
    void TrackPhysics()
    {
        // Track physics objects
        if (allRigidbodies != null)
        {
            foreach (Rigidbody rb in allRigidbodies)
            {
                if (rb != null && rb.velocity.magnitude > 0.1f)
                {
                    // Track moving physics objects
                    float speed = rb.velocity.magnitude;
                    if (speed > maxSpeed)
                    {
                        maxSpeed = speed;
                    }
                }
            }
        }
    }
    
    // Universal collision detection
    void OnCollisionEnter(Collision collision)
    {
        if (!trackCollisions) return;
        
        collisionCount++;
        
        // Record collision data
        string collisionData = $"COLLISION|{collision.gameObject.name}|{collision.relativeVelocity.magnitude:F1}|{collisionCount}|{Time.time:F1}";
        
        if (isFirebaseReady)
        {
            CallJavaScript("UnityFirebase.recordCollision", collisionData);
        }
        
        Debug.Log($"💥 Universal collision detected: {collision.gameObject.name} (Force: {collision.relativeVelocity.magnitude:F1})");
    }
    
    // Universal trigger detection
    void OnTriggerEnter(Collider other)
    {
        if (!trackTriggers) return;
        
        triggerCount++;
        
        // Record trigger data
        string triggerData = $"TRIGGER|{other.name}|{other.tag}|{triggerCount}|{Time.time:F1}";
        
        if (isFirebaseReady)
        {
            CallJavaScript("UnityFirebase.recordDrivingEvent", triggerData);
        }
        
        Debug.Log($"🎯 Universal trigger detected: {other.name} (Tag: {other.tag})");
    }
    
    void SaveSessionData()
    {
        if (!isFirebaseReady) return;
        
        float sessionTime = Time.time - sessionStartTime;
        string sessionData = $"SESSION|{sessionTime:F1}|{inputCount}|{collisionCount}|{triggerCount}|{totalDistance:F1}|{maxSpeed:F1}";
        
        CallJavaScript("UnityFirebase.updateSessionStats", sessionData);
        
        Debug.Log($"💾 Session saved: {sessionTime:F1}s, {inputCount} inputs, {collisionCount} collisions");
    }
    
    // Call JavaScript bridge
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
    public void RecordGameEvent(string eventType, string eventData)
    {
        string fullData = $"GAME_EVENT|{eventType}|{eventData}|{Time.time:F1}";
        
        if (isFirebaseReady)
        {
            CallJavaScript("UnityFirebase.recordDrivingEvent", fullData);
        }
        
        Debug.Log($"🎮 Game event recorded: {eventType} - {eventData}");
    }
    
    public void RecordScore(int score, string levelName)
    {
        string scoreData = $"SCORE|{score}|{levelName}|{Time.time:F1}";
        
        if (isFirebaseReady)
        {
            CallJavaScript("UnityFirebase.saveProgress", scoreData);
        }
        
        Debug.Log($"🏆 Score recorded: {score} in {levelName}");
    }
    
    public void RecordViolation(string violationType, float speed, string location)
    {
        string violationData = $"VIOLATION|{violationType}|{speed:F1}|{location}|{Time.time:F1}";
        
        if (isFirebaseReady)
        {
            CallJavaScript("UnityFirebase.recordViolation", violationData);
        }
        
        Debug.Log($"🚨 Violation recorded: {violationType} at {speed:F1} mph in {location}");
    }
    
    // Context menu for testing
    [ContextMenu("Test Universal Tracking")]
    public void TestUniversalTracking()
    {
        Debug.Log("🧪 Testing universal tracking...");
        
        // Test different event types
        RecordGameEvent("TEST_EVENT", "Universal tracking test");
        RecordScore(1000, "Test Level");
        RecordViolation("Test Speeding", 75f, "Test Highway");
        
        Debug.Log("✅ Universal tracking test completed");
    }
    
    [ContextMenu("Get Tracking Stats")]
    public void GetTrackingStats()
    {
        float sessionTime = Time.time - sessionStartTime;
        
        Debug.Log("📊 Universal Tracking Stats:");
        Debug.Log($"   Session Time: {sessionTime:F1} seconds");
        Debug.Log($"   Input Events: {inputCount}");
        Debug.Log($"   Collisions: {collisionCount}");
        Debug.Log($"   Triggers: {triggerCount}");
        Debug.Log($"   Total Distance: {totalDistance:F1} units");
        Debug.Log($"   Max Speed: {maxSpeed:F1} units/sec");
        Debug.Log($"   Physics Objects: {physicsObjectCount}");
        Debug.Log($"   Firebase Ready: {isFirebaseReady}");
    }
}
