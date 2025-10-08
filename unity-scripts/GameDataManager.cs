using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// Professional Game Data Manager for Unity WebGL
/// Handles all Firebase data operations through JavaScript bridge
/// </summary>
public class GameDataManager : MonoBehaviour
{
    [Header("Firebase Integration")]
    [SerializeField] private bool enableFirebaseLogging = true;
    
    [Header("Session Management")]
    [SerializeField] private float sessionUpdateInterval = 30f; // Update every 30 seconds
    
    private string userId;
    private string sessionId;
    private bool isFirebaseReady = false;
    private float sessionStartTime;
    private int totalViolations = 0;
    private int totalCollisions = 0;
    
    // Events
    public static event Action<bool> OnFirebaseReady;
    public static event Action<string> OnDataSaved;
    public static event Action<string> OnDataError;

    #region Unity Lifecycle
    
    private void Start()
    {
        InitializeGameData();
        StartCoroutine(InitializeFirebaseBridge());
    }
    
    private void Update()
    {
        // Periodic session stats update
        if (isFirebaseReady && Time.time - sessionStartTime > sessionUpdateInterval)
        {
            UpdateSessionStats();
        }
    }
    
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && isFirebaseReady)
        {
            EndSession();
        }
    }
    
    private void OnDestroy()
    {
        if (isFirebaseReady)
        {
            EndSession();
        }
    }
    
    #endregion

    #region Initialization
    
    private void InitializeGameData()
    {
        sessionStartTime = Time.time;
        userId = GenerateUserId();
        sessionId = GenerateSessionId();
        
        LogMessage("Game Data Manager initialized");
        LogMessage($"User ID: {userId}");
        LogMessage($"Session ID: {sessionId}");
    }
    
    private IEnumerator InitializeFirebaseBridge()
    {
        // Wait for JavaScript bridge to be ready
        yield return new WaitForSeconds(2f);
        
        // Test Firebase connection
        CallJavaScript("UnityFirebase.getUserStats", "");
        
        yield return new WaitForSeconds(1f);
        
        // Start session
        StartSession();
        
        isFirebaseReady = true;
        OnFirebaseReady?.Invoke(true);
        
        LogMessage("Firebase bridge initialized successfully");
    }
    
    #endregion

    #region Public API Methods
    
    /// <summary>
    /// Save game progress data
    /// </summary>
    public void SaveProgress(int level, int score, float completion, float timeSpent)
    {
        if (!isFirebaseReady) return;
        
        var progressData = new
        {
            level = level,
            score = score,
            completion = completion,
            timeSpent = timeSpent,
            timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ")
        };
        
        string jsonData = JsonUtility.ToJson(progressData);
        CallJavaScript("UnityFirebase.saveProgress", jsonData);
        
        LogMessage($"Progress saved: Level {level}, Score {score}, Completion {completion}%");
    }
    
    /// <summary>
    /// Record a traffic violation
    /// </summary>
    public void RecordViolation(string violationType, float speed = 0f, string location = "Unknown")
    {
        if (!isFirebaseReady) return;
        
        totalViolations++;
        
        var violationData = new
        {
            type = violationType,
            speed = speed,
            location = location,
            severity = CalculateSeverity(speed, violationType),
            violationNumber = totalViolations,
            timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ")
        };
        
        string jsonData = JsonUtility.ToJson(violationData);
        CallJavaScript("UnityFirebase.recordViolation", jsonData);
        
        LogMessage($"Violation recorded: {violationType} at {speed} mph");
    }
    
    /// <summary>
    /// Record a collision event
    /// </summary>
    public void RecordCollision(string collisionType, string objectHit, float impactForce = 0f)
    {
        if (!isFirebaseReady) return;
        
        totalCollisions++;
        
        var collisionData = new
        {
            type = collisionType,
            objectHit = objectHit,
            impactForce = impactForce,
            damage = CalculateDamage(impactForce),
            collisionNumber = totalCollisions,
            timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ")
        };
        
        string jsonData = JsonUtility.ToJson(collisionData);
        CallJavaScript("UnityFirebase.recordCollision", jsonData);
        
        LogMessage($"Collision recorded: {collisionType} with {objectHit}");
    }
    
    /// <summary>
    /// Record a driving event
    /// </summary>
    public void RecordDrivingEvent(string eventType, float value, Vector3 position)
    {
        if (!isFirebaseReady) return;
        
        var eventData = new
        {
            type = eventType,
            value = value,
            position = new { x = position.x, y = position.y, z = position.z },
            timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ")
        };
        
        string jsonData = JsonUtility.ToJson(eventData);
        CallJavaScript("UnityFirebase.recordDrivingEvent", jsonData);
        
        LogMessage($"Driving event recorded: {eventType} = {value}");
    }
    
    /// <summary>
    /// Start a new game session
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
    
    /// <summary>
    /// Update session statistics
    /// </summary>
    public void UpdateSessionStats()
    {
        if (!isFirebaseReady) return;
        
        float playTime = Time.time - sessionStartTime;
        
        var statsData = new
        {
            playTime = playTime,
            violationsCount = totalViolations,
            collisionsCount = totalCollisions,
            averageSpeed = GetAverageSpeed(),
            maxSpeed = GetMaxSpeed(),
            distanceDriven = GetDistanceDriven()
        };
        
        string jsonData = JsonUtility.ToJson(statsData);
        CallJavaScript("UnityFirebase.updateSessionStats", jsonData);
        
        LogMessage($"Session stats updated: {totalViolations} violations, {totalCollisions} collisions");
    }
    
    /// <summary>
    /// Load user progress from Firebase
    /// </summary>
    public void LoadProgress()
    {
        if (!isFirebaseReady) return;
        
        CallJavaScript("UnityFirebase.loadProgress", "");
        LogMessage("Loading progress from Firebase...");
    }
    
    /// <summary>
    /// Get user statistics
    /// </summary>
    public void GetUserStats()
    {
        if (!isFirebaseReady) return;
        
        CallJavaScript("UnityFirebase.getUserStats", "");
        LogMessage("Getting user statistics...");
    }
    
    #endregion

    #region Helper Methods
    
    private void CallJavaScript(string methodName, string data)
    {
        try
        {
            Application.ExternalCall(methodName, data);
        }
        catch (Exception e)
        {
            LogMessage($"Error calling JavaScript method {methodName}: {e.Message}");
            OnDataError?.Invoke(e.Message);
        }
    }
    
    private string GenerateUserId()
    {
        // Try to get existing user ID from localStorage
        try
        {
            Application.ExternalEval("var userId = localStorage.getItem('unity_user_id'); if (!userId) { userId = 'user_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9); localStorage.setItem('unity_user_id', userId); }");
        }
        catch
        {
            // Fallback to Unity-generated ID
            return "user_" + DateTime.Now.Ticks + "_" + UnityEngine.Random.Range(1000, 9999);
        }
        
        return "user_" + DateTime.Now.Ticks + "_" + UnityEngine.Random.Range(1000, 9999);
    }
    
    private string GenerateSessionId()
    {
        return "session_" + DateTime.Now.Ticks + "_" + UnityEngine.Random.Range(1000, 9999);
    }
    
    private string CalculateSeverity(float speed, string violationType)
    {
        if (violationType.Contains("Speeding"))
        {
            if (speed > 80) return "High";
            if (speed > 65) return "Medium";
            return "Low";
        }
        return "Medium";
    }
    
    private float CalculateDamage(float impactForce)
    {
        if (impactForce > 50) return 100f; // Total damage
        if (impactForce > 25) return 75f;  // Major damage
        if (impactForce > 10) return 50f;  // Moderate damage
        return 25f; // Minor damage
    }
    
    private float GetAverageSpeed()
    {
        // This would typically come from your game's speed tracking system
        return 45f; // Placeholder
    }
    
    private float GetMaxSpeed()
    {
        // This would typically come from your game's speed tracking system
        return 75f; // Placeholder
    }
    
    private float GetDistanceDriven()
    {
        // This would typically come from your game's distance tracking system
        return 5.2f; // Placeholder
    }
    
    private void LogMessage(string message)
    {
        if (enableFirebaseLogging)
        {
            Debug.Log($"[GameDataManager] {message}");
        }
    }
    
    #endregion

    #region Callbacks from JavaScript
    
    /// <summary>
    /// Called from JavaScript when data is saved successfully
    /// </summary>
    public void OnDataSavedCallback(string result)
    {
        LogMessage($"Data saved successfully: {result}");
        OnDataSaved?.Invoke(result);
    }
    
    /// <summary>
    /// Called from JavaScript when there's an error
    /// </summary>
    public void OnDataErrorCallback(string error)
    {
        LogMessage($"Data save error: {error}");
        OnDataError?.Invoke(error);
    }
    
    /// <summary>
    /// Test communication from JavaScript
    /// </summary>
    public void TestCommunication(string message)
    {
        LogMessage($"✅ C# ↔ JavaScript communication successful: {message}");
        
        // Test sending data back to JavaScript
        if (isFirebaseReady)
        {
            RecordViolation("Test Violation", 65f, "Test Location");
            LogMessage("📡 Test violation sent to JavaScript");
        }
    }
    
    #endregion
}
