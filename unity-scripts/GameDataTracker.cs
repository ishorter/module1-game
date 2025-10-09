using UnityEngine;

/// <summary>
/// Game Data Tracker - Add this to track game events in real-time
/// This script should be added to your main game objects or player controller
/// </summary>
public class GameDataTracker : MonoBehaviour
{
    [Header("Tracking Settings")]
    [SerializeField] private bool enableTracking = true;
    [SerializeField] private float speedUpdateInterval = 0.5f; // Update speed every 0.5 seconds
    
    [Header("References")]
    [SerializeField] private SimpleGameDataManager dataManager;
    [SerializeField] private PerformanceDataManager performanceManager;
    
    // Tracking variables
    private float speedUpdateTimer = 0f;
    private float currentSpeed = 0f;
    private float maxSpeed = 0f;
    private int collisionCount = 0;
    private int violationCount = 0;
    
    void Start()
    {
        // Find data managers if not assigned
        if (!dataManager) dataManager = FindObjectOfType<SimpleGameDataManager>();
        if (!performanceManager) performanceManager = FindObjectOfType<PerformanceDataManager>();
        
        Debug.Log("🎮 Game Data Tracker initialized");
    }
    
    void Update()
    {
        if (!enableTracking) return;
        
        // Update speed tracking
        UpdateSpeedTracking();
        
        // Simulate game events for testing (remove this in real game)
        SimulateGameEvents();
    }
    
    /// <summary>
    /// Update speed tracking
    /// </summary>
    private void UpdateSpeedTracking()
    {
        speedUpdateTimer += Time.deltaTime;
        
        if (speedUpdateTimer >= speedUpdateInterval)
        {
            // Get current speed (replace this with your actual speed calculation)
            currentSpeed = GetCurrentSpeed();
            
            // Update max speed
            if (currentSpeed > maxSpeed)
            {
                maxSpeed = currentSpeed;
                
                // Send to performance manager
                if (performanceManager)
                {
                    performanceManager.UpdateMaxSpeed(maxSpeed);
                }
                
                Debug.Log($"🏎️ New max speed: {maxSpeed:F1} m/s ({maxSpeed * 2.237f:F1} mph)");
            }
            
            speedUpdateTimer = 0f;
        }
    }
    
    /// <summary>
    /// Get current speed (replace with your actual speed calculation)
    /// </summary>
    private float GetCurrentSpeed()
    {
        // Replace this with your actual speed calculation
        // For testing, simulate random speed changes
        return Random.Range(20f, 80f);
    }
    
    /// <summary>
    /// Simulate game events for testing (REMOVE THIS IN REAL GAME)
    /// </summary>
    private void SimulateGameEvents()
    {
        // Simulate collision (5% chance every frame)
        if (Random.Range(0f, 1f) < 0.005f)
        {
            OnCollisionOccurred("Vehicle", "Test_Car", Random.Range(15f, 35f));
        }
        
        // Simulate violation (3% chance every frame)
        if (Random.Range(0f, 1f) < 0.003f)
        {
            string[] violations = { "Speeding", "Red Light", "Stop Sign", "Lane Change" };
            string violation = violations[Random.Range(0, violations.Length)];
            OnViolationOccurred(violation, currentSpeed, "Test Location");
        }
        
        // Simulate driving event (10% chance every frame)
        if (Random.Range(0f, 1f) < 0.01f)
        {
            string[] events = { "Braking", "Acceleration", "Turning", "Lane Change" };
            string eventType = events[Random.Range(0, events.Length)];
            OnDrivingEventOccurred(eventType, Random.Range(0.1f, 1.0f), transform.position);
        }
    }
    
    /// <summary>
    /// Call this when a collision occurs in your game
    /// </summary>
    public void OnCollisionOccurred(string collisionType, string objectHit, float impactForce)
    {
        collisionCount++;
        
        // Send to data managers
        if (dataManager)
        {
            dataManager.RecordCollision(collisionType, objectHit, impactForce);
        }
        
        if (performanceManager)
        {
            performanceManager.RecordCollision();
        }
        
        Debug.Log($"💥 Collision #{collisionCount}: {collisionType} with {objectHit} (Impact: {impactForce:F1})");
    }
    
    /// <summary>
    /// Call this when a violation occurs in your game
    /// </summary>
    public void OnViolationOccurred(string violationType, float speed, string location)
    {
        violationCount++;
        
        // Send to data managers
        if (dataManager)
        {
            dataManager.RecordViolation(violationType, speed, location);
        }
        
        if (performanceManager)
        {
            performanceManager.RecordViolation();
        }
        
        Debug.Log($"🚨 Violation #{violationCount}: {violationType} at {speed:F1} mph in {location}");
    }
    
    /// <summary>
    /// Call this when a driving event occurs in your game
    /// </summary>
    public void OnDrivingEventOccurred(string eventType, float value, Vector3 position)
    {
        // Send to data managers
        if (dataManager)
        {
            dataManager.RecordDrivingEvent(eventType, value, position);
        }
        
        Debug.Log($"🚗 Driving Event: {eventType} = {value:F2} at position {position}");
    }
    
    /// <summary>
    /// Call this when score changes in your game
    /// </summary>
    public void OnScoreChanged(int newScore)
    {
        if (performanceManager)
        {
            performanceManager.UpdateScore(newScore);
        }
        
        Debug.Log($"🏆 Score updated: {newScore}");
    }
    
    /// <summary>
    /// Call this when level changes in your game
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
    /// Call this when distance changes in your game
    /// </summary>
    public void OnDistanceChanged(float distance)
    {
        if (performanceManager)
        {
            performanceManager.UpdateDistance(distance);
        }
        
        Debug.Log($"📏 Distance updated: {distance:F2}");
    }
    
    /// <summary>
    /// Manual test methods (for testing)
    /// </summary>
    [ContextMenu("Test Collision")]
    public void TestCollision()
    {
        OnCollisionOccurred("Vehicle", "Test_Car", 25f);
    }
    
    [ContextMenu("Test Violation")]
    public void TestViolation()
    {
        OnViolationOccurred("Speeding", 75f, "Highway");
    }
    
    [ContextMenu("Test Driving Event")]
    public void TestDrivingEvent()
    {
        OnDrivingEventOccurred("Braking", 0.8f, transform.position);
    }
    
    [ContextMenu("Test Score Change")]
    public void TestScoreChange()
    {
        OnScoreChanged(1000);
    }
}
