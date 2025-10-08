using UnityEngine;
using System;

public class SimpleGameProgress : MonoBehaviour
{
    [System.Serializable]
    public class ProgressData
    {
        public int currentLevel = 1;
        public int completedLessons = 0;
        public float playTime = 0f;
        public int score = 0;
        public string lastCheckpoint = "";
    }

    public static SimpleGameProgress Instance { get; private set; }
    
    [Header("Progress Settings")]
    public float autoSaveInterval = 30f; // Auto-save every 30 seconds
    
    [Header("Current Progress")]
    public ProgressData currentProgress;
    
    private float autoSaveTimer = 0f;
    private bool isWebGL = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeProgress();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        isWebGL = Application.platform == RuntimePlatform.WebGLPlayer;
        
        if (isWebGL)
        {
            // Load progress from cloud
            LoadProgressFromCloud();
        }
    }

    void Update()
    {
        // Update play time
        if (currentProgress != null)
        {
            currentProgress.playTime += Time.deltaTime;
        }

        // Auto-save timer
        if (autoSaveTimer >= autoSaveInterval)
        {
            SaveProgress();
            autoSaveTimer = 0f;
        }
        else
        {
            autoSaveTimer += Time.deltaTime;
        }
    }

    void InitializeProgress()
    {
        currentProgress = new ProgressData();
    }

    // Public methods for game to call
    public void UpdateProgress(int level, int lessons, int score, string checkpoint = "")
    {
        if (currentProgress == null) return;

        currentProgress.currentLevel = level;
        currentProgress.completedLessons = lessons;
        currentProgress.score = score;
        currentProgress.lastCheckpoint = checkpoint;
        
        Debug.Log($"Progress Updated - Level: {level}, Lessons: {lessons}, Score: {score}");
        
        // Auto-save when progress is updated
        SaveProgress();
    }

    public void AddScore(int points)
    {
        if (currentProgress == null) return;
        
        currentProgress.score += points;
        Debug.Log($"Score added: {points}. Total: {currentProgress.score}");
    }

    public void CompleteLesson()
    {
        if (currentProgress == null) return;
        
        currentProgress.completedLessons++;
        Debug.Log($"Lesson completed! Total: {currentProgress.completedLessons}");
        
        // Auto-save when lesson is completed
        SaveProgress();
    }

    // Save progress methods
    public void SaveProgress()
    {
        if (currentProgress == null) return;

        if (isWebGL)
        {
            SaveProgressToCloud();
        }
        else
        {
            SaveProgressToLocal();
        }
    }

    void SaveProgressToLocal()
    {
        string progressJson = JsonUtility.ToJson(currentProgress, true);
        PlayerPrefs.SetString("GameProgress", progressJson);
        PlayerPrefs.Save();
        
        Debug.Log("Progress saved locally");
    }

    void SaveProgressToCloud()
    {
        if (!isWebGL) return;

        string progressJson = JsonUtility.ToJson(currentProgress, true);
        
        // Call JavaScript function to save to Firebase
        Application.ExternalCall("UnityFirebase.saveProgress", progressJson);
        
        Debug.Log("Progress saved to cloud");
    }

    // Load progress methods
    void LoadProgressFromLocal()
    {
        if (PlayerPrefs.HasKey("GameProgress"))
        {
            string progressJson = PlayerPrefs.GetString("GameProgress");
            currentProgress = JsonUtility.FromJson<ProgressData>(progressJson);
            Debug.Log("Progress loaded from local storage");
        }
    }

    void LoadProgressFromCloud()
    {
        if (!isWebGL) return;

        // Call JavaScript function to load from Firebase
        Application.ExternalCall("UnityFirebase.loadProgress");
        Debug.Log("Loading progress from cloud...");
    }

    // Callbacks from JavaScript
    public void OnProgressSaved(string result)
    {
        if (result == "success")
        {
            Debug.Log("Progress successfully saved to cloud");
        }
        else
        {
            Debug.LogError("Failed to save progress to cloud");
            // Fallback to local save
            SaveProgressToLocal();
        }
    }

    public void OnProgressLoaded(string progressJson)
    {
        if (!string.IsNullOrEmpty(progressJson) && progressJson != "{}")
        {
            try
            {
                currentProgress = JsonUtility.FromJson<ProgressData>(progressJson);
                Debug.Log("Progress loaded from cloud successfully");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error parsing cloud progress: {e.Message}");
            }
        }
        else
        {
            Debug.Log("No cloud progress found, using local data");
            LoadProgressFromLocal();
        }
    }

    // Public getters for other scripts
    public ProgressData GetCurrentProgress()
    {
        return currentProgress;
    }

    public int GetCurrentLevel()
    {
        return currentProgress?.currentLevel ?? 1;
    }

    public int GetCompletedLessons()
    {
        return currentProgress?.completedLessons ?? 0;
    }

    public int GetScore()
    {
        return currentProgress?.score ?? 0;
    }

    public float GetPlayTime()
    {
        return currentProgress?.playTime ?? 0f;
    }

    // Manual save trigger (for UI buttons, etc.)
    public void ManualSave()
    {
        SaveProgress();
        autoSaveTimer = 0f; // Reset auto-save timer
    }
}
