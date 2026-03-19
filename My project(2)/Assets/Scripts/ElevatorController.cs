using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorController : MonoBehaviour
{
    public float targetY = -4.48f;
    public float initialY = 6.34f;
    public float delayBeforeRise = 3f; 
    public float speed = 3f;

    private bool moving = false;
    private Rigidbody2D rb;
    
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        transform.position = new Vector2(transform.position.x, initialY);
    }

    public void Descend()
    {
        if (!moving)
        {
            StartCoroutine(MoveToTarget(targetY));
        }
    }

    private IEnumerator DelayRise()
    {
        yield return new WaitForSeconds(delayBeforeRise);
        if (!moving) StartCoroutine(MoveToTarget(initialY));
    }

    private IEnumerator MoveToTarget(float downY)
    {
        moving = true;
        Vector2 targetPos = new Vector2(transform.position.x, downY);

        while (Vector2.Distance(transform.position, targetPos) > 0.01f)
        {
            transform.position = Vector2.MoveTowards(transform.position, 
                targetPos,speed * Time.deltaTime);
            yield return null;
        }
        
        transform.position = targetPos;
        moving = false;

        if (downY == targetY)
        {
            StartCoroutine(DelayRise());
        }
    }

}
