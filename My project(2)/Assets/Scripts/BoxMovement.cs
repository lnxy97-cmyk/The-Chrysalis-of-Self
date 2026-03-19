using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxMovement : MonoBehaviour
{
    [SerializeField] private LayerMask groundLayer;
    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;
    
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
    }
    
    public bool MoveToDir(Vector2 dir)
    {
        if (!IsBlocked(dir))
        {
            rb.velocity = dir * 3f;
            return true;
        }
        rb.velocity = Vector2.zero;
        return false;
    }

    private bool IsBlocked(Vector2 dir)
    {
        return Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0,dir,0.3f, groundLayer);
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }

}
