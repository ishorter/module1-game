// Unity-Firebase Bridge for WebGL communication
import { collection, addDoc, doc, setDoc, getDocs, query, where, orderBy, limit, serverTimestamp } from 'https://www.gstatic.com/firebasejs/10.7.1/firebase-firestore.js';

class UnityFirebaseBridge {
  constructor() {
    this.unityInstance = null;
    this.db = null;
    this.userId = this.generateUserId();
    this.isInitialized = false;
    this.sessionData = {
      startTime: Date.now(),
      violations: [],
      collisions: [],
      progress: []
    };
  }

  // Initialize bridge with Unity instance
  async initialize(unityInstance) {
    console.log('🔧 Initializing Unity Firebase Bridge...');
    
    this.unityInstance = unityInstance;
    this.db = window.firebaseDB;
    
    if (!this.db) {
      console.error('❌ Firebase DB not available');
      console.error('🔍 Checking Firebase initialization...');
      
      // Wait for Firebase to initialize
      let attempts = 0;
      while (!this.db && attempts < 10) {
        await new Promise(resolve => setTimeout(resolve, 500));
        this.db = window.firebaseDB;
        attempts++;
        console.log(`🔄 Firebase initialization attempt ${attempts}/10`);
      }
      
      if (!this.db) {
        console.error('❌ Firebase DB still not available after retries');
        return false;
      }
    }

    this.setupUnityCallbacks();
    this.isInitialized = true;
    console.log('✅ Unity Firebase Bridge initialized successfully');
    console.log('🎯 Ready to receive Unity calls');
    
    // Test Firebase connection
    await this.testFirebaseConnection();
    
    return true;
  }


  // Test Firebase connection
  async testFirebaseConnection() {
    try {
      console.log('🧪 Testing Firebase connection...');
      
      // Try to write a test document
      const testDoc = {
        userId: this.userId,
        test: true,
        timestamp: new Date().toISOString(),
        message: 'Firebase connection test'
      };
      
      const docRef = await addDoc(collection(this.db, 'connectionTests'), testDoc);
      console.log('✅ Firebase connection test successful:', docRef.id);
      
      // Notify Unity that Firebase is ready
      if (this.unityInstance) {
        this.unityInstance.SendMessage('SimpleGameDataManager', 'OnFirebaseReady', 'connected');
        this.unityInstance.SendMessage('GameDataManager', 'OnFirebaseReady', 'connected');
        this.unityInstance.SendMessage('PerformanceDataManager', 'OnFirebaseReady', 'connected');
        this.unityInstance.SendMessage('FirebaseDataSender', 'OnFirebaseReady', 'connected');
      }
      
      // Send a test violation to verify the system works
      setTimeout(() => {
        console.log('🎮 Sending test violation to verify system...');
        this.handleViolation('Test Violation|65.5|Test Location|1');
      }, 2000);
      
      return true;
    } catch (error) {
      console.error('❌ Firebase connection test failed:', error);
      
      // Notify Unity that Firebase failed
      if (this.unityInstance) {
        this.unityInstance.SendMessage('SimpleGameDataManager', 'OnFirebaseReady', 'failed');
        this.unityInstance.SendMessage('GameDataManager', 'OnFirebaseReady', 'failed');
        this.unityInstance.SendMessage('PerformanceDataManager', 'OnFirebaseReady', 'failed');
        this.unityInstance.SendMessage('FirebaseDataSender', 'OnFirebaseReady', 'failed');
      }
      
      return false;
    }
  }

