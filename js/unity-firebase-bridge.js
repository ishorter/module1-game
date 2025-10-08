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
    this.unityInstance = unityInstance;
    this.db = window.firebaseDB;
    
    if (!this.db) {
      console.error('❌ Firebase DB not available');
      return false;
    }

    this.setupUnityCallbacks();
    this.isInitialized = true;
    console.log('✅ Unity Firebase Bridge initialized successfully');
    console.log('🎯 Ready to receive Unity calls');
    
    // Generate test data to verify Firestore connection works
    setTimeout(() => this.generateTestData(), 3000);
    
    return true;
  }

  // Generate test data to verify Firestore connection works
  async generateTestData() {
    if (!this.isInitialized || !this.db) return;
    
    console.log('🧪 FIREBASE: Generating test data to verify connection...');
    
    try {
      // Test violation data
      const testViolation = {
        userId: this.userId,
        sessionId: this.getSessionId(),
        gameId: 'DriverEdSimulator_Module1A',
        violationType: 'Test Violation',
        speed: 65,
        location: 'Test Location',
        severity: 'Low',
        timestamp: serverTimestamp(),
        websiteUrl: window.location.href
      };
      
      // Test progress data
      const testProgress = {
        userId: this.userId,
        sessionId: this.getSessionId(),
        gameId: 'DriverEdSimulator_Module1A',
        level: 1,
        score: 1000,
        completion: 25,
        timeSpent: 60,
        timestamp: serverTimestamp(),
        websiteUrl: window.location.href
      };
      
      // Save test data
      const violationRef = await addDoc(collection(this.db, 'violations'), testViolation);
      const progressRef = await addDoc(collection(this.db, 'gameProgress'), testProgress);
      
      console.log('✅ FIREBASE: Test data generated and saved successfully!');
      console.log('📊 FIREBASE: Violation ID:', violationRef.id);
      console.log('📊 FIREBASE: Progress ID:', progressRef.id);
      console.log('🎯 FIREBASE: Check your Firestore console to see the data!');
      
    } catch (error) {
      console.error('❌ FIREBASE: Error generating test data:', error);
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
      const data = typeof progressData === 'string' ? JSON.parse(progressData) : progressData;
      
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
        this.unityInstance.SendMessage('GameManager', 'OnProgressSaved', 'success');
      }
      
      return { success: true, id: docRef.id };
    } catch (error) {
      console.error('❌ FIREBASE: Error saving progress:', error);
      if (this.unityInstance) {
        this.unityInstance.SendMessage('GameManager', 'OnProgressSaved', 'error');
      }
      return { success: false, error: error.message };
    }
  }

  // Handle violation data from Unity
  async handleViolation(violationData) {
    try {
      const data = typeof violationData === 'string' ? JSON.parse(violationData) : violationData;
      
      // Update violation count
      this.sessionData.violationCount = (this.sessionData.violationCount || 0) + 1;
      
      const violation = {
        userId: this.userId,
        gameId: 'DriverEdSimulator_Module1A',
        violationType: data.type || 'Unknown',
        severity: data.severity || 'Medium',
        speed: data.speed || 0,
        location: data.location || 'Unknown',
        violationNumber: this.sessionData.violationCount,
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
        this.unityInstance.SendMessage('GameManager', 'OnViolationSaved', 'success');
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
      const data = typeof collisionData === 'string' ? JSON.parse(collisionData) : collisionData;
      
      // Update collision count
      this.sessionData.collisionCount = (this.sessionData.collisionCount || 0) + 1;
      
      const collision = {
        userId: this.userId,
        gameId: 'DriverEdSimulator_Module1A',
        collisionType: data.type || 'Unknown',
        objectHit: data.objectHit || 'Unknown',
        impactForce: data.impactForce || 0,
        damage: data.damage || 0,
        location: data.location || 'Unknown',
        collisionNumber: this.sessionData.collisionCount,
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
        this.unityInstance.SendMessage('GameManager', 'OnCollisionSaved', 'success');
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
}

// Initialize bridge
window.unityFirebaseBridge = new UnityFirebaseBridge();
console.log('Unity Firebase Bridge created - Professional Integration Ready');