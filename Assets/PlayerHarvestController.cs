using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerHarvestController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f; // Reduced for 10x10 area

    [Header("Harvest Visuals")]
    public ParticleSystem vortexSystem;
    public Transform stockpilePivot;  // Drag the 'StockpilePivot' here
    public Transform stockpileCube;   // Drag the 'StockpileCube' here  
    public float harvestDuration = 2f;
    
    [Header("Reward & Growth")]
    public float growthRate = 0.05f; 
    [Range(0, 1)] public float idealHeight = 0.8f;
    [Range(0, 1)] public float idealColor = 0.2f;

    private ForagingTarget currentTarget;
    private bool isHarvesting = false;
    private float harvestProgress = 0f; // Persistent progress (0 to 1)
    private float totalReward = 0f;
    private float currentCylinderReward = 0f;
    private ParticleSystem.Particle[] particles;

    void Start()
    {
        if (vortexSystem != null)
        {
            particles = new ParticleSystem.Particle[vortexSystem.main.maxParticles];
            var em = vortexSystem.emission;
            em.rateOverTime = 0; 
        }

        // REMOVE or COMMENT OUT the line that sets localScale = Vector3.one
        // Instead, just ensure totalReward starts at 0
        totalReward = 0f;
    }

    void Update()
    {
        if (isHarvesting) HandleHarvesting();
        else { MovePlayer(); DetectTarget(); }
    }

    void MovePlayer()
    {
        Vector2 input = new Vector2(
            (Keyboard.current.dKey.isPressed ? 1 : 0) - (Keyboard.current.aKey.isPressed ? 1 : 0),
            (Keyboard.current.wKey.isPressed ? 1 : 0) - (Keyboard.current.sKey.isPressed ? 1 : 0)
        );
        Vector3 move = new Vector3(input.x, 0, input.y).normalized;
        transform.Translate(move * moveSpeed * Time.deltaTime, Space.World);
    }

    void DetectTarget()
    {
        ForagingTarget[] targets = FindObjectsByType<ForagingTarget>(FindObjectsSortMode.None);
        foreach (var t in targets)
        {
            if (Vector3.Distance(transform.position, t.transform.position) < t.detectionRadius)
            {
                if (Keyboard.current.spaceKey.wasPressedThisFrame) 
                {
                    // If moving to a brand new target, reset progress
                    if (currentTarget != t) harvestProgress = 0f;

                    currentTarget = t;
                    isHarvesting = true;
                    currentCylinderReward = 0f;
                    
                    if (vortexSystem != null && !vortexSystem.isPlaying) 
                        vortexSystem.Play();
                }
                return;
            }
        }
    }

    void HandleHarvesting()
    {
        // Unlock movement if Space is released or target is missing
        if (!Keyboard.current.spaceKey.isPressed || currentTarget == null)
        {
            StopHarvest(false);
            return;
        }

        // Increment progress based on time, continuing from where we left off
        harvestProgress += (Time.deltaTime / harvestDuration);
        
        // Calculate Quality
        float hDiff = Mathf.Abs(currentTarget.heightValue - idealHeight);
        float cDiff = Mathf.Abs(currentTarget.colorValue - idealColor);
        float quality = 1f - (hDiff + cDiff) / 2f;
        
        currentCylinderReward += quality * Time.deltaTime;

        // Visual Faucet
        var em = vortexSystem.emission;
        em.rateOverTime = quality * 60f;

        // Shrink target based on cumulative progress
        currentTarget.Shrink(1f - Mathf.Clamp01(harvestProgress));

        if (harvestProgress >= 1f) StopHarvest(true);
    }

    void LateUpdate()
    {
        // Ensure all references are assigned in the Inspector
        if (vortexSystem != null && stockpilePivot != null && stockpileCube != null)
        {
            // 1. Get the current active particles
            int num = vortexSystem.GetParticles(particles);
            
            if (num > 0)
            {
                // Calculate the current top surface of the growing cube
                // We use the pivot position + the current calculated height
                float currentHeight = 1f + totalReward;
                Vector3 topOfCube = stockpilePivot.position + new Vector3(0, currentHeight, 0);

                for (int i = 0; i < num; i++)
                {
                    // 2. Move particles toward the TOP of the stack
                    particles[i].position = Vector3.MoveTowards(
                        particles[i].position, 
                        topOfCube, 
                        22f * Time.deltaTime
                    );
                    
                    // 3. Accumulate global reward (feeds the growth)
                    // Multiplier 0.1f keeps the growth from being too explosive
                    totalReward += (growthRate * Time.deltaTime * 0.1f);
                }

                // 4. Update the Cube Visuals
                // Keep X and Z widths (including "Skinny" logic from Editor/Start)
                stockpileCube.localScale = new Vector3(
                    stockpileCube.localScale.x, 
                    currentHeight, 
                    stockpileCube.localScale.z
                );

                // 5. PIN THE BASE: Update local position to half-height 
                // This ensures the bottom stays at the pivot while the top expands
                stockpileCube.localPosition = new Vector3(0, currentHeight / 2f, 0);
                
                // 6. Apply changes back to the particle system
                vortexSystem.SetParticles(particles, num);
            }
        }
    }

    void StopHarvest(bool completed)
    {
        if (completed && currentTarget != null)
        {
            // Send final stats to the logger
            currentTarget.CompleteHarvest(harvestDuration, currentTarget.heightValue, currentTarget.colorValue, currentCylinderReward);
            harvestProgress = 0f; // Fully reset only upon completion
        }
        
        isHarvesting = false;
        if (vortexSystem != null) 
        { 
            var em = vortexSystem.emission; 
            em.rateOverTime = 0; 
        }
    }
}