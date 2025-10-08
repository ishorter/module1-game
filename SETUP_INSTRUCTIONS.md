# DriverEdSimulator - Firebase Progress Tracking Setup

## 🚀 Quick Setup Guide

### **Step 1: Firebase Configuration**

1. **Create Firebase Project**
   - Go to [Firebase Console](https://console.firebase.google.com/)
   - Create new project: "DriverEdSimulator"
   - Enable Firestore Database

2. **Get Firebase Config**
   - Go to Project Settings > General
   - Scroll to "Your apps" section
   - Click "Add app" > Web app
   - Copy the config object

3. **Update Configuration**
   - Edit `js/firebase-config.js`
   - Replace the placeholder config with your actual Firebase config:

```javascript
const firebaseConfig = {
  apiKey: "your-actual-api-key",
  authDomain: "your-project.firebaseapp.com",
  projectId: "your-actual-project-id",
  storageBucket: "your-project.appspot.com",
  messagingSenderId: "your-actual-sender-id",
  appId: "your-actual-app-id"
};
```

### **Step 2: Firestore Security Rules**

Update your Firestore rules to allow read/write access:

```javascript
rules_version = '2';
service cloud.firestore {
  match /databases/{database}/documents {
    // Allow read/write access to game data
    match /gameProgress/{document} {
      allow read, write: if true;
    }
    match /gameScores/{document} {
      allow read, write: if true;
    }
    match /achievements/{document} {
      allow read, write: if true;
    }
  }
}
```

### **Step 3: Unity Integration**

1. **Add Progress Manager to Unity**
   - Copy `unity-scripts/SimpleGameProgress.cs` to your Unity project
   - Add it to a GameObject in your scene
   - The script will automatically handle Firebase communication

2. **Use in Your Game Code**
```csharp
// Update player progress
SimpleGameProgress.Instance.UpdateProgress(level, lessons, score);

// Complete a lesson
SimpleGameProgress.Instance.CompleteLesson();

// Add score
SimpleGameProgress.Instance.AddScore(points);
```

### **Step 4: Deploy & Test**

1. **Upload to Web Server**
   - Upload entire project folder to your web server
   - Ensure all files maintain directory structure

2. **Test the System**
   - Open `index.html` in browser
   - Play the game and make progress
   - Check `dashboard/game-progress-dashboard.html` to see tracked data

## 📊 How It Works

### **Data Flow**
```
Unity Game → JavaScript Bridge → Firebase Firestore → Dashboard
```

### **Automatic Features**
- ✅ **Auto-save every 30 seconds**
- ✅ **Cross-website synchronization**
- ✅ **Anonymous user tracking**
- ✅ **Real-time progress updates**
- ✅ **Session management**

### **Tracked Data**
- **Progress**: Current level, completed lessons, play time
- **Scores**: High scores, completion times, mistakes
- **Activity**: Session timestamps, website URLs
- **Achievements**: Unlocked achievements with timestamps

## 🌐 Cross-Website Integration

### **Embed on Other Websites**

1. **Copy Game Files**
   - Upload your Unity build to any website
   - Include the Firebase bridge files

2. **Progress Syncs Automatically**
   - Same user ID across all websites
   - Progress updates in real-time
   - No additional setup required

### **Dashboard Integration**

Add this to any website to show user progress:

```html
<iframe src="https://yoursite.com/dashboard/game-progress-dashboard.html" 
        width="100%" height="600px" frameborder="0">
</iframe>
```

## 🔧 Customization Options

### **Modify Auto-Save Interval**
```csharp
// In SimpleGameProgress.cs
public float autoSaveInterval = 60f; // Change to 60 seconds
```

### **Add Custom Progress Fields**
```csharp
// In ProgressData class
public class ProgressData
{
    // Add your custom fields
    public int customStat = 0;
    public string customString = "";
}
```

### **Custom Dashboard Styling**
- Edit `dashboard/game-progress-dashboard.html`
- Modify CSS styles in the `<style>` section
- Add your branding colors and fonts

## 🐛 Troubleshooting

### **Common Issues**

1. **Firebase not connecting**
   - Check Firebase config in `js/firebase-config.js`
   - Verify Firestore rules allow read/write access

2. **Progress not saving**
   - Check browser console for JavaScript errors
   - Verify Unity is calling progress update methods

3. **Dashboard not loading**
   - Ensure Firebase config matches in both files
   - Check user ID is consistent across sessions

### **Debug Mode**
Enable debug logging by adding this to Unity:
```csharp
Debug.Log("Current Progress: " + JsonUtility.ToJson(currentProgress));
```

## 📈 Analytics & Monitoring

### **Firebase Analytics**
- Automatically tracks user engagement
- Monitor game performance across websites
- View real-time user statistics

### **Custom Metrics**
- Track completion rates
- Monitor cross-website usage
- Analyze learning patterns

## 🚀 Production Deployment

### **Security Considerations**
1. **Update Firestore Rules** for production
2. **Enable Firebase Authentication** for user accounts
3. **Implement rate limiting** for API calls
4. **Add data validation** in Unity scripts

### **Performance Optimization**
1. **Batch progress updates** to reduce API calls
2. **Implement offline caching** for better UX
3. **Use Firebase hosting** for faster loading

## 📞 Support

For issues or questions:
1. Check browser console for errors
2. Verify Firebase configuration
3. Test with Unity's debug logging
4. Check Firestore rules and permissions

---

**Ready to track progress across all websites! 🎮**
