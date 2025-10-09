using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Real-Time Game Event Tracker for WebGL
/// Automatically detects and tracks game events in real-time
/// Stores data directly to Firebase Firestore
/// </summary>
public class RealTimeGameTracker : MonoBehaviour
{
    [Header("Real-Time Tracking Settings")]
    [SerializeField] private bool enableRealTimeTracking = true;
    [SerializeField] private float trackingUpdateInterval = 0.1f; // 10 times per second
    [SerializeField] private float speedCheckInterval = 0.5f; // Check speed twice per second
    [SerializeField] private float sessionSaveInterval = 30f; // Save session data every 30 seconds
    
    [Header("Speed Tracking")]
    [SerializeField] private float speedLimit = 50f; // mph
    [SerializeField] private bool trackSpeedViolations = true;
    [SerializeField] private float minViolationSpeed = 55f; // mph
    
    [Header("Collision Tracking")]
    [SerializeField] private bool trackCollisions = true;
    [SerializeField] private float minCollisionForce = 5f;
    [SerializeField] private LayerMask collisionLayers = -1;
    
    [Header("Data Managers")]
    [SerializeField] private SimpleGameDataManager simpleManager;
    [SerializeField] private GameDataManager gameManager;
    [SerializeField] private DrivingDataManager drivingManager;
    
    // Real-time tracking variables
    private float trackingTimer = 0f;
    private float speedCheckTimer = 0f;
    private float sessionSaveTimer = 0f;
    private float sessionStartTime = 0f;
    
    // Current game state
    private float currentSpeed = 0f;
    private float maxSpeed = 0f;
    private Vector3 lastPosition;
    private int totalViolations = 0;
    private int totalCollisions = 0;
    private int totalScore = 0;
    private int currentLevel = 1;
    private float totalDistance = 0f;
    
    // Real-time event detection
    private bool isSpeeding = false;
    private bool wasSpeeding = false;
    private List<string> recentViolations = new List<string>();
    private List<string> recentCollisions = new List<string>();
    
    // Player reference
    private Transform playerTransform;
    private Rigidbody playerRigidbody;
    
    void Start()
    {
        // Find player/vehicle
        FindPlayer();
        
        // Find data managers
        FindDataManagers();
        
        // Initialize tracking
        InitializeTracking();
        
        Debug.Log("🎮 Real-Time Game Tracker initialized");
        Debug.Log($"📊 Tracking: Speed={trackSpeedViolations}, Collisions={trackCollisions}");
    }
    
