using UnityEngine;

/// <summary>
/// Real-Time Game Event Tracker for WebGL
/// Automatically detects and tracks ALL real game events in real-time
/// Stores data directly to Firebase Firestore
/// NO TEST DATA - ONLY REAL GAME EVENTS
/// </summary>
public class RealTimeGameEventTracker : MonoBehaviour
{
    [Header("Real-Time Tracking Settings")]
    [SerializeField] private bool enableRealTimeTracking = true;
    [SerializeField] private float trackingUpdateInterval = 0.1f; // 10 times per second
    [SerializeField] private float sessionSaveInterval = 30f; // Auto-save every 30 seconds
    
    [Header("Speed & Violation Settings")]
    [SerializeField] private float speedLimit = 50f; // mph
    [SerializeField] private float minViolationSpeed = 55f; // mph
    [SerializeField] private float minCollisionForce = 5f;
    
    [Header("Game Statistics")]
    [SerializeField] private int totalViolations = 0;
    [SerializeField] private int totalCollisions = 0;
    [SerializeField] private int totalScore = 0;
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private float totalDistance = 0f;
    [SerializeField] private float maxSpeed = 0f;
    
    // Real-time tracking variables
    private float trackingTimer = 0f;
    private float sessionSaveTimer = 0f;
    private float sessionStartTime = 0f;
    private float currentSpeed = 0f;
    private Vector3 lastPosition;
    private bool isTracking = false;
    
    // References
    private Transform playerTransform;
    private Rigidbody playerRigidbody;
    private SimpleGameDataManager dataManager;
    
    void Start()
    {
        // Find player/vehicle
        FindPlayer();
        
        // Find data manager
        FindDataManager();
        
        // Initialize real-time tracking
        InitializeTracking();
        
        Debug.Log("🎮 Real-Time Game Event Tracker initialized - ONLY real game events will be tracked");
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
            SaveSessionData();
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
            // Record REAL collision
            RecordRealCollision(collision);
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (!isTracking) return;
        
        // Detect REAL traffic violations
        if (other.CompareTag("RedLight") && currentSpeed > 5f)
        {
            RecordRealViolation("Red Light", currentSpeed, other.name);
        }
        
        if (other.CompareTag("StopSign") && currentSpeed > 5f)
        {
            RecordRealViolation("Stop Sign", currentSpeed, other.name);
        }
        
        // Detect REAL level progression
        if (other.CompareTag("Checkpoint") || other.CompareTag("FinishLine"))
        {
            RecordRealProgress();
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
            Debug.Log("✅ Player found for real-time tracking");
        }
        else
        {
            Debug.LogWarning("⚠️ No player found - using camera position");
            playerTransform = Camera.main?.transform;
        }
    }
    
    private void FindDataManager()
    {
        dataManager = FindObjectOfType<SimpleGameDataManager>();
        
        if (dataManager)
        {
            Debug.Log("✅ SimpleGameDataManager found for real-time data storage");
        }
        else
        {
            Debug.LogError("❌ SimpleGameDataManager not found - create one in your scene");
        }
    }
    
