using UnityEngine;

public class ForagingTarget : MonoBehaviour
{
    [Header("Hierarchy References")]
    public Transform meshChild; 
    public ParticleSystem vortexSystem; 

    [Header("Data Settings")]
    public float colorValue;     
    public float heightValue = 0.5f; 

    [Header("Visuals")]
    public Gradient colorPalette; // Drag/Set your Viridis colors here in the Inspector  

    [Header("Vanish Settings")]
    public float vanishThreshold = 0.15f; // Stop harvesting at this height
    private bool isVanishing = false;

    [Header("Highlight Settings")]
    public float highlightIntensity = 2.0f; // How much it glows
    private Color originalColor;

    private float currentHeight;
    private ParticleSystem.Particle[] particles;
    private Transform stockpileTarget;
    private Renderer meshRenderer;
    private Renderer stockpileRenderer;

    void Start()
    {
        // 1. Initial Height setup
        currentHeight = heightValue * 2.5f;
        float visualWidth = 1.3f - heightValue;
        
        if (meshChild != null)
            {
                meshChild.localScale = new Vector3(visualWidth, currentHeight, visualWidth);
                meshChild.localPosition = new Vector3(0, currentHeight / 2f, 0);
                meshRenderer = meshChild.GetComponent<Renderer>();

                if (meshRenderer != null)
                    {
                        originalColor = colorPalette.Evaluate(colorValue);
                        meshRenderer.material.color = originalColor;
                        // Ensure emission is ready
                        meshRenderer.material.EnableKeyword("_EMISSION");
                    }

                // Apply the Viridis color based on the 0-1 colorValue from the Generator
                if (meshRenderer != null && colorPalette != null)
                {
                    // .Evaluate(colorValue) picks the exact color along your Viridis gradient
                    Color pickedColor = colorPalette.Evaluate(colorValue);
                    meshRenderer.material.color = pickedColor;
                }
            }

        if (vortexSystem != null)
        {
            particles = new ParticleSystem.Particle[vortexSystem.main.maxParticles];
            
            // Initial positioning of the emitter at the mesh top
            if (meshRenderer != null)
            {
                vortexSystem.transform.position = new Vector3(transform.position.x, meshRenderer.bounds.max.y, transform.position.z);
            }

            // Find the central stockpile
            GameObject sp = GameObject.Find("StockpileAnchor");
            if (sp != null) 
                    {
                        stockpileTarget = sp.transform;
                        // Get the renderer of the stockpile to find its "Top"
                        stockpileRenderer = sp.GetComponentInChildren<Renderer>();
                    }
            // Ensure emission is off by default
            var em = vortexSystem.emission;
            em.rateOverTime = 0;
        }
    }

    public void StartHarvesting(float amount)
    {
        if (vortexSystem == null || meshChild == null || isVanishing) return;

        var em = vortexSystem.emission;
        em.rateOverTime = 35;

        currentHeight = Mathf.Max(0, currentHeight - amount);
        
        // Update visuals
        meshChild.localScale = new Vector3(meshChild.localScale.x, currentHeight, meshChild.localScale.z);
        meshChild.localPosition = new Vector3(0, currentHeight / 2f, 0);

        if (meshRenderer != null)
        {
            float topY = meshRenderer.bounds.max.y;
            vortexSystem.transform.position = new Vector3(transform.position.x, topY, transform.position.z);
        }

        // NEW: The "Pop" Trigger
        if (currentHeight <= vanishThreshold) 
        { 
            TriggerVanish();
        }
    }

    private void TriggerVanish()
    {
        if (isVanishing) return;
        isVanishing = true;

        StopHarvesting();
        
        // 1. Flash the color to white
        if (meshRenderer != null)
        {
            meshRenderer.material.EnableKeyword("_EMISSION");
            meshRenderer.material.SetColor("_EmissionColor", Color.white * 2f); // HDR Glow
            meshRenderer.material.color = Color.white;
        }

        // 2. Scale it up slightly for a "pop" effect
        meshChild.localScale *= 1.2f;

        // 3. Kill it after a split second
        Destroy(gameObject, 0.1f); 
    }

    public void StopHarvesting()
    {
        if (vortexSystem != null)
        {
            var em = vortexSystem.emission;
            em.rateOverTime = 0;
        }
    }

    void LateUpdate()
    {
        if (vortexSystem == null || stockpileTarget == null || meshRenderer == null) return;

        int num = vortexSystem.GetParticles(particles);
        
        // THE FIX: Target the literal top surface of the stockpile
        Vector3 sinkPos;
        if (stockpileRenderer != null)
        {
            // Use the bounds.max.y to find the current top of the stockpile mesh
            float sinkTopY = stockpileRenderer.bounds.max.y;
            sinkPos = new Vector3(stockpileTarget.position.x, sinkTopY, stockpileTarget.position.z);
        }
        else
        {
            // Fallback if no renderer is found
            sinkPos = stockpileTarget.position + Vector3.up * stockpileTarget.localScale.y;
        }

        Vector3 sourcePos = new Vector3(transform.position.x, meshRenderer.bounds.max.y, transform.position.z);

        for (int i = 0; i < num; i++)
        {
            float age = particles[i].startLifetime - particles[i].remainingLifetime;
            if (age < 0.05f)
            {
                particles[i].position = sourcePos;
            }

            // Move toward the top of the stockpile
            particles[i].position = Vector3.MoveTowards(particles[i].position, sinkPos, 28f * Time.deltaTime);
            
            if (Vector3.Distance(particles[i].position, sinkPos) < 0.2f)
            {
                particles[i].remainingLifetime = 0;
            }
        }
        
        vortexSystem.SetParticles(particles, num);
    }

    public void SetHighlight(bool active)
    {
        if (meshRenderer == null || isVanishing) return;

        if (active)
        {
            // Set the emission to a brighter version of its own color
            meshRenderer.material.SetColor("_EmissionColor", originalColor * highlightIntensity);
        }
        else
        {
            // Turn off the glow
            meshRenderer.material.SetColor("_EmissionColor", Color.black);
        }
    }
}