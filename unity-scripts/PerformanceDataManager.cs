using UnityEngine;
using System;

/// <summary>
/// Performance Data Manager for Unity WebGL
/// Handles both JSON storage and PlayerPrefs with proper variable naming
/// </summary>
public class PerformanceDataManager : MonoBehaviour
{
    [System.Serializable]
    public class PerformanceData
    {
        public float maxSpeedMPH;
        public int collisionCount;
        public float sessionDurationSeconds;
        public string sessionDurationFormatted;
        public string timestamp;
        
        // Additional useful data
        public int violationCount;
        public float averageSpeed;
        public float totalDistance;
        public int score;
        public string levelName;
    }

    [Header("Performance Tracking")]
    [SerializeField] private bool enablePerformanceLogging = true;
    
    private PerformanceData currentPerformance;
    private bool isWebGL = false;
    
    // Performance tracking variables (these are your PlayerPrefs variable names)
    private float maxSpeed = 0f;
    private int collisionCount = 0;
    private int violationCount = 0;
    private float sessionStartTime = 0f;
    private float totalDistance = 0f;
    private int score = 0;
    private string levelName = "Module1A";

    void Start()
    {
        isWebGL = Application.platform == RuntimePlatform.WebGLPlayer;
        sessionStartTime = Time.time;
        
        // Initialize performance data
        currentPerformance = new PerformanceData();
        
        Debug.Log($"Performance Data Manager initialized - WebGL: {isWebGL}");
        
        // Auto-save performance data every 30 seconds
        InvokeRepeating(nameof(AutoSavePerformance), 30f, 30f);
    }
    
    /// <summary>
    /// Auto-save performance data periodically
    /// </summary>
    private void AutoSavePerformance()
    {
        if (isWebGL && enablePerformanceLogging)
        {
            float sessionTime = GetSessionTime();
            SaveLogToJson(sessionTime);
        }
    }

    /// <summary>
    /// Update maximum speed tracking
    /// </summary>
    public void UpdateMaxSpeed(float currentSpeed)
    {
        if (currentSpeed > maxSpeed)
        {
            maxSpeed = currentSpeed;
            Debug.Log($"New max speed: {maxSpeed:F1} m/s ({maxSpeed * 2.237f:F1} mph)");
        }
    }

    /// <summary>
    /// Record a collision
    /// </summary>
    public void RecordCollision()
    {
        collisionCount++;
        Debug.Log($"Collision recorded. Total: {collisionCount}");
    }

    /// <summary>
    /// Record a violation
    /// </summary>
    public void RecordViolation()
    {
        violationCount++;
        Debug.Log($"Violation recorded. Total: {violationCount}");
    }

    /// <summary>
    /// Update total distance
    /// </summary>
    public void UpdateDistance(float distance)
    {
        totalDistance = distance;
    }

    /// <summary>
    /// Update score
    /// </summary>
    public void UpdateScore(int newScore)
    {
        score = newScore;
    }

    /// <summary>
    /// Set level name
    /// </summary>
    public void SetLevelName(string name)
    {
        levelName = name;
    }

    /// <summary>
    /// Save performance data as JSON (your original method adapted for WebGL)
    /// </summary>
    public void SaveLogToJson(float sessionTime)
    {
        if (!enablePerformanceLogging) return;

        // Convert seconds → minutes:seconds format
        int minutes = Mathf.FloorToInt(sessionTime / 60f);
        int seconds = Mathf.FloorToInt(sessionTime % 60f);
        string formattedTime = string.Format("{0:00}:{1:00}", minutes, seconds);

        // Create performance data object
        currentPerformance = new PerformanceData
        {
            maxSpeedMPH = maxSpeed * 2.237f, // Convert m/s to mph
            collisionCount = collisionCount,
            sessionDurationSeconds = sessionTime,
            sessionDurationFormatted = formattedTime,
            timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            violationCount = violationCount,
            averageSpeed = CalculateAverageSpeed(),
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
            // For WebGL: Save to Firebase instead of local file
            SavePerformanceToFirebase(json);
        }
        else
        {
            // For local development: Save to file
            SavePerformanceToFile(json);
        }
        
        // Also save to PlayerPrefs with proper variable names
        SaveToPlayerPrefs();
    }

    /// <summary>
    /// Save performance data to Firebase (WebGL)
    /// </summary>
    private void SavePerformanceToFirebase(string jsonData)
    {
        try
        {
            // Send JSON data to Firebase via JavaScript bridge
            Application.ExternalCall("UnityFirebase.savePerformanceData", jsonData);
            Debug.Log("✅ Performance data sent to Firebase");
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ Failed to save performance data to Firebase: " + e.Message);
        }
    }

