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
    
    void Awake()
    {
        // Ensure this GameObject persists across scenes
        DontDestroyOnLoad(gameObject);
    }
    
    private IEnumerator InitializeFirebase()
    {
        // Wait for JavaScript bridge to be ready
        yield return new WaitForSeconds(3f);
        
        // Initialize the JavaScript bridge
        CallJavaScript("window.unityFirebaseBridge.initializeBridge", "");
        
        yield return new WaitForSeconds(1f);
        
        // Set Unity instance reference in JavaScript
        CallJavaScript("window.unityFirebaseBridge.setUnityInstance", "");
        
        yield return new WaitForSeconds(0.5f);
        
        isFirebaseReady = true;
        LogMessage("Firebase bridge ready for game data");
        
        // Test the connection with real data
        TestConnection();
        
        // Auto-generate some test data to verify system works
        yield return new WaitForSeconds(2f);
        GenerateTestData();
    }
    
    private void GenerateTestData()
    {
        LogMessage("Generating test data to verify Firebase connection...");
        
        // Send test data
        RecordViolation("Auto Test Violation", 65f, "Test Location");
        
        // Wait a bit then send more
        Invoke(nameof(SendMoreTestData), 1f);
    }
    
    private void SendMoreTestData()
    {
        RecordCollision("Auto Test Collision", "Test Object", 20f);
        SaveProgress(1, 1000, 25f, 60f);
        LogMessage("Test data generation complete - check Firestore console!");
    }
    
    private void TestConnection()
    {
        if (isFirebaseReady)
        {
            LogMessage("Testing Firebase connection...");
            // Send a test violation
            RecordViolation("Test Connection", 50f, "Test Location");
        }
    }
    
    /// <summary>
    /// Record a traffic violation - Simple version without JSON
    /// </summary>
    public void RecordViolation(string violationType, float speed, string location)
    {
        if (!isFirebaseReady) return;
        
        totalViolations++;
        
        // Send data directly to JavaScript without JSON
        string data = $"{violationType}|{speed}|{location}|{totalViolations}";
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
            Application.ExternalCall(methodName, data);
        }
        catch (System.Exception e)
        {
            LogMessage($"Error calling JavaScript method {methodName}: {e.Message}");
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
