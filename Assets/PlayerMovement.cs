using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 4f;
    public float maxHarvestDuration = 4.0f; 
    
    [Header("Detection Settings")]
    public float playerRadius = 0.5f; 

    private ForagingTarget currentTarget;
    private float harvestTimer = 0f;
    private float currentHarvestDuration = 0f;
    private bool isHarvesting = false;
    private Vector3 scaleAtHarvestStart;

    void Update()
    {
        if (isHarvesting)
        {
            HandleHarvesting();
        }
        else
        {
            FindNearestTarget();
            HandleMovement();
        }
    }

    void FindNearestTarget()
    {
        ForagingTarget[] allTargets = Object.FindObjectsByType<ForagingTarget>(FindObjectsSortMode.None);
        ForagingTarget closestInReach = null;

        foreach (var target in allTargets)
        {
            float dist = Vector2.Distance(
                new Vector2(transform.position.x, transform.position.z),
                new Vector2(target.transform.position.x, target.transform.position.z)
            );

            if (dist <= (playerRadius + target.detectionRadius))
            {
                closestInReach = target;
                break; 
            }
        }

        if (closestInReach != currentTarget)
        {
            if (currentTarget != null) currentTarget.SetHighlight(false);
            currentTarget = closestInReach;
            if (currentTarget != null) currentTarget.SetHighlight(true);
        }
    }

    void HandleMovement()
    {
        Vector2 input = new Vector2(
            (Keyboard.current.dKey.isPressed ? 1 : 0) - (Keyboard.current.aKey.isPressed ? 1 : 0),
            (Keyboard.current.wKey.isPressed ? 1 : 0) - (Keyboard.current.sKey.isPressed ? 1 : 0)
        );

        Vector3 move = new Vector3(input.x, 0, input.y).normalized;
        transform.Translate(move * moveSpeed * Time.deltaTime, Space.World);

        if (currentTarget != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            StartHarvest();
        }
    }

    void StartHarvest()
    {
        scaleAtHarvestStart = currentTarget.transform.localScale;
        currentHarvestDuration = maxHarvestDuration * currentTarget.GetCurrentPercent();
        harvestTimer = 0f;
        isHarvesting = true;
    }

    void HandleHarvesting()
    {
        if (!Keyboard.current.spaceKey.isPressed || currentTarget == null)
        {
            isHarvesting = false;
            return;
        }

        harvestTimer += Time.deltaTime;
        float progress = currentHarvestDuration > 0 ? harvestTimer / currentHarvestDuration : 1f;

        currentTarget.Shrink(scaleAtHarvestStart, progress);

        if (harvestTimer >= currentHarvestDuration)
        {
            isHarvesting = false;
            currentTarget = null;
        }
    }
}