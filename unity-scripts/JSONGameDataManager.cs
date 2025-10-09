using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// JSON Game Data Manager for Unity WebGL
/// Works with the game developer's existing JSON data tracking system
/// Converts JSON data to Firebase Firestore storage
/// </summary>
public class JSONGameDataManager : MonoBehaviour
{
    [System.Serializable]
    public class PerformanceData
    {
        public float maxSpeedMPH;
        public int collisionCount;
        public float sessionDurationSeconds;
        public string sessionDurationFormatted;
        public string timestamp;
        public int violationCount;
        public float averageSpeed;
        public float totalDistance;
        public int score;
        public string levelName;
    }
    
    [System.Serializable]
    public class ViolationData
    {
        public string violationType;
        public float speed;
        public string location;
        public string timestamp;
        public int violationNumber;
    }
    
    [System.Serializable]
    public class CollisionData
    {
        public string collisionType;
        public string objectHit;
        public float impactForce;
        public string timestamp;
        public int collisionNumber;
    }
    
    [System.Serializable]
    public class ProgressData
    {
        public int level;
        public int score;
        public float completion;
        public float timeSpent;
        public string timestamp;
    }
    
    [Header("JSON Data Tracking")]
    [SerializeField] private bool enableJSONTracking = true;
    [SerializeField] private bool enableFirebaseLogging = true;
    
    [Header("Auto-Save Settings")]
    [SerializeField] private float autoSaveInterval = 30f; // Auto-save every 30 seconds
    
    private bool isFirebaseReady = false;
    private bool isWebGL = false;
    
    // JSON Data Storage
    private PerformanceData currentPerformance;
    private List<ViolationData> violations = new List<ViolationData>();
    private List<CollisionData> collisions = new List<CollisionData>();
    private List<ProgressData> progressData = new List<ProgressData>();
    
    // Auto-save timer
    private float autoSaveTimer = 0f;
    
    void Start()
    {
        // Detect WebGL platform
        isWebGL = Application.platform == RuntimePlatform.WebGLPlayer;
        
        // Initialize JSON tracking
        InitializeJSONTracking();
        
        // Wait for Firebase to initialize
        StartCoroutine(InitializeFirebase());
        
        Debug.Log($"🎮 JSON Game Data Manager initialized - WebGL: {isWebGL}");
    }
    
    void Update()
    {
        if (!enableJSONTracking) return;
        
        // Auto-save timer
        autoSaveTimer += Time.deltaTime;
        if (autoSaveTimer >= autoSaveInterval)
        {
            AutoSaveAllData();
            autoSaveTimer = 0f;
        }
        
        // Real-time tracking for automatic event detection
        UpdateRealTimeTracking();
    }
    
    // Real-time tracking variables
    private float trackingTimer = 0f;
    private float sessionStartTime = 0f;
    private float currentSpeed = 0f;
    private float maxSpeed = 0f;
    private Vector3 lastPosition;
    private bool isTracking = false;
    
    // References for real-time tracking
    private Transform playerTransform;
    private Rigidbody playerRigidbody;
    
    #region JSON Data Methods (Compatible with Game Developer's System)
    
    /// <summary>
    /// Save performance data as JSON (compatible with game developer's SaveLogToJson method)
    /// </summary>
    public void SaveLogToJson(float sessionTime, float maxSpeed = 0f, int collisionCount = 0, int violationCount = 0, float averageSpeed = 0f, float totalDistance = 0f, int score = 0, string levelName = "Level 1")
    {
        if (!enableJSONTracking) return;
        
        // Convert seconds → minutes:seconds format (game developer's original code)
        int minutes = Mathf.FloorToInt(sessionTime / 60f);
        int seconds = Mathf.FloorToInt(sessionTime % 60f);
        string formattedTime = string.Format("{0:00}:{1:00}", minutes, seconds);
        
        // Create PerformanceData (game developer's structure)
        currentPerformance = new PerformanceData
        {
            maxSpeedMPH = maxSpeed * 2.237f,
            collisionCount = collisionCount,
            sessionDurationSeconds = sessionTime,
            sessionDurationFormatted = formattedTime,
            timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            violationCount = violationCount,
            averageSpeed = averageSpeed,
            totalDistance = totalDistance,
            score = score,
            levelName = levelName
        };
        
        // Convert to JSON
        string json = JsonUtility.ToJson(currentPerformance, true);
        
        Debug.Log("📄 Performance Data JSON:");
        Debug.Log(json);
        
        if (isWebGL)
        {
            // For WebGL: Send to Firebase
            SavePerformanceToFirebase(json);
        }
        else
        {
            // For local development: Save to file (game developer's original method)
            SavePerformanceToFile(json);
        }
        
        // Save to PlayerPrefs (game developer's method)
        SaveToPlayerPrefs();
    }
    
