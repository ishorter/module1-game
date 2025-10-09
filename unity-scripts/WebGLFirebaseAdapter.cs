using UnityEngine;
using System;

/// <summary>
/// WebGL Firebase Adapter - Converts local file saving to Firebase for WebGL
/// This script modifies the game developer's existing SaveLogToJson method to work with WebGL
/// </summary>
public class WebGLFirebaseAdapter : MonoBehaviour
{
    [System.Serializable]
    public class PerformanceData
    {
        public float maxSpeedMPH;
        public int collisionCount;
        public float sessionDurationSeconds;
        public string sessionDurationFormatted;
        public string timestamp;
        
        // Additional variables for PlayerPrefs
        public int violationCount;
        public float averageSpeed;
        public float totalDistance;
        public int score;
        public string levelName;
    }

    [Header("Tracking Variables")]
    [SerializeField] private float maxSpeed = 0f;
    [SerializeField] private int collisionCount = 0;
    [SerializeField] private int violationCount = 0;
    [SerializeField] private float totalDistance = 0f;
    [SerializeField] private int score = 0;
    [SerializeField] private string levelName = "Module1A";
    
    [Header("Firebase Settings")]
    [SerializeField] private bool enableFirebaseLogging = true;
    [SerializeField] private bool autoSaveEvery30Seconds = true;
    
    private bool isWebGL = false;
    private float sessionStartTime = 0f;
    
    void Start()
    {
        isWebGL = Application.platform == RuntimePlatform.WebGLPlayer;
        sessionStartTime = Time.time;
        
        Debug.Log($"🎮 WebGL Firebase Adapter initialized - WebGL: {isWebGL}");
        
        if (autoSaveEvery30Seconds && isWebGL)
        {
            // Auto-save every 30 seconds for WebGL
            InvokeRepeating(nameof(AutoSaveToFirebase), 30f, 30f);
        }
    }

    /// <summary>
    /// MODIFIED VERSION of the game developer's SaveLogToJson method
    /// This version works for both local development and WebGL deployment
    /// </summary>
    public void SaveLogToJson(float sessionTime)
    {
        // Convert seconds → minutes:seconds format (EXACT SAME CODE)
        int minutes = Mathf.FloorToInt(sessionTime / 60f);
        int seconds = Mathf.FloorToInt(sessionTime % 60f);
        string formattedTime = string.Format("{0:00}:{1:00}", minutes, seconds);

        // Create PerformanceData object (EXACT SAME CODE)
        PerformanceData data = new PerformanceData
        {
            maxSpeedMPH = maxSpeed * 2.237f,
            collisionCount = collisionCount,
            sessionDurationSeconds = sessionTime,
            sessionDurationFormatted = formattedTime,
            timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            
            // Additional data for complete tracking
            violationCount = violationCount,
            averageSpeed = CalculateAverageSpeed(),
            totalDistance = totalDistance,
            score = score,
            levelName = levelName
        };

        // Convert to JSON (EXACT SAME CODE)
        string json = JsonUtility.ToJson(data, true);
        
        Debug.Log("📄 JSON Content:\n" + json);

        if (isWebGL)
        {
            // For WebGL: Send to Firebase instead of saving to file
            SaveToFirebase(json, data);
        }
        else
        {
            // For local development: Save to file (EXACT SAME CODE)
            SaveToLocalFile(json);
        }
        
        // ALWAYS save to PlayerPrefs with variable names (for both WebGL and local)
        SaveToPlayerPrefs(data);
    }

    /// <summary>
    /// Save JSON data to Firebase (WebGL only)
    /// </summary>
    private void SaveToFirebase(string jsonData, PerformanceData data)
    {
        try
        {
            // Send JSON data to Firebase via JavaScript bridge
            Application.ExternalCall("UnityFirebase.savePerformanceData", jsonData);
            Debug.Log("✅ Performance data sent to Firebase (WebGL)");
            Debug.Log($"📊 Data: Max Speed {data.maxSpeedMPH:F1} mph, Collisions: {data.collisionCount}, Violations: {data.violationCount}");
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ Failed to save JSON to Firebase: " + e.Message);
        }
    }