    void Update()
    {
        if (!enableRealTimeTracking) return;
        
        // Update timers
        trackingTimer += Time.deltaTime;
        speedCheckTimer += Time.deltaTime;
        sessionSaveTimer += Time.deltaTime;
        
        // Real-time speed tracking
        if (speedCheckTimer >= speedCheckInterval)
        {
            UpdateSpeedTracking();
            speedCheckTimer = 0f;
        }
        
        // Real-time position tracking
        if (trackingTimer >= trackingUpdateInterval)
        {
            UpdatePositionTracking();
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
        if (!trackCollisions) return;
        
        // Calculate impact force
        float impactForce = collision.relativeVelocity.magnitude;
        
        if (impactForce >= minCollisionForce)
        {
            // Determine collision type
            string collisionType = DetermineCollisionType(collision);
            string objectHit = collision.gameObject.name;
            
            // Record collision
            RecordCollision(collisionType, objectHit, impactForce, collision.contacts[0].point);
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        // Detect traffic violations
        if (other.CompareTag("RedLight") || other.CompareTag("StopSign"))
        {
            if (currentSpeed > 5f) // If moving through red light/stop sign
            {
                string violationType = other.CompareTag("RedLight") ? "Red Light" : "Stop Sign";
                RecordViolation(violationType, currentSpeed, other.name);
            }
        }
        
        // Detect level progression
        if (other.CompareTag("Checkpoint") || other.CompareTag("FinishLine"))
        {
            RecordProgress();
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
            Debug.Log("✅ Player found for tracking");
        }
        else
        {
            Debug.LogWarning("⚠️ No player found - using camera position");
            playerTransform = Camera.main?.transform;
        }
    }
    
    private void FindDataManagers()
    {
        if (!simpleManager) simpleManager = FindObjectOfType<SimpleGameDataManager>();
        if (!gameManager) gameManager = FindObjectOfType<GameDataManager>();
        if (!drivingManager) drivingManager = DrivingDataManager.Instance;
        
        Debug.Log($"📊 Data Managers: Simple={simpleManager != null}, Game={gameManager != null}, Driving={drivingManager != null}");
    }
    
    private void InitializeTracking()
    {
        sessionStartTime = Time.time;
        lastPosition = playerTransform ? playerTransform.position : Vector3.zero;
        
        // Start session
        StartSession();
    }
    
    private void UpdateSpeedTracking()
    {
        if (!playerRigidbody) return;
        
        // Calculate current speed
        currentSpeed = playerRigidbody.velocity.magnitude * 2.237f; // Convert m/s to mph
        
        // Update max speed
        if (currentSpeed > maxSpeed)
        {
            maxSpeed = currentSpeed;
        }
        
        // Check for speed violations
        if (trackSpeedViolations && currentSpeed > minViolationSpeed)
        {
            isSpeeding = true;
            
            // Record violation if just started speeding
            if (!wasSpeeding)
            {
                RecordViolation("Speeding", currentSpeed, GetCurrentLocation());
            }
        }
        else
        {
            isSpeeding = false;
        }
        
        wasSpeeding = isSpeeding;
    }
    
    private void UpdatePositionTracking()
    {
        if (!playerTransform) return;
        
        // Calculate distance traveled
        Vector3 currentPosition = playerTransform.position;
        float distanceThisFrame = Vector3.Distance(lastPosition, currentPosition);
        totalDistance += distanceThisFrame;
        lastPosition = currentPosition;
        
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
    
    #region Data Recording Methods
    
    private void RecordViolation(string violationType, float speed, string location)
    {
        totalViolations++;
        
        // Avoid duplicate violations
        string violationKey = $"{violationType}_{location}_{Time.time:F0}";
        if (recentViolations.Contains(violationKey)) return;
        
        recentViolations.Add(violationKey);
        if (recentViolations.Count > 10) recentViolations.RemoveAt(0);
        
        Debug.Log($"🚨 Violation #{totalViolations}: {violationType} at {speed:F1} mph in {location}");
        
        // Send to data managers
        if (simpleManager)
        {
            simpleManager.RecordViolation(violationType, speed, location);
        }
        
        if (gameManager)
        {
            gameManager.RecordViolation(violationType, speed, location);
        }
        
        if (drivingManager)
        {
            drivingManager.RecordViolation(violationType, speed, playerTransform.position, location, 7f);
        }
    }
    
    private void RecordCollision(string collisionType, string objectHit, float impactForce, Vector3 collisionPoint)
    {
        totalCollisions++;
        
        // Avoid duplicate collisions
        string collisionKey = $"{collisionType}_{objectHit}_{Time.time:F0}";
        if (recentCollisions.Contains(collisionKey)) return;
        
        recentCollisions.Add(collisionKey);
        if (recentCollisions.Count > 10) recentCollisions.RemoveAt(0);
        
        Debug.Log($"💥 Collision #{totalCollisions}: {collisionType} with {objectHit} (Impact: {impactForce:F1})");
        
        // Send to data managers
        if (simpleManager)
        {
            simpleManager.RecordCollision(collisionType, objectHit, impactForce);
        }
        
        if (gameManager)
        {
            gameManager.RecordCollision(collisionType, objectHit, impactForce);
        }
        
        if (drivingManager)
        {
            drivingManager.RecordCollision(collisionType, impactForce, collisionPoint, Vector3.zero, objectHit, 50f);
        }
    }
    
    private void RecordDrivingEvent(string eventType, float value, Vector3 position)
    {
        // Send to data managers
        if (simpleManager)
        {
            simpleManager.RecordDrivingEvent(eventType, value, position);
        }
        
        if (gameManager)
        {
            gameManager.RecordDrivingEvent(eventType, value, position);
        }
        
        if (drivingManager)
        {
            drivingManager.RecordDrivingEvent(eventType, value, position, GetCurrentLocation());
        }
    }
    
    private void RecordProgress()
    {
        float sessionTime = Time.time - sessionStartTime;
        float completion = Mathf.Clamp01((totalDistance / 1000f) * 100f); // Estimate completion
        
        Debug.Log($"📈 Progress: Level {currentLevel}, Score {totalScore}, Distance {totalDistance:F1}m");
        
        // Send to data managers
        if (simpleManager)
        {
            simpleManager.SaveProgress(currentLevel, totalScore, completion, sessionTime);
        }
        
        if (gameManager)
        {
            gameManager.SaveProgress(currentLevel, totalScore, completion, sessionTime);
        }
    }
    
    private void SaveSessionData()
    {
        float sessionTime = Time.time - sessionStartTime;
        
        Debug.Log($"💾 Auto-saving session data: {sessionTime:F1}s, {totalViolations} violations, {totalCollisions} collisions");
        
        // Update session stats
        if (simpleManager)
        {
            simpleManager.UpdateSessionStats();
        }
        
        if (gameManager)
        {
            gameManager.UpdateSessionStats();
        }
    }
    
    private void StartSession()
    {
        Debug.Log("🎮 Starting real-time tracking session");
        
        if (simpleManager)
        {
            simpleManager.StartSession();
        }
        
        if (gameManager)
        {
            gameManager.StartSession();
        }
    }
    
    private void EndSession()
    {
        Debug.Log("🏁 Ending real-time tracking session");
        
        if (simpleManager)
        {
            simpleManager.EndSession();
        }
        
        if (gameManager)
        {
            gameManager.EndSession();
        }
    }
    
    #endregion
    
    #region Public Methods for External Control
    
    public void UpdateScore(int points)
    {
        totalScore += points;
        Debug.Log($"🏆 Score updated: +{points} (Total: {totalScore})");
    }
    
    public void SetLevel(int level)
    {
        currentLevel = level;
        Debug.Log($"🎮 Level set to: {level}");
    }
    
    public void SetSpeedLimit(float limit)
    {
        speedLimit = limit;
        minViolationSpeed = limit + 5f;
        Debug.Log($"🚦 Speed limit set to: {limit} mph");
    }
    
    public void ForceSaveSession()
    {
        SaveSessionData();
    }
    
    public void GetCurrentStats()
    {
        float sessionTime = Time.time - sessionStartTime;
        
        Debug.Log($"📊 Current Stats:");
        Debug.Log($"   Session Time: {sessionTime:F1}s");
        Debug.Log($"   Current Speed: {currentSpeed:F1} mph");
        Debug.Log($"   Max Speed: {maxSpeed:F1} mph");
        Debug.Log($"   Total Distance: {totalDistance:F1}m");
        Debug.Log($"   Violations: {totalViolations}");
        Debug.Log($"   Collisions: {totalCollisions}");
        Debug.Log($"   Score: {totalScore}");
        Debug.Log($"   Level: {currentLevel}");
    }
    
    #endregion
    
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            EndSession();
        }
    }
    
    void OnDestroy()
    {
        EndSession();
    }
    
    // Manual test methods
    [ContextMenu("Test Violation")]
    public void TestViolation() => RecordViolation("Test Violation", 75f, "Test Location");
    
    [ContextMenu("Test Collision")]
    public void TestCollision() => RecordCollision("Test Object", "Test_Car", 25f, Vector3.zero);
    
    [ContextMenu("Test Progress")]
    public void TestProgress() => RecordProgress();
    
    [ContextMenu("Get Stats")]
    public void TestGetStats() => GetCurrentStats();
}
