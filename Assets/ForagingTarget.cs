using UnityEngine;

public class ForagingTarget : MonoBehaviour
{
    [Header("Data & Metadata")]
    public DataLogger logger;
    public string targetType;

    private Renderer rend;
    private Color originalColor;
    private Vector3 spawnScale;

    public float detectionRadius { get; private set; } 

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    void Start()
    {
        rend = GetComponent<Renderer>();
        if (rend != null) 
        {
            // Cache color using URP property or fallback
            originalColor = rend.material.HasProperty(BaseColorId) ? 
                            rend.material.GetColor(BaseColorId) : rend.material.color;

            rend.material.EnableKeyword("_EMISSION");
            rend.material.SetColor(EmissionColorId, Color.black);
        }

        spawnScale = transform.localScale;
        detectionRadius = spawnScale.x * 0.5f;
    }

    public void SetHighlight(bool highlight)
    {
        if (rend == null) return;

        Color emission = highlight ? originalColor * 2.5f : Color.black;
        rend.material.SetColor(EmissionColorId, emission);
        
        // Re-apply base color to maintain consistency
        if (rend.material.HasProperty(BaseColorId))
            rend.material.SetColor(BaseColorId, originalColor);
        else
            rend.material.color = originalColor;
    }

    public float GetCurrentPercent()
    {
        return spawnScale.x > 0 ? transform.localScale.x / spawnScale.x : 0f;
    }

    public void Shrink(Vector3 startScale, float progress)
    {
        transform.localScale = Vector3.Lerp(startScale, Vector3.zero, Mathf.Clamp01(progress));
        if (progress >= 1f) CompleteHarvest();
    }

    private void CompleteHarvest()
    {
        if (logger != null) logger.LogEvent("Object_Harvested", targetType);
        Destroy(gameObject);
    }
}