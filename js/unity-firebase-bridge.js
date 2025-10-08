// Unity-Firebase Bridge for WebGL communication
import { collection, addDoc, doc, setDoc, getDocs, query, where, orderBy, limit, serverTimestamp } from 'https://www.gstatic.com/firebasejs/10.7.1/firebase-firestore.js';

class UnityFirebaseBridge {
  constructor() {
    this.unityInstance = null;
    this.db = null;
    this.userId = this.generateUserId();
    this.isInitialized = false;
  }

  // Initialize bridge with Unity instance
  async initialize(unityInstance) {
    this.unityInstance = unityInstance;
    this.db = window.firebaseDB;
    
    if (!this.db) {
      console.error('Firebase DB not available');
      return;
    }

    this.setupUnityCallbacks();
    this.isInitialized = true;
    console.log('Unity Firebase Bridge initialized');
  }

  // Generate unique user ID for anonymous tracking
  generateUserId() {
    let userId = localStorage.getItem('unity_user_id');
    if (!userId) {
      userId = 'user_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9);
      localStorage.setItem('unity_user_id', userId);
    }
    return userId;
  }

    // Setup Unity C# callbacks
    setupUnityCallbacks() {
      console.log('🔧 FIREBASE: Setting up Unity callbacks...');
      
      // Expose methods to Unity C# via SendMessage
      window.UnityFirebase = {
        saveProgress: (progressData) => {
          console.log('🎯 FIREBASE: saveProgress called from Unity:', progressData);
          this.saveProgress(progressData);
        },
        loadProgress: () => {
          console.log('🎯 FIREBASE: loadProgress called from Unity');
          this.loadProgress();
        },
        saveScore: (scoreData) => {
          console.log('🎯 FIREBASE: saveScore called from Unity:', scoreData);
          this.saveScore(scoreData);
        },
        saveAchievement: (achievementData) => {
          console.log('🎯 FIREBASE: saveAchievement called from Unity:', achievementData);
          this.saveAchievement(achievementData);
        },
        saveViolation: (violationData) => {
          console.log('🎯 FIREBASE: saveViolation called from Unity:', violationData);
          this.saveViolation(violationData);
        },
        saveCollision: (collisionData) => {
          console.log('🎯 FIREBASE: saveCollision called from Unity:', collisionData);
          this.saveCollision(collisionData);
        },
        saveDrivingEvent: (eventData) => {
          console.log('🎯 FIREBASE: saveDrivingEvent called from Unity:', eventData);
          this.saveDrivingEvent(eventData);
        },
        saveSessionData: (sessionData) => {
          console.log('🎯 FIREBASE: saveSessionData called from Unity:', sessionData);
          this.saveSessionData(sessionData);
        },
        getUserStats: () => {
          console.log('🎯 FIREBASE: getUserStats called from Unity');
          this.getUserStats();
        }
      };
      
      console.log('✅ FIREBASE: Unity callbacks set up successfully');
      console.log('🔍 FIREBASE: Available methods:', Object.keys(window.UnityFirebase));
    }

  // Save game progress to Firestore
  async saveProgress(progressData) {
    if (!this.isInitialized || !this.db) {
      console.error('Bridge not initialized');
      return;
    }

    try {
      const progress = {
        userId: this.userId,
        gameId: 'DriverEdSimulator_Module1A',
        progressData: JSON.parse(progressData),
        timestamp: serverTimestamp(),
        websiteUrl: window.location.href,
        userAgent: navigator.userAgent,
        sessionId: this.getSessionId()
      };

      await addDoc(collection(this.db, 'gameProgress'), progress);
      
      // Notify Unity of success
      if (this.unityInstance) {
        this.unityInstance.SendMessage('GameManager', 'OnProgressSaved', 'success');
      }
      
      console.log('Progress saved successfully');
    } catch (error) {
      console.error('Error saving progress:', error);
      if (this.unityInstance) {
        this.unityInstance.SendMessage('GameManager', 'OnProgressSaved', 'error');
      }
    }
  }

