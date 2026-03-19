using UnityEngine;

public class SimplePlayerClimb : MonoBehaviour
{
    [Header("梯子设置")]
    [SerializeField] private float climbSpeed = 3f;
    
    [Header("组件引用")]
    private Rigidbody2D rb;
    private BoxCollider2D playerCollider;
    
    [Header("状态")]
    private bool canClimb = false;
    private bool isClimbing = false;
    private Collider2D currentLadder = null;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        float verticalInput = Input.GetAxis("Vertical");
        
        // 1. 进入梯子：按上/下键且在梯子范围内
        if (!isClimbing && canClimb && Mathf.Abs(verticalInput) > 0.1f)
        {
            StartClimbing();
        }
        
        // 2. 梯子移动逻辑
        if (isClimbing)
        {
            // 垂直移动
            rb.velocity = new Vector2(0, verticalInput * climbSpeed);
            
            // 退出条件
            CheckExitConditions(verticalInput);
        }
    }
    
    void StartClimbing()
    {
        if (!isClimbing && currentLadder != null)
        {
            isClimbing = true;
            rb.gravityScale = 0;
            rb.velocity = Vector2.zero;
            
            // 对齐到梯子中心
            if (currentLadder != null)
            {
                Vector3 ladderPos = currentLadder.transform.position;
                transform.position = new Vector3(ladderPos.x, transform.position.y, transform.position.z);
            }
            
            Debug.Log("开始爬梯子");
        }
    }
    
    void StopClimbing()
    {
        if (isClimbing)
        {
            isClimbing = false;
            rb.gravityScale = 1;
            
            // 确保不会穿地
            if (IsGrounded())
            {
                rb.velocity = new Vector2(rb.velocity.x, 0);
            }
            
            Debug.Log("停止爬梯子");
        }
    }
    
    void CheckExitConditions(float verticalInput)
    {
        // 1. 跳跃键退出
        if (Input.GetButtonDown("Jump"))
        {
            StopClimbing();
            return;
        }
        
        // 2. 离开梯子范围
        if (!canClimb)
        {
            StopClimbing();
            return;
        }
        
        // 3. 向下到底部
        if (verticalInput < 0 && IsGrounded())
        {
            StopClimbing();
            return;
        }
        
        // 4. 手动停止（按反方向）
        if (verticalInput < -0.5f && IsGrounded())
        {
            StopClimbing();
        }
    }
    
    bool IsGrounded()
    {
        float checkDistance = 0.1f;
        RaycastHit2D hit = Physics2D.BoxCast(
            playerCollider.bounds.center,
            playerCollider.bounds.size,
            0f,
            Vector2.down,
            checkDistance,
            LayerMask.GetMask("Ground")
        );
        
        return hit.collider != null;
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ladder"))
        {
            canClimb = true;
            currentLadder = other;
            Debug.Log("进入梯子范围");
        }
    }
    
    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Ladder"))
        {
            canClimb = true;
            currentLadder = other;
        }
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Ladder"))
        {
            canClimb = false;
            currentLadder = null;
            Debug.Log("离开梯子范围");
        }
    }
    
    public bool GetIsClimbing()
    {
        return isClimbing;
    }
}