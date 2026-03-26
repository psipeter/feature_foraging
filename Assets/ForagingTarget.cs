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

    private float currentHeight;
    private ParticleSystem.Particle[] particles;
    private Transform stockpileTarget;
    private Renderer meshRenderer;

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
            if (sp != null) stockpileTarget = sp.transform;

            // Ensure emission is off by default
            var em = vortexSystem.emission;
            em.rateOverTime = 0;
        }
    }

    public void StartHarvesting(float amount)
    {
        if (vortexSystem == null || meshChild == null) return;

        // 1. Turn on the "Hose"
        var em = vortexSystem.emission;
        em.rateOverTime = 35;

        // 2. Shrink the cylinder
        currentHeight = Mathf.Max(0, currentHeight - amount);
        meshChild.localScale = new Vector3(meshChild.localScale.x, currentHeight, meshChild.localScale.z);
        
        // 3. Keep the BOTTOM pinned to the ground (Y=0)
        // Because the pivot is in the middle, we must offset by half the current height
        meshChild.localPosition = new Vector3(0, currentHeight / 2f, 0);

        // 4. Move the Emitter to the literal top of the mesh bounds
        if (meshRenderer != null)
        {
            float topY = meshRenderer.bounds.max.y;
            vortexSystem.transform.position = new Vector3(transform.position.x, topY, transform.position.z);
        }

        if (currentHeight <= 0.05f) 
        { 
            StopHarvesting(); 
            Destroy(gameObject, 0.5f); 
        }
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
        
        // Target: The top of the stockpile (using localScale.y as a height proxy)
        Vector3 sinkPos = stockpileTarget.position + Vector3.up * stockpileTarget.localScale.y;
        
        // Source: The literal top of the cylinder mesh right now
        Vector3 sourcePos = new Vector3(transform.position.x, meshRenderer.bounds.max.y, transform.position.z);

        for (int i = 0; i < num; i++)
        {
            // NEWBORN CHECK: If the particle is less than 1 frame old, 
            // force it to the sourcePos to kill the center-spawn flicker.
            float age = particles[i].startLifetime - particles[i].remainingLifetime;
            if (age < 0.05f)
            {
                particles[i].position = sourcePos;
            }

            // Movement toward the sink
            particles[i].position = Vector3.MoveTowards(particles[i].position, sinkPos, 28f * Time.deltaTime);
            
            // Arrival logic
            if (Vector3.Distance(particles[i].position, sinkPos) < 0.2f)
            {
                particles[i].remainingLifetime = 0;
            }
        }
        
        vortexSystem.SetParticles(particles, num);
    }
}