  // Test C# ↔ JavaScript communication (DISABLED - Real game data only)
  testCommunication() {
    console.log('🎮 FIREBASE: Real game data tracking enabled - test data disabled');
    console.log('📡 FIREBASE: Waiting for Unity game events...');
    
    if (this.unityInstance) {
      console.log('✅ FIREBASE: Unity instance available for real game data');
      console.log('🎯 FIREBASE: Ready to receive real game events from Unity');
    } else {
      console.warn('⚠️ FIREBASE: Unity instance not available yet');
    }
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

  // Setup Unity C# callbacks - Professional integration
  setupUnityCallbacks() {
    console.log('🔧 Setting up Unity callbacks...');
    
    // Expose methods to Unity C# via SendMessage
    window.UnityFirebase = {
      // Game Progress
      saveProgress: (progressData) => this.handleProgress(progressData),
      loadProgress: () => this.loadProgress(),
      
      // Driving Events
      recordViolation: (violationData) => this.handleViolation(violationData),
      recordCollision: (collisionData) => this.handleCollision(collisionData),
      recordDrivingEvent: (eventData) => this.handleDrivingEvent(eventData),
      
      // Performance Data
      savePerformanceData: (performanceData) => this.handlePerformanceData(performanceData),
      
      // Session Management
      startSession: () => this.startSession(),
      endSession: () => this.endSession(),
      updateSessionStats: (statsData) => this.updateSessionStats(statsData),
      
      // Analytics
      getUserStats: () => this.getUserStats(),
      getSessionData: () => this.getCurrentSessionData()
    };
    
    console.log('✅ Unity callbacks ready:', Object.keys(window.UnityFirebase));
  }

  // Handle progress data from Unity
  async handleProgress(progressData) {
    try {
      console.log('📊 FIREBASE: Received progress data:', progressData);
      
      if (!this.db) {
        throw new Error('Firebase database not available');
      }
      
      let data;
      
      // Check if it's JSON or pipe-separated data
      if (progressData.includes('|')) {
        // Simple pipe-separated format: "level|score|completion|timeSpent"
        const parts = progressData.split('|');
        data = {
          level: parseInt(parts[0]) || 1,
          score: parseInt(parts[1]) || 0,
          completion: parseFloat(parts[2]) || 0,
          timeSpent: parseFloat(parts[3]) || 0
        };
        console.log('📊 FIREBASE: Parsed pipe-separated data:', data);
      } else {
        // JSON format
        try {
          data = typeof progressData === 'string' ? JSON.parse(progressData) : progressData;
          console.log('📊 FIREBASE: Parsed JSON data:', data);
        } catch (jsonError) {
          console.error('❌ FIREBASE: JSON parsing failed:', jsonError);
          throw new Error('Invalid JSON format');
        }
      }
      
      const progress = {
        userId: this.userId,
        gameId: 'DriverEdSimulator_Module1A',
        level: data.level || 1,
        score: data.score || 0,
        completion: data.completion || 0,
        timeSpent: data.timeSpent || 0,
        timestamp: serverTimestamp(),
        websiteUrl: window.location.href,
        sessionId: this.getSessionId()
      };

      console.log('🚀 FIREBASE: Saving progress:', progress);
      const docRef = await addDoc(collection(this.db, 'gameProgress'), progress);
      console.log('✅ FIREBASE: Progress saved with ID:', docRef.id);
      
      // Notify Unity of success
      if (this.unityInstance) {
        this.unityInstance.SendMessage('SimpleGameDataManager', 'OnProgressSaved', 'success');
      }
      
      return { success: true, id: docRef.id };
    } catch (error) {
      console.error('❌ FIREBASE: Error saving progress:', error);
      console.error('❌ FIREBASE: Error details:', {
        name: error.name,
        message: error.message,
        code: error.code,
        stack: error.stack
      });
      
      if (this.unityInstance) {
        this.unityInstance.SendMessage('SimpleGameDataManager', 'OnProgressSaved', 'error');
      }
      return { success: false, error: error.message };
    }
  }

  // Handle violation data from Unity
  async handleViolation(violationData) {
    try {
      let data;
      
      // Check if it's JSON or pipe-separated data
      if (violationData.includes('|')) {
        // Simple pipe-separated format: "type|speed|location|violationNumber"
        const parts = violationData.split('|');
        data = {
          type: parts[0] || 'Unknown',
          speed: parseFloat(parts[1]) || 0,
          location: parts[2] || 'Unknown',
          violationNumber: parseInt(parts[3]) || 1
        };
      } else {
        // JSON format
        data = typeof violationData === 'string' ? JSON.parse(violationData) : violationData;
      }
      
      // Update violation count
      this.sessionData.violationCount = (this.sessionData.violationCount || 0) + 1;
      
      const violation = {
        userId: this.userId,
        gameId: 'DriverEdSimulator_Module1A',
        violationType: data.type || 'Unknown',
        severity: this.calculateSeverity(data.speed, data.type),
        speed: data.speed || 0,
        location: data.location || 'Unknown',
        violationNumber: data.violationNumber || this.sessionData.violationCount,
        timestamp: serverTimestamp(),
        websiteUrl: window.location.href,
        sessionId: this.getSessionId()
      };

      console.log('🚨 FIREBASE: Saving violation:', violation);
      const docRef = await addDoc(collection(this.db, 'violations'), violation);
      console.log('✅ FIREBASE: Violation saved with ID:', docRef.id);
      
      // Track in session
      this.sessionData.violations.push(violation);
      
      // Auto-update session stats
      this.updateSessionStats();
      
      // Notify Unity
      if (this.unityInstance) {
        this.unityInstance.SendMessage('SimpleGameDataManager', 'OnViolationSaved', 'success');
      }
      
      return { success: true, id: docRef.id };
    } catch (error) {
      console.error('❌ FIREBASE: Error saving violation:', error);
      return { success: false, error: error.message };
    }
  }

  // Handle collision data from Unity
  async handleCollision(collisionData) {
    try {
      let data;
      
      // Check if it's JSON or pipe-separated data
      if (collisionData.includes('|')) {
        // Simple pipe-separated format: "type|objectHit|impactForce|collisionNumber"
        const parts = collisionData.split('|');
        data = {
          type: parts[0] || 'Unknown',
          objectHit: parts[1] || 'Unknown',
          impactForce: parseFloat(parts[2]) || 0,
          collisionNumber: parseInt(parts[3]) || 1
        };
      } else {
        // JSON format
        data = typeof collisionData === 'string' ? JSON.parse(collisionData) : collisionData;
      }
      
      // Update collision count
      this.sessionData.collisionCount = (this.sessionData.collisionCount || 0) + 1;
      
      const collision = {
        userId: this.userId,
        gameId: 'DriverEdSimulator_Module1A',
        collisionType: data.type || 'Unknown',
        objectHit: data.objectHit || 'Unknown',
        impactForce: data.impactForce || 0,
        damage: this.calculateDamage(data.impactForce),
        location: data.location || 'Unknown',
        collisionNumber: data.collisionNumber || this.sessionData.collisionCount,
        timestamp: serverTimestamp(),
        websiteUrl: window.location.href,
        sessionId: this.getSessionId()
      };

      console.log('💥 FIREBASE: Saving collision:', collision);
      const docRef = await addDoc(collection(this.db, 'collisions'), collision);
      console.log('✅ FIREBASE: Collision saved with ID:', docRef.id);
      
      // Track in session
      this.sessionData.collisions.push(collision);
      
      // Auto-update session stats
      this.updateSessionStats();
      
      // Notify Unity
      if (this.unityInstance) {
        this.unityInstance.SendMessage('SimpleGameDataManager', 'OnCollisionSaved', 'success');
      }
      
      return { success: true, id: docRef.id };
    } catch (error) {
      console.error('❌ FIREBASE: Error saving collision:', error);
      return { success: false, error: error.message };
    }
  }

  // Handle driving events from Unity
  async handleDrivingEvent(eventData) {
    try {
      const data = typeof eventData === 'string' ? JSON.parse(eventData) : eventData;
      
      const event = {
        userId: this.userId,
        gameId: 'DriverEdSimulator_Module1A',
        eventType: data.type || 'Unknown',
        value: data.value || 0,
        position: data.position || {},
        timestamp: serverTimestamp(),
        websiteUrl: window.location.href,
        sessionId: this.getSessionId()
      };

      console.log('🚗 FIREBASE: Saving driving event:', event);
      const docRef = await addDoc(collection(this.db, 'drivingEvents'), event);
      console.log('✅ FIREBASE: Driving event saved with ID:', docRef.id);
      
      return { success: true, id: docRef.id };
    } catch (error) {
      console.error('❌ FIREBASE: Error saving driving event:', error);
      return { success: false, error: error.message };
    }
  }

  // Handle performance data from Unity
  async handlePerformanceData(performanceData) {
    try {
      console.log('📊 FIREBASE: Received performance data:', performanceData);
      
      if (!this.db) {
        throw new Error('Firebase database not available');
      }
      
      let data;
      
      // Parse JSON data
      try {
        data = typeof performanceData === 'string' ? JSON.parse(performanceData) : performanceData;
        console.log('📊 FIREBASE: Parsed performance data:', data);
      } catch (jsonError) {
        console.error('❌ FIREBASE: JSON parsing failed:', jsonError);
        throw new Error('Invalid JSON format for performance data');
      }
      
      const performance = {
        userId: this.userId,
        gameId: 'DriverEdSimulator_Module1A',
        maxSpeedMPH: data.maxSpeedMPH || 0,
        collisionCount: data.collisionCount || 0,
        violationCount: data.violationCount || 0,
        sessionDurationSeconds: data.sessionDurationSeconds || 0,
        sessionDurationFormatted: data.sessionDurationFormatted || '00:00',
        averageSpeed: data.averageSpeed || 0,
        totalDistance: data.totalDistance || 0,
        score: data.score || 0,
        levelName: data.levelName || 'Unknown',
        timestamp: serverTimestamp(),
        originalTimestamp: data.timestamp || new Date().toISOString(),
        websiteUrl: window.location.href,
        sessionId: this.getSessionId()
      };

      console.log('🚀 FIREBASE: Saving performance data:', performance);
      const docRef = await addDoc(collection(this.db, 'performanceData'), performance);
      console.log('✅ FIREBASE: Performance data saved with ID:', docRef.id);
      
      // Notify Unity of success
      if (this.unityInstance) {
        this.unityInstance.SendMessage('PerformanceDataManager', 'OnPerformanceDataSaved', 'success');
      }
      
      return { success: true, id: docRef.id };
    } catch (error) {
      console.error('❌ FIREBASE: Error saving performance data:', error);
      console.error('❌ FIREBASE: Error details:', {
        name: error.name,
        message: error.message,
        code: error.code,
        stack: error.stack
      });
      
      // Notify Unity of error
      if (this.unityInstance) {
        this.unityInstance.SendMessage('PerformanceDataManager', 'OnPerformanceDataError', error.message);
      }
      
      return { success: false, error: error.message };
    }
  }

  // Start new session
  startSession() {
    this.sessionData = {
      startTime: Date.now(),
      violations: [],
      collisions: [],
      progress: []
    };
    console.log('🎮 FIREBASE: New session started');
    return { success: true, sessionId: this.getSessionId() };
  }

  // End session and save summary
  async endSession() {
    try {
      const sessionSummary = {
        userId: this.userId,
        gameId: 'DriverEdSimulator_Module1A',
        sessionId: this.getSessionId(),
        startTime: this.sessionData.startTime,
        endTime: Date.now(),
        duration: Date.now() - this.sessionData.startTime,
        violationsCount: this.sessionData.violations.length,
        collisionsCount: this.sessionData.collisions.length,
        progressCount: this.sessionData.progress.length,
        timestamp: serverTimestamp(),
        websiteUrl: window.location.href
      };

      console.log('📊 FIREBASE: Saving session summary:', sessionSummary);
      const docRef = await addDoc(collection(this.db, 'sessions'), sessionSummary);
      console.log('✅ FIREBASE: Session saved with ID:', docRef.id);
      
      return { success: true, id: docRef.id, summary: sessionSummary };
    } catch (error) {
      console.error('❌ FIREBASE: Error saving session:', error);
      return { success: false, error: error.message };
    }
  }

  // Update session statistics
  updateSessionStats(statsData = null) {
    try {
      if (statsData) {
        const data = typeof statsData === 'string' ? JSON.parse(statsData) : statsData;
        this.sessionData.stats = {
          ...this.sessionData.stats,
          ...data,
          lastUpdate: Date.now()
        };
      }
      
      // Auto-save session data every few events
      const shouldAutoSave = this.shouldAutoSaveSession();
      if (shouldAutoSave) {
        this.saveSessionData();
      }
      
      console.log('📈 FIREBASE: Session stats updated:', this.sessionData.stats);
      return { success: true };
    } catch (error) {
      console.error('❌ FIREBASE: Error updating session stats:', error);
      return { success: false, error: error.message };
    }
  }

  // Determine if session should be auto-saved
  shouldAutoSaveSession() {
    const violations = this.sessionData.violationCount || 0;
    const collisions = this.sessionData.collisionCount || 0;
    const lastSave = this.sessionData.lastAutoSave || 0;
    const now = Date.now();
    
    // Auto-save every 5 violations, 3 collisions, or every 5 minutes
    return violations > 0 && violations % 5 === 0 ||
           collisions > 0 && collisions % 3 === 0 ||
           now - lastSave > 300000; // 5 minutes
  }

  // Save session data to Firestore
  async saveSessionData() {
    try {
      const sessionSummary = {
        userId: this.userId,
        gameId: 'DriverEdSimulator_Module1A',
        sessionId: this.getSessionId(),
        startTime: this.sessionData.startTime,
        endTime: Date.now(),
        duration: Date.now() - this.sessionData.startTime,
        violationsCount: this.sessionData.violationCount || 0,
        collisionsCount: this.sessionData.collisionCount || 0,
        progressCount: this.sessionData.progress?.length || 0,
        stats: this.sessionData.stats || {},
        timestamp: serverTimestamp(),
        websiteUrl: window.location.href,
        autoSaved: true
      };

      console.log('📊 FIREBASE: Auto-saving session data:', sessionSummary);
      const docRef = await addDoc(collection(this.db, 'sessions'), sessionSummary);
      console.log('✅ FIREBASE: Session auto-saved with ID:', docRef.id);
      
      // Update last save time
      this.sessionData.lastAutoSave = Date.now();
      
      return { success: true, id: docRef.id };
    } catch (error) {
      console.error('❌ FIREBASE: Error auto-saving session:', error);
      return { success: false, error: error.message };
    }
  }

  // Get current session data
  getCurrentSessionData() {
    return {
      sessionId: this.getSessionId(),
      startTime: this.sessionData.startTime,
      duration: Date.now() - this.sessionData.startTime,
      violationsCount: this.sessionData.violations.length,
      collisionsCount: this.sessionData.collisions.length,
      progressCount: this.sessionData.progress.length,
      stats: this.sessionData.stats || {}
    };
  }

  // Load user progress
  async loadProgress() {
    try {
      const q = query(
        collection(this.db, 'gameProgress'),
        where('userId', '==', this.userId),
        orderBy('timestamp', 'desc'),
        limit(1)
      );
      
      const querySnapshot = await getDocs(q);
      if (!querySnapshot.empty) {
        const latestProgress = querySnapshot.docs[0].data();
        console.log('📥 FIREBASE: Latest progress loaded:', latestProgress);
        return { success: true, data: latestProgress };
      } else {
        console.log('📥 FIREBASE: No progress found for user');
        return { success: true, data: null };
      }
    } catch (error) {
      console.error('❌ FIREBASE: Error loading progress:', error);
      return { success: false, error: error.message };
    }
  }

  // Get user statistics
  async getUserStats() {
    try {
      const [progressQuery, violationsQuery, collisionsQuery] = await Promise.all([
        getDocs(query(collection(this.db, 'gameProgress'), where('userId', '==', this.userId))),
        getDocs(query(collection(this.db, 'violations'), where('userId', '==', this.userId))),
        getDocs(query(collection(this.db, 'collisions'), where('userId', '==', this.userId)))
      ]);

      const stats = {
        totalProgress: progressQuery.size,
        totalViolations: violationsQuery.size,
        totalCollisions: collisionsQuery.size,
        userId: this.userId
      };

      console.log('📊 FIREBASE: User stats loaded:', stats);
      return { success: true, data: stats };
    } catch (error) {
      console.error('❌ FIREBASE: Error loading user stats:', error);
      return { success: false, error: error.message };
    }
  }

  // Generate session ID
  getSessionId() {
    let sessionId = sessionStorage.getItem('unity_session_id');
    if (!sessionId) {
      sessionId = 'session_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9);
      sessionStorage.setItem('unity_session_id', sessionId);
    }
    return sessionId;
  }

  // Calculate violation severity based on speed and type
  calculateSeverity(speed, violationType) {
    if (violationType && violationType.toLowerCase().includes('speeding')) {
      if (speed > 80) return 'High';
      if (speed > 65) return 'Medium';
      return 'Low';
    }
    if (violationType && violationType.toLowerCase().includes('red light')) {
      return 'High';
    }
    if (violationType && violationType.toLowerCase().includes('stop sign')) {
      return 'Medium';
    }
    return 'Medium';
  }

  // Calculate damage based on impact force
  calculateDamage(impactForce) {
    if (impactForce > 50) return 100; // Total damage
    if (impactForce > 25) return 75;  // Major damage
    if (impactForce > 10) return 50;  // Moderate damage
    return 25; // Minor damage
  }
}

// Initialize bridge
window.unityFirebaseBridge = new UnityFirebaseBridge();
console.log('Unity Firebase Bridge created - Professional Integration Ready');