using UnityEngine;

public class PlayerController2D : MonoBehaviour
{
    [Header("移动参数")]
    public float moveSpeed = 8f;
    
    [Header("移动控制")]
    public bool movementEnabled = true;
    
    [Header("地面检测")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.3f;
    public LayerMask groundLayer;
    
    [Header("视觉效果")]
    public bool facingRight = true;
    public Animator animator;
    
    // 组件引用
    private Rigidbody2D rb;
    
    // 状态变量
    private bool isGrounded;
    private float moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        
        if (rb != null)
        {
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }
        
        if (groundCheck == null)
        {
            CreateGroundCheck();
        }
    }

    void Update()
    {
        if (!movementEnabled) 
        {
            moveInput = 0f;
            HandleAnimation();
            return;
        }
        
        GetInput();
        CheckGround();
        HandleFacingDirection();
        HandleAnimation();
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    void GetInput()
    {
        moveInput = Input.GetAxisRaw("Horizontal");
    }

    void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    void HandleMovement()
    {
        if (!movementEnabled || rb == null) 
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
            return;
        }
        
        // 直接设置水平速度，没有加速度/减速度
        Vector2 velocity = rb.velocity;
        velocity.x = moveInput * moveSpeed;
        rb.velocity = velocity;
    }

    void HandleFacingDirection()
    {
        if (!movementEnabled) return;
        
        // 只有在有明确输入时改变朝向
        if (Mathf.Abs(moveInput) > 0.1f)
        {
            bool shouldFaceRight = moveInput > 0;
            
            if (shouldFaceRight != facingRight)
            {
                Flip();
            }
        }
    }

    void HandleAnimation()
    {
        if (animator != null)
        {
            // 使用输入值而不是实际速度，这样动画更响应输入
            float displaySpeed = movementEnabled ? Mathf.Abs(moveInput) : 0f;
            animator.SetFloat("Speed", displaySpeed);
            animator.SetBool("IsGrounded", isGrounded);
        }
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }

    void CreateGroundCheck()
    {
        GameObject groundCheckObj = new GameObject("GroundCheck");
        groundCheckObj.transform.SetParent(transform);
        groundCheckObj.transform.localPosition = new Vector3(0, -0.5f, 0);
        groundCheck = groundCheckObj.transform;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
    
    // 公共方法
    public bool IsGrounded()
    {
        return isGrounded;
    }
    
    public bool IsMoving()
    {
        return Mathf.Abs(moveInput) > 0.1f;
    }
    
    // 外部控制移动的方法
    public void SetMovementEnabled(bool enabled)
    {
        movementEnabled = enabled;
        if (!enabled && rb != null)
        {
            // 立即停止所有水平移动
            rb.velocity = new Vector2(0f, rb.velocity.y);
        }
    }
    
    // 新增：强制停止移动的方法
    public void StopImmediately()
    {
        if (rb != null)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
        }
        moveInput = 0f;
    }
}