    /// <summary>
    /// Save performance data to local file (Development)
    /// </summary>
    private void SavePerformanceToFile(string jsonData)
    {
        try
        {
            string fileName = $"performance_log_{System.DateTime.Now:yyyyMMdd_HHmmss}.json";
            string path = System.IO.Path.Combine(Application.persistentDataPath, fileName);
            
            System.IO.File.WriteAllText(path, jsonData);
            Debug.Log("✅ Performance log saved as JSON at: " + path);
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ Failed to save JSON log: " + e.Message);
        }
    }

    /// <summary>
    /// Save performance data to PlayerPrefs with proper variable names
    /// </summary>
    private void SaveToPlayerPrefs()
    {
        try
        {
            // Save individual values with descriptive variable names
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
            
            // Save the complete JSON as well
            PlayerPrefs.SetString("PerformanceDataJSON", JsonUtility.ToJson(currentPerformance, true));
            
            PlayerPrefs.Save();
            
            Debug.Log("✅ Performance data saved to PlayerPrefs");
            Debug.Log($"📊 PlayerPrefs Variables:");
            Debug.Log($"   MaxSpeedMPH: {PlayerPrefs.GetFloat("MaxSpeedMPH", 0f)}");
            Debug.Log($"   CollisionCount: {PlayerPrefs.GetInt("CollisionCount", 0)}");
            Debug.Log($"   ViolationCount: {PlayerPrefs.GetInt("ViolationCount", 0)}");
            Debug.Log($"   SessionDurationSeconds: {PlayerPrefs.GetFloat("SessionDurationSeconds", 0f)}");
            Debug.Log($"   Score: {PlayerPrefs.GetInt("Score", 0)}");
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ Failed to save to PlayerPrefs: " + e.Message);
        }
    }

    /// <summary>
    /// Load performance data from PlayerPrefs
    /// </summary>
    public void LoadFromPlayerPrefs()
    {
        try
        {
            maxSpeed = PlayerPrefs.GetFloat("MaxSpeedMPH", 0f) / 2.237f; // Convert mph to m/s
            collisionCount = PlayerPrefs.GetInt("CollisionCount", 0);
            violationCount = PlayerPrefs.GetInt("ViolationCount", 0);
            totalDistance = PlayerPrefs.GetFloat("TotalDistance", 0f);
            score = PlayerPrefs.GetInt("Score", 0);
            levelName = PlayerPrefs.GetString("LevelName", "Module1A");
            
            Debug.Log("✅ Performance data loaded from PlayerPrefs");
            Debug.Log($"📊 Loaded Data:");
            Debug.Log($"   MaxSpeedMPH: {PlayerPrefs.GetFloat("MaxSpeedMPH", 0f)}");
            Debug.Log($"   CollisionCount: {PlayerPrefs.GetInt("CollisionCount", 0)}");
            Debug.Log($"   ViolationCount: {PlayerPrefs.GetInt("ViolationCount", 0)}");
            Debug.Log($"   Score: {PlayerPrefs.GetInt("Score", 0)}");
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ Failed to load from PlayerPrefs: " + e.Message);
        }
    }

    /// <summary>
    /// Calculate average speed (placeholder - implement based on your game logic)
    /// </summary>
    private float CalculateAverageSpeed()
    {
        // This should be calculated based on your game's speed tracking
        // For now, return a placeholder
        return maxSpeed * 0.7f; // Assume average is 70% of max speed
    }

    /// <summary>
    /// Get current session time
    /// </summary>
    public float GetSessionTime()
    {
        return Time.time - sessionStartTime;
    }

    /// <summary>
    /// Reset performance tracking
    /// </summary>
    public void ResetPerformanceData()
    {
        maxSpeed = 0f;
        collisionCount = 0;
        violationCount = 0;
        totalDistance = 0f;
        score = 0;
        sessionStartTime = Time.time;
        
        Debug.Log("🔄 Performance data reset");
    }

    /// <summary>
    /// Callback from JavaScript when performance data is saved to Firebase
    /// </summary>
    public void OnPerformanceDataSaved(string result)
    {
        Debug.Log($"✅ Performance data saved to Firebase: {result}");
    }

    /// <summary>
    /// Callback from JavaScript when there's an error saving performance data
    /// </summary>
    public void OnPerformanceDataError(string error)
    {
        Debug.LogError($"❌ Firebase performance data save error: {error}");
    }

    // Public getters for external access
    public float GetMaxSpeedMPH() => maxSpeed * 2.237f;
    public int GetCollisionCount() => collisionCount;
    public int GetViolationCount() => violationCount;
    public float GetTotalDistance() => totalDistance;
    public int GetScore() => score;
    public string GetLevelName() => levelName;
}
