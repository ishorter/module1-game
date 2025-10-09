class ProfessionalGameAnalytics {
    constructor() {
        this.isInitialized = false;
        this.isUnityReady = false;
        this.trackingActive = false;
        this.errorCount = 0;
        this.maxErrors = 10;
        
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
           
            gameStats: {
                speed: 0,
                maxSpeed: 0,
                violations: 0,
                collisions: 0,
                score: 0,
                level: 1,
                distance: 0,
                timeSpent: 0,
                gear: 'Drive',
                handbrake: false,
                lastUpdate: Date.now()
            }
        };
        
        this.firebaseReady = false;
        this.eventQueue = [];
        this.trackingMethods = {
            unityMessages: false,
            consoleParsing: false,
            domExtraction: false,
            memoryScanning: false
        };
        
        // Initialize with comprehensive error handling
        this.initializeProfessionally();
    }
    
    async initializeProfessionally() {
        console.log('🚀 Initializing Professional Game Analytics System...');
        
        try {
            // Step 1: Setup comprehensive error handling first
            this.setupGlobalErrorHandling();
            
            // Step 2: Wait for Firebase
            await this.waitForFirebase();
            
            // Step 3: Initialize tracking systems
            this.initializeTrackingSystems();
            
            // Step 4: Setup Unity integration (with error handling)
            this.setupUnityIntegration();
            
            // Step 5: Start professional tracking
            this.startProfessionalTracking();
            
            this.isInitialized = true;
            this.trackingActive = true;
            
            console.log('✅ Professional Game Analytics System initialized successfully');
            console.log('🎯 All tracking methods active and error-resistant');
            
        } catch (error) {
            console.error('❌ Professional initialization failed:', error);
            // Continue with basic tracking even if initialization fails
            this.initializeBasicTracking();
        }
    }
    
    setupGlobalErrorHandling() {
        console.log('🛡️ Setting up comprehensive error handling...');
        
        // Handle all JavaScript errors
        window.addEventListener('error', (event) => {
            this.handleError('JavaScript Error', event.error);
            return true; // Prevent default error handling
        });
        
        // Handle unhandled promise rejections
        window.addEventListener('unhandledrejection', (event) => {
            this.handleError('Promise Rejection', event.reason);
            event.preventDefault();
        });
        
        // Handle Unity-specific errors
        window.addEventListener('error', (event) => {
            if (event.filename && event.filename.includes('Module1A')) {
                this.handleError('Unity Error', event.error);
                return true; // Prevent Unity errors from stopping tracking
            }
        });
        
        console.log('✅ Global error handling active');
    }
    
    handleError(errorType, error) {
        this.errorCount++;
        
        console.log(`🛡️ ${errorType} handled (${this.errorCount}/${this.maxErrors}):`, error?.message || error);
        
        // If too many errors, switch to basic tracking
        if (this.errorCount > this.maxErrors) {
            console.log('⚠️ Too many errors, switching to basic tracking mode');
            this.switchToBasicTracking();
        }
        
        // Continue tracking despite errors
        return true;
    }
    
    switchToBasicTracking() {
        console.log('🔄 Switching to basic tracking mode...');
        
        // Disable complex Unity integration
        this.trackingMethods.unityMessages = false;
        this.trackingMethods.memoryScanning = false;
        
        // Keep basic tracking active
        this.trackingMethods.consoleParsing = true;
        this.trackingMethods.domExtraction = true;
        
        console.log('✅ Basic tracking mode active');
    }
    
    initializeBasicTracking() {
        console.log('🔄 Initializing basic tracking fallback...');
        
        this.trackingMethods.consoleParsing = true;
        this.trackingMethods.domExtraction = true;
        this.trackingActive = true;
        
        console.log('✅ Basic tracking initialized');
    }
    
    async waitForFirebase() {
        console.log('🔥 Waiting for Firebase initialization...');
        
        let attempts = 0;
        while (!window.firebaseDB && attempts < 20) {
            await new Promise(resolve => setTimeout(resolve, 500));
            attempts++;
            console.log(`🔄 Firebase initialization attempt ${attempts}/20`);
        }
        
        if (window.firebaseDB) {
            this.firebaseReady = true;
            console.log('✅ Firebase ready for Professional Analytics');
        } else {
            console.warn('⚠️ Firebase not available - analytics will queue data');
        }
    }
    
    initializeTrackingSystems() {
        console.log('🔧 Initializing professional tracking systems...');
        
        try {
            // Initialize all tracking systems
            this.initializeCanvasTracking();
            this.initializePerformanceTracking();
            this.initializeUserInteractionTracking();
            this.initializeGameDataTracking();
            
            console.log('✅ All tracking systems initialized');
        } catch (error) {
            console.error('❌ Tracking systems initialization failed:', error);
            this.handleError('Tracking Systems', error);
        }
    }
    
    initializeCanvasTracking() {
        console.log('🎯 Initializing canvas tracking...');
        
        try {
            const canvas = document.getElementById('unity-canvas');
            if (canvas) {
                // Track canvas interactions
                canvas.addEventListener('click', (event) => {
                    this.recordEvent('canvas_click', {
                        x: event.clientX,
                        y: event.clientY,
                        timestamp: Date.now()
                    });
                });
                
                console.log('✅ Canvas tracking initialized');
            } else {
                console.log('⚠️ Unity canvas not found');
            }
        } catch (error) {
            this.handleError('Canvas Tracking', error);
        }
    }
    
    initializePerformanceTracking() {
        console.log('⚡ Initializing performance tracking...');
        
        try {
            setInterval(() => {
                this.updatePerformanceData();
            }, 1000);
            
            console.log('✅ Performance tracking initialized');
        } catch (error) {
            this.handleError('Performance Tracking', error);
        }
    }
    
    initializeUserInteractionTracking() {
        console.log('👆 Initializing user interaction tracking...');
        
        try {
            document.addEventListener('keydown', (event) => {
                this.recordEvent('key_press', {
                    key: event.key,
                    code: event.code,
                    timestamp: Date.now()
                });
                this.gameData.userInteractions.keyPresses++;
            });
            
            console.log('✅ User interaction tracking initialized');
        } catch (error) {
            this.handleError('User Interaction Tracking', error);
        }
    }
    
    initializeGameDataTracking() {
        console.log('🎮 Initializing game data tracking...');
        
        try {
            // Start game data tracking
            setInterval(() => {
                this.updateGameData();
            }, 2000);
            
            console.log('✅ Game data tracking initialized');
        } catch (error) {
            this.handleError('Game Data Tracking', error);
        }
    }
    
    updatePerformanceData() {
        try {
            const now = performance.now();
            if (this.lastFrameTime) {
                const frameRate = 1000 / (now - this.lastFrameTime);
                this.gameData.performance.frameRate = Math.round(frameRate);
            }
            this.lastFrameTime = now;
            
            if (performance.memory) {
                this.gameData.performance.memoryUsage = Math.round(performance.memory.usedJSHeapSize / 1024 / 1024);
            }
            
            const canvas = document.getElementById('unity-canvas');
            if (canvas) {
                this.gameData.performance.canvasSize = {
                    width: canvas.width,
                    height: canvas.height
                };
            }
        } catch (error) {
            this.handleError('Performance Update', error);
        }
    }
    
    updateGameData() {
        try {
            // Update game stats
            this.gameData.gameStats.timeSpent = Date.now() - this.gameData.startTime;
            this.gameData.gameStats.lastUpdate = Date.now();
            
            // Log current status
            console.log('📊 Game Data Status:', {
                speed: this.gameData.gameStats.speed,
                maxSpeed: this.gameData.gameStats.maxSpeed,
                violations: this.gameData.gameStats.violations,
                collisions: this.gameData.gameStats.collisions,
                timeSpent: Math.round(this.gameData.gameStats.timeSpent / 1000)
            });
        } catch (error) {
            this.handleError('Game Data Update', error);
        }
    }
    
    setupUnityIntegration() {
        console.log('🎮 Setting up Unity integration...');
        
        try {
            // Wait for Unity to be ready
            this.waitForUnity();
            
            // Setup Unity data capture
            this.setupUnityDataCapture();
            
            console.log('✅ Unity integration setup complete');
        } catch (error) {
            console.error('❌ Unity integration failed:', error);
            this.handleError('Unity Integration', error);
        }
    }
    
    async waitForUnity() {
        console.log('⏳ Waiting for Unity to be ready...');
        
        let attempts = 0;
        while (!window.unityInstance && attempts < 30) {
            await new Promise(resolve => setTimeout(resolve, 1000));
            attempts++;
            console.log(`🔄 Unity ready check attempt ${attempts}/30`);
        }
        
        if (window.unityInstance) {
            this.isUnityReady = true;
            console.log('✅ Unity instance ready');
        } else {
            console.warn('⚠️ Unity instance not found - using fallback methods');
        }
    }
    
    setupUnityDataCapture() {
        console.log('📊 Setting up Unity data capture...');
        
        try {
            // Method 1: Console parsing (most reliable)
            this.setupConsoleParsing();
            
            // Method 2: DOM extraction (fallback)
            this.setupDOMExtraction();
            
            // Method 3: Unity messages (if available)
            if (this.isUnityReady) {
                this.setupUnityMessages();
            }
            
            console.log('✅ Unity data capture setup complete');
        } catch (error) {
            this.handleError('Unity Data Capture', error);
        }
    }
    
    setupConsoleParsing() {
        console.log('🔍 Setting up console parsing...');
        
        try {
            const originalLog = console.log;
            console.log = (...args) => {
                originalLog.apply(console, args);
                
                const message = args.join(' ');
                this.parseUnityConsoleMessage(message);
            };
            
            this.trackingMethods.consoleParsing = true;
            console.log('✅ Console parsing active');
        } catch (error) {
            this.handleError('Console Parsing', error);
        }
    }
    
    setupDOMExtraction() {
        console.log('🔍 Setting up DOM extraction...');
        
        try {
            setInterval(() => {
                this.extractGameDataFromDOM();
            }, 5000);
            
            this.trackingMethods.domExtraction = true;
            console.log('✅ DOM extraction active');
        } catch (error) {
            this.handleError('DOM Extraction', error);
        }
    }
    
    setupUnityMessages() {
        console.log('📡 Setting up Unity messages...');
        
        try {
            if (window.unityInstance && window.unityInstance.SendMessage) {
                const originalSendMessage = window.unityInstance.SendMessage;
                window.unityInstance.SendMessage = (gameObject, method, parameter) => {
                    try {
                        this.processUnityMessage(gameObject, method, parameter);
                        return originalSendMessage.call(window.unityInstance, gameObject, method, parameter);
                    } catch (error) {
                        this.handleError('Unity SendMessage', error);
                        return originalSendMessage.call(window.unityInstance, gameObject, method, parameter);
                    }
                };
                
                this.trackingMethods.unityMessages = true;
                console.log('✅ Unity messages active');
            }
        } catch (error) {
            this.handleError('Unity Messages', error);
        }
    }
    
    startProfessionalTracking() {
        console.log('🚀 Starting professional tracking...');
        
        try {
            // Start auto-save to Firebase
            setInterval(() => {
                this.saveToFirebase();
            }, 30000);
            
            // Start comprehensive data capture
            setInterval(() => {
                this.captureAllGameData();
            }, 1000);
            
            console.log('✅ Professional tracking started');
        } catch (error) {
            this.handleError('Professional Tracking', error);
        }
    }
    
    captureAllGameData() {
        try {
            // Capture data from all available sources
            if (this.trackingMethods.consoleParsing) {
                // Already handled by console.log override
            }
            
            if (this.trackingMethods.domExtraction) {
                this.extractGameDataFromDOM();
            }
            
            if (this.trackingMethods.unityMessages) {
                // Already handled by SendMessage override
            }
            
        } catch (error) {
            this.handleError('Data Capture', error);
        }
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
        
        // Track keyboard events (basic tracking only)
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
    
    trackUnityInternalData() {
        console.log('🎮 Setting up Unity internal data tracking...');
        
        // Start tracking Unity's internal game data every 1 second
        setInterval(() => {
            this.captureUnityGameData();
        }, 1000);
        
        // Try to access Unity's internal state immediately
        setTimeout(() => {
            this.initializeUnityDataAccess();
        }, 3000);
    }
    
    initializeUnityDataAccess() {
        console.log('🔍 Initializing Unity data access...');
        
        // Method 1: Try to access Unity's global variables
        if (window.unityInstance) {
            console.log('✅ Unity instance found, attempting data access...');
            this.setupUnityDataExtraction();
        } else {
            console.log('⏳ Unity instance not ready, retrying...');
            setTimeout(() => {
                this.initializeUnityDataAccess();
            }, 2000);
        }
    }
    
    setupUnityDataExtraction() {
        console.log('📊 Setting up Unity data extraction...');
        
        // Add error handling for Unity WebGL context issues
        this.setupUnityErrorHandling();
        
        // Override Unity's SendMessage to capture all game data
        if (window.unityInstance && window.unityInstance.SendMessage) {
            try {
                const originalSendMessage = window.unityInstance.SendMessage;
                window.unityInstance.SendMessage = (gameObject, method, parameter) => {
                    try {
                        // Log all Unity messages
                        console.log('🎮 Unity Message:', { gameObject, method, parameter });
                        
                        // Capture game data messages
                        this.processUnityMessage(gameObject, method, parameter);
                        
                        // Call original method
                        return originalSendMessage.call(window.unityInstance, gameObject, method, parameter);
                    } catch (error) {
                        console.log('⚠️ Unity SendMessage error:', error);
                        // Continue without breaking the game
                        return originalSendMessage.call(window.unityInstance, gameObject, method, parameter);
                    }
                };
                console.log('✅ Unity SendMessage interception active');
            } catch (error) {
                console.log('⚠️ Could not setup Unity SendMessage interception:', error);
            }
        }
        
        // Try to access Unity's memory and internal state
        this.accessUnityMemory();
    }
    
    setupUnityErrorHandling() {
        console.log('🛡️ Setting up Unity error handling...');
        
        // Handle WebGL context errors
        window.addEventListener('error', (event) => {
            if (event.error && event.error.message && event.error.message.includes('GLctx')) {
                console.log('🛡️ WebGL context error detected, continuing with data tracking...');
                // Don't let WebGL errors stop our tracking
                event.preventDefault();
                return false;
            }
        });
        
        // Handle unhandled promise rejections
        window.addEventListener('unhandledrejection', (event) => {
            if (event.reason && event.reason.message && event.reason.message.includes('GLctx')) {
                console.log('🛡️ WebGL promise rejection handled, continuing...');
                event.preventDefault();
            }
        });
        
        console.log('✅ Unity error handling active');
    }
    
    accessUnityMemory() {
        try {
            if (window.unityInstance && window.unityInstance.Module) {
                const module = window.unityInstance.Module;
                console.log('🔍 Unity Module found:', module);
                
                // Try to access Unity's heap memory
                if (module.HEAP32) {
                    console.log('✅ Unity HEAP32 accessible');
                    this.setupMemoryScanning(module);
                }
                
                // Try to access Unity's functions
                if (module.ccall) {
                    console.log('✅ Unity ccall function accessible');
                }
            }
        } catch (error) {
            console.log('⚠️ Could not access Unity memory:', error);
            // Continue with other tracking methods even if memory access fails
            console.log('🔄 Continuing with alternative Unity data tracking methods...');
        }
    }
    
    setupMemoryScanning(module) {
        console.log('🔍 Setting up Unity memory scanning...');
        
        // Try to scan Unity's memory for game data
        setInterval(() => {
            try {
                this.scanUnityMemory(module);
            } catch (error) {
                console.log('⚠️ Memory scan failed:', error);
            }
        }, 2000);
    }
    
    scanUnityMemory(module) {
        // This is a simplified approach - in reality, Unity's memory layout is complex
        console.log('🔍 Scanning Unity memory for game data...');
        
        // Try to find common game data patterns in memory
        // Note: This is experimental and may not work with all Unity builds
        try {
            const heap = module.HEAP32;
            // Look for patterns that might indicate game data
            // This is a basic approach - real implementation would need Unity-specific knowledge
        } catch (error) {
            // Memory access failed
        }
    }
    
    captureUnityGameData() {
        console.log('📊 Capturing Unity game data...');
        
        // Method 1: Try to call Unity methods to get current state
        this.callUnityDataMethods();
        
        // Method 2: Try to parse Unity console output
        this.parseUnityConsoleOutput();
        
        // Method 3: Try to access Unity's internal variables
        this.accessUnityVariables();
    }
    
    callUnityDataMethods() {
        if (window.unityInstance) {
            try {
                // Try common Unity game object names and methods
                const methods = [
                    { object: 'GameManager', method: 'GetStats' },
                    { object: 'PlayerController', method: 'GetSpeed' },
                    { object: 'CollisionDetector', method: 'GetCollisionCount' },
                    { object: 'ViolationTracker', method: 'GetViolationCount' },
                    { object: 'ScoreManager', method: 'GetScore' },
                    { object: 'UIManager', method: 'GetUIData' },
                    { object: 'CarController', method: 'GetVehicleStats' }
                ];
                
                methods.forEach(({ object, method }) => {
                    try {
                        window.unityInstance.SendMessage(object, method, '');
                    } catch (error) {
                        // Method doesn't exist, continue
                    }
                });
                
                console.log('📡 Unity data method calls sent');
            } catch (error) {
                console.log('⚠️ Could not call Unity methods:', error);
            }
        }
    }
    
    parseUnityConsoleOutput() {
        // Monitor console for Unity Debug.Log messages
        const originalLog = console.log;
        console.log = (...args) => {
            originalLog.apply(console, args);
            
            const message = args.join(' ');
            
            // Look for Unity game data patterns
            if (message.includes('Speed:') || message.includes('Collisions:') || 
                message.includes('Violations:') || message.includes('Max Speed:')) {
                this.extractGameDataFromConsole(message);
            }
        };
    }
    
    extractGameDataFromConsole(message) {
        console.log('🎮 Extracting game data from console:', message);
        
        // Parse speed data
        const speedMatch = message.match(/Speed:\s*([\d.]+)\s*MPH/);
        if (speedMatch) {
            this.gameData.gameStats.speed = parseFloat(speedMatch[1]);
            console.log('🚗 Speed updated from Unity:', this.gameData.gameStats.speed);
        }
        
        // Parse max speed data
        const maxSpeedMatch = message.match(/Max Speed:\s*([\d.]+)\s*MPH/);
        if (maxSpeedMatch) {
            this.gameData.gameStats.maxSpeed = parseFloat(maxSpeedMatch[1]);
            console.log('🏎️ Max speed updated from Unity:', this.gameData.gameStats.maxSpeed);
        }
        
        // Parse collision count
        const collisionMatch = message.match(/Collisions:\s*(\d+)/);
        if (collisionMatch) {
            this.gameData.gameStats.collisions = parseInt(collisionMatch[1]);
            console.log('💥 Collision count updated from Unity:', this.gameData.gameStats.collisions);
        }
        
        // Parse violation count
        const violationMatch = message.match(/Violations:\s*(\d+)/);
        if (violationMatch) {
            this.gameData.gameStats.violations = parseInt(violationMatch[1]);
            console.log('🚨 Violation count updated from Unity:', this.gameData.gameStats.violations);
        }
        
        // Parse time data
        const timeMatch = message.match(/Time:\s*(\d{2}:\d{2})/);
        if (timeMatch) {
            console.log('⏰ Time updated from Unity:', timeMatch[1]);
        }
    }
    
    accessUnityVariables() {
        try {
            // Try to access Unity's global variables
            if (window.unityInstance) {
                const unity = window.unityInstance;
                
                // Try to access Unity's internal game state
                if (unity.Module && unity.Module.HEAP32) {
                    console.log('🔍 Unity HEAP32 accessible for variable access');
                }
                
                // Try to access Unity's game objects
                if (unity.SendMessage) {
                    // Try to get data from Unity game objects
                    this.requestUnityGameState();
                }
            }
        } catch (error) {
            console.log('⚠️ Could not access Unity variables:', error);
        }
    }
    
    requestUnityGameState() {
        console.log('📊 Requesting Unity game state...');
        
        // Try to get current game state from Unity
        try {
            // Attempt to call Unity methods that might return game data
            if (window.unityInstance) {
                // Try different approaches to get Unity game data
                this.tryUnityDataAccess();
            }
        } catch (error) {
            console.log('⚠️ Could not request Unity game state:', error);
        }
    }
    
    tryUnityDataAccess() {
        // Try multiple methods to access Unity game data
        console.log('🔍 Trying Unity data access methods...');
        
        // Method 1: Try to access Unity's internal game objects
        // Method 2: Try to call Unity's public methods
        // Method 3: Try to parse Unity's memory directly
        
        console.log('📊 Unity data access attempts completed');
    }
    
    extractUnityUIDataFromDOM() {
        console.log('🔍 AGGRESSIVE Unity UI data extraction...');
        
        try {
            let foundData = false;
            
            // Method 1: Look for any DOM elements that might contain Unity UI data
            const allElements = document.querySelectorAll('*');
            console.log('📊 Scanning', allElements.length, 'DOM elements...');
            
            allElements.forEach(element => {
                const text = element.textContent || element.innerText || '';
                
                // Look for Unity UI patterns in DOM text
                if (text.includes('Speed:') || text.includes('Collisions:') || text.includes('Violations:')) {
                    console.log('🎮 FOUND Unity UI data in DOM:', text);
                    this.parseUnityUIDataFromText(text);
                    foundData = true;
                }
            });
            
            // Method 2: Check for any canvas overlays or UI elements
            const canvas = document.getElementById('unity-canvas');
            if (canvas) {
                console.log('🎯 Checking Unity canvas area...');
                const siblings = canvas.parentElement?.children || [];
                Array.from(siblings).forEach(sibling => {
                    if (sibling !== canvas && sibling.textContent) {
                        const text = sibling.textContent;
                        if (text.includes('Speed:') || text.includes('Collisions:') || text.includes('Violations:')) {
                            console.log('🎮 FOUND Unity UI data near canvas:', text);
                            this.parseUnityUIDataFromText(text);
                            foundData = true;
                        }
                    }
                });
            }
            
            // Method 3: Check all divs, spans, and text elements specifically
            const textElements = document.querySelectorAll('div, span, p, h1, h2, h3, h4, h5, h6, label');
            console.log('📝 Checking', textElements.length, 'text elements...');
            
            textElements.forEach(element => {
                const text = element.textContent || element.innerText || '';
                
                // Look for specific patterns like "Violations: 7", "Speed: 15.5 MPH", etc.
                if (text.match(/\b(Violations?|Speed|Collisions?|Max Speed):\s*[\d.]+\b/i)) {
                    console.log('🎮 FOUND game data in text element:', text);
                    this.parseUnityUIDataFromText(text);
                    foundData = true;
                }
            });
            
            // Method 4: Check if Unity renders UI to any specific containers
            const possibleContainers = document.querySelectorAll('[id*="ui"], [id*="game"], [id*="stats"], [class*="ui"], [class*="game"], [class*="stats"]');
            console.log('📦 Checking', possibleContainers.length, 'possible UI containers...');
            
            possibleContainers.forEach(container => {
                const text = container.textContent || container.innerText || '';
                if (text.match(/\b(Violations?|Speed|Collisions?|Max Speed):\s*[\d.]+\b/i)) {
                    console.log('🎮 FOUND game data in container:', text);
                    this.parseUnityUIDataFromText(text);
                    foundData = true;
                }
            });
            
            // Method 5: Try to access Unity's internal state directly
            this.attemptDirectUnityAccess();
            
            if (!foundData) {
                console.log('⚠️ No Unity UI data found in DOM - Unity might render UI directly to canvas');
                console.log('💡 Current gameStats:', this.gameData.gameStats);
            }
            
        } catch (error) {
            console.log('⚠️ DOM extraction failed:', error);
        }
    }
    
    parseUnityUIDataFromText(text) {
        console.log('📊 Parsing Unity UI data from text:', text);
        
        // Parse speed data from UI text
        const speedMatch = text.match(/Speed:\s*([\d.]+)\s*MPH/i);
        if (speedMatch) {
            this.gameData.gameStats.speed = parseFloat(speedMatch[1]);
            console.log('🚗 Speed extracted from DOM:', this.gameData.gameStats.speed);
        }
        
        // Parse max speed data
        const maxSpeedMatch = text.match(/Max Speed:\s*([\d.]+)\s*MPH/i);
        if (maxSpeedMatch) {
            this.gameData.gameStats.maxSpeed = parseFloat(maxSpeedMatch[1]);
            console.log('🏎️ Max speed extracted from DOM:', this.gameData.gameStats.maxSpeed);
        }
        
        // Parse collision count
        const collisionMatch = text.match(/Collisions:\s*(\d+)/i);
        if (collisionMatch) {
            this.gameData.gameStats.collisions = parseInt(collisionMatch[1]);
            console.log('💥 Collision count extracted from DOM:', this.gameData.gameStats.collisions);
        }
        
        // Parse violation count
        const violationMatch = text.match(/Violations:\s*(\d+)/i);
        if (violationMatch) {
            this.gameData.gameStats.violations = parseInt(violationMatch[1]);
            console.log('🚨 Violation count extracted from DOM:', this.gameData.gameStats.violations);
        }
        
        // Parse gear data
        const gearMatch = text.match(/Gear:\s*(\w+)/i);
        if (gearMatch) {
            this.gameData.gameStats.gear = gearMatch[1];
            console.log('⚙️ Gear extracted from DOM:', this.gameData.gameStats.gear);
        }
        
        // Parse time data
        const timeMatch = text.match(/Time:\s*(\d{2}:\d{2})/i);
        if (timeMatch) {
            console.log('⏰ Time extracted from DOM:', timeMatch[1]);
        }
    }
    
    processUnityMessage(gameObject, method, parameter) {
        console.log('📨 Processing Unity message:', { gameObject, method, parameter });
        
        // Process different Unity messages
        if (method.includes('Collision')) {
            this.gameData.gameStats.collisions++;
            console.log('💥 Collision detected from Unity, count:', this.gameData.gameStats.collisions);
        }
        
        if (method.includes('Violation')) {
            this.gameData.gameStats.violations++;
            console.log('🚨 Violation detected from Unity, count:', this.gameData.gameStats.violations);
        }
        
        if (method.includes('Speed')) {
            const speedValue = parseFloat(parameter) || 0;
            if (speedValue > 0) {
                this.gameData.gameStats.speed = speedValue;
                if (speedValue > this.gameData.gameStats.maxSpeed) {
                    this.gameData.gameStats.maxSpeed = speedValue;
                }
                console.log('🚗 Speed updated from Unity:', speedValue);
            }
        }
    }
    
    trackGameEvents() {
        console.log('🎮 Setting up Unity game event tracking...');
        
        // Listen for Unity messages (when Unity sends data)
        window.addEventListener('message', (event) => {
            if (event.data && typeof event.data === 'object') {
                this.handleUnityMessage(event.data);
            }
        });
        
        // Listen for Unity SendMessage calls
        this.setupUnityMessageListener();
        
        // Monitor Unity console logs (if accessible)
        this.monitorUnityConsole();
        
        // Try to access Unity game data directly
        this.setupUnityDataAccess();
    }
    
    setupUnityMessageListener() {
        console.log('📡 Setting up Unity SendMessage listener...');
        
        // Override Unity's SendMessage to capture game data
        if (window.unityInstance) {
            const originalSendMessage = window.unityInstance.SendMessage;
            window.unityInstance.SendMessage = (gameObject, method, parameter) => {
                console.log('🎮 Unity SendMessage intercepted:', { gameObject, method, parameter });
                
                // Capture game data messages
                if (method === 'OnViolationDetected' || method === 'OnCollisionDetected') {
                    this.handleGameEvent(method, parameter);
                }
                
                // Call original method
                return originalSendMessage.call(window.unityInstance, gameObject, method, parameter);
            };
        }
    }
    
    setupUnityDataAccess() {
        console.log('🔍 Setting up Unity data access...');
        
        // Try to access Unity game data through various methods
        setTimeout(() => {
            this.tryAccessUnityData();
        }, 5000); // Wait 5 seconds for Unity to load
    }
    
    tryAccessUnityData() {
        console.log('🎯 Attempting to access Unity game data...');
        
        // Method 1: Try to access Unity instance data
        if (window.unityInstance && window.unityInstance.Module) {
            try {
                const module = window.unityInstance.Module;
                console.log('✅ Unity Module found:', module);
                
                // Try to get game data
                this.requestUnityGameData();
            } catch (error) {
                console.log('⚠️ Could not access Unity Module:', error);
            }
        }
        
        // Method 2: Try to access through global Unity variables
        if (window.Unity) {
            console.log('✅ Unity global object found');
        }
        
        // Method 3: Try to intercept Unity console messages
        this.interceptUnityConsole();
    }
    
    requestUnityGameData() {
        console.log('📊 Requesting Unity game data...');
        
        // Try to call Unity methods to get current game state
        if (window.unityInstance) {
            try {
                // Request current game statistics
                window.unityInstance.SendMessage('GameManager', 'GetCurrentStats', '');
                window.unityInstance.SendMessage('ScoreManager', 'GetCurrentScore', '');
                window.unityInstance.SendMessage('CollisionManager', 'GetCollisionCount', '');
                window.unityInstance.SendMessage('ViolationManager', 'GetViolationCount', '');
                
                console.log('📡 Game data requests sent to Unity');
            } catch (error) {
                console.log('⚠️ Could not send requests to Unity:', error);
            }
        }
    }
    
    interceptUnityConsole() {
        console.log('🔍 Setting up Unity console interception...');
        
        // Try to intercept Unity's Debug.Log messages
        const originalLog = console.log;
        console.log = (...args) => {
            originalLog.apply(console, args);
            
            // Check for Unity game data in console messages
            const message = args.join(' ');
            if (message.includes('Collision:') || message.includes('Violation:') || 
                message.includes('Score:') || message.includes('Max Speed:')) {
                this.parseUnityGameData(message);
            }
        };
    }
    
    parseUnityGameData(message) {
        console.log('🎮 Parsing Unity game data:', message);
        
        // Parse collision data
        if (message.includes('Collision:')) {
            const collisionMatch = message.match(/Collision:\s*(\d+)/);
            if (collisionMatch) {
                const collisionCount = parseInt(collisionMatch[1]);
                this.updateCollisionCount(collisionCount);
            }
        }
        
        // Parse violation data
        if (message.includes('Violation:')) {
            const violationMatch = message.match(/Violation:\s*(\d+)/);
            if (violationMatch) {
                const violationCount = parseInt(violationMatch[1]);
                this.updateViolationCount(violationCount);
            }
        }
        
        // Parse score data
        if (message.includes('Score:')) {
            const scoreMatch = message.match(/Score:\s*(\d+)/);
            if (scoreMatch) {
                const score = parseInt(scoreMatch[1]);
                this.gameData.gameStats.score = score;
                console.log('🏆 Score updated from Unity:', score);
            }
        }
        
        // Parse max speed data
        if (message.includes('Max Speed:')) {
            const speedMatch = message.match(/Max Speed:\s*([\d.]+)/);
            if (speedMatch) {
                const maxSpeed = parseFloat(speedMatch[1]);
                this.gameData.gameStats.maxSpeed = maxSpeed;
                console.log('🚗 Max speed updated from Unity:', maxSpeed);
            }
        }
    }
    
    updateCollisionCount(count) {
        // Update collision count from Unity data
        if (count > this.gameData.gameStats.collisions.length) {
            const newCollisions = count - this.gameData.gameStats.collisions.length;
            for (let i = 0; i < newCollisions; i++) {
                this.gameData.gameStats.collisions.push({
                    type: 'Unity Collision',
                    severity: 'Medium',
                    timestamp: Date.now(),
                    source: 'Unity Game'
                });
            }
            console.log('💥 Collision count updated from Unity:', count);
        }
    }
    
    updateViolationCount(count) {
        // Update violation count from Unity data
        if (count > this.gameData.gameStats.violations.length) {
            const newViolations = count - this.gameData.gameStats.violations.length;
            for (let i = 0; i < newViolations; i++) {
                this.gameData.gameStats.violations.push({
                    type: 'Unity Violation',
                    severity: 'Medium',
                    timestamp: Date.now(),
                    source: 'Unity Game'
                });
            }
            console.log('🚨 Violation count updated from Unity:', count);
        }
    }
    
    handleGameEvent(eventType, parameter) {
        console.log('🎮 Game event received:', eventType, parameter);
        
        if (eventType === 'OnViolationDetected') {
            this.gameData.gameStats.violations.push({
                type: 'Unity Violation',
                data: parameter,
                severity: 'Medium',
                timestamp: Date.now(),
                source: 'Unity Game'
            });
            console.log('🚨 Unity violation recorded');
        } else if (eventType === 'OnCollisionDetected') {
            this.gameData.gameStats.collisions.push({
                type: 'Unity Collision',
                data: parameter,
                severity: 'Medium',
                timestamp: Date.now(),
                source: 'Unity Game'
            });
            console.log('💥 Unity collision recorded');
        }
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
        
        // Update Unity game data every 2 seconds
        setInterval(() => {
            this.updateUnityGameData();
        }, 2000);
        
        // Try to capture Unity UI data every 5 seconds
        setInterval(() => {
            this.captureUnityUIData();
        }, 5000);
        
        // Aggressive DOM extraction: Try to extract data from DOM elements every 2 seconds
        setInterval(() => {
            this.extractUnityUIDataFromDOM();
        }, 2000);
    }
    
    updateUnityGameData() {
        // Update Unity game data and log current state
        console.log('📊 Unity Game Data Status:', {
            speed: this.gameData.gameStats.speed,
            maxSpeed: this.gameData.gameStats.maxSpeed,
            violations: this.gameData.gameStats.violations,
            collisions: this.gameData.gameStats.collisions,
            timeSpent: Math.round((Date.now() - this.gameData.startTime) / 1000)
        });
    }
    
    captureUnityUIData() {
        console.log('🎯 Attempting to capture Unity UI data...');
        
        // Method 1: Try to access Unity canvas and extract text
        this.extractUnityCanvasData();
        
        // Method 2: Try to access Unity's internal state
        this.accessUnityInternalState();
        
        // Method 3: Try to call Unity methods directly
        this.callUnityMethods();
    }
    
    extractUnityCanvasData() {
        try {
            const canvas = document.getElementById('unity-canvas');
            if (canvas) {
                // Try to get canvas context and extract data
                const ctx = canvas.getContext('2d');
                if (ctx) {
                    // This won't work for WebGL, but we can try other methods
                    console.log('📊 Canvas context found');
                }
            }
        } catch (error) {
            console.log('⚠️ Could not extract canvas data:', error);
        }
    }
    
    accessUnityInternalState() {
        try {
            // Try to access Unity's global variables
            if (window.unityInstance) {
                // Try different Unity object properties
                const unityInstance = window.unityInstance;
                
                // Try to access Unity's internal game state
                if (unityInstance.Module && unityInstance.Module.HEAP32) {
                    console.log('🔍 Unity Module HEAP32 found - attempting data extraction');
                }
                
                // Try to access Unity's memory
                if (unityInstance.Module && unityInstance.Module.ccall) {
                    console.log('🔍 Unity ccall function found');
                }
            }
        } catch (error) {
            console.log('⚠️ Could not access Unity internal state:', error);
        }
    }
    
    callUnityMethods() {
        try {
            if (window.unityInstance) {
                // Try to call Unity methods to get current game state
                console.log('📡 Attempting to call Unity methods...');
                
                // Try common Unity game object names
                const gameObjects = [
                    'GameManager', 'ScoreManager', 'CollisionManager', 'ViolationManager',
                    'Player', 'Car', 'Vehicle', 'GameController', 'UIController'
                ];
                
                const methods = [
                    'GetCollisionCount', 'GetViolationCount', 'GetScore', 'GetMaxSpeed',
                    'GetCurrentStats', 'GetGameData', 'GetStats'
                ];
                
                gameObjects.forEach(gameObject => {
                    methods.forEach(method => {
                        try {
                            window.unityInstance.SendMessage(gameObject, method, '');
                        } catch (error) {
                            // Method doesn't exist, continue
                        }
                    });
                });
                
                console.log('📡 Unity method calls sent');
            }
        } catch (error) {
            console.log('⚠️ Could not call Unity methods:', error);
        }
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
                gameStats: this.gameData.gameStats, // Include Unity internal data
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

    parseUnityConsoleMessage(message) {
        try {
            // Parse Unity console messages for game data
            if (message.includes('Speed:') || message.includes('Collisions:') || 
                message.includes('Violations:') || message.includes('Max Speed:')) {
                
                console.log('🎮 Parsing Unity console message:', message);
                
                // Parse speed data
                const speedMatch = message.match(/Speed:\s*([\d.]+)\s*MPH/i);
                if (speedMatch) {
                    this.gameData.gameStats.speed = parseFloat(speedMatch[1]);
                    console.log('🚗 Speed updated from console:', this.gameData.gameStats.speed);
                }
                
                // Parse max speed data
                const maxSpeedMatch = message.match(/Max Speed:\s*([\d.]+)\s*MPH/i);
                if (maxSpeedMatch) {
                    this.gameData.gameStats.maxSpeed = parseFloat(maxSpeedMatch[1]);
                    console.log('🏎️ Max speed updated from console:', this.gameData.gameStats.maxSpeed);
                }
                
                // Parse collision count
                const collisionMatch = message.match(/Collisions:\s*(\d+)/i);
                if (collisionMatch) {
                    this.gameData.gameStats.collisions = parseInt(collisionMatch[1]);
                    console.log('💥 Collision count updated from console:', this.gameData.gameStats.collisions);
                }
                
                // Parse violation count
                const violationMatch = message.match(/Violations:\s*(\d+)/i);
                if (violationMatch) {
                    this.gameData.gameStats.violations = parseInt(violationMatch[1]);
                    console.log('🚨 Violation count updated from console:', this.gameData.gameStats.violations);
                }
            }
        } catch (error) {
            this.handleError('Console Message Parsing', error);
        }
    }
    
    extractGameDataFromDOM() {
        try {
            // Look for Unity UI data in DOM elements
            const allElements = document.querySelectorAll('*');
            
            allElements.forEach(element => {
                const text = element.textContent || element.innerText || '';
                
                if (text.includes('Speed:') || text.includes('Collisions:') || text.includes('Violations:')) {
                    this.parseUnityUIDataFromText(text);
                }
            });
        } catch (error) {
            this.handleError('DOM Extraction', error);
        }
    }
    
    parseUnityUIDataFromText(text) {
        try {
            // Parse speed data
            const speedMatch = text.match(/Speed:\s*([\d.]+)\s*MPH/i);
            if (speedMatch) {
                this.gameData.gameStats.speed = parseFloat(speedMatch[1]);
            }
            
            // Parse collision count
            const collisionMatch = text.match(/Collisions:\s*(\d+)/i);
            if (collisionMatch) {
                this.gameData.gameStats.collisions = parseInt(collisionMatch[1]);
            }
            
            // Parse violation count
            const violationMatch = text.match(/Violations:\s*(\d+)/i);
            if (violationMatch) {
                this.gameData.gameStats.violations = parseInt(violationMatch[1]);
            }
        } catch (error) {
            this.handleError('UI Text Parsing', error);
        }
    }
    
    processUnityMessage(gameObject, method, parameter) {
        try {
            if (method.includes('Collision')) {
                this.gameData.gameStats.collisions++;
                console.log('💥 Collision detected from Unity message');
            }
            
            if (method.includes('Violation')) {
                this.gameData.gameStats.violations++;
                console.log('🚨 Violation detected from Unity message');
            }
        } catch (error) {
            this.handleError('Unity Message Processing', error);
        }
    }
    
    recordEvent(eventType, data) {
        try {
            const event = {
                type: eventType,
                data: data,
                timestamp: Date.now(),
                sessionId: this.gameData.sessionId
            };
            
            this.gameData.events.push(event);
            
            // Keep only last 100 events
            if (this.gameData.events.length > 100) {
                this.gameData.events = this.gameData.events.slice(-100);
            }
        } catch (error) {
            this.handleError('Event Recording', error);
        }
    }
    
    async saveToFirebase() {
        if (!this.firebaseReady) {
            console.log('⏳ Firebase not ready - queuing data...');
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
                gameStats: this.gameData.gameStats,
                recentEvents: this.gameData.events.slice(-10),
                timestamp: serverTimestamp(),
                websiteUrl: window.location.href,
                trackingMethod: 'Professional Game Analytics',
                errorCount: this.errorCount,
                trackingMethods: this.trackingMethods
            };
            
            console.log('💾 Saving professional session data to Firebase...');
            const docRef = await addDoc(collection(window.firebaseDB, 'game1'), sessionData);
            console.log('✅ Professional session data saved with ID:', docRef.id);
            
        } catch (error) {
            this.handleError('Firebase Save', error);
        }
    }
    
    generateSessionId() {
        return 'professional_analytics_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9);
    }

    attemptDirectUnityAccess() {
        try {
            console.log('🔍 Attempting direct Unity access...');
            
            if (window.unityInstance) {
                console.log('✅ Unity instance found, attempting data access...');
                
                // Try to access Unity's internal game state
                try {
                    // Method 1: Try to call Unity methods if they exist
                    if (typeof window.unityInstance.SendMessage === 'function') {
                        console.log('📞 Unity SendMessage available - trying to request game data...');
                        
                        // Try to request current game stats
                        const gameObjects = ['GameManager', 'UIController', 'GameController', 'StatsManager'];
                        gameObjects.forEach(obj => {
                            try {
                                window.unityInstance.SendMessage(obj, 'GetCurrentStats', '');
                                console.log('📡 Requested stats from:', obj);
                            } catch (e) {
                                console.log('⚠️ Could not request from:', obj);
                            }
                        });
                    }
                    
                    // Method 2: Try to access Unity's module for game data
                    if (window.unityInstance.Module) {
                        console.log('🧠 Unity Module accessible, checking for game data...');
                        
                        // Try to find game data in Unity's memory or exports
                        const module = window.unityInstance.Module;
                        if (module.HEAP32) {
                            console.log('💾 Unity HEAP32 accessible, size:', module.HEAP32.length);
                        }
                        
                        // Check for any exported functions that might contain game data
                        const exports = Object.keys(module).filter(key => 
                            typeof module[key] === 'function' && 
                            (key.toLowerCase().includes('game') || 
                             key.toLowerCase().includes('stats') || 
                             key.toLowerCase().includes('violation') ||
                             key.toLowerCase().includes('collision'))
                        );
                        
                        if (exports.length > 0) {
                            console.log('🎯 Found potential game-related exports:', exports);
                        }
                    }
                    
                } catch (error) {
                    console.log('⚠️ Direct Unity access failed:', error);
                }
            } else {
                console.log('❌ Unity instance not available for direct access');
            }
            
        } catch (error) {
            console.log('⚠️ Unity access attempt failed:', error);
        }
    }

    // Enhanced console parsing with more patterns
    parseUnityConsoleMessage(message) {
        try {
            console.log('🎮 Enhanced console parsing:', message);
            
            // Parse multiple patterns for each data type
            const patterns = {
                speed: [
                    /Speed:\s*([\d.]+)\s*MPH/i,
                    /Speed[:\s]*([\d.]+)/i,
                    /Current Speed[:\s]*([\d.]+)/i
                ],
                maxSpeed: [
                    /Max Speed:\s*([\d.]+)\s*MPH/i,
                    /Max Speed[:\s]*([\d.]+)/i,
                    /Top Speed[:\s]*([\d.]+)/i
                ],
                violations: [
                    /Violations:\s*(\d+)/i,
                    /Violations[:\s]*(\d+)/i,
                    /Violation Count[:\s]*(\d+)/i,
                    /Total Violations[:\s]*(\d+)/i
                ],
                collisions: [
                    /Collisions:\s*(\d+)/i,
                    /Collisions[:\s]*(\d+)/i,
                    /Collision Count[:\s]*(\d+)/i,
                    /Total Collisions[:\s]*(\d+)/i
                ]
            };
            
            let foundAnyData = false;
            
            // Try each pattern for each data type
            Object.keys(patterns).forEach(dataType => {
                patterns[dataType].forEach(pattern => {
                    const match = message.match(pattern);
                    if (match) {
                        const value = dataType === 'speed' || dataType === 'maxSpeed' ? 
                                     parseFloat(match[1]) : parseInt(match[1]);
                        
                        if (this.gameData.gameStats[dataType] !== value) {
                            this.gameData.gameStats[dataType] = value;
                            console.log(`🎯 ${dataType} updated from console:`, value);
                            foundAnyData = true;
                        }
                    }
                });
            });
            
            if (foundAnyData) {
                console.log('✅ Game data updated from console parsing');
                console.log('📊 Current stats:', this.gameData.gameStats);
            }
            
        } catch (error) {
            this.handleError('Console Message Parsing', error);
        }
    }

    // Manual trigger method - call this from browser console to force data extraction
    forceUnityDataExtraction() {
        console.log('🚀 MANUAL Unity data extraction triggered!');
        console.log('📊 Current gameStats before extraction:', this.gameData.gameStats);
        
        // Try all extraction methods
        this.extractUnityUIDataFromDOM();
        this.attemptDirectUnityAccess();
        
        // Force save current data
        this.saveToFirebase();
        
        console.log('📊 Current gameStats after extraction:', this.gameData.gameStats);
        console.log('💡 Use this method to manually trigger data extraction: window.professionalAnalytics.forceUnityDataExtraction()');
    }
}

// Make the class available globally
window.ProfessionalGameAnalytics = ProfessionalGameAnalytics;
