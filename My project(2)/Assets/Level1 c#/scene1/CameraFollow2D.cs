using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    [Header("跟随目标")]
    public Transform target;
    public float smoothSpeed = 0.125f;
    public Vector3 offset = new Vector3(0, 0, -10);

    [Header("边界限制")]
    public bool useBounds = false;
    public float minX = -10f, maxX = 10f, minY = -5f, maxY = 5f;

    private Vector3 velocity = Vector3.zero;
    
    void Start()
    {
        if (target == null)
            target = GameObject.FindGameObjectWithTag("Player").transform;
            
        // 立即设置位置，避免后退效果
        if (target != null)
        {
            transform.position = GetTargetPosition();
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPosition = GetTargetPosition();
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothSpeed);
    }

    Vector3 GetTargetPosition()
    {
        Vector3 targetPos = target.position + offset;
        
        if (useBounds)
        {
            targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
            targetPos.y = Mathf.Clamp(targetPos.y, minY, maxY);
        }
        
        return targetPos;
    }

    public void SetCameraBounds(float minX, float maxX, float minY, float maxY)
    {
        this.minX = minX;
        this.maxX = maxX;
        this.minY = minY;
        this.maxY = maxY;
        useBounds = true;
    }
}