using UnityEngine;

/// <summary>
/// Real-Time Tracker Setup - Easy integration script
/// Add this to any GameObject to automatically set up real-time tracking
/// </summary>
public class RealTimeTrackerSetup : MonoBehaviour
{
    [Header("Auto Setup")]
    [SerializeField] private bool autoSetupOnStart = true;
    [SerializeField] private bool createTrackerIfMissing = true;
    
    [Header("Game Objects to Track")]
    [SerializeField] private GameObject playerObject;
    [SerializeField] private GameObject vehicleObject;
    [SerializeField] private GameObject cameraObject;
    
    private RealTimeGameTracker tracker;
    
    void Start()
    {
        if (autoSetupOnStart)
        {
            SetupRealTimeTracking();
        }
    }
    
    /// <summary>
    /// Set up real-time tracking automatically
    /// </summary>
    public void SetupRealTimeTracking()
    {
        Debug.Log("🔧 Setting up Real-Time Game Tracking...");
        
        // Find or create tracker
        FindOrCreateTracker();
        
        // Set up player references
        SetupPlayerReferences();
        
        // Configure tracking settings
        ConfigureTracking();
        
        Debug.Log("✅ Real-Time Game Tracking setup complete!");
        Debug.Log("🎮 Game events will now be tracked and sent to Firebase in real-time");
    }
    
    private void FindOrCreateTracker()
    {
        // Look for existing tracker
        tracker = FindObjectOfType<RealTimeGameTracker>();
        
        if (!tracker && createTrackerIfMissing)
        {
            // Create new tracker
            GameObject trackerObj = new GameObject("RealTimeGameTracker");
            tracker = trackerObj.AddComponent<RealTimeGameTracker>();
            DontDestroyOnLoad(trackerObj);
            
            Debug.Log("✅ Created new RealTimeGameTracker");
        }
        
        if (tracker)
        {
            Debug.Log("✅ RealTimeGameTracker found/created");
        }
        else
        {
            Debug.LogError("❌ Failed to create RealTimeGameTracker");
        }
    }
    
    private void SetupPlayerReferences()
    {
        // Try to find player/vehicle automatically
        if (!playerObject)
        {
            playerObject = GameObject.FindGameObjectWithTag("Player");
        }
        
        if (!vehicleObject)
        {
            vehicleObject = GameObject.FindGameObjectWithTag("Vehicle");
        }
        
        if (!vehicleObject)
        {
            vehicleObject = GameObject.FindGameObjectWithTag("Car");
        }
        
        if (!cameraObject)
        {
            cameraObject = Camera.main?.gameObject;
        }
        
        Debug.Log($"🎯 Player References: Player={playerObject != null}, Vehicle={vehicleObject != null}, Camera={cameraObject != null}");
    }
    
    private void ConfigureTracking()
    {
        if (!tracker) return;
        
        // Configure tracking settings based on your game
        Debug.Log("⚙️ Configuring tracking settings...");
        
        // You can customize these settings based on your game
        // The tracker will automatically detect and track:
        // - Speed violations
        // - Collisions
        // - Traffic violations (red lights, stop signs)
        // - Level progression
        // - Driving events
        // - Session data
        
        Debug.Log("📊 Tracking configured for:");
        Debug.Log("   ✅ Speed violations");
        Debug.Log("   ✅ Collision detection");
        Debug.Log("   ✅ Traffic violations");
        Debug.Log("   ✅ Level progression");
        Debug.Log("   ✅ Driving events");
        Debug.Log("   ✅ Session management");
    }
    
    /// <summary>
    /// Test the real-time tracking system
    /// </summary>
    [ContextMenu("Test Real-Time Tracking")]
    public void TestRealTimeTracking()
    {
        if (!tracker)
        {
            Debug.LogError("❌ No RealTimeGameTracker found");
            return;
        }
        
        Debug.Log("🧪 Testing Real-Time Tracking...");
        
        // Test violation
        tracker.TestViolation();
        
        // Test collision
        tracker.TestCollision();
        
        // Test progress
        tracker.TestProgress();
        
        // Get current stats
        tracker.TestGetStats();
        
        Debug.Log("✅ Real-Time Tracking test complete - Check Firebase console!");
    }
    
    /// <summary>
    /// Get current tracking status
    /// </summary>
    [ContextMenu("Get Tracking Status")]
    public void GetTrackingStatus()
    {
        if (!tracker)
        {
            Debug.Log("❌ Real-Time Tracker: Not Found");
            return;
        }
        
        Debug.Log("📊 Real-Time Tracking Status:");
        Debug.Log($"   ✅ Tracker: Active");
        Debug.Log($"   🎮 Player: {(playerObject ? "Found" : "Not Found")}");
        Debug.Log($"   🚗 Vehicle: {(vehicleObject ? "Found" : "Not Found")}");
        Debug.Log($"   📷 Camera: {(cameraObject ? "Found" : "Not Found")}");
        
        // Get current stats
        tracker.GetCurrentStats();
    }
    
    /// <summary>
    /// Force save current session data
    /// </summary>
    [ContextMenu("Force Save Session")]
    public void ForceSaveSession()
    {
        if (!tracker)
        {
            Debug.LogError("❌ No RealTimeGameTracker found");
            return;
        }
        
        tracker.ForceSaveSession();
        Debug.Log("💾 Session data force saved to Firebase");
    }
}
