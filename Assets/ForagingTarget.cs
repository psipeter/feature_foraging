using UnityEngine;

public class ForagingTarget : MonoBehaviour
{
    // We will link this in the Unity Inspector
    public DataLogger logger; 
    
    // Identifiers for your research analysis
    public string targetType = "Red_Berry"; 
    public int pointValue = 10;

    // This triggers when the Player (with a Rigidbody) enters the Sphere's area
    private void OnTriggerEnter(Collider other)
    {
        // Check if the object touching us is the Player
        if (other.CompareTag("Player"))
        {
            // 1. Tell the logger to record the collection
            if (logger != null)
            {
                logger.LogEvent("Target_Collected", targetType);
            }

            // 2. Provide feedback (optional)
            Debug.Log($"Collected {targetType}! +{pointValue} points.");

            // 3. Make the target "disappear"
            gameObject.SetActive(false); 
            
            // Note: In a real foraging task, you might 'Destroy' it 
            // or move it to a new random location instead.
        }
    }
}