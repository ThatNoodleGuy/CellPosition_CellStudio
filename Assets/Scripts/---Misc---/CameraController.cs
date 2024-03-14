using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml; // Make sure to include System.Xml for XML manipulation

public class CameraController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5.0f; // Camera movement speed
    [SerializeField] private float rotationSpeed = 45.0f; // Camera rotation speed
    [SerializeField] private KeyCode resetRotationKey = KeyCode.R; // Key to reset camera rotation
    [SerializeField] private KeyCode topViewKey = KeyCode.T; // Key to switch to top view
    [SerializeField] private Transform rotationCenter; // The point around which the camera rotates

    private Vector3 areaSize;

    private void Start()
    {
        LoadConfigurationFromXML("Assets/Resources/ExampleReduced_SV.xml");
    }

    private void Update()
    {
        if (Input.GetKeyDown(topViewKey))
        {
            // Move to top view when T key is pressed
            transform.position = new Vector3(rotationCenter.position.x, transform.position.y, rotationCenter.position.z);
            transform.rotation = Quaternion.Euler(90, 0, 0);
            return; // Skip the rest of the update to avoid moving or rotating the camera further
        }

        // Camera movement
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        float upDownInput = Input.GetAxis("UpDown");

        Vector3 cameraForward = transform.forward;
        Vector3 cameraRight = transform.right;
        Vector3 cameraUp = transform.up;

        Vector3 moveDirection = (cameraRight * horizontalInput + cameraForward * verticalInput + cameraUp * upDownInput).normalized;

        Vector3 newPosition = transform.position + moveDirection * moveSpeed * Time.deltaTime;
        transform.position = newPosition;

        // Rotate around the dynamic point on Y-axis
        if (Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.Q))
        {
            float rotationInput = Input.GetKey(KeyCode.E) ? -rotationSpeed : (Input.GetKey(KeyCode.Q) ? rotationSpeed : 0);
            // Rotate around the Y axis at the rotation center
            transform.RotateAround(rotationCenter.position, Vector3.up, rotationInput * Time.deltaTime);

            // After rotation, adjust the camera to look at the rotation center
            Vector3 lookDirection = rotationCenter.position - transform.position;
            // Calculate the rotation looking at the rotation center with an upwards direction parallel to world's Y-axis
            Quaternion lookRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
            // Apply the rotation
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, lookRotation.eulerAngles.y, transform.rotation.eulerAngles.z);
        }

        // Reset camera rotation on key press
        if (Input.GetKeyDown(resetRotationKey))
        {
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
        }
    }


    void LoadConfigurationFromXML(string filePath)
    {
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(filePath);

        XmlNode areaSizeNode = xmlDoc.SelectSingleNode("//area_size");
        areaSize = new Vector3(
            float.Parse(areaSizeNode.SelectSingleNode("x").InnerText),
            float.Parse(areaSizeNode.SelectSingleNode("y").InnerText),
            float.Parse(areaSizeNode.SelectSingleNode("z").InnerText));

        // Assuming areaSize represents the total size, calculate the center point
        Vector3 centerPoint = new Vector3(areaSize.x / 2, areaSize.y / 2, areaSize.z / 2);

        // Set the rotation center to this calculated center point
        rotationCenter.position = centerPoint;
    }
}