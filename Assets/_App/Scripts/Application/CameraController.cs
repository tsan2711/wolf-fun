using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Camera Movement Settings")]
    [SerializeField] private float dragSpeed = 1f;
    [SerializeField] private float lerpSpeed = 5f;
    
    [Header("Position Constraints")]
    [SerializeField] private float minX = 6f;
    [SerializeField] private float maxX = 32f;
    [SerializeField] private float minZ = 15f;
    [SerializeField] private float maxZ = 27f;
    [SerializeField] private float fixedY = 10f;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebug = true;
    
    private Vector3 targetPosition;
    private Vector3 lastMousePos;
    private bool isDragging = false;
    private Camera cam;
    
    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
            cam = Camera.main;
            
        targetPosition = transform.position;
        targetPosition.y = fixedY;
        
        if (enableDebug)
            Debug.Log($"Camera Controller Start - Initial Position: {targetPosition}");
    }

    void Update()
    {
        HandleMouseInput();
        MoveCameraToTarget();
    }
    
    private void HandleMouseInput()
    {
        // Mouse down - bắt đầu drag
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            lastMousePos = Input.mousePosition;
            
            if (enableDebug)
                Debug.Log($"Start Drag - Mouse Position: {lastMousePos}");
        }
        
        // Mouse up - dừng drag
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            
            if (enableDebug)
                Debug.Log("Stop Drag");
        }
        
        // Đang drag
        if (isDragging && Input.GetMouseButton(0))
        {
            Vector3 currentMousePos = Input.mousePosition;
            Vector3 mouseDelta = currentMousePos - lastMousePos;
            
            if (enableDebug && mouseDelta.magnitude > 0.1f)
                Debug.Log($"Mouse Delta: {mouseDelta}");
            
            // Chuyển đổi mouse movement thành camera movement
            // Sử dụng camera's right và forward vectors
            Vector3 right = transform.right;
            Vector3 forward = transform.forward;
            
            // Project vectors onto XZ plane (loại bỏ Y component)
            right.y = 0;
            forward.y = 0;
            right.Normalize();
            forward.Normalize();
            
            // Tính toán movement
            Vector3 movement = (-right * mouseDelta.x - forward * mouseDelta.y) * dragSpeed * Time.deltaTime;
            
            if (enableDebug && movement.magnitude > 0.001f)
                Debug.Log($"Movement: {movement}");
            
            // Cập nhật target position
            targetPosition += movement;
            
            // Áp dụng constraints
            targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
            targetPosition.z = Mathf.Clamp(targetPosition.z, minZ, maxZ);
            targetPosition.y = fixedY;
            
            lastMousePos = currentMousePos;
        }
    }
    
    private void MoveCameraToTarget()
    {
        Vector3 oldPos = transform.position;
        transform.position = Vector3.Lerp(transform.position, targetPosition, lerpSpeed * Time.deltaTime);
        
        if (enableDebug && Vector3.Distance(oldPos, transform.position) > 0.001f)
            Debug.Log($"Camera moved to: {transform.position}");
    }
    
    public void SetCameraPosition(Vector3 position)
    {
        position.x = Mathf.Clamp(position.x, minX, maxX);
        position.z = Mathf.Clamp(position.z, minZ, maxZ);
        position.y = fixedY;
        
        targetPosition = position;
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = new Vector3((minX + maxX) / 2f, fixedY, (minZ + maxZ) / 2f);
        Vector3 size = new Vector3(maxX - minX, 0.1f, maxZ - minZ);
        Gizmos.DrawWireCube(center, size);
        
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(targetPosition, 0.5f);
        
        // Draw camera direction
        Gizmos.color = Color.blue;
        if (Application.isPlaying)
        {
            Gizmos.DrawRay(transform.position, transform.forward * 5f);
            Gizmos.DrawRay(transform.position, transform.right * 3f);
        }
    }
}