using UnityEngine;
using System;
using System.Collections.Generic;

public class DrivingDataManager : MonoBehaviour
{
    [System.Serializable]
    public class ViolationData
    {
        public string violationType; // "Speeding", "RedLight", "StopSign", "LaneChange", "TurnSignal"
        public float speed;
        public Vector3 position;
        public string roadName;
        public float severity; // 1-10 scale
        public string description;
        public DateTime timestamp;
    }

    [System.Serializable]
    public class CollisionData
    {
        public string collisionType; // "Vehicle", "Pedestrian", "Object", "Barrier"
        public float impactForce;
        public Vector3 collisionPoint;
        public Vector3 impactDirection;
        public string objectHit;
        public float damage;
        public string roadName;
        public DateTime timestamp;
    }

    [System.Serializable]
    public class DrivingEventData
    {
        public string eventType; // "Acceleration", "Braking", "Turning", "LaneChange", "Parking"
        public float value; // Speed, angle, distance, etc.
        public Vector3 position;
        public string roadName;
        public DateTime timestamp;
    }

    [System.Serializable]
    public class SessionData
    {
        public float totalDistance;
        public float totalTime;
        public int violationsCount;
        public int collisionsCount;
        public float averageSpeed;
        public float maxSpeed;
        public int laneChanges;
        public int turns;
        public int stops;
        public float fuelEfficiency;
        public DateTime sessionStart;
        public DateTime sessionEnd;
    }

    public static DrivingDataManager Instance { get; private set; }
    
    [Header("Data Collection Settings")]
    public bool enableDataCollection = true;
    public float eventSaveInterval = 5f; // Save events every 5 seconds
    
    [Header("Current Session Data")]
    public SessionData currentSession;
    
    private List<ViolationData> violations = new List<ViolationData>();
    private List<CollisionData> collisions = new List<CollisionData>();
    private List<DrivingEventData> drivingEvents = new List<DrivingEventData>();
    
