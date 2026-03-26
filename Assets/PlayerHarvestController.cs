using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerHarvestController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float growthRate = 1.0f;

    [Header("Stockpile References")]
    public Transform stockpileMesh;
    
    private float totalReward = 0f;
    private ForagingTarget currentTarget;

    void Update()
    {
        MovePlayer();
        
        if (stockpileMesh != null)
        {
            float h = 1f + totalReward;
            stockpileMesh.localScale = new Vector3(stockpileMesh.localScale.x, h, stockpileMesh.localScale.z);
            stockpileMesh.localPosition = new Vector3(0, h / 2f, 0);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Target"))
        {
            ForagingTarget target = other.GetComponent<ForagingTarget>();
            if (target != null)
            {
                currentTarget = target;
                currentTarget.SetHighlight(true); // Turn on the glow
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // Find the ForagingTarget script on the object or its parent
        ForagingTarget target = other.GetComponent<ForagingTarget>() ?? other.GetComponentInParent<ForagingTarget>();

        if (target != null)
        {
            currentTarget = target;

            if (Keyboard.current.spaceKey.isPressed)
            {
                float amount = growthRate * Time.deltaTime;
                currentTarget.StartHarvesting(amount);
                
                // Only add reward if it's still "alive"
                if (currentTarget != null) 
                {
                    totalReward += (amount * 0.4f);
                }
            }
            else
            {
                currentTarget.StopHarvesting();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Target"))
        {
            ForagingTarget target = other.GetComponent<ForagingTarget>();
            if (target != null)
            {
                target.SetHighlight(false); // Turn off the glow
                if (currentTarget == target) currentTarget = null;
            }
        }
    }

    void MovePlayer()
    {
        float mx = (Keyboard.current.dKey.isPressed ? 1 : 0) - (Keyboard.current.aKey.isPressed ? 1 : 0);
        float mz = (Keyboard.current.wKey.isPressed ? 1 : 0) - (Keyboard.current.sKey.isPressed ? 1 : 0);
        transform.Translate(new Vector3(mx, 0, mz).normalized * moveSpeed * Time.deltaTime, Space.World);
    }
}