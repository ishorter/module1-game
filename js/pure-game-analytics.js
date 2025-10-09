// Pure JavaScript Game Analytics for WebGL
// Tracks game data without Unity scripts - monitors Unity WebGL canvas directly

class PureGameAnalytics {
    constructor() {
        this.isInitialized = false;
        this.gameData = {
            sessionId: this.generateSessionId(),
            startTime: Date.now(),
            events: [],
            performance: {
                frameRate: 0,
                memoryUsage: 0,
                canvasSize: { width: 0, height: 0 }
            },
            userInteractions: {
                clicks: 0,
                keyPresses: 0,
                mouseMovements: 0
            }
        };
        
        this.firebaseReady = false;
        this.eventQueue = [];
        
        this.init();
    }
    
    async init() {
        console.log('🎮 Initializing Pure JavaScript Game Analytics...');
        
        // Wait for Firebase to be ready
        await this.waitForFirebase();
        
        // Initialize tracking
        this.initializeTracking();
        
        this.isInitialized = true;
        console.log('✅ Pure Game Analytics initialized - tracking WITHOUT Unity scripts');
    }
    
    async waitForFirebase() {
        let attempts = 0;
        while (!window.firebaseDB && attempts < 20) {
            await new Promise(resolve => setTimeout(resolve, 500));
            attempts++;
            console.log(`🔄 Waiting for Firebase... attempt ${attempts}/20`);
        }
        
        if (window.firebaseDB) {
            this.firebaseReady = true;
            console.log('✅ Firebase ready for Pure Analytics');
        } else {
            console.warn('⚠️ Firebase not available - analytics will queue data');
        }
    }
    
    initializeTracking() {
        // Track Unity canvas interactions
        this.trackCanvasInteractions();
        
        // Track browser performance
        this.trackPerformance();
        
        // Track user interactions
        this.trackUserInteractions();
        
        // Track game events (when Unity sends them)
        this.trackGameEvents();
        
        // Auto-save data periodically
        this.startAutoSave();
        
        console.log('📊 All tracking systems active - monitoring game in real-time');
    }
    
    trackCanvasInteractions() {
        const canvas = document.getElementById('unity-canvas');
        if (!canvas) {
            console.warn('⚠️ Unity canvas not found');
            return;
        }
        
        console.log('🎯 Tracking Unity canvas interactions...');
        
        // Track canvas clicks
        canvas.addEventListener('click', (event) => {
            this.recordEvent('canvas_click', {
                x: event.clientX,
                y: event.clientY,
                timestamp: Date.now()
            });
        });
        
        // Track canvas mouse movements (throttled)
        let mouseMoveTimer = null;
        canvas.addEventListener('mousemove', (event) => {
            if (mouseMoveTimer) return;
            
            mouseMoveTimer = setTimeout(() => {
                this.recordEvent('canvas_mouse_move', {
                    x: event.clientX,
                    y: event.clientY,
                    timestamp: Date.now()
                });
                mouseMoveTimer = null;
            }, 100); // Throttle to 10 times per second
        });
        
        // Track canvas focus/blur
        canvas.addEventListener('focus', () => {
            this.recordEvent('canvas_focus', { timestamp: Date.now() });
        });
        
        canvas.addEventListener('blur', () => {
            this.recordEvent('canvas_blur', { timestamp: Date.now() });
        });
    }
    
    trackPerformance() {
        console.log('⚡ Tracking browser performance...');
        
        setInterval(() => {
            // Track frame rate (approximate)
            const now = performance.now();
            if (this.lastFrameTime) {
                const frameRate = 1000 / (now - this.lastFrameTime);
                this.gameData.performance.frameRate = Math.round(frameRate);
            }
            this.lastFrameTime = now;
            
            // Track memory usage (if available)
            if (performance.memory) {
                this.gameData.performance.memoryUsage = Math.round(performance.memory.usedJSHeapSize / 1024 / 1024);
            }
            
            // Track canvas size
            const canvas = document.getElementById('unity-canvas');
            if (canvas) {
                this.gameData.performance.canvasSize = {
                    width: canvas.width,
                    height: canvas.height
                };
            }
            
        }, 1000); // Update every second
    }
    
    trackUserInteractions() {
        console.log('👆 Tracking user interactions...');
        
        // Track keyboard events
        document.addEventListener('keydown', (event) => {
            this.recordEvent('key_press', {
                key: event.key,
                code: event.code,
                timestamp: Date.now()
            });
            this.gameData.userInteractions.keyPresses++;
        });
        
        // Track mouse clicks outside canvas
        document.addEventListener('click', (event) => {
            if (event.target.id !== 'unity-canvas') {
                this.gameData.userInteractions.clicks++;
            }
        });
    }
    
    trackGameEvents() {
        console.log('🎮 Setting up Unity game event tracking...');
        
        // Listen for Unity messages (when Unity sends data)
        window.addEventListener('message', (event) => {
            if (event.data && typeof event.data === 'object') {
                this.handleUnityMessage(event.data);
            }
        });
        
        // Monitor Unity console logs (if accessible)
        this.monitorUnityConsole();
    }
    
