using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private BoxCollider2D coll;
    private SpriteRenderer sprite;
    private Animator anim;
    private SimplePlayerClimb playerClimb;

    [SerializeField] private LayerMask jumpableGround;

    private float dirX = 0f;
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float airMoveSpeed = 4f;
    
    [SerializeField] private float wallCheckDistance = 0.1f;
    
    private enum MovementState { idle, run, jump, fall }
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<BoxCollider2D>();
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        playerClimb = GetComponent<SimplePlayerClimb>();
    }

    private void Update()
    {
        // 如果在爬梯子，跳过正常移动逻辑
        if (playerClimb != null && playerClimb.GetIsClimbing())
        {
            UpdateAnimationState();
            return;
        }

        dirX = Input.GetAxisRaw("Horizontal");
        
        bool isTouchingSideWall = CheckSideWallCollision();
        
        if (IsGrounded())
        {
            rb.velocity = new Vector2(dirX * moveSpeed, rb.velocity.y);
        }
        else
        {
            if (isTouchingSideWall && dirX != 0)
            {
                rb.velocity = new Vector2(0, rb.velocity.y);
            }
            else
            {
                rb.velocity = new Vector2(dirX * airMoveSpeed, rb.velocity.y);
            }
        }

        if (dirX != 0)
        {
            Vector2 detectPos = transform.position + new Vector3(dirX * 5f, 0f, 0f);
            Collider2D boxCollider = Physics2D.OverlapBox(detectPos, new Vector2(0.3f, 1f), 0f, LayerMask.GetMask("Box"));

            if (boxCollider != null)
            {
                BoxMovement box = boxCollider.GetComponent<BoxMovement>();
                if (box != null)
                {
                    box.MoveToDir(new Vector2(dirX, 0));
                }
            }
        }

        if (Input.GetButtonDown("Jump") && IsGrounded())
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }

        UpdateAnimationState();
    }

    private bool CheckSideWallCollision()
    {
        Vector2 rayOrigin = coll.bounds.center;
        float rayLength = coll.bounds.extents.x + wallCheckDistance;
        
        RaycastHit2D hitLeft = Physics2D.Raycast(rayOrigin, Vector2.left, rayLength, jumpableGround);
        RaycastHit2D hitRight = Physics2D.Raycast(rayOrigin, Vector2.right, rayLength, jumpableGround);
        
        return hitLeft.collider != null || hitRight.collider != null;
    }

    private void UpdateAnimationState()
    {
        MovementState state;

        if (dirX > 0f)
        {
            state = MovementState.run;
            sprite.flipX = false;
        }
        else if (dirX < 0f)
        {
            state = MovementState.run;
            sprite.flipX = true;
        }
        else
        {
            state = MovementState.idle;
        }

        if (rb.velocity.y > .1f)
        {
            state = MovementState.jump;
        }
        else if (rb.velocity.y < -.1f)
        {
            state = MovementState.fall;
        }

        anim.SetInteger("state", (int)state);
    }

    private bool IsGrounded()
    {
        return Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.down, .1f, jumpableGround);
    }
    
    private void OnDrawGizmosSelected()
    {
        if (coll != null)
        {
            Gizmos.color = Color.blue;
            Vector2 rayOrigin = coll.bounds.center;
            float rayLength = coll.bounds.extents.x + wallCheckDistance;
            
            Gizmos.DrawLine(rayOrigin, rayOrigin + Vector2.left * rayLength);
            Gizmos.DrawLine(rayOrigin, rayOrigin + Vector2.right * rayLength);
            
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(coll.bounds.center + Vector3.down * 0.1f, coll.bounds.size);
        }
    }
}