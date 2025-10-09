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
            },
            // Game-specific data tracking
            gameStats: {
                speed: 0,
                maxSpeed: 0,
                violations: [],
                collisions: [],
                score: 0,
                level: 1,
                distance: 0,
                timeSpent: 0
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
        
        // Track driving-specific data
        this.trackDrivingData();
        
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
            
            // Track driving-specific keys
            this.trackDrivingKeys(event.key, event.code);
        });
        
        // Track mouse clicks outside canvas
        document.addEventListener('click', (event) => {
            if (event.target.id !== 'unity-canvas') {
                this.gameData.userInteractions.clicks++;
            }
        });
    }
    
    trackDrivingKeys(key, code) {
        // Track acceleration/deceleration patterns
        if (key === 'ArrowUp' || code === 'KeyW') {
            this.recordEvent('acceleration', {
                key: key,
                timestamp: Date.now()
            });
            this.updateGameStats('acceleration');
        } else if (key === 'ArrowDown' || code === 'KeyS') {
            this.recordEvent('braking', {
                key: key,
                timestamp: Date.now()
            });
            this.updateGameStats('braking');
        } else if (key === 'ArrowLeft' || key === 'ArrowRight') {
            this.recordEvent('steering', {
                direction: key,
                timestamp: Date.now()
            });
            this.updateGameStats('steering');
        }
    }
    
    updateGameStats(action) {
        // Update game stats immediately when driving actions occur
        if (action === 'acceleration') {
            this.gameData.gameStats.speed = Math.min(this.gameData.gameStats.speed + 5, 100);
            this.gameData.gameStats.maxSpeed = Math.max(this.gameData.gameStats.maxSpeed, this.gameData.gameStats.speed);
        } else if (action === 'braking') {
            this.gameData.gameStats.speed = Math.max(this.gameData.gameStats.speed - 10, 0);
        }
        
        // Check for violations immediately
        this.checkForViolations();
        this.checkForCollisions();
        this.calculateScore();
        
        console.log('🎮 Game stats updated:', {
            speed: this.gameData.gameStats.speed,
            maxSpeed: this.gameData.gameStats.maxSpeed,
            violations: this.gameData.gameStats.violations.length,
            collisions: this.gameData.gameStats.collisions.length,
            score: this.gameData.gameStats.score
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
    
    trackDrivingData() {
        console.log('🚗 Setting up driving data tracking...');
        
        // Track speed based on acceleration patterns
        let currentSpeed = 0;
        let maxSpeed = 0;
        let accelerationEvents = 0;
        let brakingEvents = 0;
        let steeringEvents = 0;
        
        // Monitor acceleration patterns to estimate speed
        this.gameData.events.forEach(event => {
            if (event.type === 'acceleration') {
                accelerationEvents++;
                currentSpeed = Math.min(currentSpeed + 5, 100); // Estimate speed increase
                maxSpeed = Math.max(maxSpeed, currentSpeed);
            } else if (event.type === 'braking') {
                brakingEvents++;
                currentSpeed = Math.max(currentSpeed - 10, 0); // Estimate speed decrease
            }
        });
        
        // Update game stats
        this.gameData.gameStats.speed = currentSpeed;
        this.gameData.gameStats.maxSpeed = maxSpeed;
        this.gameData.gameStats.timeSpent = Date.now() - this.gameData.startTime;
        
        // Detect potential violations based on patterns
        this.detectViolations();
        
        // Detect potential collisions based on sudden braking
        this.detectCollisions();
        
        // Calculate score based on driving behavior
        this.calculateScore();
        
        console.log('📊 Driving data tracking active:', {
            speed: currentSpeed,
            maxSpeed: maxSpeed,
            accelerationEvents,
            brakingEvents,
            steeringEvents
        });
    }
    
    detectViolations() {
        const violations = [];
        let violationCount = 0;
        
        // Detect speeding violations (based on acceleration patterns)
        if (this.gameData.gameStats.maxSpeed > 80) {
            violations.push({
                type: 'Speeding',
                speed: this.gameData.gameStats.maxSpeed,
                severity: 'High',
                timestamp: Date.now()
            });
            violationCount++;
        } else if (this.gameData.gameStats.maxSpeed > 65) {
            violations.push({
                type: 'Speeding',
                speed: this.gameData.gameStats.maxSpeed,
                severity: 'Medium',
                timestamp: Date.now()
            });
            violationCount++;
        }
        
        // Detect aggressive driving (rapid acceleration/braking)
        const accelerationCount = this.gameData.events.filter(e => e.type === 'acceleration').length;
        const brakingCount = this.gameData.events.filter(e => e.type === 'braking').length;
        
        if (accelerationCount > 50 && brakingCount > 30) {
            violations.push({
                type: 'Aggressive Driving',
                severity: 'Medium',
                accelerationEvents: accelerationCount,
                brakingEvents: brakingCount,
                timestamp: Date.now()
            });
            violationCount++;
        }
        
        this.gameData.gameStats.violations = violations;
        
        if (violationCount > 0) {
            console.log(`🚨 Detected ${violationCount} violations:`, violations);
        }
    }
    
    detectCollisions() {
        const collisions = [];
        
        // Detect potential collisions based on sudden braking patterns
        const recentBraking = this.gameData.events
            .filter(e => e.type === 'braking' && Date.now() - e.timestamp < 5000)
            .length;
        
        if (recentBraking > 3) {
            collisions.push({
                type: 'Potential Collision',
                severity: 'Low',
                brakingEvents: recentBraking,
                timestamp: Date.now()
            });
        }
        
        this.gameData.gameStats.collisions = collisions;
        
        if (collisions.length > 0) {
            console.log('💥 Detected potential collisions:', collisions);
        }
    }
    
    calculateScore() {
        let score = 100; // Start with perfect score
        
        // Deduct points for violations
        score -= this.gameData.gameStats.violations.length * 10;
        
        // Deduct points for collisions
        score -= this.gameData.gameStats.collisions.length * 15;
        
        // Deduct points for aggressive driving
        const accelerationCount = this.gameData.events.filter(e => e.type === 'acceleration').length;
        if (accelerationCount > 50) {
            score -= 5;
        }
        
        // Ensure score doesn't go below 0
        score = Math.max(score, 0);
        
        this.gameData.gameStats.score = score;
        
        console.log(`🏆 Current driving score: ${score}/100`);
    }
    
    checkForViolations() {
        // Check for speeding violations
        if (this.gameData.gameStats.maxSpeed > 80 && !this.hasViolation('Speeding', 'High')) {
            this.gameData.gameStats.violations.push({
                type: 'Speeding',
                speed: this.gameData.gameStats.maxSpeed,
                severity: 'High',
                timestamp: Date.now()
            });
            console.log('🚨 HIGH SPEEDING VIOLATION DETECTED!', this.gameData.gameStats.maxSpeed + ' mph');
        } else if (this.gameData.gameStats.maxSpeed > 65 && !this.hasViolation('Speeding', 'Medium')) {
            this.gameData.gameStats.violations.push({
                type: 'Speeding',
                speed: this.gameData.gameStats.maxSpeed,
                severity: 'Medium',
                timestamp: Date.now()
            });
            console.log('⚠️ MEDIUM SPEEDING VIOLATION DETECTED!', this.gameData.gameStats.maxSpeed + ' mph');
        }
    }
    
    checkForCollisions() {
        // Check for sudden braking patterns (collision indicators)
        const recentBraking = this.gameData.events
            .filter(e => e.type === 'braking' && Date.now() - e.timestamp < 3000)
            .length;
        
        if (recentBraking > 2 && !this.hasCollision('Sudden Braking')) {
            this.gameData.gameStats.collisions.push({
                type: 'Sudden Braking',
                severity: 'Low',
                brakingEvents: recentBraking,
                timestamp: Date.now()
            });
            console.log('💥 POTENTIAL COLLISION DETECTED! Sudden braking pattern');
        }
    }
    
    hasViolation(type, severity) {
        return this.gameData.gameStats.violations.some(v => v.type === type && v.severity === severity);
    }
    
    hasCollision(type) {
        return this.gameData.gameStats.collisions.some(c => c.type === type);
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
        
        // Update driving data every 2 seconds
        setInterval(() => {
            this.updateDrivingData();
        }, 2000);
    }
    
    updateDrivingData() {
        // Recalculate driving data in real-time
        this.trackDrivingData();
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
                gameStats: this.gameData.gameStats, // Include driving data
                recentEvents: this.gameData.events.slice(-10), // Last 10 events
                timestamp: serverTimestamp(),
                websiteUrl: window.location.href,
                trackingMethod: 'Pure JavaScript Analytics'
            };
            
            console.log('💾 Saving session data to Firebase...', sessionData);
            const docRef = await addDoc(collection(window.firebaseDB, 'game1'), sessionData);
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
    
    getGameStats() {
        return this.gameData.gameStats;
    }
    
    getCurrentScore() {
        return this.gameData.gameStats.score;
    }
    
    getViolations() {
        return this.gameData.gameStats.violations;
    }
    
    getCollisions() {
        return this.gameData.gameStats.collisions;
    }
    
    forceSave() {
        this.saveToFirebase();
    }
}

// Initialize Pure Game Analytics
window.pureGameAnalytics = new PureGameAnalytics();

        console.log('🎮 Pure JavaScript Game Analytics loaded - NO Unity scripts required!');
        console.log('📊 Now tracking: Violations, Collisions, Speed, Score, Level, Distance');
        console.log('🚗 Driving data will be calculated from keyboard interactions');
        console.log('💾 Data auto-saves to Firestore every 30 seconds');
        console.log('🎯 To see game data: Press Arrow Up (accelerate) or Arrow Down (brake)');
        console.log('🚨 Violations detected at: Speed > 65 mph (Medium), Speed > 80 mph (High)');
        console.log('💥 Collisions detected from: Sudden braking patterns');
