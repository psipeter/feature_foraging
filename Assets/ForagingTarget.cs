using UnityEngine;

public class ForagingTarget : MonoBehaviour
{
    public DataLogger logger;
    public string targetType = "Cylinder";
    public float heightValue; 
    public float colorValue;
    public float detectionRadius = 1.0f;
    
    private Vector3 spawnedScale; // The scale AFTER the generator modifies it

    void Start() 
    {
        // Height: map 0-1 to a range (e.g., 0.5 to 2.5)
        float visualHeight = heightValue * 2.5f; 
        
        // Width (Skinny Logic): Inverse of height. 
        // As heightValue goes UP (1.0), width goes DOWN (0.4).
        // As heightValue goes DOWN (0.1), width goes UP (1.2).
        float visualWidth = 1.3f - heightValue; 

        // Apply the research-driven scale
        transform.localScale = new Vector3(visualWidth, visualHeight, visualWidth);
        
        // Record this as our 100% starting point for the shrinking animation
        spawnedScale = transform.localScale;
    }

    public void Shrink(float percentRemaining)
    {
        // Multiply the modified spawned scale by the percentage
        transform.localScale = spawnedScale * percentRemaining;
    }

    public void CompleteHarvest(float timeSpent, float h, float c, float reward)
    {
        if (logger != null) 
        {
            string stats = $"Time:{timeSpent:F2}, H:{h:F2}, C:{c:F2}, Rew:{reward:F2}";
            logger.LogEvent("Object_Harvested", $"{targetType} | {stats}");
        }
        Destroy(gameObject);
    }
}