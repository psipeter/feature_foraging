using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 10f;

    void Update()
    {
        Vector3 moveDirection = Vector3.zero;

        // Use the New Input System to check keys
        if (Keyboard.current.wKey.isPressed) moveDirection.z = 1; // Move "Into" the depth
        if (Keyboard.current.sKey.isPressed) moveDirection.z = -1; // Move "Out" toward camera
        if (Keyboard.current.aKey.isPressed) moveDirection.x = -1; // Move Left
        if (Keyboard.current.dKey.isPressed) moveDirection.x = 1;  // Move Right

        // Move the player based on world coordinates
        transform.Translate(moveDirection.normalized * speed * Time.deltaTime, Space.World);
    }
}