using UnityEngine;

public class FacingCamera : MonoBehaviour
{
    [SerializeField] private bool reverseDirection = false;
    [SerializeField] private bool lockX = false;
    [SerializeField] private bool lockY = false;
    [SerializeField] private bool lockZ = false;
    
    private Camera mainCamera;
    
    void Start()
    {
        // Get the main camera
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindFirstObjectByType<Camera>();
        }
    }

    void Update()
    {
        if (mainCamera == null) return;
        
        // Calculate direction to camera
        Vector3 directionToCamera = mainCamera.transform.position - transform.position;
        
        // Reverse direction if needed (useful for some UI elements)
        if (reverseDirection)
        {
            directionToCamera = -directionToCamera;
        }
        
        // Apply axis locks
        if (lockX) directionToCamera.x = 0;
        if (lockY) directionToCamera.y = 0;
        if (lockZ) directionToCamera.z = 0;
        
        // Only rotate if there's a valid direction
        if (directionToCamera != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(directionToCamera);
        }
    }
}
