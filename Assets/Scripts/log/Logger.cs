using UnityEngine;
using System.IO;

public class Logger : MonoBehaviour
{
    private string logFilePath;

    private void Awake()
    {
        // Set the log file path. You can change "Application.persistentDataPath" to any path you prefer.
        logFilePath = Path.Combine(Application.persistentDataPath, "gameLog.txt");

        // Optional: Clear the log file at the start of the session.
        File.WriteAllText(logFilePath, string.Empty);
    }

    public void Log(string message)
    {
        // Include the current time for each log entry for better tracking
        string logMessage = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ": " + message;

        // Append the log message to the log file
        File.AppendAllText(logFilePath, logMessage + "\n");

        // Also log to the Unity Console if needed
        Debug.Log(message);
    }
}