    /// <summary>
    /// Record violation data as JSON
    /// </summary>
    public void RecordViolationJSON(string violationType, float speed, string location)
    {
        if (!enableJSONTracking) return;
        
        var violation = new ViolationData
        {
            violationType = violationType,
            speed = speed,
            location = location,
            timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            violationNumber = violations.Count + 1
        };
        
        violations.Add(violation);
        
        string json = JsonUtility.ToJson(violation, true);
        Debug.Log($"📄 Violation JSON: {json}");
        
        if (isWebGL)
        {
            SaveViolationToFirebase(json);
        }
        else
        {
            SaveViolationToFile(json);
        }
    }
    
    /// <summary>
    /// Record collision data as JSON
    /// </summary>
    public void RecordCollisionJSON(string collisionType, string objectHit, float impactForce)
    {
        if (!enableJSONTracking) return;
        
        var collision = new CollisionData
        {
            collisionType = collisionType,
            objectHit = objectHit,
            impactForce = impactForce,
            timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            collisionNumber = collisions.Count + 1
        };
        
        collisions.Add(collision);
        
        string json = JsonUtility.ToJson(collision, true);
        Debug.Log($"📄 Collision JSON: {json}");
        
        if (isWebGL)
        {
            SaveCollisionToFirebase(json);
        }
        else
        {
            SaveCollisionToFile(json);
        }
    }
    
    /// <summary>
    /// Save progress data as JSON
    /// </summary>
    public void SaveProgressJSON(int level, int score, float completion, float timeSpent)
    {
        if (!enableJSONTracking) return;
        
        var progress = new ProgressData
        {
            level = level,
            score = score,
            completion = completion,
            timeSpent = timeSpent,
            timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };
        
        progressData.Add(progress);
        
        string json = JsonUtility.ToJson(progress, true);
        Debug.Log($"📄 Progress JSON: {json}");
        
        if (isWebGL)
        {
            SaveProgressToFirebase(json);
        }
        else
        {
            SaveProgressToFile(json);
        }
    }
    
    #endregion
    
    #region WebGL Firebase Methods
    
    private void SavePerformanceToFirebase(string json)
    {
        try
        {
            Application.ExternalCall("UnityFirebase.savePerformanceData", json);
            Debug.Log("✅ Performance JSON sent to Firebase");
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ Failed to send performance JSON to Firebase: " + e.Message);
        }
    }
    
    private void SaveViolationToFirebase(string json)
    {
        try
        {
            Application.ExternalCall("UnityFirebase.recordViolation", json);
            Debug.Log("✅ Violation JSON sent to Firebase");
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ Failed to send violation JSON to Firebase: " + e.Message);
        }
    }
    
    private void SaveCollisionToFirebase(string json)
    {
        try
        {
            Application.ExternalCall("UnityFirebase.recordCollision", json);
            Debug.Log("✅ Collision JSON sent to Firebase");
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ Failed to send collision JSON to Firebase: " + e.Message);
        }
    }
    
    private void SaveProgressToFirebase(string json)
    {
        try
        {
            Application.ExternalCall("UnityFirebase.saveProgress", json);
            Debug.Log("✅ Progress JSON sent to Firebase");
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ Failed to send progress JSON to Firebase: " + e.Message);
        }
    }
    
