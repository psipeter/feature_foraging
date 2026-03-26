using UnityEngine;
using System.Collections.Generic;

public class StimulusGenerator : MonoBehaviour
{
    [Header("Prefab & Parent")]
    public GameObject targetPrefab;   
    public Transform spawnParent;    

    [Header("Spawn Area")]
    public float spawnRadius = 15f;
    public int totalTargets = 10;

    [Header("Spacing Logic")]
    public float minDistance = 3.0f; // Increase this if they still overlap
    public int maxAttempts = 100;

    [Header("Safe Zone")]
    public float safeZoneRadius = 5.0f; // No cylinders can spawn within this distance of the player
    public Transform playerTransform;   // Drag your Player object here

    private List<Vector3> spawnedPositions = new List<Vector3>();

    void Start()
    {
        GenerateStimuli();
    }

    public void GenerateStimuli()
    {
        spawnedPositions.Clear();

        // 1. Clean up old targets
        Transform parent = spawnParent != null ? spawnParent : transform;
        for (int j = parent.childCount - 1; j >= 0; j--)
        {
            DestroyImmediate(parent.GetChild(j).gameObject);
        }

        // 2. Spawn Loop
        for (int i = 0; i < totalTargets; i++)
        {
            Vector3 spawnPos = FindValidPosition();

            if (spawnPos != Vector3.zero)
            {
                GameObject newTarget = Instantiate(targetPrefab, spawnPos, Quaternion.identity, parent);
                spawnedPositions.Add(spawnPos);
                
                // 3. Configure the ForagingTarget script
                ForagingTarget script = newTarget.GetComponent<ForagingTarget>();
                if (script != null)
                {
                    script.heightValue = Random.Range(0.3f, 1.0f);
                    script.colorValue = Random.value; 
                }
            }
        }
    }

    private Vector3 FindValidPosition()
    {
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            Vector2 randomPoint = Random.insideUnitCircle * spawnRadius;
            Vector3 potentialPos = new Vector3(randomPoint.x, 0, randomPoint.y);

            // 1. CLEARING ZONE CHECK (The "Player" check)
            if (playerTransform != null)
            {
                if (Vector3.Distance(potentialPos, playerTransform.position) < safeZoneRadius)
                {
                    continue; // Too close to player, try again
                }
            }

            // 2. MIN DISTANCE CHECK (The "Other Cylinders" check)
            bool isTooClose = false;
            foreach (Vector3 pos in spawnedPositions)
            {
                if (Vector3.Distance(potentialPos, pos) < minDistance)
                {
                    isTooClose = true;
                    break;
                }
            }

            if (!isTooClose) return potentialPos;
        }

        return Vector3.zero; 
    }
}