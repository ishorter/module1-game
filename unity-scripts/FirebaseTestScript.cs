using UnityEngine;

/// <summary>
/// Firebase Test Script - Use this to test Firebase integration
/// Add this to any GameObject in your scene to test Firebase connection
/// </summary>
public class FirebaseTestScript : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private bool autoTestOnStart = true;
    [SerializeField] private float testInterval = 10f;
    
    private GameDataIntegration dataIntegration;
    private float testTimer = 0f;
    private int testCounter = 0;
    
    void Start()
    {
        dataIntegration = FindObjectOfType<GameDataIntegration>();
        
        if (!dataIntegration)
        {
            // Create GameDataIntegration if it doesn't exist
            GameObject integrationObj = new GameObject("GameDataIntegration");
            dataIntegration = integrationObj.AddComponent<GameDataIntegration>();
            DontDestroyOnLoad(integrationObj);
        }
        
        if (autoTestOnStart)
        {
            Invoke(nameof(RunFirebaseTest), 5f); // Wait 5 seconds for Firebase to initialize
        }
    }
    
    void Update()
    {
        if (autoTestOnStart)
        {
            testTimer += Time.deltaTime;
            if (testTimer >= testInterval)
            {
                RunFirebaseTest();
                testTimer = 0f;
            }
        }
    }
    
    /// <summary>
    /// Run a comprehensive Firebase test
    /// </summary>
    public void RunFirebaseTest()
    {
        testCounter++;
        Debug.Log($"🧪 Running Firebase Test #{testCounter}");
        
        // Test 1: Simulate max speed
        float testSpeed = Random.Range(60f, 90f);
        dataIntegration.OnMaxSpeedAchieved(testSpeed);
        
        // Test 2: Simulate collision (30% chance)
        if (Random.Range(0f, 1f) < 0.3f)
        {
            dataIntegration.OnCollisionOccurred();
        }
        
        // Test 3: Simulate violation (40% chance)
        if (Random.Range(0f, 1f) < 0.4f)
        {
            string[] violations = { "Speeding", "Red Light", "Stop Sign", "Lane Change" };
            string violation = violations[Random.Range(0, violations.Length)];
            float speed = Random.Range(45f, 75f);
            string location = "Test Location " + Random.Range(1, 10);
            
            dataIntegration.OnViolationOccurred(violation, speed, location);
        }
        
        // Test 4: Update score
        int scoreIncrease = Random.Range(10, 100);
        dataIntegration.OnScoreUpdated(scoreIncrease);
        
        // Test 5: Update distance
        float distance = Random.Range(0.1f, 2.0f);
        dataIntegration.OnDistanceUpdated(distance);
        
        // Test 6: Save all data
        dataIntegration.SaveAllData();
        
        Debug.Log($"✅ Firebase Test #{testCounter} completed");
        Debug.Log($"📊 Performance Summary: {dataIntegration.GetPerformanceSummary()}");
    }
    
    /// <summary>
    /// Manual test methods (call from Inspector or other scripts)
    /// </summary>
    [ContextMenu("Test Max Speed")]
    public void TestMaxSpeed()
    {
        dataIntegration.OnMaxSpeedAchieved(75f);
    }
    
    [ContextMenu("Test Collision")]
    public void TestCollision()
    {
        dataIntegration.OnCollisionOccurred();
    }
    
    [ContextMenu("Test Violation")]
    public void TestViolation()
    {
        dataIntegration.OnViolationOccurred("Speeding", 65f, "Highway Test");
    }
    
    [ContextMenu("Test Score")]
    public void TestScore()
    {
        dataIntegration.OnScoreUpdated(500);
    }
    
    [ContextMenu("Save All Data")]
    public void TestSaveAll()
    {
        dataIntegration.SaveAllData();
    }
    
    /// <summary>
    /// Check Firebase connection status
    /// </summary>
    [ContextMenu("Check Firebase Status")]
    public void CheckFirebaseStatus()
    {
        Debug.Log("🔍 Checking Firebase Status...");
        Debug.Log($"📊 Data Integration: {(dataIntegration ? "✅ Connected" : "❌ Not Found")}");
        
        if (dataIntegration)
        {
            Debug.Log($"📈 Performance Summary: {dataIntegration.GetPerformanceSummary()}");
        }
        
        // Check if we're in WebGL
        bool isWebGL = Application.platform == RuntimePlatform.WebGLPlayer;
        Debug.Log($"🌐 Platform: {(isWebGL ? "WebGL" : "Local Development")}");
        
        if (isWebGL)
        {
            Debug.Log("✅ WebGL detected - data will be sent to Firebase");
        }
        else
        {
            Debug.Log("ℹ️ Local development - data will be saved locally");
        }
    }
}
