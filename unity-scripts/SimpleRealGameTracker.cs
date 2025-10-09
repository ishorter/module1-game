using UnityEngine;

/// <summary>
/// Simple Real Game Tracker - Clean, minimal real game data tracking
/// Tracks only REAL game events and stores them in Firebase Firestore
/// NO TEST DATA - ONLY REAL GAME EVENTS
/// </summary>
public class SimpleRealGameTracker : MonoBehaviour
{
    [Header("Real Game Tracking")]
    [SerializeField] private bool enableTracking = true;
    [SerializeField] private float saveInterval = 30f; // Save every 30 seconds
    
    [Header("Speed Settings")]
    [SerializeField] private float speedLimit = 50f; // mph
    [SerializeField] private float minViolationSpeed = 55f; // mph
    
    [Header("Collision Settings")]
    [SerializeField] private float minCollisionForce = 5f;
    
    // Tracking variables
    private float saveTimer = 0f;
    private float sessionStartTime = 0f;
    private float currentSpeed = 0f;
    private float maxSpeed = 0f;
    private int violations = 0;
    private int collisions = 0;
    private int score = 0;
    private int level = 1;
    
    // References
    private Transform player;
    private Rigidbody playerRb;
    private SimpleGameDataManager dataManager;
    
    void Start()
    {
        // Find player
        FindPlayer();
        
        // Find data manager
        dataManager = FindObjectOfType<SimpleGameDataManager>();
        if (!dataManager)
        {
            Debug.LogError("❌ SimpleGameDataManager not found! Add it to your scene.");
            return;
        }
        
        // Start session
        sessionStartTime = Time.time;
        dataManager.StartSession();
        
        Debug.Log("🎮 Simple Real Game Tracker started - ONLY real game events will be tracked");
    }
    
    void Update()
    {
        if (!enableTracking || !dataManager) return;
        
        // Update timer
        saveTimer += Time.deltaTime;
        
        // Auto-save session data
        if (saveTimer >= saveInterval)
        {
            dataManager.UpdateSessionStats();
            saveTimer = 0f;
        }
        
        // Track speed
        TrackSpeed();
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if (!enableTracking || !dataManager) return;
        
        float impactForce = collision.relativeVelocity.magnitude;
        
        if (impactForce >= minCollisionForce)
        {
            collisions++;
            string objectHit = collision.gameObject.name;
            string collisionType = collision.gameObject.CompareTag("Vehicle") ? "Vehicle" : "Object";
            
            Debug.Log($"💥 REAL Collision: {collisionType} with {objectHit}");
            dataManager.RecordCollision(collisionType, objectHit, impactForce);
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (!enableTracking || !dataManager) return;
        
        // Red light violation
        if (other.CompareTag("RedLight") && currentSpeed > 5f)
        {
            violations++;
            Debug.Log($"🚨 REAL Red Light Violation at {currentSpeed:F1} mph");
            dataManager.RecordViolation("Red Light", currentSpeed, other.name);
        }
        
        // Stop sign violation
        if (other.CompareTag("StopSign") && currentSpeed > 5f)
        {
            violations++;
            Debug.Log($"🛑 REAL Stop Sign Violation at {currentSpeed:F1} mph");
            dataManager.RecordViolation("Stop Sign", currentSpeed, other.name);
        }
        
        // Level completion
        if (other.CompareTag("Checkpoint") || other.CompareTag("FinishLine"))
        {
            Debug.Log($"🎯 REAL Level Progress: Level {level}, Score {score}");
            dataManager.SaveProgress(level, score, 100f, Time.time - sessionStartTime);
        }
    }
    
    private void FindPlayer()
    {
        // Try to find player/vehicle
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (!playerObj) playerObj = GameObject.FindGameObjectWithTag("Vehicle");
        if (!playerObj) playerObj = GameObject.FindGameObjectWithTag("Car");
        
        if (playerObj)
        {
            player = playerObj.transform;
            playerRb = playerObj.GetComponent<Rigidbody>();
            Debug.Log("✅ Player found for real tracking");
        }
        else
        {
            Debug.LogWarning("⚠️ No player found - tracking disabled");
        }
    }
    
    private void TrackSpeed()
    {
        if (!playerRb) return;
        
        // Calculate speed in mph
        currentSpeed = playerRb.velocity.magnitude * 2.237f;
        
        // Update max speed
        if (currentSpeed > maxSpeed)
        {
            maxSpeed = currentSpeed;
        }
        
        // Check for speeding violation
        if (currentSpeed > minViolationSpeed)
        {
            violations++;
            Debug.Log($"🏎️ REAL Speeding Violation: {currentSpeed:F1} mph");
            dataManager.RecordViolation("Speeding", currentSpeed, "Current Location");
        }
    }
    
    // Public methods for external control
    public void UpdateScore(int points)
    {
        score += points;
        Debug.Log($"🏆 REAL Score updated: +{points} (Total: {score})");
    }
    
    public void SetLevel(int newLevel)
    {
        level = newLevel;
        Debug.Log($"🎮 REAL Level set to: {level}");
    }
    
    public void GetStats()
    {
        float sessionTime = Time.time - sessionStartTime;
        
        Debug.Log($"📊 REAL Game Stats:");
        Debug.Log($"   Session Time: {sessionTime:F1}s");
        Debug.Log($"   Current Speed: {currentSpeed:F1} mph");
        Debug.Log($"   Max Speed: {maxSpeed:F1} mph");
        Debug.Log($"   Violations: {violations}");
        Debug.Log($"   Collisions: {collisions}");
        Debug.Log($"   Score: {score}");
        Debug.Log($"   Level: {level}");
    }
    
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
    
    // Get stats method
    [ContextMenu("Get Real Stats")]
    public void ShowStats() => GetStats();
}
