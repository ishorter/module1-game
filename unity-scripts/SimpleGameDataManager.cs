using UnityEngine;
using System.Collections;

/// <summary>
/// Simple Game Data Manager for Unity WebGL
/// Sends data to Firebase without JSON serialization
/// </summary>
public class SimpleGameDataManager : MonoBehaviour
{
    [Header("Firebase Integration")]
    [SerializeField] private bool enableFirebaseLogging = true;
    
    private bool isFirebaseReady = false;
    private int totalViolations = 0;
    private int totalCollisions = 0;
    
    void Start()
    {
        // Wait for Firebase to initialize
        StartCoroutine(InitializeFirebase());
    }
    
    private IEnumerator InitializeFirebase()
    {
        // Wait for JavaScript bridge to be ready
        yield return new WaitForSeconds(3f);
        
        LogMessage("Waiting for Firebase initialization...");
        
        // Don't set isFirebaseReady to true yet - wait for JavaScript callback
        // Test the connection
        TestConnection();
    }
    
    private void TestConnection()
    {
        LogMessage("Testing Firebase connection...");
        // Send a test violation (will be queued if Firebase not ready)
        RecordViolation("Test Connection", 50f, "Test Location");
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
        }
        else if (status == "failed")
        {
            isFirebaseReady = false;
            LogMessage("❌ Firebase connection failed - check configuration");
        }
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
        }
    }
    
    private void LogMessage(string message)
    {
        if (enableFirebaseLogging)
        {
            Debug.Log($"[SimpleGameDataManager] {message}");
        }
    }
    
    // Public methods for easy testing
    public void TestViolation() => RecordViolation("Speeding", 75f, "Highway");
    public void TestCollision() => RecordCollision("Vehicle", "Car_A", 25f);
    public void TestProgress() => SaveProgress(1, 1000, 50f, 120f);
    public void TestDrivingEvent() => RecordDrivingEvent("Braking", 0.8f, Vector3.zero);
}