  // Load user progress from Firestore
  async loadProgress() {
    if (!this.isInitialized || !this.db) {
      console.error('Bridge not initialized');
      return;
    }

    try {
      const q = query(
        collection(this.db, 'gameProgress'),
        where('userId', '==', this.userId),
        where('gameId', '==', 'DriverEdSimulator_Module1A'),
        orderBy('timestamp', 'desc'),
        limit(1)
      );

      const querySnapshot = await getDocs(q);
      let latestProgress = null;

      querySnapshot.forEach((doc) => {
        latestProgress = doc.data();
      });

      // Send progress data back to Unity
      const progressJson = latestProgress ? JSON.stringify(latestProgress.progressData) : '{}';
      
      if (this.unityInstance) {
        this.unityInstance.SendMessage('GameManager', 'OnProgressLoaded', progressJson);
      }
      
      console.log('Progress loaded successfully');
    } catch (error) {
      console.error('Error loading progress:', error);
      if (this.unityInstance) {
        this.unityInstance.SendMessage('GameManager', 'OnProgressLoaded', '{}');
      }
    }
  }

  // Save high scores
  async saveScore(scoreData) {
    if (!this.isInitialized || !this.db) return;

    try {
      const score = {
        userId: this.userId,
        gameId: 'DriverEdSimulator_Module1A',
        scoreData: JSON.parse(scoreData),
        timestamp: serverTimestamp(),
        websiteUrl: window.location.href,
        sessionId: this.getSessionId()
      };

      await addDoc(collection(this.db, 'gameScores'), score);
      
      if (this.unityInstance) {
        this.unityInstance.SendMessage('GameManager', 'OnScoreSaved', 'success');
      }
      
      console.log('Score saved successfully');
    } catch (error) {
      console.error('Error saving score:', error);
    }
  }

  // Save achievements
  async saveAchievement(achievementData) {
    if (!this.isInitialized || !this.db) return;

    try {
      const achievement = {
        userId: this.userId,
        gameId: 'DriverEdSimulator_Module1A',
        achievementData: JSON.parse(achievementData),
        timestamp: serverTimestamp(),
        websiteUrl: window.location.href,
        sessionId: this.getSessionId()
      };

      await addDoc(collection(this.db, 'achievements'), achievement);
      
      if (this.unityInstance) {
        this.unityInstance.SendMessage('GameManager', 'OnAchievementSaved', 'success');
      }
      
      console.log('Achievement saved successfully');
    } catch (error) {
      console.error('Error saving achievement:', error);
    }
  }

  // Save traffic violations
  async saveViolation(violationData) {
    if (!this.isInitialized || !this.db) return;

    try {
      const violation = {
        userId: this.userId,
        gameId: 'DriverEdSimulator_Module1A',
        violationData: JSON.parse(violationData),
        timestamp: serverTimestamp(),
        websiteUrl: window.location.href,
        sessionId: this.getSessionId()
      };

      await addDoc(collection(this.db, 'violations'), violation);
      
      if (this.unityInstance) {
        this.unityInstance.SendMessage('GameManager', 'OnViolationSaved', 'success');
      }
      
      console.log('Violation saved successfully');
    } catch (error) {
      console.error('Error saving violation:', error);
    }
  }

  // Save collision events
  async saveCollision(collisionData) {
    if (!this.isInitialized || !this.db) return;

    try {
      const collision = {
        userId: this.userId,
        gameId: 'DriverEdSimulator_Module1A',
        collisionData: JSON.parse(collisionData),
        timestamp: serverTimestamp(),
        websiteUrl: window.location.href,
        sessionId: this.getSessionId()
      };

      await addDoc(collection(this.db, 'collisions'), collision);
      
      if (this.unityInstance) {
        this.unityInstance.SendMessage('GameManager', 'OnCollisionSaved', 'success');
      }
      
      console.log('Collision saved successfully');
    } catch (error) {
      console.error('Error saving collision:', error);
    }
  }

  // Save general driving events
  async saveDrivingEvent(eventData) {
    if (!this.isInitialized || !this.db) return;

    try {
      const event = {
        userId: this.userId,
        gameId: 'DriverEdSimulator_Module1A',
        eventData: JSON.parse(eventData),
        timestamp: serverTimestamp(),
        websiteUrl: window.location.href,
        sessionId: this.getSessionId()
      };

      await addDoc(collection(this.db, 'drivingEvents'), event);
      
      if (this.unityInstance) {
        this.unityInstance.SendMessage('GameManager', 'OnDrivingEventSaved', 'success');
      }
      
      console.log('Driving event saved successfully');
    } catch (error) {
      console.error('Error saving driving event:', error);
    }
  }