    /// <summary>
    /// Save JSON data to local file (Local development only)
    /// This is the EXACT SAME CODE as the game developer's original method
    /// </summary>
    private void SaveToLocalFile(string jsonData)
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
    /// Save data to PlayerPrefs with proper variable names
    /// This shows how PlayerPrefs variable names work
    /// </summary>
    private void SaveToPlayerPrefs(PerformanceData data)
    {
        try
        {
            // Save individual values with descriptive variable names
            PlayerPrefs.SetFloat("MaxSpeedMPH", data.maxSpeedMPH);
            PlayerPrefs.SetInt("CollisionCount", data.collisionCount);
            PlayerPrefs.SetInt("ViolationCount", data.violationCount);
            PlayerPrefs.SetFloat("SessionDurationSeconds", data.sessionDurationSeconds);
            PlayerPrefs.SetString("SessionDurationFormatted", data.sessionDurationFormatted);
            PlayerPrefs.SetFloat("AverageSpeed", data.averageSpeed);
            PlayerPrefs.SetFloat("TotalDistance", data.totalDistance);
            PlayerPrefs.SetInt("Score", data.score);
            PlayerPrefs.SetString("LevelName", data.levelName);
            PlayerPrefs.SetString("PerformanceTimestamp", data.timestamp);
            
            // Save the complete JSON as well
            string jsonData = JsonUtility.ToJson(data, true);
            PlayerPrefs.SetString("PerformanceDataJSON", jsonData);
            
            PlayerPrefs.Save();
            
            Debug.Log("✅ Performance data saved to PlayerPrefs with variable names");
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
    /// Auto-save to Firebase every 30 seconds
    /// </summary>
    private void AutoSaveToFirebase()
    {
        if (isWebGL)
        {
            float sessionTime = Time.time - sessionStartTime;
            SaveLogToJson(sessionTime);
        }
    }

    /// <summary>
    /// Update tracking variables (call these from your game events)
    /// </summary>
    public void UpdateMaxSpeed(float speed)
    {
        if (speed > maxSpeed)
        {
            maxSpeed = speed;
            Debug.Log($"🏎️ New max speed: {maxSpeed:F1} m/s ({maxSpeed * 2.237f:F1} mph)");
        }
    }

    public void RecordCollision()
    {
        collisionCount++;
        Debug.Log($"💥 Collision recorded. Total: {collisionCount}");
    }

    public void RecordViolation()
    {
        violationCount++;
        Debug.Log($"🚨 Violation recorded. Total: {violationCount}");
    }

    public void UpdateDistance(float distance)
    {
        totalDistance = distance;
    }

    public void UpdateScore(int newScore)
    {
        score = newScore;
    }

    public void SetLevelName(string name)
    {
        levelName = name;
    }

    /// <summary>
    /// Calculate average speed (placeholder - implement based on your game logic)
    /// </summary>
    private float CalculateAverageSpeed()
    {
        // This should be calculated based on your game's speed tracking
        return maxSpeed * 0.7f; // Placeholder: assume average is 70% of max speed
    }

    /// <summary>
    /// Get current session time
    /// </summary>
    public float GetSessionTime()
    {
        return Time.time - sessionStartTime;
    }

    /// <summary>
    /// Manual save method (for testing)
    /// </summary>
    [ContextMenu("Save Performance Data")]
    public void ManualSave()
    {
        float sessionTime = GetSessionTime();
        SaveLogToJson(sessionTime);
    }

    /// <summary>
    /// Test methods for simulating game data
    /// </summary>
    [ContextMenu("Simulate Game Data")]
    public void SimulateGameData()
    {
        UpdateMaxSpeed(Random.Range(60f, 80f));
        RecordCollision();
        RecordViolation();
        UpdateScore(Random.Range(1000, 2000));
        UpdateDistance(Random.Range(1f, 10f));
        
        Debug.Log("🎮 Game data simulated - check Firebase console!");
    }

    /// <summary>
    /// Load data from PlayerPrefs (example of how to read the variable names)
    /// </summary>
    [ContextMenu("Load from PlayerPrefs")]
    public void LoadFromPlayerPrefs()
    {
        try
        {
            float maxSpeedMPH = PlayerPrefs.GetFloat("MaxSpeedMPH", 0f);
            int collisionCount = PlayerPrefs.GetInt("CollisionCount", 0);
            int violationCount = PlayerPrefs.GetInt("ViolationCount", 0);
            float sessionDuration = PlayerPrefs.GetFloat("SessionDurationSeconds", 0f);
            string formattedTime = PlayerPrefs.GetString("SessionDurationFormatted", "00:00");
            int score = PlayerPrefs.GetInt("Score", 0);
            string levelName = PlayerPrefs.GetString("LevelName", "Unknown");
            
            Debug.Log($"📥 Loaded from PlayerPrefs:");
            Debug.Log($"   MaxSpeedMPH: {maxSpeedMPH}");
            Debug.Log($"   CollisionCount: {collisionCount}");
            Debug.Log($"   ViolationCount: {violationCount}");
            Debug.Log($"   SessionDuration: {sessionDuration}s ({formattedTime})");
            Debug.Log($"   Score: {score}");
            Debug.Log($"   LevelName: {levelName}");
            
            // Load complete JSON
            string jsonData = PlayerPrefs.GetString("PerformanceDataJSON", "");
            if (!string.IsNullOrEmpty(jsonData))
            {
                PerformanceData data = JsonUtility.FromJson<PerformanceData>(jsonData);
                Debug.Log($"📄 Complete JSON loaded: {jsonData}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ Failed to load from PlayerPrefs: " + e.Message);
        }
    }
}
