using UnityEngine;
using System.IO;
using UnityEngine.InputSystem; // 1. Add this line at the very top

public class DataLogger : MonoBehaviour
{
    private string filePath;

    void Start()
    {
        filePath = Application.dataPath + "/foraging_data.csv";
        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, "Timestamp,Event,Object_ID\n");
        }
    }

    void Update()
    {
        // 2. Change the old Input check to this new one:
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            LogEvent("Manual_Test", "Keyboard_Input");
            Debug.Log("Spacebar pressed: Sent test data to CSV.");
        }
    }

    public void LogEvent(string eventName, string objectID)
    {
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        string entry = $"{timestamp},{eventName},{objectID}\n";
        File.AppendAllText(filePath, entry);
    }
}