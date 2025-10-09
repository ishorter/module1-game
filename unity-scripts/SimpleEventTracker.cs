using UnityEngine;
using System.Collections;

/// <summary>
/// Simple Event Tracker - Works with ANY Unity game
/// Tracks basic game events without requiring specific setup
/// Minimal, reliable, and universal
/// </summary>
public class SimpleEventTracker : MonoBehaviour
{
    [Header("Simple Tracking Settings")]
    [SerializeField] private bool enableTracking = true;
    [SerializeField] private float saveInterval = 5f; // Save every 5 seconds
    
    // Basic tracking variables
    private float sessionStartTime;
    private float lastSaveTime;
    private int eventCount = 0;
    private bool isFirebaseReady = false;
    
    // Simple counters
    private int mouseClicks = 0;
    private int keyPresses = 0;
    private int collisions = 0;
    private int triggers = 0;
    
    void Start()
    {
        sessionStartTime = Time.time;
        lastSaveTime = Time.time;
        
        Debug.Log("🎯 Simple Event Tracker started");
        Debug.Log("📊 Tracking basic game events...");
        
        // Initialize Firebase
        StartCoroutine(InitializeFirebase());
    }
    
    IEnumerator InitializeFirebase()
    {
        yield return new WaitForSeconds(3f); // Wait for Firebase
        
        try
        {
            CallJavaScript("UnityFirebase.startSession", "");
            isFirebaseReady = true;
            Debug.Log("✅ Firebase ready for simple tracking");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"⚠️ Firebase not ready: {e.Message}");
        }
    }
    
    void Update()
    {
        if (!enableTracking) return;
        
        // Simple input tracking
        if (Input.GetMouseButtonDown(0))
        {
            mouseClicks++;
            RecordEvent("MOUSE_CLICK", "Left click");
        }
        
        if (Input.anyKeyDown)
        {
            keyPresses++;
            RecordEvent("KEY_PRESS", "Key pressed");
        }
        
        // Auto-save every few seconds
        if (Time.time - lastSaveTime >= saveInterval)
        {
            SaveGameData();
            lastSaveTime = Time.time;
        }
    }
    
    // Universal collision detection
    void OnCollisionEnter(Collision collision)
    {
        collisions++;
        RecordEvent("COLLISION", collision.gameObject.name);
    }
    
    // Universal trigger detection
    void OnTriggerEnter(Collider other)
    {
        triggers++;
        RecordEvent("TRIGGER", other.name);
    }
    
    void RecordEvent(string eventType, string eventData)
    {
        eventCount++;
        
        string data = $"EVENT|{eventType}|{eventData}|{Time.time:F1}|{eventCount}";
        
        if (isFirebaseReady)
        {
            CallJavaScript("UnityFirebase.recordDrivingEvent", data);
        }
        
        Debug.Log($"📝 Event #{eventCount}: {eventType} - {eventData}");
    }
    
    void SaveGameData()
    {
        if (!isFirebaseReady) return;
        
        float sessionTime = Time.time - sessionStartTime;
        
        // Create simple game data
        string gameData = $"GAME_DATA|{sessionTime:F1}|{eventCount}|{mouseClicks}|{keyPresses}|{collisions}|{triggers}";
        
        CallJavaScript("UnityFirebase.updateSessionStats", gameData);
        
        Debug.Log($"💾 Game data saved: {sessionTime:F1}s, {eventCount} events");
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
    public void RecordGameEvent(string eventType, string eventData)
    {
        RecordEvent(eventType, eventData);
    }
    
    public void RecordScore(int score)
    {
        RecordEvent("SCORE", score.ToString());
    }
    
    public void RecordLevel(string levelName)
    {
        RecordEvent("LEVEL", levelName);
    }
    
    // Context menu for testing
    [ContextMenu("Test Simple Tracking")]
    public void TestSimpleTracking()
    {
        Debug.Log("🧪 Testing simple tracking...");
        
        RecordEvent("TEST", "Simple tracking test");
        RecordScore(1000);
        RecordLevel("Test Level");
        
        Debug.Log("✅ Simple tracking test completed");
    }
    
    [ContextMenu("Get Simple Stats")]
    public void GetSimpleStats()
    {
        float sessionTime = Time.time - sessionStartTime;
        
        Debug.Log("📊 Simple Tracking Stats:");
        Debug.Log($"   Session Time: {sessionTime:F1} seconds");
        Debug.Log($"   Total Events: {eventCount}");
        Debug.Log($"   Mouse Clicks: {mouseClicks}");
        Debug.Log($"   Key Presses: {keyPresses}");
        Debug.Log($"   Collisions: {collisions}");
        Debug.Log($"   Triggers: {triggers}");
        Debug.Log($"   Firebase Ready: {isFirebaseReady}");
    }
}
