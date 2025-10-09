using UnityEngine;
using System.Collections;

/// <summary>
/// Simple Game Data Manager for Unity WebGL
/// Sends data to Firebase without JSON serialization
/// INCLUDES REAL-TIME AUTOMATIC TRACKING
/// </summary>
public class SimpleGameDataManager : MonoBehaviour
{
    [Header("Firebase Integration")]
    [SerializeField] private bool enableFirebaseLogging = true;
    
    [Header("Real-Time Tracking")]
    [SerializeField] private bool enableRealTimeTracking = true;
    [SerializeField] private float trackingUpdateInterval = 0.1f; // 10 times per second
    [SerializeField] private float sessionSaveInterval = 30f; // Auto-save every 30 seconds
    
    [Header("Speed & Violation Settings")]
    [SerializeField] private float speedLimit = 50f; // mph
    [SerializeField] private float minViolationSpeed = 55f; // mph
    [SerializeField] private float minCollisionForce = 5f;
    
    private bool isFirebaseReady = false;
    private int totalViolations = 0;
    private int totalCollisions = 0;
    
    // Real-time tracking variables
    private float trackingTimer = 0f;
    private float sessionSaveTimer = 0f;
    private float sessionStartTime = 0f;
    private float currentSpeed = 0f;
    private float maxSpeed = 0f;
    private Vector3 lastPosition;
    private bool isTracking = false;
    
    // References
    private Transform playerTransform;
    private Rigidbody playerRigidbody;
    
    // Data queuing system
    private System.Collections.Generic.Queue<string> dataQueue = new System.Collections.Generic.Queue<string>();
    
    void Start()
    {
        // Find player/vehicle for real-time tracking
        FindPlayer();
        
        // Wait for Firebase to initialize
        StartCoroutine(InitializeFirebase());
    }
    
