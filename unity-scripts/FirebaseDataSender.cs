using UnityEngine;

/// <summary>
/// Firebase Data Sender - Simple script to test and send game data to Firebase
/// Add this to any GameObject in your scene to start sending test data
/// </summary>
public class FirebaseDataSender : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private bool autoSendTestData = true;
    [SerializeField] private float testInterval = 10f;
    [SerializeField] private bool enableDebugLogs = true;
    
    private float testTimer = 0f;
    private int testCounter = 0;
    
    void Start()
    {
        Debug.Log("🎮 Firebase Data Sender started");
        
        if (autoSendTestData)
        {
            // Wait 5 seconds for Firebase to initialize, then start sending test data
            Invoke(nameof(SendTestData), 5f);
        }
    }
    
    void Update()
    {
        if (autoSendTestData)
        {
            testTimer += Time.deltaTime;
            if (testTimer >= testInterval)
            {
                SendTestData();
                testTimer = 0f;
            }
        }
    }
    
    /// <summary>
    /// Send test data to Firebase
    /// </summary>
    public void SendTestData()
    {
        testCounter++;
        Debug.Log($"🧪 Sending Firebase Test Data #{testCounter}");
        
        // Test 1: Send performance data
        SendPerformanceData();
        
        // Test 2: Send violation data
        SendViolationData();
        
        // Test 3: Send collision data
        SendCollisionData();
        
        // Test 4: Send progress data
        SendProgressData();
        
        // Test 5: Send driving event
        SendDrivingEventData();
    }
    
    /// <summary>
    /// Send performance data to Firebase
    /// </summary>
    public void SendPerformanceData()
    {
        try
        {
            // Create performance data JSON
            string performanceJson = @"{
                ""maxSpeedMPH"": 75.5,
                ""collisionCount"": 2,
                ""violationCount"": 3,
                ""sessionDurationSeconds"": 180,
                ""sessionDurationFormatted"": ""03:00"",
                ""averageSpeed"": 45.2,
                ""totalDistance"": 5.5,
                ""score"": 1250,
                ""levelName"": ""Module1A"",
                ""timestamp"": """ + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + @"""
            }";
            
            // Send to Firebase
            Application.ExternalCall("UnityFirebase.savePerformanceData", performanceJson);
            LogMessage("✅ Performance data sent to Firebase");
        }
        catch (System.Exception e)
        {
            LogMessage($"❌ Failed to send performance data: {e.Message}");
        }
    }
    
    /// <summary>
    /// Send violation data to Firebase
    /// </summary>
    public void SendViolationData()
    {
        try
        {
            string violationType = "Speeding";
            float speed = Random.Range(60f, 80f);
            string location = "Test Highway";
            
            // Send using SimpleGameDataManager format (pipe-separated)
            string violationData = $"{violationType}|{speed:F1}|{location}|{testCounter}";
            
            Application.ExternalCall("UnityFirebase.recordViolation", violationData);
            LogMessage($"✅ Violation data sent: {violationData}");
        }
        catch (System.Exception e)
        {
            LogMessage($"❌ Failed to send violation data: {e.Message}");
        }
    }
    
    /// <summary>
    /// Send collision data to Firebase
    /// </summary>
    public void SendCollisionData()
    {
        try
        {
            string collisionType = "Vehicle";
            string objectHit = "Test_Car";
            float impactForce = Random.Range(15f, 35f);
            
            // Send using SimpleGameDataManager format (pipe-separated)
            string collisionData = $"{collisionType}|{objectHit}|{impactForce:F1}|{testCounter}";
            
            Application.ExternalCall("UnityFirebase.recordCollision", collisionData);
            LogMessage($"✅ Collision data sent: {collisionData}");
        }
        catch (System.Exception e)
        {
            LogMessage($"❌ Failed to send collision data: {e.Message}");
        }
    }
    
    /// <summary>
    /// Send progress data to Firebase
    /// </summary>
    public void SendProgressData()
    {
        try
        {
            int level = 1;
            int score = Random.Range(1000, 2000);
            float completion = Random.Range(50f, 100f);
            float timeSpent = Random.Range(60f, 300f);
            
            // Send using SimpleGameDataManager format (pipe-separated)
            string progressData = $"{level}|{score}|{completion:F1}|{timeSpent:F1}";
            
            Application.ExternalCall("UnityFirebase.saveProgress", progressData);
            LogMessage($"✅ Progress data sent: {progressData}");
        }
        catch (System.Exception e)
        {
            LogMessage($"❌ Failed to send progress data: {e.Message}");
        }
    }
    
    /// <summary>
    /// Send driving event data to Firebase
    /// </summary>
    public void SendDrivingEventData()
    {
        try
        {
            string eventType = "Braking";
            float value = Random.Range(0.5f, 1.0f);
            Vector3 position = new Vector3(Random.Range(-10f, 10f), 0, Random.Range(-10f, 10f));
            
            // Send using SimpleGameDataManager format (pipe-separated)
            string eventData = $"{eventType}|{value:F2}|{position.x:F1}|{position.y:F1}|{position.z:F1}";
            
            Application.ExternalCall("UnityFirebase.recordDrivingEvent", eventData);
            LogMessage($"✅ Driving event data sent: {eventData}");
        }
        catch (System.Exception e)
        {
            LogMessage($"❌ Failed to send driving event data: {e.Message}");
        }
    }
    
    /// <summary>
    /// Start a new session
    /// </summary>
    public void StartSession()
    {
        try
        {
            Application.ExternalCall("UnityFirebase.startSession", "");
            LogMessage("✅ Session started");
        }
        catch (System.Exception e)
        {
            LogMessage($"❌ Failed to start session: {e.Message}");
        }
    }
    
    /// <summary>
    /// End current session
    /// </summary>
    public void EndSession()
    {
        try
        {
            Application.ExternalCall("UnityFirebase.endSession", "");
            LogMessage("✅ Session ended");
        }
        catch (System.Exception e)
        {
            LogMessage($"❌ Failed to end session: {e.Message}");
        }
    }
    
    private void LogMessage(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[FirebaseDataSender] {message}");
        }
    }
    
    // Manual test methods (call from Inspector)
    [ContextMenu("Send Test Performance Data")]
    public void TestPerformance() => SendPerformanceData();
    
    [ContextMenu("Send Test Violation Data")]
    public void TestViolation() => SendViolationData();
    
    [ContextMenu("Send Test Collision Data")]
    public void TestCollision() => SendCollisionData();
    
    [ContextMenu("Send Test Progress Data")]
    public void TestProgress() => SendProgressData();
    
    [ContextMenu("Send Test Driving Event")]
    public void TestDrivingEvent() => SendDrivingEventData();
    
    [ContextMenu("Start Session")]
    public void TestStartSession() => StartSession();
    
    [ContextMenu("End Session")]
    public void TestEndSession() => EndSession();
    
    [ContextMenu("Send All Test Data")]
    public void TestAllData() => SendTestData();
}
