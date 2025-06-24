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
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindFirstObjectByType<Camera>();
        }
    }

    void Update()
    {
        if (mainCamera == null) return;
        
        Vector3 directionToCamera = mainCamera.transform.position - transform.position;
        
        if (reverseDirection)
        {
            directionToCamera = -directionToCamera;
        }

        if (lockX) directionToCamera.x = 0;
        if (lockY) directionToCamera.y = 0;
        if (lockZ) directionToCamera.z = 0;
        
        if (directionToCamera != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(directionToCamera);
        }
    }
}