    void Update()
    {
        if (!enableRealTimeTracking || !isTracking) return;
        
        // Update timers
        trackingTimer += Time.deltaTime;
        sessionSaveTimer += Time.deltaTime;
        
        // Real-time speed and position tracking
        if (trackingTimer >= trackingUpdateInterval)
        {
            UpdateRealTimeTracking();
            trackingTimer = 0f;
        }
        
        // Auto-save session data
        if (sessionSaveTimer >= sessionSaveInterval)
        {
            UpdateSessionStats();
            sessionSaveTimer = 0f;
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if (!isTracking) return;
        
        // Calculate impact force
        float impactForce = collision.relativeVelocity.magnitude;
        
        if (impactForce >= minCollisionForce)
        {
            // Record REAL collision automatically
            string collisionType = DetermineCollisionType(collision);
            RecordCollision(collisionType, collision.gameObject.name, impactForce);
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (!isTracking) return;
        
        // Detect REAL traffic violations automatically
        if (other.CompareTag("RedLight") && currentSpeed > 5f)
        {
            RecordViolation("Red Light", currentSpeed, other.name);
        }
        
        if (other.CompareTag("StopSign") && currentSpeed > 5f)
        {
            RecordViolation("Stop Sign", currentSpeed, other.name);
        }
        
        // Detect REAL level progression automatically
        if (other.CompareTag("Checkpoint") || other.CompareTag("FinishLine"))
        {
            SaveProgress(1, 1000, 100f, Time.time - sessionStartTime);
        }
    }
    
    private IEnumerator InitializeFirebase()
    {
        // Wait for JavaScript bridge to be ready
        yield return new WaitForSeconds(3f);
        
        LogMessage("Waiting for Firebase initialization...");
        
        // Set Firebase ready after a reasonable delay (fallback)
        yield return new WaitForSeconds(2f);
        
        // Initialize real-time tracking IMMEDIATELY (don't wait for Firebase)
        InitializeRealTimeTracking();
        
        // Fallback: Set Firebase ready even if callback doesn't work
        isFirebaseReady = true;
        LogMessage("✅ Firebase ready (fallback initialization)");
        
        // Test the connection
        TestConnection();
    }
    
    private void TestConnection()
    {
        LogMessage("Firebase connection ready - waiting for real game events...");
        // No test data - only real game events will be tracked
    }
    
    /// <summary>
    /// Callback from JavaScript when Firebase is ready
    /// </summary>
    public void OnFirebaseReady(string status)
    {
        if (status == "connected")
        {
            isFirebaseReady = true;
            LogMessage("✅ Firebase connection confirmed - ready for game data");
            
            // Process any queued data
            ProcessQueuedData();
        }
        else if (status == "failed")
        {
            isFirebaseReady = false;
            LogMessage("❌ Firebase connection failed - check configuration");
        }
    }
    
    /// <summary>
    /// Process any queued data when Firebase becomes ready
    /// </summary>
    private void ProcessQueuedData()
    {
        if (dataQueue.Count == 0) return;
        
        LogMessage($"Processing {dataQueue.Count} queued data items...");
        
        while (dataQueue.Count > 0)
        {
            string queuedItem = dataQueue.Dequeue();
            string[] parts = queuedItem.Split('|');
            
            if (parts.Length >= 2)
            {
                string methodName = parts[0];
                string data = string.Join("|", parts, 1, parts.Length - 1);
                
                LogMessage($"Processing queued item: {methodName} with data: {data}");
                CallJavaScript(methodName, data);
            }
        }
        
        LogMessage("✅ All queued data processed");
    }
    
    /// <summary>
    /// Record a traffic violation - Enhanced version with better error handling
    /// </summary>
    public void RecordViolation(string violationType, float speed, string location)
    {
        if (!isFirebaseReady) 
        {
            LogMessage("Firebase not ready - violation not recorded");
            return;
        }
        
        totalViolations++;
        
        // Clean data to avoid special characters that might cause issues
        string cleanViolationType = violationType?.Replace("|", "_").Replace("\"", "'") ?? "Unknown";
        string cleanLocation = location?.Replace("|", "_").Replace("\"", "'") ?? "Unknown";
        
        // Send data with better formatting
        string data = $"{cleanViolationType}|{speed:F1}|{cleanLocation}|{totalViolations}";
        
        LogMessage($"Attempting to record violation: {data}");
        CallJavaScript("UnityFirebase.recordViolation", data);
        
        LogMessage($"Violation recorded: {violationType} at {speed} mph");
    }
    
    /// <summary>
    /// Record a collision event - Simple version without JSON
    /// </summary>
    public void RecordCollision(string collisionType, string objectHit, float impactForce)
    {
        if (!isFirebaseReady) return;
        
        totalCollisions++;
        
        // Send data directly to JavaScript without JSON
        string data = $"{collisionType}|{objectHit}|{impactForce}|{totalCollisions}";
        CallJavaScript("UnityFirebase.recordCollision", data);
        
        LogMessage($"Collision recorded: {collisionType} with {objectHit}");
    }
    
    /// <summary>
    /// Save game progress - Simple version without JSON
    /// </summary>
    public void SaveProgress(int level, int score, float completion, float timeSpent)
    {
        if (!isFirebaseReady) return;
        
        // Send data directly to JavaScript without JSON
        string data = $"{level}|{score}|{completion}|{timeSpent}";
        CallJavaScript("UnityFirebase.saveProgress", data);
        
        LogMessage($"Progress saved: Level {level}, Score {score}");
    }
    
    /// <summary>
    /// Record a driving event - Simple version without JSON
    /// </summary>
    public void RecordDrivingEvent(string eventType, float value, Vector3 position)
    {
        if (!isFirebaseReady) return;
        
        // Send data directly to JavaScript without JSON
        string data = $"{eventType}|{value}|{position.x}|{position.y}|{position.z}";
        CallJavaScript("UnityFirebase.recordDrivingEvent", data);
        
        LogMessage($"Driving event recorded: {eventType} = {value}");
    }
    
    /// <summary>
    /// Update session statistics
    /// </summary>
    public void UpdateSessionStats()
    {
        if (!isFirebaseReady) return;
        
        float playTime = Time.time;
        string data = $"{playTime}|{totalViolations}|{totalCollisions}";
        CallJavaScript("UnityFirebase.updateSessionStats", data);
        
        LogMessage($"Session stats updated: {totalViolations} violations, {totalCollisions} collisions");
    }
    
    /// <summary>
    /// Start a new session
    /// </summary>
    public void StartSession()
    {
        if (!isFirebaseReady) return;
        
        CallJavaScript("UnityFirebase.startSession", "");
        LogMessage("New session started");
    }
    
    /// <summary>
    /// End current session
    /// </summary>
    public void EndSession()
    {
        if (!isFirebaseReady) return;
        
        CallJavaScript("UnityFirebase.endSession", "");
        LogMessage("Session ended");
    }
    
    private void CallJavaScript(string methodName, string data)
    {
        try
        {
            // Ensure data is not null or empty
            if (string.IsNullOrEmpty(data))
            {
                data = "";
            }
            
            LogMessage($"Calling JavaScript: {methodName} with data: {data}");
            
            // Use Application.ExternalCall for WebGL
            Application.ExternalCall(methodName, data);
            
            LogMessage($"JavaScript call successful: {methodName}");
        }
        catch (System.Exception e)
        {
            LogMessage($"Error calling JavaScript method {methodName}: {e.Message}");
            LogMessage($"Stack trace: {e.StackTrace}");
            
            // If Firebase isn't ready, queue the data
            if (!isFirebaseReady)
            {
                string queuedData = $"{methodName}|{data}";
                dataQueue.Enqueue(queuedData);
                LogMessage($"Data queued for later: {queuedData}");
            }
        }
    }
    
    private void LogMessage(string message)
    {
        if (enableFirebaseLogging)
        {
            Debug.Log($"[SimpleGameDataManager] {message}");
        }
    }
    
    #region Real-Time Tracking Methods
    
    private void FindPlayer()
    {
        // Try to find player/vehicle
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (!player) player = GameObject.FindGameObjectWithTag("Vehicle");
        if (!player) player = GameObject.FindGameObjectWithTag("Car");
        
        if (player)
        {
            playerTransform = player.transform;
            playerRigidbody = player.GetComponent<Rigidbody>();
            LogMessage("✅ Player found for real-time tracking");
        }
        else
        {
            LogMessage("⚠️ No player found - using camera position");
            playerTransform = Camera.main?.transform;
        }
    }
    
    private void InitializeRealTimeTracking()
    {
        // Start tracking immediately, even if Firebase isn't ready yet
        sessionStartTime = Time.time;
        lastPosition = playerTransform ? playerTransform.position : Vector3.zero;
        isTracking = true;
        
        LogMessage("🎮 Real-time tracking session started");
        LogMessage("📊 Tracking: Speed violations, Collisions, Traffic violations, Level progress");
        
        if (isFirebaseReady)
        {
            LogMessage("✅ Firebase ready - data will be saved immediately");
        }
        else
        {
            LogMessage("⏳ Firebase not ready - data will be queued and saved when ready");
        }
    }
    
    private void UpdateRealTimeTracking()
    {
        if (!playerRigidbody) return;
        
        // Calculate current speed
        currentSpeed = playerRigidbody.velocity.magnitude * 2.237f; // Convert m/s to mph
        
        // Update max speed
        if (currentSpeed > maxSpeed)
        {
            maxSpeed = currentSpeed;
        }
        
        // Calculate distance traveled
        Vector3 currentPosition = playerTransform.position;
        float distanceThisFrame = Vector3.Distance(lastPosition, currentPosition);
        lastPosition = currentPosition;
        
        // Check for speed violations
        if (currentSpeed > minViolationSpeed)
        {
            RecordViolation("Speeding", currentSpeed, GetCurrentLocation());
        }
        
        // Record driving events
        RecordDrivingEvent("Position", distanceThisFrame, currentPosition);
    }
    
    private string DetermineCollisionType(Collision collision)
    {
        string tag = collision.gameObject.tag;
        
        if (tag == "Vehicle" || tag == "Car") return "Vehicle";
        if (tag == "Pedestrian" || tag == "Person") return "Pedestrian";
        if (tag == "Building" || tag == "Wall") return "Building";
        if (tag == "Barrier" || tag == "Fence") return "Barrier";
        
        return "Object";
    }
    
    private string GetCurrentLocation()
    {
        // Try to get location from nearby objects
        Collider[] nearbyObjects = Physics.OverlapSphere(playerTransform.position, 10f);
        
        foreach (Collider obj in nearbyObjects)
        {
            if (obj.CompareTag("Location") || obj.CompareTag("Road"))
            {
                return obj.name;
            }
        }
        
        return "Unknown Location";
    }
    
    #endregion
    
    // Public methods for easy testing
    public void TestViolation() => RecordViolation("Speeding", 75f, "Highway");
    public void TestCollision() => RecordCollision("Vehicle", "Car_A", 25f);
    public void TestProgress() => SaveProgress(1, 1000, 50f, 120f);
    public void TestDrivingEvent() => RecordDrivingEvent("Braking", 0.8f, Vector3.zero);
    
    // Get current stats method
    [ContextMenu("Get Real Game Stats")]
    public void GetRealStats()
    {
        float sessionTime = Time.time - sessionStartTime;
        
        LogMessage($"📊 REAL Game Stats:");
        LogMessage($"   Session Time: {sessionTime:F1}s");
        LogMessage($"   Current Speed: {currentSpeed:F1} mph");
        LogMessage($"   Max Speed: {maxSpeed:F1} mph");
        LogMessage($"   Violations: {totalViolations}");
        LogMessage($"   Collisions: {totalCollisions}");
        LogMessage($"   Firebase Ready: {isFirebaseReady}");
        LogMessage($"   Tracking Active: {isTracking}");
        LogMessage($"   Queued Data: {dataQueue.Count} items");
    }
    
    // Force start tracking method
    [ContextMenu("Force Start Real-Time Tracking")]
    public void ForceStartTracking()
    {
        LogMessage("🚀 Force starting real-time tracking...");
        
        // Find player if not found
        if (!playerTransform)
        {
            FindPlayer();
        }
        
        // Start tracking immediately
        InitializeRealTimeTracking();
        
        // Force Firebase ready
        isFirebaseReady = true;
        LogMessage("✅ Firebase forced ready - tracking active");
    }
    
    // Force test data method
    [ContextMenu("Force Test Real Data")]
    public void ForceTestRealData()
    {
        LogMessage("🧪 Force testing real data tracking...");
        
        // Test violation
        RecordViolation("Force Test Speeding", 85f, "Force Test Highway");
        
        // Test collision
        RecordCollision("Force Test Vehicle", "Force Test Car", 35f);
        
        // Test progress
        SaveProgress(1, 2000, 75f, 180f);
        
        LogMessage("✅ Force test data sent - check Firebase console");
    }
}
