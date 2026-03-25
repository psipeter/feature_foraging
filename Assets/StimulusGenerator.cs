using UnityEngine;
using System.Collections.Generic;

public class StimulusGenerator : MonoBehaviour
{
    [Header("References")]
    public GameObject cylinderPrefab;
    public DataLogger logger;

    [Header("Environment Parameters")]
    public int numberOfTargets = 50;
    public float arenaSize = 50f; 
    public float minDistanceBetween = 2f;

    [Header("Feature Visuals")]
    public Gradient colorGradient; 
    public float minHeight = 0.5f;
    public float maxHeight = 3.0f;

    private List<Vector3> spawnedPositions = new List<Vector3>();
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    void Start()
    {
        GenerateEnvironment();
    }

    public void GenerateEnvironment()
    {
        for (int i = 0; i < numberOfTargets; i++)
        {
            Vector3 spawnPos = GetValidPosition();
            if (spawnPos != Vector3.zero) CreateTarget(spawnPos);
        }
    }

    Vector3 GetValidPosition()
    {
        for (int attempt = 0; attempt < 100; attempt++)
        {
            Vector3 pos = new Vector3(
                Random.Range(-arenaSize * 0.5f, arenaSize * 0.5f), 
                0f, 
                Random.Range(-arenaSize * 0.5f, arenaSize * 0.5f)
            );

            bool tooClose = false;
            foreach (Vector3 existingPos in spawnedPositions)
            {
                if (Vector3.Distance(pos, existingPos) < minDistanceBetween)
                {
                    tooClose = true;
                    break;
                }
            }

            if (!tooClose)
            {
                spawnedPositions.Add(pos);
                return pos;
            }
        }
        return Vector3.zero;
    }

    void CreateTarget(Vector3 position)
    {
        GameObject obj = Instantiate(cylinderPrefab, position, Quaternion.identity, transform);
        
        float tShape = Random.value; 
        float tColor = Random.value; 

        float finalHeight = Mathf.Lerp(minHeight, maxHeight, tShape);
        obj.transform.localScale = new Vector3(1f, finalHeight, 1f);
        obj.transform.position = new Vector3(position.x, finalHeight, position.z);

        Renderer rend = obj.GetComponent<Renderer>();
        if (rend != null)
        {
            Color chosenColor = colorGradient.Evaluate(tColor);
            if (rend.material.HasProperty(BaseColorId))
                rend.material.SetColor(BaseColorId, chosenColor);
            else
                rend.material.color = chosenColor;
        }

        if (obj.TryGetComponent(out ForagingTarget targetScript))
        {
            targetScript.logger = logger;
            targetScript.targetType = $"H:{tShape:F2}_C:{tColor:F2}";
        }
    }
}