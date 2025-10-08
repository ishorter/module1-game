// Firebase configuration for Unity WebGL integration
import { initializeApp } from 'https://www.gstatic.com/firebasejs/10.7.1/firebase-app.js';
import { getFirestore } from 'https://www.gstatic.com/firebasejs/10.7.1/firebase-firestore.js';

export const firebaseConfig = {
  apiKey: "AIzaSyAJ3wZzKUjkrFKiahV64Rnl2OIk7eWcWFI",
  authDomain: "drivesafeacademy.firebaseapp.com",
  projectId: "drivesafeacademy",
  storageBucket: "drivesafeacademy.firebasestorage.app",
  messagingSenderId: "1073277848731",
  appId: "1:1073277848731:web:c250fd9cfb7f7416a08a3e"
};

// Initialize Firebase
const app = initializeApp(firebaseConfig);
const db = getFirestore(app);

// Export for Unity WebGL communication
window.firebaseDB = db;
window.firebaseApp = app;

console.log('Firebase initialized successfully');
