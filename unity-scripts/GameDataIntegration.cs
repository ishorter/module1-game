using UnityEngine;

/// <summary>
/// Game Data Integration - Connects all data managers to Firebase
/// Add this script to your main game manager or create an empty GameObject with this script
/// </summary>
public class GameDataIntegration : MonoBehaviour
{
    [Header("Data Managers")]
    [SerializeField] private PerformanceDataManager performanceManager;
    [SerializeField] private SimpleGameDataManager simpleDataManager;
    [SerializeField] private DrivingDataManager drivingDataManager;
    
    [Header("Auto-Integration")]
    [SerializeField] private bool autoFindManagers = true;
    [SerializeField] private bool enableRealTimeUpdates = true;
    
    private void Start()
    {
        if (autoFindManagers)
        {
            // Automatically find all data managers
            if (!performanceManager) performanceManager = FindObjectOfType<PerformanceDataManager>();
            if (!simpleDataManager) simpleDataManager = FindObjectOfType<SimpleGameDataManager>();
            if (!drivingDataManager) drivingDataManager = DrivingDataManager.Instance;
        }
        
        // Create PerformanceDataManager if it doesn't exist
        if (!performanceManager)
        {
            GameObject perfObj = new GameObject("PerformanceDataManager");
            performanceManager = perfObj.AddComponent<PerformanceDataManager>();
            DontDestroyOnLoad(perfObj);
        }
        
        Debug.Log("🎮 Game Data Integration initialized");
        Debug.Log($"📊 Performance Manager: {(performanceManager ? "✅" : "❌")}");
        Debug.Log($"🎯 Simple Data Manager: {(simpleDataManager ? "✅" : "❌")}");
        Debug.Log($"🚗 Driving Data Manager: {(drivingDataManager ? "✅" : "❌")}");
    }
    
    private void Update()
    {
        if (!enableRealTimeUpdates) return;
        
        // Update performance data from other managers
        UpdatePerformanceData();
    }
    
    /// <summary>
    /// Update performance data from other managers
    /// </summary>
    private void UpdatePerformanceData()
    {
        if (!performanceManager) return;
        
        // Update from DrivingDataManager
        if (drivingDataManager)
        {
            // Update collision count
            int currentCollisions = drivingDataManager.GetCollisionsCount();
            if (currentCollisions != performanceManager.GetCollisionCount())
            {
                for (int i = performanceManager.GetCollisionCount(); i < currentCollisions; i++)
                {
                    performanceManager.RecordCollision();
                }
            }
            
            // Update violation count
            int currentViolations = drivingDataManager.GetViolationsCount();
            if (currentViolations != performanceManager.GetViolationCount())
            {
                for (int i = performanceManager.GetViolationCount(); i < currentViolations; i++)
                {
                    performanceManager.RecordViolation();
                }
            }
            
            // Update session time
            float sessionTime = drivingDataManager.GetSessionTime();
            if (sessionTime > 0)
            {
                // Session time is automatically tracked in PerformanceDataManager
            }
        }
    }
    
    /// <summary>
    /// Call this when player achieves a new max speed
    /// </summary>
    public void OnMaxSpeedAchieved(float speed)
    {
        if (performanceManager)
        {
            performanceManager.UpdateMaxSpeed(speed);
        }
        
        Debug.Log($"🏎️ New max speed achieved: {speed:F1} m/s ({speed * 2.237f:F1} mph)");
    }
    
    /// <summary>
    /// Call this when player has a collision
    /// </summary>
    public void OnCollisionOccurred()
    {
        if (performanceManager)
        {
            performanceManager.RecordCollision();
        }
        
        if (simpleDataManager)
        {
            simpleDataManager.RecordCollision("Vehicle", "Unknown", 25f);
        }
        
        Debug.Log("💥 Collision recorded in all data managers");
    }
    
    /// <summary>
    /// Call this when player commits a violation
    /// </summary>
    public void OnViolationOccurred(string violationType, float speed, string location)
    {
        if (performanceManager)
        {
            performanceManager.RecordViolation();
        }
        
        if (simpleDataManager)
        {
            simpleDataManager.RecordViolation(violationType, speed, location);
        }
        
        Debug.Log($"🚨 Violation recorded: {violationType} at {speed} mph");
    }
    
    /// <summary>
    /// Call this when player completes a level or achieves a score
    /// </summary>
    public void OnScoreUpdated(int newScore)
    {
        if (performanceManager)
        {
            performanceManager.UpdateScore(newScore);
        }
        
        Debug.Log($"🏆 Score updated: {newScore}");
    }
    
    /// <summary>
    /// Call this when player travels a certain distance
    /// </summary>
    public void OnDistanceUpdated(float distance)
    {
        if (performanceManager)
        {
            performanceManager.UpdateDistance(distance);
        }
        
        Debug.Log($"📏 Distance updated: {distance:F2} units");
    }
    
    /// <summary>
    /// Call this when level changes
    /// </summary>
    public void OnLevelChanged(string levelName)
    {
        if (performanceManager)
        {
            performanceManager.SetLevelName(levelName);
        }
        
        Debug.Log($"🎮 Level changed to: {levelName}");
    }
    
    /// <summary>
    /// Force save all performance data
    /// </summary>
    public void SaveAllData()
    {
        if (performanceManager)
        {
            float sessionTime = performanceManager.GetSessionTime();
            performanceManager.SaveLogToJson(sessionTime);
        }
        
        if (simpleDataManager)
        {
            simpleDataManager.UpdateSessionStats();
        }
        
        if (drivingDataManager)
        {
            drivingDataManager.SaveAllData();
        }
        
        Debug.Log("💾 All data saved to Firebase");
    }
    
    /// <summary>
    /// Get current performance summary
    /// </summary>
    public string GetPerformanceSummary()
    {
        if (!performanceManager) return "Performance Manager not available";
        
        return $"Max Speed: {performanceManager.GetMaxSpeedMPH():F1} mph, " +
               $"Collisions: {performanceManager.GetCollisionCount()}, " +
               $"Violations: {performanceManager.GetViolationCount()}, " +
               $"Score: {performanceManager.GetScore()}";
    }
}
