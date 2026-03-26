using UnityEngine;

public class StimulusGenerator : MonoBehaviour
{
    public GameObject targetPrefab; 
    public DataLogger loggerReference; 
    
    [Header("Radial Distribution Settings")]
    [Tooltip("The central area where no objects will spawn.")]
    public float clearingRadius = 3f; 
    
    [Tooltip("The outer limit of the spawning zone.")]
    public float maxRadius = 25f; 
    
    [Tooltip("How many objects to attempt to spawn.")]
    public int numberOfTargets = 50;

    [Header("Collision & Placement")]
    public float minDistanceBetweenObjects = 1.5f;
    public LayerMask targetLayer; // Set this to the "Target" layer in the Inspector
    
    [Header("Visuals")]
    public Gradient colorGradient; 

    void Start()
    {
        GenerateForest();
    }

    void GenerateForest()
    {
        int spawnedCount = 0;
        int attempts = 0;
        int maxAttempts = 2000; // High limit to handle dense radial packing

        while (spawnedCount < numberOfTargets && attempts < maxAttempts)
        {
            attempts++;

            // 1. RADIAL MATH: Pick a random angle (0 to 360 degrees)
            float angle = Random.Range(0f, Mathf.PI * 2f);
            
            // 2. DENSITY MATH: Use a power function (t^2) to cluster distance toward the center
            // t = 0 is the center, t = 1 is the outer edge
            float t = Random.value; 
            float weightedDistance = t * t * (maxRadius - clearingRadius);
            
            // Offset by the clearing so the middle stays empty
            float finalDistance = clearingRadius + weightedDistance;

            // Convert Polar (Angle/Dist) to Cartesian (X/Z)
            Vector3 potentialPos = new Vector3(
                Mathf.Cos(angle) * finalDistance,
                0.5f, // Check slightly above floor height
                Mathf.Sin(angle) * finalDistance
            );

            // 3. VALIDATION: Check if this spot is already occupied on the Target layer
            if (!Physics.CheckSphere(potentialPos, minDistanceBetweenObjects, targetLayer))
            {
                SpawnTarget(new Vector3(potentialPos.x, 0, potentialPos.z));
                spawnedCount++;
            }
        }

        Debug.Log($"Generation Complete: Spawned {spawnedCount} objects in {attempts} attempts.");
    }

    void SpawnTarget(Vector3 pos)
    {
        GameObject newTarget = Instantiate(targetPrefab, pos, Quaternion.identity);
        ForagingTarget script = newTarget.GetComponent<ForagingTarget>();

        if (script != null)
        {
            script.logger = loggerReference;
            
            // Research values: Height (0.3 to 1.0) and Color (0.0 to 1.0)
            script.heightValue = Random.Range(0.3f, 1.0f); 
            script.colorValue = Random.value;

            // Apply the color from the gradient
            Renderer rend = newTarget.GetComponent<Renderer>();
            if (rend != null && colorGradient != null)
            {
                rend.material.color = colorGradient.Evaluate(script.colorValue);
            }
        }
    }
}