  // Save complete session data
  async saveSessionData(sessionData) {
    if (!this.isInitialized || !this.db) return;

    try {
      const session = {
        userId: this.userId,
        gameId: 'DriverEdSimulator_Module1A',
        sessionData: JSON.parse(sessionData),
        timestamp: serverTimestamp(),
        websiteUrl: window.location.href,
        sessionId: this.getSessionId()
      };

      await addDoc(collection(this.db, 'sessions'), session);
      
      if (this.unityInstance) {
        this.unityInstance.SendMessage('GameManager', 'OnSessionDataSaved', 'success');
      }
      
      console.log('Session data saved successfully');
    } catch (error) {
      console.error('Error saving session data:', error);
    }
  }

  // Get user statistics for external websites
  async getUserStats() {
    if (!this.isInitialized || !this.db) return null;

    try {
      const progressQuery = query(
        collection(this.db, 'gameProgress'),
        where('userId', '==', this.userId),
        where('gameId', '==', 'DriverEdSimulator_Module1A')
      );

      const scoreQuery = query(
        collection(this.db, 'gameScores'),
        where('userId', '==', this.userId),
        where('gameId', '==', 'DriverEdSimulator_Module1A')
      );

      const [progressSnapshot, scoreSnapshot] = await Promise.all([
        getDocs(progressQuery),
        getDocs(scoreQuery)
      ]);

      return {
        totalProgressSaves: progressSnapshot.size,
        totalScores: scoreSnapshot.size,
        lastPlayed: this.getLastPlayedDate(progressSnapshot),
        highScore: this.getHighScore(scoreSnapshot),
        userId: this.userId
      };
    } catch (error) {
      console.error('Error getting user stats:', error);
      return null;
    }
  }

  getLastPlayedDate(snapshot) {
    let latestDate = null;
    snapshot.forEach(doc => {
      const data = doc.data();
      if (!latestDate || data.timestamp > latestDate) {
        latestDate = data.timestamp;
      }
    });
    return latestDate;
  }

  getHighScore(snapshot) {
    let highScore = 0;
    snapshot.forEach(doc => {
      const data = doc.data();
      const score = data.scoreData?.score || 0;
      if (score > highScore) {
        highScore = score;
      }
    });
    return highScore;
  }

  getSessionId() {
    let sessionId = sessionStorage.getItem('unity_session_id');
    if (!sessionId) {
      sessionId = 'session_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9);
      sessionStorage.setItem('unity_session_id', sessionId);
    }
    return sessionId;
  }

  // Test Firebase connection and data storage
  async testFirebaseConnection() {
    console.log('🧪 FIREBASE TEST: Starting Firebase connection test...');
    
    if (!this.isInitialized) {
      console.error('❌ FIREBASE TEST: Bridge not initialized');
      return;
    }

    try {
      // Test 1: Save test progress data
      console.log('🧪 FIREBASE TEST: Testing progress save...');
      const testProgressData = JSON.stringify({
        level: 1,
        score: 1000,
        completion: 50,
        timeSpent: 120
      });
      await this.saveProgress(testProgressData);

      // Test 2: Save test violation data
      console.log('🧪 FIREBASE TEST: Testing violation save...');
      const testViolationData = JSON.stringify({
        type: 'Speeding',
        severity: 'Medium',
        location: 'Highway',
        speed: 75
      });
      await this.saveViolation(testViolationData);

      // Test 3: Save test collision data
      console.log('🧪 FIREBASE TEST: Testing collision save...');
      const testCollisionData = JSON.stringify({
        type: 'Rear End',
        damage: 'Minor',
        location: 'Intersection'
      });
      await this.saveCollision(testCollisionData);

      // Test 4: Save test session data
      console.log('🧪 FIREBASE TEST: Testing session data save...');
      const testSessionData = JSON.stringify({
        playTime: 300,
        distanceDriven: 5.2,
        maxSpeed: 65,
        violationsCount: 2,
        collisionsCount: 1
      });
      await this.saveSessionData(testSessionData);

      console.log('✅ FIREBASE TEST: All tests completed successfully!');
      console.log('📊 FIREBASE TEST: Check Firebase Console to see the test data');
      console.log('🔗 FIREBASE TEST: Visit: https://console.firebase.google.com/');
      
    } catch (error) {
      console.error('❌ FIREBASE TEST: Test failed:', error);
    }
  }
}

// Initialize bridge
window.unityFirebaseBridge = new UnityFirebaseBridge();
console.log('Unity Firebase Bridge created');
