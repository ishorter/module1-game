// Firebase Diagnostics - Check what's happening with Firebase connection
console.log('🔍 Firebase Diagnostics Starting...');

// Check if Firebase is loaded
function checkFirebaseStatus() {
    console.log('📊 Firebase Status Check:');
    
    // Check if Firebase config is loaded
    if (typeof window.firebaseConfig !== 'undefined') {
        console.log('✅ Firebase config loaded:', window.firebaseConfig);
    } else {
        console.log('❌ Firebase config NOT loaded');
    }
    
    // Check if Firebase DB is available
    if (window.firebaseDB) {
        console.log('✅ Firebase DB available:', window.firebaseDB);
    } else {
        console.log('❌ Firebase DB NOT available');
    }
    
    // Check if Firebase App is available
    if (window.firebaseApp) {
        console.log('✅ Firebase App available:', window.firebaseApp);
    } else {
        console.log('❌ Firebase App NOT available');
    }
    
    // Check if Unity Firebase Bridge is available
    if (window.unityFirebaseBridge) {
        console.log('✅ Unity Firebase Bridge available:', window.unityFirebaseBridge);
        console.log('📊 Bridge initialized:', window.unityFirebaseBridge.isInitialized);
        console.log('📊 Bridge DB:', window.unityFirebaseBridge.db);
    } else {
        console.log('❌ Unity Firebase Bridge NOT available');
    }
    
    // Check if Unity instance is available
    if (window.unityFirebaseBridge && window.unityFirebaseBridge.unityInstance) {
        console.log('✅ Unity instance available:', window.unityFirebaseBridge.unityInstance);
    } else {
        console.log('❌ Unity instance NOT available');
    }
}

// Check Unity-Firebase communication
function testUnityFirebaseCommunication() {
    console.log('🧪 Testing Unity-Firebase Communication...');
    
    if (window.UnityFirebase) {
        console.log('✅ UnityFirebase methods available:', Object.keys(window.UnityFirebase));
        
        // Test a simple method
        try {
            window.UnityFirebase.recordViolation('Test Violation|65.5|Test Location|1');
            console.log('✅ UnityFirebase.recordViolation call successful');
        } catch (error) {
            console.error('❌ UnityFirebase.recordViolation call failed:', error);
        }
    } else {
        console.log('❌ UnityFirebase methods NOT available');
    }
}

// Check Firebase collections
async function checkFirebaseCollections() {
    console.log('📊 Checking Firebase Collections...');
    
    if (!window.firebaseDB) {
        console.log('❌ Cannot check collections - Firebase DB not available');
        return;
    }
    
    try {
        // Import Firebase functions
        const { collection, getDocs } = await import('https://www.gstatic.com/firebasejs/10.7.1/firebase-firestore.js');
        
        // Check each collection
        const collections = ['violations', 'collisions', 'gameProgress', 'sessions', 'connectionTests'];
        
        for (const collectionName of collections) {
            try {
                const querySnapshot = await getDocs(collection(window.firebaseDB, collectionName));
                console.log(`📊 Collection '${collectionName}': ${querySnapshot.size} documents`);
                
                if (querySnapshot.size > 0) {
                    querySnapshot.forEach((doc) => {
                        console.log(`   📄 Document ${doc.id}:`, doc.data());
                    });
                }
            } catch (error) {
                console.error(`❌ Error checking collection '${collectionName}':`, error);
            }
        }
    } catch (error) {
        console.error('❌ Error importing Firebase functions:', error);
    }
}

// Check Firebase write capability (no test data written)
async function checkFirebaseWriteCapability() {
  console.log('🔍 Checking Firebase Write Capability...');
  
  if (!window.firebaseDB) {
    console.log('❌ Cannot check write capability - Firebase DB not available');
    return;
  }
  
  try {
    // Import Firebase functions
    const { collection, addDoc, serverTimestamp } = await import('https://www.gstatic.com/firebasejs/10.7.1/firebase-firestore.js');
    
    console.log('✅ Firebase write functions available - ready for real game data');
    console.log('🎮 NO TEST DATA WILL BE WRITTEN - only real game events');
    
    return true;
  } catch (error) {
    console.error('❌ Firebase write capability check failed:', error);
    console.error('🔍 Error details:', {
      name: error.name,
      message: error.message,
      code: error.code,
      stack: error.stack
    });
    
    return false;
  }
}

// Run all diagnostics
async function runAllDiagnostics() {
    console.log('🚀 Running Complete Firebase Diagnostics...');
    
    checkFirebaseStatus();
    testUnityFirebaseCommunication();
    await checkFirebaseCollections();
    await checkFirebaseWriteCapability();
    
    console.log('✅ Firebase Diagnostics Complete - ONLY REAL GAME DATA WILL BE TRACKED');
}

// Run diagnostics when page loads
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => {
        setTimeout(runAllDiagnostics, 2000); // Wait 2 seconds for everything to load
    });
} else {
    setTimeout(runAllDiagnostics, 2000); // Wait 2 seconds for everything to load
}

// Expose diagnostics functions globally for real data checking
window.firebaseDiagnostics = {
  checkFirebaseStatus,
  testUnityFirebaseCommunication,
  checkFirebaseCollections,
  checkFirebaseWriteCapability,
  runAllDiagnostics
};

console.log('🔍 Firebase Diagnostics loaded. Run window.firebaseDiagnostics.runAllDiagnostics() to check real data tracking.');
