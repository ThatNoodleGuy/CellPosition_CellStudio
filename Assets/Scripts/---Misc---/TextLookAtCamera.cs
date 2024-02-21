using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TextLookAtCamera : MonoBehaviour
{
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main; // Find the main camera in the scene
#if UNITY_EDITOR
        // This object will only be active in the Unity Editor.
        gameObject.SetActive(true);
#else
        // Deactivate the GameObject when not in the editor (e.g., in a build).
        gameObject.SetActive(false);
#endif
    }

    private void LateUpdate()
    {
        // Make the text always face the camera
        transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward, mainCamera.transform.rotation * Vector3.up);
    }
}