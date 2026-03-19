using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RisingTraps : MonoBehaviour
{
    [Header("Trap Settings")] 
    [SerializeField] public float riseSpeed = 5f;
    [SerializeField] public float targetY = 3.66f;

    private bool isRisen = false;
    Rigidbody2D rb;
    
    // Start is called before the first frame update
    void Start()
    {
        rb =  GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isRisen && other.CompareTag("Player"))
        {
            StartCoroutine(RiseTrap());
        }
    }

    IEnumerator RiseTrap()
    {
        isRisen = true;
        Vector2 targetPos = new Vector2(transform.position.x, targetY);

        while (transform.position.y < targetY - 0.01f)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPos, riseSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPos;
    }
}
