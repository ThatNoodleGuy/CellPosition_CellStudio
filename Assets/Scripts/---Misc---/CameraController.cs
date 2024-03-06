using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5.0f; // Camera movement speed
    [SerializeField] private float rotationSpeed = 45.0f; // Camera rotation speed
    [SerializeField] private KeyCode resetRotationKey = KeyCode.R; // Key to reset camera rotation

    private void Update()
    {
        // Camera movement
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        float upDownInput = Input.GetAxis("UpDown");

        // Get the camera's forward and right vectors
        Vector3 cameraForward = transform.forward;
        Vector3 cameraRight = transform.right;
        Vector3 cameraUp = transform.up;

        // Project the input onto the camera's vectors to get the movement direction
        Vector3 moveDirection = (cameraRight * horizontalInput + cameraForward * verticalInput + cameraUp * upDownInput).normalized;

        Vector3 newPosition = transform.position + moveDirection * moveSpeed * Time.deltaTime;

        transform.position = newPosition;

        // Camera rotation based on Q and E keys
        float rotation = 0.0f;

        if (Input.GetKey(KeyCode.Q))
        {
            rotation -= rotationSpeed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.E))
        {
            rotation += rotationSpeed * Time.deltaTime;
        }

        transform.Rotate(0, rotation, 0);

        // Reset camera rotation on key press
        if (Input.GetKeyDown(resetRotationKey))
        {
            transform.rotation = Quaternion.identity;
        }
    }
}