    private float eventSaveTimer = 0f;
    private bool isWebGL = false;
    private DateTime sessionStartTime;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSession();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        isWebGL = Application.platform == RuntimePlatform.WebGLPlayer;
        sessionStartTime = DateTime.Now;
    }

    void Update()
    {
        if (!enableDataCollection) return;

        // Update session data
        if (currentSession != null)
        {
            currentSession.totalTime = (float)(DateTime.Now - sessionStartTime).TotalSeconds;
        }

        // Auto-save events
        if (eventSaveTimer >= eventSaveInterval)
        {
            SaveAllData();
            eventSaveTimer = 0f;
        }
        else
        {
            eventSaveTimer += Time.deltaTime;
        }
    }

    void InitializeSession()
    {
        currentSession = new SessionData();
        currentSession.sessionStart = DateTime.Now;
    }

    // Public methods for game to call
    public void RecordViolation(string violationType, float speed, Vector3 position, string roadName, float severity = 5f)
    {
        if (!enableDataCollection) return;

        var violation = new ViolationData
        {
            violationType = violationType,
            speed = speed,
            position = position,
            roadName = roadName,
            severity = severity,
            description = $"{violationType} at {speed:F1} km/h on {roadName}",
            timestamp = DateTime.Now
        };

        violations.Add(violation);
        currentSession.violationsCount++;

        Debug.Log($"Violation Recorded: {violation.description}");

        // Save immediately for critical violations
        if (severity >= 8f)
        {
            SaveViolation(violation);
        }
    }

    public void RecordCollision(string collisionType, float impactForce, Vector3 collisionPoint, Vector3 impactDirection, string objectHit, float damage = 0f)
    {
        if (!enableDataCollection) return;

        var collision = new CollisionData
        {
            collisionType = collisionType,
            impactForce = impactForce,
            collisionPoint = collisionPoint,
            impactDirection = impactDirection,
            objectHit = objectHit,
            damage = damage,
            roadName = GetCurrentRoadName(),
            timestamp = DateTime.Now
        };

        collisions.Add(collision);
        currentSession.collisionsCount++;

        Debug.Log($"Collision Recorded: {collisionType} with {objectHit} - Impact: {impactForce:F1}");

        // Save collision immediately
        SaveCollision(collision);
    }

    public void RecordDrivingEvent(string eventType, float value, Vector3 position, string roadName = "")
    {
        if (!enableDataCollection) return;

        var drivingEvent = new DrivingEventData
        {
            eventType = eventType,
            value = value,
            position = position,
            roadName = roadName != "" ? roadName : GetCurrentRoadName(),
            timestamp = DateTime.Now
        };

        drivingEvents.Add(drivingEvent);

        // Update session statistics
        UpdateSessionStats(eventType, value);

        Debug.Log($"Driving Event: {eventType} - Value: {value:F1}");
    }

    void UpdateSessionStats(string eventType, float value)
    {
        switch (eventType)
        {
            case "LaneChange":
                currentSession.laneChanges++;
                break;
            case "Turn":
                currentSession.turns++;
                break;
            case "Stop":
                currentSession.stops++;
                break;
            case "Speed":
                if (value > currentSession.maxSpeed)
                    currentSession.maxSpeed = value;
                currentSession.averageSpeed = (currentSession.averageSpeed + value) / 2f;
                break;
        }
    }

    // Save methods
    void SaveViolation(ViolationData violation)
    {
        if (!isWebGL) return;

        string violationJson = JsonUtility.ToJson(violation, true);
        Application.ExternalCall("UnityFirebase.saveViolation", violationJson);
    }

    void SaveCollision(CollisionData collision)
    {
        if (!isWebGL) return;

        string collisionJson = JsonUtility.ToJson(collision, true);
        Application.ExternalCall("UnityFirebase.saveCollision", collisionJson);
    }

    void SaveDrivingEvent(DrivingEventData drivingEvent)
    {
        if (!isWebGL) return;

        string eventJson = JsonUtility.ToJson(drivingEvent, true);
        Application.ExternalCall("UnityFirebase.saveDrivingEvent", eventJson);
    }

    void SaveSessionData()
    {
        if (!isWebGL) return;

        currentSession.sessionEnd = DateTime.Now;
        string sessionJson = JsonUtility.ToJson(currentSession, true);
        Application.ExternalCall("UnityFirebase.saveSessionData", sessionJson);
    }

    public void SaveAllData()
    {
        if (!enableDataCollection) return;

        // Save all violations
        foreach (var violation in violations)
        {
            SaveViolation(violation);
        }

        // Save all collisions
        foreach (var collision in collisions)
        {
            SaveCollision(collision);
        }

        // Save recent driving events (last 10)
        var recentEvents = drivingEvents.GetRange(Math.Max(0, drivingEvents.Count - 10), Math.Min(10, drivingEvents.Count));
        foreach (var drivingEvent in recentEvents)
        {
            SaveDrivingEvent(drivingEvent);
        }

        // Save session data
        SaveSessionData();

        Debug.Log("All driving data saved to Firestore");
    }

    // Utility methods
    string GetCurrentRoadName()
    {
        // This should be implemented based on your game's road system
        // For now, return a placeholder
        return "Main Street";
    }

    // Callbacks from JavaScript
    public void OnViolationSaved(string result)
    {
        Debug.Log($"Violation save result: {result}");
    }

    public void OnCollisionSaved(string result)
    {
        Debug.Log($"Collision save result: {result}");
    }

    public void OnDrivingEventSaved(string result)
    {
        Debug.Log($"Driving event save result: {result}");
    }

    public void OnSessionDataSaved(string result)
    {
        Debug.Log($"Session data save result: {result}");
    }

    // Public getters
    public int GetViolationsCount()
    {
        return violations.Count;
    }

    public int GetCollisionsCount()
    {
        return collisions.Count;
    }

    public float GetSessionTime()
    {
        return currentSession?.totalTime ?? 0f;
    }

    public float GetAverageSpeed()
    {
        return currentSession?.averageSpeed ?? 0f;
    }

    public float GetMaxSpeed()
    {
        return currentSession?.maxSpeed ?? 0f;
    }

    // End session
    public void EndSession()
    {
        SaveAllData();
        SaveSessionData();
        Debug.Log("Driving session ended and saved");
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveAllData();
        }
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            SaveAllData();
        }
    }
}
