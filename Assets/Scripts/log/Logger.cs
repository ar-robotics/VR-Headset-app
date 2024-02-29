using UnityEngine;
using System.IO;

/// <summary>
/// This script is used to log messages to a file. It is used for testing purposes.
/// </summary>
public class Logger : MonoBehaviour
{
    private string logFilePath;

    /// <summary>
    /// Awake is called when the script instance is being loaded. It gets the path to the log file and clears it at the start of the session.
    /// </summary>
    private void Awake()
    {
        // Set the log file path to the persistent data path
        logFilePath = Path.Combine(Application.persistentDataPath, "gameLog.txt");

        // Clear the log file at the start of the session.
        File.WriteAllText(logFilePath, string.Empty);
    }

    /// <summary>
    /// This method is used to log messages to the log file and the Unity Console.
    /// </summary>
    /// <param name="message"></param>
    public void Log(string message)
    {
        string logMessage = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ": " + message;
        File.AppendAllText(logFilePath, logMessage + "\n");

        // For testing purposes, also log the message to the Unity Console
        Debug.Log(message);
    }
}
