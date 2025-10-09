// Firebase configuration for Unity WebGL integration
import { initializeApp } from 'https://www.gstatic.com/firebasejs/10.7.1/firebase-app.js';
import { getFirestore, connectFirestoreEmulator } from 'https://www.gstatic.com/firebasejs/10.7.1/firebase-firestore.js';

export const firebaseConfig = {
  apiKey: "AIzaSyAJ3wZzKUjkrFKiahV64Rnl2OIk7eWcWFI",
  authDomain: "drivesafeacademy.firebaseapp.com",
  projectId: "drivesafeacademy",
  storageBucket: "drivesafeacademy.firebasestorage.app",
  messagingSenderId: "1073277848731",
  appId: "1:1073277848731:web:c250fd9cfb7f7416a08a3e"
};

// Initialize Firebase with error handling
let app, db;

try {
  console.log('🔧 Initializing Firebase...');
  app = initializeApp(firebaseConfig);
  db = getFirestore(app);
  
  // Export for Unity WebGL communication
  window.firebaseDB = db;
  window.firebaseApp = app;
  
  console.log('✅ Firebase initialized successfully');
  console.log('📊 Project ID:', firebaseConfig.projectId);
  
} catch (error) {
  console.error('❌ Firebase initialization failed:', error);
  console.error('🔍 Error details:', {
    name: error.name,
    message: error.message,
    code: error.code
  });
  
  // Still set the objects to null so other code can handle the error
  window.firebaseDB = null;
  window.firebaseApp = null;
}