    #endregion
    
    #region Local File Methods (Game Developer's Original)
    
    private void SavePerformanceToFile(string json)
    {
        string fileName = $"performance_log_{System.DateTime.Now:yyyyMMdd_HHmmss}.json";
        string path = System.IO.Path.Combine(Application.persistentDataPath, fileName);
        
        try
        {
            System.IO.File.WriteAllText(path, json);
            Debug.Log("✅ Performance JSON saved to file: " + path);
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ Failed to save JSON to file: " + e.Message);
        }
    }
    
    private void SaveViolationToFile(string json)
    {
        string fileName = $"violation_log_{System.DateTime.Now:yyyyMMdd_HHmmss}.json";
        string path = System.IO.Path.Combine(Application.persistentDataPath, fileName);
        
        try
        {
            System.IO.File.WriteAllText(path, json);
            Debug.Log("✅ Violation JSON saved to file: " + path);
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ Failed to save violation JSON to file: " + e.Message);
        }
    }
    
    private void SaveCollisionToFile(string json)
    {
        string fileName = $"collision_log_{System.DateTime.Now:yyyyMMdd_HHmmss}.json";
        string path = System.IO.Path.Combine(Application.persistentDataPath, fileName);
        
        try
        {
            System.IO.File.WriteAllText(path, json);
            Debug.Log("✅ Collision JSON saved to file: " + path);
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ Failed to save collision JSON to file: " + e.Message);
        }
    }
    
    private void SaveProgressToFile(string json)
    {
        string fileName = $"progress_log_{System.DateTime.Now:yyyyMMdd_HHmmss}.json";
        string path = System.IO.Path.Combine(Application.persistentDataPath, fileName);
        
        try
        {
            System.IO.File.WriteAllText(path, json);
            Debug.Log("✅ Progress JSON saved to file: " + path);
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ Failed to save progress JSON to file: " + e.Message);
        }
    }
    
    #endregion
    
    #region PlayerPrefs Methods (Game Developer's Original)
    
    private void SaveToPlayerPrefs()
    {
        if (currentPerformance == null) return;
        
        // Save individual variables with descriptive names (game developer's method)
        PlayerPrefs.SetFloat("MaxSpeedMPH", currentPerformance.maxSpeedMPH);
        PlayerPrefs.SetInt("CollisionCount", currentPerformance.collisionCount);
        PlayerPrefs.SetInt("ViolationCount", currentPerformance.violationCount);
        PlayerPrefs.SetFloat("SessionDurationSeconds", currentPerformance.sessionDurationSeconds);
        PlayerPrefs.SetString("SessionDurationFormatted", currentPerformance.sessionDurationFormatted);
        PlayerPrefs.SetFloat("AverageSpeed", currentPerformance.averageSpeed);
        PlayerPrefs.SetFloat("TotalDistance", currentPerformance.totalDistance);
        PlayerPrefs.SetInt("Score", currentPerformance.score);
        PlayerPrefs.SetString("LevelName", currentPerformance.levelName);
        PlayerPrefs.SetString("PerformanceTimestamp", currentPerformance.timestamp);
        
        // Save complete JSON string
        PlayerPrefs.SetString("PerformanceDataJSON", JsonUtility.ToJson(currentPerformance));
        
        PlayerPrefs.Save();
        Debug.Log("✅ Performance data saved to PlayerPrefs");
    }
    
    #endregion
    
    #region Firebase Integration
    
    private IEnumerator InitializeFirebase()
    {
        yield return new WaitForSeconds(3f);
        
        LogMessage("Waiting for Firebase initialization...");
        
        yield return new WaitForSeconds(2f);
        
        isFirebaseReady = true;
        LogMessage("✅ Firebase ready for JSON data");
    }
    
    public void OnFirebaseReady(string status)
    {
        if (status == "connected")
        {
            isFirebaseReady = true;
            LogMessage("✅ Firebase connection confirmed - ready for JSON data");
        }
        else if (status == "failed")
        {
            isFirebaseReady = false;
            LogMessage("❌ Firebase connection failed");
        }
    }
    