    handleUnityMessage(data) {
        console.log('📨 Received Unity message:', data);
        
        if (data.type === 'game_event') {
            this.recordEvent(data.eventType, data.data);
        } else if (data.type === 'performance') {
            this.updatePerformanceData(data.data);
        } else if (data.type === 'violation') {
            this.recordViolation(data.data);
        } else if (data.type === 'collision') {
            this.recordCollision(data.data);
        }
    }
    
    monitorUnityConsole() {
        // Try to capture Unity console logs
        const originalConsoleLog = console.log;
        console.log = (...args) => {
            originalConsoleLog.apply(console, args);
            
            // Check if this looks like Unity game data
            const message = args.join(' ');
            if (message.includes('violation') || message.includes('collision') || message.includes('progress')) {
                this.recordEvent('unity_console_event', {
                    message: message,
                    timestamp: Date.now()
                });
            }
        };
    }
    
    recordEvent(eventType, data) {
        const event = {
            type: eventType,
            data: data,
            timestamp: Date.now(),
            sessionId: this.gameData.sessionId
        };
        
        this.gameData.events.push(event);
        
        // Keep only last 100 events to prevent memory issues
        if (this.gameData.events.length > 100) {
            this.gameData.events = this.gameData.events.slice(-100);
        }
        
        console.log(`📊 Event recorded: ${eventType}`, data);
    }
    
    recordViolation(violationData) {
        this.recordEvent('violation', {
            ...violationData,
            severity: this.calculateSeverity(violationData.speed, violationData.type),
            timestamp: Date.now()
        });
    }
    
    recordCollision(collisionData) {
        this.recordEvent('collision', {
            ...collisionData,
            damage: this.calculateDamage(collisionData.impactForce),
            timestamp: Date.now()
        });
    }
    
    updatePerformanceData(performanceData) {
        this.gameData.performance = {
            ...this.gameData.performance,
            ...performanceData
        };
    }
    
    startAutoSave() {
        console.log('💾 Starting auto-save every 30 seconds...');
        
        setInterval(() => {
            this.saveToFirebase();
        }, 30000); // Save every 30 seconds
    }
    
    async saveToFirebase() {
        if (!this.firebaseReady) {
            console.log('⏳ Firebase not ready - queuing data...');
            this.eventQueue.push({
                type: 'auto_save',
                data: this.getSessionSummary(),
                timestamp: Date.now()
            });
            return;
        }
        
        try {
            const { collection, addDoc, serverTimestamp } = await import('https://www.gstatic.com/firebasejs/10.7.1/firebase-firestore.js');
            
            const sessionData = {
                sessionId: this.gameData.sessionId,
                startTime: this.gameData.startTime,
                endTime: Date.now(),
                duration: Date.now() - this.gameData.startTime,
                eventsCount: this.gameData.events.length,
                performance: this.gameData.performance,
                userInteractions: this.gameData.userInteractions,
                recentEvents: this.gameData.events.slice(-10), // Last 10 events
                timestamp: serverTimestamp(),
                websiteUrl: window.location.href,
                trackingMethod: 'Pure JavaScript Analytics'
            };
            
            console.log('💾 Saving session data to Firebase...', sessionData);
            const docRef = await addDoc(collection(window.firebaseDB, 'pureAnalytics'), sessionData);
            console.log('✅ Session data saved with ID:', docRef.id);
            
            // Process queued events
            await this.processQueuedEvents();
            
        } catch (error) {
            console.error('❌ Error saving to Firebase:', error);
            this.eventQueue.push({
                type: 'save_error',
                data: { error: error.message },
                timestamp: Date.now()
            });
        }
    }
    
    async processQueuedEvents() {
        if (this.eventQueue.length === 0) return;
        
        console.log(`🔄 Processing ${this.eventQueue.length} queued events...`);
        
        try {
            const { collection, addDoc } = await import('https://www.gstatic.com/firebasejs/10.7.1/firebase-firestore.js');
            
            for (const event of this.eventQueue) {
                await addDoc(collection(window.firebaseDB, 'queuedEvents'), {
                    ...event,
                    sessionId: this.gameData.sessionId,
                    processedAt: Date.now()
                });
            }
            
            this.eventQueue = [];
            console.log('✅ All queued events processed');
            
        } catch (error) {
            console.error('❌ Error processing queued events:', error);
        }
    }
    
    getSessionSummary() {
        return {
            sessionId: this.gameData.sessionId,
            duration: Date.now() - this.gameData.startTime,
            eventsCount: this.gameData.events.length,
            performance: this.gameData.performance,
            userInteractions: this.gameData.userInteractions
        };
    }
    
    calculateSeverity(speed, violationType) {
        if (speed > 80) return 'High';
        if (speed > 65) return 'Medium';
        return 'Low';
    }
    
    calculateDamage(impactForce) {
        if (impactForce > 50) return 100;
        if (impactForce > 25) return 75;
        if (impactForce > 10) return 50;
        return 25;
    }
    
    generateSessionId() {
        return 'pure_analytics_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9);
    }
    
    // Public methods for manual tracking
    trackCustomEvent(eventType, data) {
        this.recordEvent(eventType, { ...data, timestamp: Date.now() });
    }
    
    getAnalyticsData() {
        return this.getSessionSummary();
    }
    
    forceSave() {
        this.saveToFirebase();
    }
}

// Initialize Pure Game Analytics
window.pureGameAnalytics = new PureGameAnalytics();

console.log('🎮 Pure JavaScript Game Analytics loaded - NO Unity scripts required!');
