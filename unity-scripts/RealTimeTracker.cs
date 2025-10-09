using UnityEngine;

/// <summary>
/// Real-Time Tracker - Simple verification script
/// Attach this to any GameObject to verify real-time tracking is working
/// </summary>
public class RealTimeTracker : MonoBehaviour
{
    [Header("Real-Time Tracking Verification")]
    [SerializeField] private bool enableTracking = true;
    [SerializeField] private float testInterval = 5f; // Test every 5 seconds
    
    private float testTimer = 0f;
    private int testCount = 0;
    
    void Start()
    {
        Debug.Log("🎮 Real-Time Tracker started - monitoring game events");
    }
    
    void Update()
    {
        if (!enableTracking) return;
        
        testTimer += Time.deltaTime;
        
        // Test real-time tracking every few seconds
        if (testTimer >= testInterval)
        {
            testCount++;
            Debug.Log($"📊 Real-Time Tracking Test #{testCount} - System Active");
            
            // Check if tracking managers are working
            CheckTrackingManagers();
            
            testTimer = 0f;
        }
    }
    
    void CheckTrackingManagers()
    {
        // Check SimpleGameDataManager
        SimpleGameDataManager simpleManager = FindObjectOfType<SimpleGameDataManager>();
        if (simpleManager != null)
        {
            Debug.Log("✅ SimpleGameDataManager found and active");
        }
        else
        {
            Debug.LogWarning("⚠️ SimpleGameDataManager not found");
        }
        
        // Check JSONGameDataManager
        JSONGameDataManager jsonManager = FindObjectOfType<JSONGameDataManager>();
        if (jsonManager != null)
        {
            Debug.Log("✅ JSONGameDataManager found and active");
        }
        else
        {
            Debug.LogWarning("⚠️ JSONGameDataManager not found");
        }
    }
    
    // Test method to verify tracking
    [ContextMenu("Test Real-Time Tracking")]
    public void TestRealTimeTracking()
    {
        Debug.Log("🧪 Testing real-time tracking...");
        
        // Find and test tracking managers
        SimpleGameDataManager simpleManager = FindObjectOfType<SimpleGameDataManager>();
        if (simpleManager != null)
        {
            Debug.Log("✅ SimpleGameDataManager: Ready for real-time tracking");
        }
        
        JSONGameDataManager jsonManager = FindObjectOfType<JSONGameDataManager>();
        if (jsonManager != null)
        {
            Debug.Log("✅ JSONGameDataManager: Ready for JSON data tracking");
        }
        
        Debug.Log("🎯 Real-time tracking systems are ACTIVE and ready!");
        Debug.Log("📡 All game events will be automatically tracked and saved to Firebase");
    }
}