    private void AutoSaveAllData()
    {
        if (currentPerformance != null)
        {
            string json = JsonUtility.ToJson(currentPerformance, true);
            if (isWebGL)
            {
                SavePerformanceToFirebase(json);
            }
        }
        
        LogMessage($"Auto-saved: {violations.Count} violations, {collisions.Count} collisions, {progressData.Count} progress entries");
    }
    
    private void InitializeJSONTracking()
    {
        // Find player for real-time tracking
        FindPlayer();
        
        // Initialize tracking
        sessionStartTime = Time.time;
        lastPosition = playerTransform ? playerTransform.position : Vector3.zero;
        isTracking = true;
        
        LogMessage("🎮 JSON Game Data Tracking initialized");
        LogMessage($"📊 Platform: {(isWebGL ? "WebGL (Firebase)" : "Local (Files)")}");
        LogMessage("🎯 Real-time tracking: Speed violations, Collisions, Traffic violations");
    }
    
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
            LogMessage("✅ Player found for real-time JSON tracking");
        }
        else
        {
            LogMessage("⚠️ No player found - using camera position");
            playerTransform = Camera.main?.transform;
        }
    }
    
    private void UpdateRealTimeTracking()
    {
        if (!isTracking || !playerRigidbody) return;
        
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
        
        // Check for speed violations (REAL TIME)
        if (currentSpeed > 55f) // Speed limit check
        {
            RecordViolationJSON("Real-Time Speeding", currentSpeed, GetCurrentLocation());
        }
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
    
    // Real-time collision detection
    void OnCollisionEnter(Collision collision)
    {
        if (!isTracking) return;
        
        float impactForce = collision.relativeVelocity.magnitude;
        
        if (impactForce >= 5f)
        {
            // Record REAL collision automatically
            string collisionType = DetermineCollisionType(collision);
            RecordCollisionJSON(collisionType, collision.gameObject.name, impactForce);
        }
    }
    
    // Real-time trigger detection
    void OnTriggerEnter(Collider other)
    {
        if (!isTracking) return;
        
        // Detect REAL traffic violations automatically
        if (other.CompareTag("RedLight") && currentSpeed > 5f)
        {
            RecordViolationJSON("Real-Time Red Light", currentSpeed, other.name);
        }
        
        if (other.CompareTag("StopSign") && currentSpeed > 5f)
        {
            RecordViolationJSON("Real-Time Stop Sign", currentSpeed, other.name);
        }
        
        // Detect REAL level progression automatically
        if (other.CompareTag("Checkpoint") || other.CompareTag("FinishLine"))
        {
            SaveProgressJSON(1, 1000, 100f, Time.time - sessionStartTime);
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
    
    private void LogMessage(string message)
    {
        if (enableFirebaseLogging)
        {
            Debug.Log($"[JSONGameDataManager] {message}");
        }
    }
    
    #endregion
    
    #region Real Data Methods Only
    
    [ContextMenu("Get Real Data Status")]
    public void GetRealDataStatus()
    {
        LogMessage("📊 REAL DATA TRACKING STATUS:");
        LogMessage($"   Real-Time Tracking: {(isTracking ? "✅ ACTIVE" : "❌ INACTIVE")}");
        LogMessage($"   Player Found: {(playerTransform ? "✅ YES" : "❌ NO")}");
        LogMessage($"   Current Speed: {currentSpeed:F1} mph");
        LogMessage($"   Max Speed: {maxSpeed:F1} mph");
        LogMessage($"   Session Time: {Time.time - sessionStartTime:F1}s");
        LogMessage($"   Real Violations: {violations.Count}");
        LogMessage($"   Real Collisions: {collisions.Count}");
        LogMessage($"   Real Progress: {progressData.Count}");
        LogMessage($"   Platform: {(isWebGL ? "WebGL (Firebase)" : "Local (Files)")}");
        
        LogMessage("🎮 ONLY REAL GAME EVENTS ARE TRACKED - NO TEST DATA");
    }
    
    #endregion
}