    private void InitializeTracking()
    {
        if (!dataManager)
        {
            Debug.LogError("❌ Cannot start tracking - no data manager found");
            return;
        }
        
        sessionStartTime = Time.time;
        lastPosition = playerTransform ? playerTransform.position : Vector3.zero;
        isTracking = true;
        
        // Start session
        dataManager.StartSession();
        
        Debug.Log("🎮 Real-time tracking session started");
        Debug.Log("📊 Tracking: Speed violations, Collisions, Traffic violations, Level progress");
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
        totalDistance += distanceThisFrame;
        lastPosition = currentPosition;
        
        // Check for speed violations
        if (currentSpeed > minViolationSpeed)
        {
            RecordRealViolation("Speeding", currentSpeed, GetCurrentLocation());
        }
        
        // Record driving events
        RecordDrivingEvent("Position", distanceThisFrame, currentPosition);
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
    
    #region Real Event Recording Methods
    
    private void RecordRealViolation(string violationType, float speed, string location)
    {
        totalViolations++;
        
        Debug.Log($"🚨 REAL Violation #{totalViolations}: {violationType} at {speed:F1} mph in {location}");
        
        // Send REAL data to Firebase
        if (dataManager)
        {
            dataManager.RecordViolation(violationType, speed, location);
        }
    }
    
    private void RecordRealCollision(Collision collision)
    {
        totalCollisions++;
        
        float impactForce = collision.relativeVelocity.magnitude;
        string collisionType = DetermineCollisionType(collision);
        string objectHit = collision.gameObject.name;
        
        Debug.Log($"💥 REAL Collision #{totalCollisions}: {collisionType} with {objectHit} (Impact: {impactForce:F1})");
        
        // Send REAL data to Firebase
        if (dataManager)
        {
            dataManager.RecordCollision(collisionType, objectHit, impactForce);
        }
    }
    
    private void RecordRealProgress()
    {
        float sessionTime = Time.time - sessionStartTime;
        float completion = Mathf.Clamp01((totalDistance / 1000f) * 100f); // Estimate completion
        
        Debug.Log($"📈 REAL Progress: Level {currentLevel}, Score {totalScore}, Distance {totalDistance:F1}m");
        
        // Send REAL data to Firebase
        if (dataManager)
        {
            dataManager.SaveProgress(currentLevel, totalScore, completion, sessionTime);
        }
    }
    
    private void RecordDrivingEvent(string eventType, float value, Vector3 position)
    {
        // Send REAL driving event to Firebase
        if (dataManager)
        {
            dataManager.RecordDrivingEvent(eventType, value, position);
        }
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
    
    private void SaveSessionData()
    {
        float sessionTime = Time.time - sessionStartTime;
        
        Debug.Log($"💾 Auto-saving REAL session data: {sessionTime:F1}s, {totalViolations} violations, {totalCollisions} collisions");
        
        // Update session stats
        if (dataManager)
        {
            dataManager.UpdateSessionStats();
        }
    }
    
    #endregion
    
    #region Public Methods for External Control
    
    public void UpdateScore(int points)
    {
        totalScore += points;
        Debug.Log($"🏆 REAL Score updated: +{points} (Total: {totalScore})");
    }
    
    public void SetLevel(int level)
    {
        currentLevel = level;
        Debug.Log($"🎮 REAL Level set to: {level}");
    }
    
    public void SetSpeedLimit(float limit)
    {
        speedLimit = limit;
        minViolationSpeed = limit + 5f;
        Debug.Log($"🚦 REAL Speed limit set to: {limit} mph");
    }
    
    public void GetCurrentStats()
    {
        float sessionTime = Time.time - sessionStartTime;
        
        Debug.Log($"📊 REAL Game Stats:");
        Debug.Log($"   Session Time: {sessionTime:F1}s");
        Debug.Log($"   Current Speed: {currentSpeed:F1} mph");
        Debug.Log($"   Max Speed: {maxSpeed:F1} mph");
        Debug.Log($"   Total Distance: {totalDistance:F1}m");
        Debug.Log($"   Violations: {totalViolations}");
        Debug.Log($"   Collisions: {totalCollisions}");
        Debug.Log($"   Score: {totalScore}");
        Debug.Log($"   Level: {currentLevel}");
    }
    
    public void ForceSaveSession()
    {
        SaveSessionData();
    }
    
    #endregion
    
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && dataManager)
        {
            dataManager.EndSession();
        }
    }
    
    void OnDestroy()
    {
        if (dataManager)
        {
            dataManager.EndSession();
        }
    }
    
    // Get current stats method
    [ContextMenu("Get Real Game Stats")]
    public void ShowRealStats() => GetCurrentStats();
}
