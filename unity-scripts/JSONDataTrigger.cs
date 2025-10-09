using UnityEngine;

/// <summary>
/// JSON Data Trigger - Calls the existing JSON data tracking methods
/// This script will trigger the game developer's existing JSON data methods
/// </summary>
public class JSONDataTrigger : MonoBehaviour
{
    [Header("Auto Testing")]
    [SerializeField] private bool enableAutoTesting = true;
    [SerializeField] private float testInterval = 15f;
    
    [Header("Data Managers")]
    [SerializeField] private SimpleGameDataManager simpleManager;
    [SerializeField] private GameDataManager gameManager;
    [SerializeField] private DrivingDataManager drivingManager;
    
    private float testTimer = 0f;
    private int testCounter = 0;
    
    void Start()
    {
        // Find existing data managers
        if (!simpleManager) simpleManager = FindObjectOfType<SimpleGameDataManager>();
        if (!gameManager) gameManager = FindObjectOfType<GameDataManager>();
        if (!drivingManager) drivingManager = DrivingDataManager.Instance;
        
        Debug.Log("🎮 JSON Data Trigger initialized");
        Debug.Log($"📊 Simple Manager: {(simpleManager ? "✅" : "❌")}");
        Debug.Log($"📊 Game Manager: {(gameManager ? "✅" : "❌")}");
        Debug.Log($"📊 Driving Manager: {(drivingManager ? "✅" : "❌")}");
        
        if (enableAutoTesting)
        {
            // Start testing after 8 seconds (wait for Firebase to be ready)
            Invoke(nameof(TriggerJSONData), 8f);
        }
    }
    
    void Update()
    {
        if (enableAutoTesting)
        {
            testTimer += Time.deltaTime;
            if (testTimer >= testInterval)
            {
                TriggerJSONData();
                testTimer = 0f;
            }
        }
    }
    
    /// <summary>
    /// Trigger the existing JSON data tracking methods
    /// </summary>
    public void TriggerJSONData()
    {
        testCounter++;
        Debug.Log($"🚀 Triggering JSON Data Tracking #{testCounter}");
        
        // Trigger SimpleGameDataManager methods
        if (simpleManager)
        {
            Debug.Log("📊 Triggering SimpleGameDataManager methods...");
            
            // Test violation tracking
            string violationType = "Speeding";
            float speed = Random.Range(60f, 85f);
            string location = "Highway Test";
            simpleManager.RecordViolation(violationType, speed, location);
            
            // Test collision tracking
            string collisionType = "Vehicle";
            string objectHit = "Test_Car";
            float impactForce = Random.Range(15f, 35f);
            simpleManager.RecordCollision(collisionType, objectHit, impactForce);
            
            // Test progress tracking
            int level = 1;
            int score = Random.Range(1000, 2500);
            float completion = Random.Range(60f, 100f);
            float timeSpent = Random.Range(120f, 300f);
            simpleManager.SaveProgress(level, score, completion, timeSpent);
            
            // Test driving event tracking
            string eventType = "Braking";
            float value = Random.Range(0.5f, 1.0f);
            Vector3 position = new Vector3(Random.Range(-10f, 10f), 0, Random.Range(-10f, 10f));
            simpleManager.RecordDrivingEvent(eventType, value, position);
            
            // Test session stats
            simpleManager.UpdateSessionStats();
            
            Debug.Log("✅ SimpleGameDataManager methods triggered");
        }
        
        // Trigger GameDataManager methods (if exists)
        if (gameManager)
        {
            Debug.Log("📊 Triggering GameDataManager methods...");
            
            // Test JSON violation tracking
            gameManager.RecordViolation("Red Light", 45f, "Intersection");
            
            // Test JSON collision tracking
            gameManager.RecordCollision("Pedestrian", "Person", 20f);
            
            // Test JSON driving event tracking
            gameManager.RecordDrivingEvent("Acceleration", 0.8f, Vector3.zero);
            
            Debug.Log("✅ GameDataManager methods triggered");
        }
        
        // Trigger DrivingDataManager methods (if exists)
        if (drivingManager)
        {
            Debug.Log("📊 Triggering DrivingDataManager methods...");
            
            // Test driving data tracking
            drivingManager.RecordViolation("Stop Sign", 30f, Vector3.zero, "Main Street", 7f);
            drivingManager.RecordCollision("Object", 25f, Vector3.zero, Vector3.forward, "Barrier", 50f);
            drivingManager.RecordDrivingEvent("LaneChange", 0.6f, Vector3.zero, "Highway");
            
            Debug.Log("✅ DrivingDataManager methods triggered");
        }
        
        Debug.Log($"✅ JSON Data Tracking #{testCounter} completed - Check Firebase console!");
    }
    
    /// <summary>
    /// Manual trigger methods for testing
    /// </summary>
    [ContextMenu("Trigger All JSON Data")]
    public void ManualTrigger() => TriggerJSONData();
    
    [ContextMenu("Test Simple Manager Only")]
    public void TestSimpleManager()
    {
        if (simpleManager)
        {
            simpleManager.RecordViolation("Manual Test", 70f, "Manual Location");
            simpleManager.RecordCollision("Manual Test", "Manual Object", 30f);
            simpleManager.SaveProgress(2, 1500, 80f, 200f);
            Debug.Log("✅ SimpleManager manual test completed");
        }
        else
        {
            Debug.Log("❌ SimpleManager not found");
        }
    }
    
    [ContextMenu("Test Game Manager Only")]
    public void TestGameManager()
    {
        if (gameManager)
        {
            gameManager.RecordViolation("Manual Test", 65f, "Manual Location");
            gameManager.RecordCollision("Manual Test", "Manual Object", 25f);
            Debug.Log("✅ GameManager manual test completed");
        }
        else
        {
            Debug.Log("❌ GameManager not found");
        }
    }
    
    [ContextMenu("Check Firebase Status")]
    public void CheckFirebaseStatus()
    {
        Debug.Log("🔍 Checking Firebase Status...");
        Debug.Log($"📊 SimpleManager Ready: {(simpleManager ? "✅" : "❌")}");
        Debug.Log($"📊 GameManager Ready: {(gameManager ? "✅" : "❌")}");
        Debug.Log($"📊 DrivingManager Ready: {(drivingManager ? "✅" : "❌")}");
        Debug.Log($"🌐 Platform: {(Application.platform == RuntimePlatform.WebGLPlayer ? "WebGL" : "Local")}");
        
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            Debug.Log("✅ WebGL detected - data should be sent to Firebase");
        }
        else
        {
            Debug.Log("ℹ️ Local development - data will be saved locally");
        }
    }
}
